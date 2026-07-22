using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class AutenticacionService
    {
        private readonly DbWrapper _dbWrapper;

        public AutenticacionService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerUsuarios(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerUsuarios(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuarios para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ObtenerUsuarios para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener los usuarios." };
            }
        }

        public ModelResponse ObtenerUsuarioPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerUsuarioPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuarioPorId para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ObtenerUsuarioPorId para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse ObtenerUsuarioPorNombreUsuario(string nombreUsuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerUsuarioPorNombreUsuario(nombreUsuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuarioPorNombreUsuario para usuario {Usuario}", nombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ObtenerUsuarioPorNombreUsuario para usuario {Usuario}", nombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse ObtenerUsuarioPorCorreo(string correo)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(correo)) { throw new ArgumentException("El correo es requerido."); }

                return _dbWrapper.ObtenerUsuarioPorCorreo(correo);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuarioPorCorreo para correo {Correo}", correo);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ObtenerUsuarioPorCorreo para correo {Correo}", correo);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener el usuario." };
            }
        }

        public ModelResponse GuardarOActualizarUsuario(Usuario usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario.NombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (usuario.NombreUsuario.Length > 25) { throw new ArgumentException("El nombre de usuario no puede exceder los 25 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Contrasena)) { throw new ArgumentException("La contraseña es requerida."); }
                if (usuario.Contrasena.Length > 250) { throw new ArgumentException("La contraseña no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Correo)) { throw new ArgumentException("El correo es requerido."); }
                if (usuario.Correo.Length > 250) { throw new ArgumentException("El correo no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Nombre)) { throw new ArgumentException("El nombre es requerido."); }
                if (usuario.Nombre.Length > 150) { throw new ArgumentException("El nombre no puede exceder los 150 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Apellido)) { throw new ArgumentException("El apellido es requerido."); }
                if (usuario.Apellido.Length > 250) { throw new ArgumentException("El apellido no puede exceder los 250 caracteres."); }
                if (usuario.Sucursal == null || usuario.Sucursal.Id <= 0) { throw new ArgumentException("La sucursal es requerida."); }
                if (usuario.Area == null || usuario.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (usuario.Empresa == null || usuario.Empresa.Id <= 0) { throw new ArgumentException("La empresa es requerida."); }
                if (string.IsNullOrWhiteSpace(usuario.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                return _dbWrapper.GuardarOActualizarUsuario(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarUsuario para usuario {Usuario}", usuario.NombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.GuardarOActualizarUsuario para usuario {Usuario}", usuario.NombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el usuario." };
            }
        }

        public ModelResponse GuardarOActualizarUsuarioAdmin(Usuario usuario, string usuarioAdmin)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario.NombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (usuario.NombreUsuario.Length > 25) { throw new ArgumentException("El nombre de usuario no puede exceder los 25 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Contrasena)) { throw new ArgumentException("La contraseña es requerida."); }
                if (usuario.Contrasena.Length > 250) { throw new ArgumentException("La contraseña no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Correo)) { throw new ArgumentException("El correo es requerido."); }
                if (usuario.Correo.Length > 250) { throw new ArgumentException("El correo no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Nombre)) { throw new ArgumentException("El nombre es requerido."); }
                if (usuario.Nombre.Length > 150) { throw new ArgumentException("El nombre no puede exceder los 150 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Apellido)) { throw new ArgumentException("El apellido es requerido."); }
                if (usuario.Apellido.Length > 250) { throw new ArgumentException("El apellido no puede exceder los 250 caracteres."); }
                if (usuario.Sucursal == null || usuario.Sucursal.Id <= 0) { throw new ArgumentException("La sucursal es requerida."); }
                if (usuario.Area == null || usuario.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (usuario.Empresa == null || usuario.Empresa.Id <= 0) { throw new ArgumentException("La empresa es requerida."); }
                if (string.IsNullOrWhiteSpace(usuario.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAdmin)) { throw new ArgumentException("El usuario administrador es requerido."); }

                return _dbWrapper.GuardarOActualizarUsuarioAdmin(usuario, usuarioAdmin);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarUsuarioAdmin para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.GuardarOActualizarUsuarioAdmin para usuario {UsuarioAdmin}", usuarioAdmin);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el usuario." };
            }
        }

        public ModelResponse ActualizarPerfilUsuario(Usuario usuario, string usuarioAutenticado)
        {
            try
            {
                if (usuario.Id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario.NombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (usuario.NombreUsuario.Length > 25) { throw new ArgumentException("El nombre de usuario no puede exceder los 25 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Correo)) { throw new ArgumentException("El correo es requerido."); }
                if (usuario.Correo.Length > 250) { throw new ArgumentException("El correo no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Nombre)) { throw new ArgumentException("El nombre es requerido."); }
                if (usuario.Nombre.Length > 150) { throw new ArgumentException("El nombre no puede exceder los 150 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.Apellido)) { throw new ArgumentException("El apellido es requerido."); }
                if (usuario.Apellido.Length > 250) { throw new ArgumentException("El apellido no puede exceder los 250 caracteres."); }
                if (usuario.Sucursal == null || usuario.Sucursal.Id <= 0) { throw new ArgumentException("La sucursal es requerida."); }
                if (usuario.Area == null || usuario.Area.Id <= 0) { throw new ArgumentException("El área es requerida."); }
                if (usuario.Empresa == null || usuario.Empresa.Id <= 0) { throw new ArgumentException("La empresa es requerida."); }
                if (string.IsNullOrWhiteSpace(usuario.ModificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                return _dbWrapper.ActualizarPerfilUsuario(usuario, usuarioAutenticado);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ActualizarPerfilUsuario para usuario {UsuarioAutenticado}", usuarioAutenticado);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ActualizarPerfilUsuario para usuario {UsuarioAutenticado}", usuarioAutenticado);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al actualizar el perfil." };
            }
        }

        public ModelResponse EliminarUsuario(long id, string modificadoPor, DateTime fechaModificacion)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                return _dbWrapper.EliminarUsuario(id, modificadoPor, fechaModificacion);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarUsuario para usuario {Id}", id);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.EliminarUsuario para usuario {Id}", id);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el usuario." };
            }
        }

        public ModelResponse AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombreUsuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(contrasena)) { throw new ArgumentException("La contraseña es requerida."); }

                return _dbWrapper.AutenticarUsuario(nombreUsuario, contrasena);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en AutenticarUsuario para usuario {NombreUsuario}", nombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.AutenticarUsuario para usuario {NombreUsuario}", nombreUsuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al autenticar el usuario." };
            }
        }

        public ModelResponse InsertarTokenRecuperacion(long usuarioId, string token, DateTime fechaExpiracion, string creadoPor)
        {
            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentException("El token es requerido."); }
                if (fechaExpiracion <= DateTime.Now) { throw new ArgumentException("La fecha de expiración debe ser mayor a la fecha actual."); }
                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                return _dbWrapper.InsertarTokenRecuperacion(usuarioId, token, fechaExpiracion, creadoPor);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en InsertarTokenRecuperacion para usuario {UsuarioId}", usuarioId);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.InsertarTokenRecuperacion para usuario {UsuarioId}", usuarioId);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el token." };
            }
        }

        public ModelResponse ObtenerTokenRecuperacion(string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentException("El token es requerido."); }

                return _dbWrapper.ObtenerTokenRecuperacion(token);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTokenRecuperacion para token {Token}", token);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ObtenerTokenRecuperacion para token {Token}", token);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener el token." };
            }
        }

        public ModelResponse ActualizarTokenUsado(long id, string modificadoPor)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del token es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }

                return _dbWrapper.ActualizarTokenUsado(id, modificadoPor);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ActualizarTokenUsado para token {Id}", id);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ActualizarTokenUsado para token {Id}", id);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al actualizar el token." };
            }
        }

        public ModelResponse ActualizarContrasena(Usuario usuario, string usuarioAutenticado)
        {
            try
            {
                if (usuario.Id <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario.Contrasena)) { throw new ArgumentException("La contraseña es requerida."); }
                if (usuario.Contrasena.Length < 6) { throw new ArgumentException("La contraseña debe tener al menos 6 caracteres."); }
                if (usuario.Contrasena.Length > 250) { throw new ArgumentException("La contraseña no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(usuario.ModificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuarioAutenticado)) { throw new ArgumentException("El usuario autenticado es requerido."); }

                return _dbWrapper.ActualizarContrasena(usuario, usuarioAutenticado);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ActualizarContrasena para usuario {Id}", usuario.Id);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ActualizarContrasena para usuario {Id}", usuario.Id);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al actualizar la contraseña." };
            }
        }

        public ModelResponse ValidarRecetearContrasenia(string correo)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(correo)) { throw new ArgumentException("El correo es requerido."); }

                var userResponse = _dbWrapper.ObtenerUsuarioPorCorreo(correo);

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
                var tokenResponse = _dbWrapper.InsertarTokenRecuperacion(usuario.Id, token, fechaExpiracion, "system");

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
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ValidarRecetearContrasenia para correo {Correo}", correo);
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.ValidarRecetearContrasenia para correo {Correo}", correo);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al procesar la solicitud";
            }

            return modelResponse;
        }

        public ModelResponse RestablecerContrasenia(string token, string nuevaContrasena)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(token)) { throw new ArgumentException("El token es requerido."); }
                if (string.IsNullOrWhiteSpace(nuevaContrasena)) { throw new ArgumentException("La nueva contraseña es requerida."); }
                if (nuevaContrasena.Length < 6) { throw new ArgumentException("La contraseña debe tener al menos 6 caracteres."); }

                // Validar token
                var tokenResponse = _dbWrapper.ObtenerTokenRecuperacion(token);

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
                    Contrasena = nuevaContrasena,
                    ModificadoPor = tokenInfo.NombreUsuario,
                    FechaModificacion = DateTime.Now
                };

                var updateResponse = _dbWrapper.ActualizarContrasena(usuario, tokenInfo.NombreUsuario);

                if (!updateResponse.IsSuccess)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = updateResponse.Message;
                    return modelResponse;
                }

                // Marcar token como usado
                _dbWrapper.ActualizarTokenUsado(tokenInfo.Id, tokenInfo.NombreUsuario);

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Contraseña actualizada correctamente";
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en RestablecerContrasenia para token {Token}", token);
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AutenticacionService.RestablecerContrasenia para token {Token}", token);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al procesar la solicitud";
            }

            return modelResponse;
        }

        private bool EnviarCorreoNuevoUsuario(Usuario usuario, string contrasenaTemporal)
        {
            try
            {
                Log.Debug("Preparando plantilla de correo para nuevo usuario: {Email}", usuario.Correo);

                // Obtener URL base del Web.config
                string baseUri = System.Configuration.ConfigurationManager.AppSettings["BaseUri"];
                string urlLogin = $"{baseUri}Home/Autentication";

                // Leer template
                string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Template/Template_NuevoUsuario.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    Log.Error("No se encontró la plantilla de correo en: {TemplatePath}", templatePath);
                    return false;
                }

                string templateHtml = System.IO.File.ReadAllText(templatePath);

                // Reemplazar variables en el template
                templateHtml = templateHtml.Replace("{{NombreCompleto}}", $"{usuario.Nombre} {usuario.Apellido}");
                templateHtml = templateHtml.Replace("{{Correo}}", usuario.Correo);
                templateHtml = templateHtml.Replace("{{NombreUsuario}}", usuario.NombreUsuario);
                templateHtml = templateHtml.Replace("{{ContrasenaTemporal}}", contrasenaTemporal);
                templateHtml = templateHtml.Replace("{{UrlLogin}}", urlLogin);

                Log.Debug("Plantilla procesada, enviando correo a: {Email}", usuario.Correo);

                // Enviar correo
                var para = new List<string> { usuario.Correo };
                EmailHelper.EnvioEmaiil(para, "Bienvenido a Service Desk DESI - Tus credenciales de acceso", templateHtml, false);

                Log.Information("Correo de nuevo usuario enviado exitosamente a: {Email}", usuario.Correo);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "FALLO al enviar correo de nuevo usuario a: {Email}", usuario.Correo);
                return false;
            }
        }
    }
}