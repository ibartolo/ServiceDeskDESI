using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Hosting;

namespace ServiceDeskDESIWebApi.Services
{
    public class PersonaActivoService
    {
        private const string TipoCorreoAsignacion = "AsignacionActivo";
        private const string AsuntoCorreoAsignacion = "Asignación de activo - Service Desk DESI";
        private const string MensajeErrorCorreo = "No se pudo enviar el correo de confirmación de asignación. La asignación fue revertida. Verifique la configuración de correo (SMTP) e intente nuevamente.";

        private const string TipoCorreoDesvinculacion = "DesvinculacionActivo";
        private const string AsuntoCorreoDesvinculacion = "Desvinculación de activo - Service Desk DESI";

        private readonly DbWrapper _dbWrapper;

        public PersonaActivoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<PersonaActivoDTO>> ObtenerActivosPorPersona(long personaId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerActivosPorPersona para PersonaId {PersonaId} usuario {Usuario}", personaId, usuario);

                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerActivosPorPersona(personaId, usuario);
                Log.Information("PersonaActivoService.ObtenerActivosPorPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerActivosPorPersona para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerActivosPorPersona para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los activos de la persona." };
            }
        }

        public ModelResponse<List<Activo>> ObtenerActivosDisponibles(string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerActivosDisponibles para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerActivosDisponibles(usuario);
                Log.Information("PersonaActivoService.ObtenerActivosDisponibles RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerActivosDisponibles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Activo>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerActivosDisponibles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Activo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los activos disponibles." };
            }
        }

        public ModelResponse AsignarActivoPersona(long personaId, long activoId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.AsignarActivoPersona para PersonaId {PersonaId} ActivoId {ActivoId} usuario {Usuario}", personaId, activoId, usuario);

                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (activoId <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.AsignarActivoPersona(personaId, activoId, usuario);
                Log.Information("PersonaActivoService.AsignarActivoPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);

                if (!result.IsSuccess)
                {
                    return result;
                }

                var newId = Convert.ToInt64(result.Response);

                // 1. Obtener datos para los correos (persona, activo, asignador/admin, usuarios de la empresa)
                var personaResponse = _dbWrapper.ObtenerPersonaPorId(personaId, usuario);
                var activoResponse = _dbWrapper.ObtenerActivoPorId(activoId, usuario);
                var asignadorResponse = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario, usuario);

                if (personaResponse == null || !personaResponse.IsSuccess || personaResponse.Response == null ||
                    activoResponse == null || !activoResponse.IsSuccess || activoResponse.Response == null ||
                    asignadorResponse == null || !asignadorResponse.IsSuccess || asignadorResponse.Response == null)
                {
                    Log.Error("AsignarActivoPersona: no se pudieron obtener los datos (persona/activo/asignador) para el correo. PersonaActivoId {PersonaActivoId}", newId);
                    CompensarAsignacionFallida(newId, usuario, personaResponse?.Response?.Correo, "No se pudieron obtener los datos necesarios para el envío del correo.");
                    return new ModelResponse { IsSuccess = false, Message = MensajeErrorCorreo };
                }

                var persona = personaResponse.Response;
                var activo = activoResponse.Response;
                var asignador = asignadorResponse.Response;

                // 2. Resolver el USUARIO vinculado a la persona (Usuarios.PersonaId = personaId)
                var usuarioVinculado = ResolverUsuarioVinculado(personaId, usuario);
                if (usuarioVinculado == null || string.IsNullOrWhiteSpace(usuarioVinculado.Correo))
                {
                    Log.Error("AsignarActivoPersona: no se encontró el usuario vinculado a la persona. PersonaActivoId {PersonaActivoId}", newId);
                    CompensarAsignacionFallida(newId, usuario, persona.Correo, "No se encontró el usuario vinculado a la persona.");
                    return new ModelResponse { IsSuccess = false, Message = MensajeErrorCorreo };
                }

                // 3. Generar y persistir el token de confirmación
                var token = Guid.NewGuid();
                var tokenResponse = _dbWrapper.GenerarTokenConfirmacion(newId, token);

                if (!tokenResponse.IsSuccess)
                {
                    Log.Error("AsignarActivoPersona: no se pudo persistir el token de confirmación. PersonaActivoId {PersonaActivoId}", newId);
                    CompensarAsignacionFallida(newId, usuario, usuarioVinculado.Correo, "No se pudo persistir el token de confirmación.");
                    return new ModelResponse { IsSuccess = false, Message = MensajeErrorCorreo };
                }

                // 4. Construir la URL de confirmación (apunta a la página MVC pública VerAsignacion)
                string baseUri = ConfigurationManager.AppSettings["BaseUri"];
                string urlConfirmacion = $"{baseUri}Home/VerAsignacion/{token}";

                // 5. Resolver el template con los placeholders (null-safe). Versión usuario (con liga)
                string templateUsuario = ResolverTemplateAsignacion(persona, activo, asignador, urlConfirmacion);
                // Versión admin: informativo, SIN liga
                string templateAdmin = ResolverTemplateAsignacion(persona, activo, asignador, string.Empty);

                // 6. Enviar 2 correos: (a) admin informativo sin liga; (b) usuario vinculado con liga
                bool correoAdminOk = false;
                bool correoUsuarioOk = false;
                string errorCorreo = null;

                try
                {
                    EmailHelper.EnvioEmaiil(new List<string> { asignador.Correo }, AsuntoCorreoAsignacion, templateAdmin, false);
                    correoAdminOk = true;
                }
                catch (Exception ex)
                {
                    errorCorreo = ex.Message;
                    Log.Error(ex, "AsignarActivoPersona: falló el envío del correo informativo al admin {Correo}. PersonaActivoId {PersonaActivoId}", asignador.Correo, newId);
                }

                try
                {
                    EmailHelper.EnvioEmaiil(new List<string> { usuarioVinculado.Correo }, AsuntoCorreoAsignacion, templateUsuario, false);
                    correoUsuarioOk = true;
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(errorCorreo)) errorCorreo = ex.Message;
                    Log.Error(ex, "AsignarActivoPersona: falló el envío del correo de confirmación al usuario {Correo}. PersonaActivoId {PersonaActivoId}", usuarioVinculado.Correo, newId);
                }

                // Si falla CUALQUIERA de los 2 envíos → compensar (desvincular + bitácora Fallido)
                if (!correoAdminOk || !correoUsuarioOk)
                {
                    CompensarAsignacionFallida(newId, usuario, usuarioVinculado.Correo, errorCorreo ?? "Fallo en el envío de correo.");
                    return new ModelResponse { IsSuccess = false, Message = MensajeErrorCorreo };
                }

                // 7. Registrar 2 filas de bitácora (una por correo enviado)
                try
                {
                    _dbWrapper.RegistrarBitacoraCorreo(TipoCorreoAsignacion, asignador.Correo, AsuntoCorreoAsignacion, "Enviado", null, newId);
                    _dbWrapper.RegistrarBitacoraCorreo(TipoCorreoAsignacion, usuarioVinculado.Correo, AsuntoCorreoAsignacion, "Enviado", null, newId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "AsignarActivoPersona: no se pudo registrar la bitácora de envío. PersonaActivoId {PersonaActivoId}", newId);
                }

                Log.Information("AsignarActivoPersona: correos enviados (admin {CorreoAdmin} / usuario {CorreoUsuario}) para PersonaActivoId {PersonaActivoId}", asignador.Correo, usuarioVinculado.Correo, newId);

                return new ModelResponse
                {
                    IsSuccess = true,
                    Response = newId,
                    Message = "Activo asignado correctamente."
                };
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en AsignarActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.AsignarActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al asignar el activo." };
            }
        }

        public ModelResponse DesvincularActivoPersona(long personaActivoId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.DesvincularActivoPersona para PersonaActivoId {PersonaActivoId} usuario {Usuario}", personaActivoId, usuario);

                if (personaActivoId <= 0) { throw new ArgumentException("El ID de la asignación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.DesvincularActivoPersona(personaActivoId, usuario);
                Log.Information("PersonaActivoService.DesvincularActivoPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en DesvincularActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.DesvincularActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al desvincular el activo." };
            }
        }

        /// <summary>
        /// "Mis Activos": deriva el PersonaId desde Usuarios.PersonaId del usuario autenticado.
        /// Sin vínculo → lista vacía (IsSuccess=true, sin error de permiso).
        /// </summary>
        public ModelResponse<List<PersonaActivoDTO>> ObtenerMisActivos(string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerMisActivos para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var personaIdResponse = _dbWrapper.ObtenerPersonaIdPorUsuario(usuario);
                if (personaIdResponse == null || !personaIdResponse.IsSuccess || personaIdResponse.Response == null)
                {
                    Log.Information("PersonaActivoService.ObtenerMisActivos: el usuario {Usuario} no tiene persona vinculada. Retorna lista vacía.", usuario);
                    return new ModelResponse<List<PersonaActivoDTO>>
                    {
                        IsSuccess = true,
                        Response = new List<PersonaActivoDTO>(),
                        Message = "El usuario no tiene persona vinculada."
                    };
                }

                var result = _dbWrapper.ObtenerActivosPorPersona(personaIdResponse.Response.Value, usuario);
                Log.Information("PersonaActivoService.ObtenerMisActivos RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerMisActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerMisActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener sus activos." };
            }
        }

        public ModelResponse ConfirmarRecepcion(Guid token, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ConfirmarRecepcion para usuario {Usuario} (token omitido por seguridad)", usuario);

                if (token == Guid.Empty) { throw new ArgumentException("El token de confirmación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ConfirmarRecepcionActivo(token, usuario);
                Log.Information("PersonaActivoService.ConfirmarRecepcion RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);

                var estado = result != null && result.Response != null ? Convert.ToInt64(result.Response) : 0L;

                switch (estado)
                {
                    case 1:
                        return new ModelResponse { IsSuccess = true, Response = estado, Message = "Recepción confirmada correctamente." };
                    case 2:
                        return new ModelResponse { IsSuccess = true, Response = estado, Message = "La recepción de este activo ya fue confirmada anteriormente." };
                    case 3:
                        return new ModelResponse { IsSuccess = false, Response = estado, Message = "La asignación no corresponde a su usuario." };
                    default:
                        return new ModelResponse { IsSuccess = false, Response = estado, Message = "El enlace de confirmación no es válido o ha sido alterado." };
                }
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ConfirmarRecepcion");
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ConfirmarRecepcion");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al confirmar la recepción del activo." };
            }
        }

        public ModelResponse DesvincularConfirmacion(Guid token, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.DesvincularConfirmacion para usuario {Usuario} (token omitido por seguridad)", usuario);

                if (token == Guid.Empty) { throw new ArgumentException("El token de desvinculación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.DesvincularActivoPersonaConfirmacion(token, usuario);
                Log.Information("PersonaActivoService.DesvincularConfirmacion RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en DesvincularConfirmacion");
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.DesvincularConfirmacion");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al desvincular el activo." };
            }
        }

        /// <summary>
        /// Inicia la desvinculación (lado admin): NO setea FechaFin; envía correo al usuario
        /// con la liga VerAsignacion?accion=desvincular. Si falla el correo → IsSuccess=false.
        /// </summary>
        public ModelResponse IniciarDesvinculacion(long personaActivoId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.IniciarDesvinculacion para PersonaActivoId {PersonaActivoId} usuario {Usuario}", personaActivoId, usuario);

                if (personaActivoId <= 0) { throw new ArgumentException("El ID de la asignación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var asignacionResponse = _dbWrapper.ObtenerPersonaActivoPorId(personaActivoId);
                if (asignacionResponse == null || !asignacionResponse.IsSuccess || asignacionResponse.Response == null)
                {
                    return new ModelResponse { IsSuccess = false, Message = asignacionResponse?.Message ?? "No se encontró la asignación especificada." };
                }

                var asignacion = asignacionResponse.Response;
                if (asignacion.FechaFin != null)
                {
                    return new ModelResponse { IsSuccess = false, Message = "La asignación ya se encuentra desvinculada." };
                }

                // Token de desvinculación = TokenConfirmacion existente; si NULL → generarlo
                Guid token = asignacion.TokenConfirmacion ?? Guid.Empty;
                if (token == Guid.Empty)
                {
                    token = Guid.NewGuid();
                    var tokenResponse = _dbWrapper.GenerarTokenConfirmacion(personaActivoId, token);
                    if (!tokenResponse.IsSuccess)
                    {
                        return new ModelResponse { IsSuccess = false, Message = "No se pudo generar el token de desvinculación." };
                    }
                }

                // Resolver el usuario vinculado a la persona de la asignación
                var usuarioVinculado = ResolverUsuarioVinculado(asignacion.PersonaId, usuario);
                if (usuarioVinculado == null || string.IsNullOrWhiteSpace(usuarioVinculado.Correo))
                {
                    return new ModelResponse { IsSuccess = false, Message = "La persona no tiene un usuario vinculado con correo." };
                }

                string baseUri = ConfigurationManager.AppSettings["BaseUri"];
                string urlDesvinculacion = $"{baseUri}Home/VerAsignacion/{token}?accion=desvincular";

                string templateHtml = ResolverTemplateDesvinculacion(usuarioVinculado, urlDesvinculacion);

                try
                {
                    EmailHelper.EnvioEmaiil(new List<string> { usuarioVinculado.Correo }, AsuntoCorreoDesvinculacion, templateHtml, false);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "IniciarDesvinculacion: falló el envío del correo de desvinculación. PersonaActivoId {PersonaActivoId}", personaActivoId);
                    return new ModelResponse { IsSuccess = false, Message = "No se pudo enviar el correo de desvinculación." };
                }

                try
                {
                    _dbWrapper.RegistrarBitacoraCorreo(TipoCorreoDesvinculacion, usuarioVinculado.Correo, AsuntoCorreoDesvinculacion, "Enviado", null, personaActivoId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "IniciarDesvinculacion: no se pudo registrar la bitácora de desvinculación. PersonaActivoId {PersonaActivoId}", personaActivoId);
                }

                return new ModelResponse
                {
                    IsSuccess = true,
                    Response = personaActivoId,
                    Message = "Se envió el correo de desvinculación al usuario."
                };
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en IniciarDesvinculacion");
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.IniciarDesvinculacion");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al iniciar la desvinculación." };
            }
        }

        /// <summary>
        /// Obtiene la asignación por token (anónimo, para render de la página VerAsignacion).
        /// </summary>
        public ModelResponse<AsignacionActivoDetalleDTO> ObtenerAsignacionPorToken(Guid token)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerAsignacionPorToken (token omitido por seguridad)");

                if (token == Guid.Empty) { throw new ArgumentException("El token es requerido."); }

                var result = _dbWrapper.ObtenerAsignacionPorToken(token);
                Log.Information("PersonaActivoService.ObtenerAsignacionPorToken RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerAsignacionPorToken");
                return new ModelResponse<AsignacionActivoDetalleDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerAsignacionPorToken");
                return new ModelResponse<AsignacionActivoDetalleDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener la asignación." };
            }
        }

        #region Helpers

        private UsuarioDTO ResolverUsuarioVinculado(long personaId, string usuario)
        {
            var usuariosResponse = _dbWrapper.ObtenerUsuarios(usuario);
            if (usuariosResponse != null && usuariosResponse.IsSuccess && usuariosResponse.Response != null)
            {
                return usuariosResponse.Response.FirstOrDefault(u => u.PersonaId == personaId);
            }
            return null;
        }

        private string ResolverTemplateAsignacion(PersonaDTO persona, ActivoDTO activo, UsuarioDTO asignador, string urlConfirmacion)
        {
            string templatePath = HostingEnvironment.MapPath("~/Template/Template_AsignacionActivo.html");
            string templateHtml = File.ReadAllText(templatePath);

            // Correo informativo (admin) sin liga: quitar el botón "Confirmar Recepción".
            if (string.IsNullOrEmpty(urlConfirmacion))
            {
                templateHtml = Regex.Replace(templateHtml, @"<a\s+href=""\{\{UrlConfirmacion\}\}""[\s\S]*?</a>", string.Empty);
            }

            templateHtml = templateHtml.Replace("{{NombreUsuario}}", $"{persona.Nombre ?? string.Empty} {persona.Apellido ?? string.Empty}");
            templateHtml = templateHtml.Replace("{{AsignadoPor}}", $"{asignador.Nombre ?? string.Empty} {asignador.Apellido ?? string.Empty}");
            templateHtml = templateHtml.Replace("{{NombreActivo}}", activo.Nombre ?? string.Empty);
            templateHtml = templateHtml.Replace("{{Serial}}", activo.Serial ?? string.Empty);
            templateHtml = templateHtml.Replace("{{TipoActivo}}", activo.TipoActivoNombre ?? string.Empty);
            templateHtml = templateHtml.Replace("{{Marca}}", activo.MarcaNombre ?? string.Empty);
            templateHtml = templateHtml.Replace("{{Modelo}}", activo.ModeloNombre ?? string.Empty);
            templateHtml = templateHtml.Replace("{{FechaAsignacion}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            templateHtml = templateHtml.Replace("{{PuestoUsuario}}", persona.PuestoNombre ?? string.Empty);
            templateHtml = templateHtml.Replace("{{CorreoUsuario}}", persona.Correo ?? string.Empty);
            templateHtml = templateHtml.Replace("{{UrlConfirmacion}}", urlConfirmacion ?? string.Empty);

            return templateHtml;
        }

        private string ResolverTemplateDesvinculacion(UsuarioDTO usuario, string urlDesvinculacion)
        {
            string templatePath = HostingEnvironment.MapPath("~/Template/Template_DesvinculacionActivo.html");
            string templateHtml = File.ReadAllText(templatePath);

            templateHtml = templateHtml.Replace("{{NombreUsuario}}", $"{usuario.Nombre ?? string.Empty} {usuario.Apellido ?? string.Empty}");
            templateHtml = templateHtml.Replace("{{UrlDesvinculacion}}", urlDesvinculacion ?? string.Empty);

            return templateHtml;
        }

        /// <summary>
        /// Compensa una asignación cuyo correo falló: desvincula (conserva histórico) y
        /// registra bitácora "Fallido". Nunca propaga excepción; el invariante es devolver IsSuccess=false.
        /// </summary>
        private void CompensarAsignacionFallida(long personaActivoId, string usuario, string destinatario, string error)
        {
            try
            {
                _dbWrapper.DesvincularActivoPersona(personaActivoId, usuario);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompensarAsignacionFallida: error al desvincular la asignación {PersonaActivoId}", personaActivoId);
            }

            try
            {
                _dbWrapper.RegistrarBitacoraCorreo(TipoCorreoAsignacion, destinatario, AsuntoCorreoAsignacion, "Fallido", error, personaActivoId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "CompensarAsignacionFallida: error al registrar bitácora Fallido para la asignación {PersonaActivoId}", personaActivoId);
            }
        }

        #endregion
    }
}
