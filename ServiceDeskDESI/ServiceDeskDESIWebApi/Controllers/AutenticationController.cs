using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Autentication")]
    public class AutenticationController : BaseController
    {

        private readonly DbWrapper dbWrapper;

        public AutenticationController()
        {
            dbWrapper = new DbWrapper();
        }

        /// <summary>
        /// Obtiene todos los usuarios activos
        /// </summary>
        /// <returns>Lista de usuarios con sus sucursales y áreas</returns>
        [HttpGet, Route("User/Lista")]
        public ModelResponse ObtenerUsuarios()
        {
            var result = dbWrapper.ObtenerUsuarios();
            return result;
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado</returns>
        [HttpGet, Route("User/{id:long}")]
        public ModelResponse ObtenerUsuarioPorId(long id)
        {
            var result = dbWrapper.ObtenerUsuarioPorId(id);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un usuario
        /// </summary>
        /// <param name="u">Objeto usuario con los datos</param>
        /// <returns>Usuario guardado con su ID actualizado</returns>
        [HttpPost, Route("User")]
        public ModelResponse GuardarOActualizarUsuario(Usuario u)
        {
            var result = dbWrapper.GuardarOActualizarUsuario(u);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un usuario
        /// </summary>
        /// <param name="u">Usuario a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("User")]
        public ModelResponse EliminarUsuario(Usuario u)
        {
            u.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarUsuario(u.Id, u.ModificadoPor, u.FechaModificacion.Value);
            return result;
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="u">Objeto usuario con NombreUsuario y Contrasena</param>
        /// <returns>Usuario autenticado con sus datos completos</returns>
        [HttpPost, Route("autenticar")]
        public ModelResponse AutenticarUsuario(Usuario u)
        {
            var result = dbWrapper.AutenticarUsuario(u.NombreUsuario, u.Contrasena);
            return result;
        }

        /// <summary>
        /// Método que se va a encargar de mandar un correo para resaurar la contraseña
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        [HttpPost, Route("ValidarRecetearContrasenia")]
        public ModelResponse ValidarRecetearContrasenia(Usuario u)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var userResponse = dbWrapper.ObtenerUsuarioPorNombreUsuario(u.NombreUsuario);

                if (userResponse == null || !userResponse.IsSuccess || userResponse.Response == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La información proporcionada no es correcta";
                    return modelResponse;
                }

                var usuario = (Usuario)userResponse.Response;

                // Generar token único para recuperación
                string token = Guid.NewGuid().ToString();
                int vigenciaMinutos = 10;
                DateTime fechaExpiracion = DateTime.Now.AddMinutes(vigenciaMinutos);

                // Guardar token en base de datos
                var tokenResponse = dbWrapper.InsertarTokenRecuperacion(usuario.Id, token, fechaExpiracion, "system");

                if (!tokenResponse.IsSuccess)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Error al generar la solicitud de recuperación";
                    return modelResponse;
                }

                // Obtener URL base del Web.config
                string baseUri = System.Configuration.ConfigurationManager.AppSettings["BaseUri"];
                string urlRecuperacion = $"{baseUri}Home/RecoverPassword/{token}";

                // Leer template
                string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Template/Template_RecuperarEmail.html");
                string templateHtml = System.IO.File.ReadAllText(templatePath);

                // Reemplazar variables en el template
                templateHtml = templateHtml.Replace("{{Nombre}}", usuario.Nombre);
                templateHtml = templateHtml.Replace("{{Apellido}}", usuario.Apellido);
                templateHtml = templateHtml.Replace("{{UrlRecuperacion}}", urlRecuperacion);

                // Enviar correo
                var para = new List<string> { usuario.Correo };
                EmailHelper.EnvioEmaiil(para, "Recuperación de contraseña - Service Desk DESI", templateHtml, false);

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Se ha enviado un correo para restablecer la contraseña";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al procesar la solicitud";
            }

            return modelResponse;
        }

        /// Valida el token de recuperación de contraseña
        /// </summary>
        /// <param name="token">Token GUID</param>
        /// <returns>Información del token y usuario</returns>
        [HttpGet, Route("validarToken/{token}")]
        public ModelResponse ValidarTokenRecuperacion(string token)
        {
            var result = dbWrapper.ObtenerTokenRecuperacion(token);
            return result;
        }

        /// <summary>
        /// Actualiza la contraseña del usuario usando el token
        /// </summary>
        /// <param name="request">Objeto con token y nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost, Route("restablecerContrasenia")]
        public ModelResponse RestablecerContrasenia(RestablecerContraseniaRequest request)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validar token
                var tokenResponse = dbWrapper.ObtenerTokenRecuperacion(request.Token);

                if (!tokenResponse.IsSuccess || tokenResponse.Response == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El enlace de recuperación no es válido o ha expirado";
                    return modelResponse;
                }

                dynamic tokenInfo = tokenResponse.Response;

                // Actualizar contraseña del usuario
                var usuario = new Usuario
                {
                    Id = tokenInfo.UsuarioId,
                    Contrasena = request.NuevaContrasena,
                    ModificadoPor = tokenInfo.NombreUsuario,
                    FechaModificacion = DateTime.Now
                };

                var updateResponse = dbWrapper.ActualizarContrasena(usuario);

                if (!updateResponse.IsSuccess)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = updateResponse.Message;
                    return modelResponse;
                }

                // Marcar token como usado
                dbWrapper.ActualizarTokenUsado(tokenInfo.Id, tokenInfo.NombreUsuario);

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Contraseña actualizada correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al procesar la solicitud";
            }

            return modelResponse;
        }
    }
}
