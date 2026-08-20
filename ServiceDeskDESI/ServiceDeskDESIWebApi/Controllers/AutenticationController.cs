using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Autentication")]
    public class AutenticationController : BaseController
    {
        private readonly AutenticacionService _autenticacionService;

        public AutenticationController()
        {
            _autenticacionService = new AutenticacionService();
        }

        /// <summary>
        /// Obtiene todos los usuarios de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de usuarios</returns>
        [HttpGet, Route("User/List")]
        public ModelResponse<List<UsuarioDTO>> ObtenerUsuarios()
        {
            var usuario = User.Identity.Name;
            var result = _autenticacionService.ObtenerUsuarios(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Usuario encontrado</returns>
        [HttpGet, Route("User/{id:long}")]
        public ModelResponse<UsuarioDTO> ObtenerUsuarioPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _autenticacionService.ObtenerUsuarioPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un usuario
        /// </summary>
        /// <param name="u">Objeto usuario con los datos</param>
        /// <returns>Usuario guardado con su ID actualizado</returns>
        [Permiso("Usuarios")]
        [HttpPost, Route("User")]
        public ModelResponse<Usuario> GuardarOActualizarUsuario(Usuario u)
        {
            var result = _autenticacionService.GuardarOActualizarUsuario(u);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un usuario en una empresa
        /// </summary>
        /// <param name="u">Objeto usuario con los datos</param>
        /// <returns>Usuario guardado con su ID actualizado</returns>
        [Permiso("Usuarios")]
        [HttpPost, Route("User/Empresa")]
        public ModelResponse<Usuario> GuardarUsuarioEmpresa(Usuario u)
        {
            var result = _autenticacionService.GuardarOActualizarUsuario(u);
            return result;
        }

        /// <summary>
        /// Actualiza el perfil del usuario autenticado
        /// </summary>
        /// <param name="usuario">Objeto usuario con los datos a actualizar</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Mi Perfil", "Editar")]
        [HttpPost, Route("ActualizarPerfil")]
        public ModelResponse<Usuario> ActualizarPerfilUsuario(Usuario usuario)
        {
            var usuarioAutenticado = User.Identity.Name;
            var result = _autenticacionService.ActualizarPerfilUsuario(usuario, usuarioAutenticado);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un usuario
        /// </summary>
        /// <param name="u">Usuario a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Usuarios", "Eliminar")]
        [HttpDelete, Route("User")]
        public ModelResponse EliminarUsuario(Usuario u)
        {
            u.FechaModificacion = DateTime.Now;
            var result = _autenticacionService.EliminarUsuario(u.Id, u.ModificadoPor, u.FechaModificacion.Value);
            return result;
        }

        /// <summary>
        /// Autentica un usuario en el sistema
        /// </summary>
        /// <param name="u">Objeto usuario con NombreUsuario y Contrasena</param>
        /// <returns>Usuario autenticado con sus datos completos y empresa</returns>
        [AllowAnonymous]
        [HttpPost, Route("autenticar")]
        public ModelResponse<UsuarioDTO> AutenticarUsuario(Usuario u)
        {
            var result = _autenticacionService.AutenticarUsuario(u.NombreUsuario, u.Contrasena);
            return result;
        }

        /// <summary>
        /// Método que se va a encargar de mandar un correo para restaurar la contraseña
        /// </summary>
        /// <param name="u">Objeto usuario con Correo</param>
        /// <returns>Resultado de la operación</returns>
        [AllowAnonymous]
        [HttpPost, Route("ValidarRecetearContrasenia")]
        public ModelResponse ValidarRecetearContrasenia(Usuario u)
        {
            var result = _autenticacionService.ValidarRecetearContrasenia(u.Correo);
            return result;
        }

        /// <summary>
        /// Valida el token de recuperación de contraseña
        /// </summary>
        /// <param name="token">Token GUID</param>
        /// <returns>Información del token y usuario</returns>
        [AllowAnonymous]
        [HttpGet, Route("validarToken/{token}")]
        public ModelResponse<TokenRecuperacionDTO> ValidarTokenRecuperacion(string token)
        {
            var result = _autenticacionService.ObtenerTokenRecuperacion(token);
            return result;
        }

        /// <summary>
        /// Actualiza la contraseña del usuario usando el token
        /// </summary>
        /// <param name="request">Objeto con token y nueva contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [AllowAnonymous]
        [HttpPost, Route("restablecerContrasenia")]
        public ModelResponse RestablecerContrasenia(RestablecerContraseniaRequest request)
        {
            var result = _autenticacionService.RestablecerContrasenia(request.Token, request.NuevaContrasena);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un usuario por parte del administrador
        /// </summary>
        /// <param name="usuario">Objeto usuario con los datos</param>
        /// <returns>Usuario guardado con su ID actualizado</returns>
        [Permiso("Usuarios")]
        [HttpPost, Route("Admin/Usuario")]
        public ModelResponse<Usuario> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            var usuarioAdmin = User.Identity.Name;
            var result = _autenticacionService.GuardarOActualizarUsuarioAdmin(usuario, usuarioAdmin);
            return result;
        }
    }

    public class RestablecerContraseniaRequest
    {
        public string Token { get; set; }
        public string NuevaContrasena { get; set; }
    }
}