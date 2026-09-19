using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace ServiceDeskDESIWebApi.Services
{
    public class TicketService
    {
        private readonly DbWrapper _dbWrapper;

        public TicketService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TicketDTO>> ObtenerTickets(string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTickets para usuario {Usuario}", usuario);
                Log.Information("TicketService.ObtenerTickets ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTickets(usuario);
                Log.Information("TicketService.ObtenerTickets RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTickets SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTickets para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTickets para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets."
                };
            }
        }

        public ModelResponse<TicketDTO> ObtenerTicketPorId(long id, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketPorId para Id {Id} usuario {Usuario}", id, usuario);
                Log.Information("TicketService.ObtenerTicketPorId ENTRADA: {Json}", LogSanitizer.ToJson(new { id, usuario }));

                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketPorId(id, usuario);
                Log.Information("TicketService.ObtenerTicketPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketPorId SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TicketDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TicketDTO>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener el ticket."
                };
            }
        }

        public ModelResponse<Ticket> GuardarOActualizarTicket(Ticket ticket, string usuario)
        {
            try
            {
                Log.Information("TicketService.GuardarOActualizarTicket para usuario {Usuario}", usuario);
                Log.Information("TicketService.GuardarOActualizarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticket, usuario }));

                if (ticket.AreaId <= 0) { throw new ArgumentException("El área es requerida."); }
                if (ticket.CategoriaId <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (ticket.Urgencia <= 0 || ticket.Urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(ticket.Titulo)) { throw new ArgumentException("El título es requerido."); }
                if (ticket.Titulo.Length > 250) { throw new ArgumentException("El título no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(ticket.Descripcion)) { throw new ArgumentException("La descripción es requerida."); }
                if (ticket.TicketEstatusId <= 0) { throw new ArgumentException("El estatus del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(ticket.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                // Valor autoritativo del servidor: el ticket guardado siempre queda activo.
                ticket.Estatus = true;

                ModelResponse<Ticket> result;

                if (ticket.Id <= 0)
                {
                    // CREACIÓN: generar el folio dentro de la transacción (atómicamente con el insert).
                    // Se comparte la MISMA instancia de DbWrapper (conexión/transacción ambiental).
                    _dbWrapper.BeginTransaction();
                    try
                    {
                        var foliadorService = new FoliadorService(_dbWrapper);
                        var consecutivo = foliadorService.ActualizarConsecutivo("Ticket", usuario);
                        ticket.Folio = FoliadorService.FormatearFolio(consecutivo);
                        Log.Information("TicketService.GuardarOActualizarTicket: folio generado {Folio} para usuario {Usuario}", ticket.Folio, usuario);

                        result = _dbWrapper.GuardarOActualizarTicket(ticket, usuario);
                        if (!result.IsSuccess || result.Response == null)
                        {
                            _dbWrapper.RollbackTransaction();
                            return result;
                        }

                        _dbWrapper.CommitTransaction();
                    }
                    catch
                    {
                        _dbWrapper.RollbackTransaction();
                        throw;
                    }
                }
                else
                {
                    // ACTUALIZACIÓN: se conserva el folio existente (no se regenera).
                    result = _dbWrapper.GuardarOActualizarTicket(ticket, usuario);
                }

                Log.Information("TicketService.GuardarOActualizarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.GuardarOActualizarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarTicket para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.GuardarOActualizarTicket para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el ticket."
                };
            }
        }

        /// <summary>
        /// Guarda el ticket y sus evidencias (anexos) en UNA sola transacción.
        /// Si falla el ticket o cualquier evidencia, se revierte todo (BD) y se
        /// eliminan los archivos ya escritos: "todo o nada".
        /// </summary>
        public ModelResponse<Ticket> GuardarTicketConEvidencias(Ticket ticket, HttpFileCollection files, string usuario, long empresaId)
        {
            try
            {
                Log.Information("TicketService.GuardarTicketConEvidencias para usuario {Usuario}, EmpresaId {EmpresaId}", usuario, empresaId);
                Log.Information("TicketService.GuardarTicketConEvidencias ENTRADA: {Json}", LogSanitizer.ToJson(new { ticket, usuario, empresaId, archivos = files?.Count }));

                // Validaciones de ticket (espejo de GuardarOActualizarTicket).
                if (ticket == null) { throw new ArgumentException("El ticket es requerido."); }
                if (ticket.AreaId <= 0) { throw new ArgumentException("El área es requerida."); }
                if (ticket.CategoriaId <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (ticket.Urgencia <= 0 || ticket.Urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(ticket.Titulo)) { throw new ArgumentException("El título es requerido."); }
                if (ticket.Titulo.Length > 250) { throw new ArgumentException("El título no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(ticket.Descripcion)) { throw new ArgumentException("La descripción es requerida."); }
                if (ticket.TicketEstatusId <= 0) { throw new ArgumentException("El estatus del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(ticket.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                // Valor autoritativo del servidor.
                ticket.Estatus = true;

                // Archivos recibidos.
                var archivos = new List<HttpPostedFile>();
                if (files != null)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var f = files[i];
                        if (f != null && f.ContentLength > 0) archivos.Add(f);
                    }
                }

                // Validación de evidencias (solo si hay archivos).
                if (archivos.Count > 0)
                {
                    var config = ObtenerConfiguracionEvidencias();

                    if (archivos.Count > config.MaxArchivos)
                        return new ModelResponse<Ticket> { IsSuccess = false, Message = $"No puede adjuntar más de {config.MaxArchivos} archivos a este ticket." };

                    long maxTamanoBytes = (long)config.MaxTamanoMB * 1024 * 1024;
                    foreach (var f in archivos)
                    {
                        var ext = Path.GetExtension(f.FileName).TrimStart('.').ToLowerInvariant();
                        if (!config.ExtensionesPermitidas.Contains(ext))
                            return new ModelResponse<Ticket> { IsSuccess = false, Message = "Extensión no permitida. Solo se aceptan: " + string.Join(", ", config.ExtensionesPermitidas) + "." };
                        if (f.ContentLength > maxTamanoBytes)
                            return new ModelResponse<Ticket> { IsSuccess = false, Message = $"El archivo '{f.FileName}' supera el tamaño máximo de {config.MaxTamanoMB} MB." };
                    }
                }

                // FASE transaccional: ticket + evidencias juntos.
                var evidenciasEscritas = new List<TicketEvidencia>();

                _dbWrapper.BeginTransaction();
                try
                {
                    // Generar el folio dentro de la transacción (atómicamente con el insert).
                    // Se comparte la MISMA instancia de DbWrapper (conexión/transacción ambiental).
                    var foliadorService = new FoliadorService(_dbWrapper);
                    var consecutivo = foliadorService.ActualizarConsecutivo("Ticket", usuario);
                    ticket.Folio = FoliadorService.FormatearFolio(consecutivo);
                    Log.Information("TicketService.GuardarTicketConEvidencias: folio generado {Folio} para usuario {Usuario}", ticket.Folio, usuario);

                    var ticketResp = _dbWrapper.GuardarOActualizarTicket(ticket, usuario);
                    if (!ticketResp.IsSuccess || ticketResp.Response == null)
                    {
                        _dbWrapper.RollbackTransaction();
                        return new ModelResponse<Ticket> { IsSuccess = false, Message = ticketResp != null ? ticketResp.Message : "No se pudo guardar el ticket." };
                    }

                    ticket = ticketResp.Response;
                    long ticketId = ticket.Id;

                    if (archivos.Count > 0)
                    {
                        if (empresaId <= 0)
                            throw new InvalidOperationException("No se pudo determinar la empresa del usuario.");

                        foreach (var f in archivos)
                        {
                            var extension = Path.GetExtension(f.FileName).ToLowerInvariant();
                            var nombreFisico = Guid.NewGuid().ToString("N") + extension;
                            var rutaRelativa = $"Evidencias/{empresaId}/{ticketId}/{nombreFisico}";

                            var rutaAbsoluta = HostingEnvironment.MapPath("~/" + rutaRelativa);
                            if (string.IsNullOrEmpty(rutaAbsoluta))
                                throw new InvalidOperationException("No se pudo resolver la ruta de almacenamiento de evidencias.");

                            var directorio = Path.GetDirectoryName(rutaAbsoluta);
                            if (!Directory.Exists(directorio))
                                Directory.CreateDirectory(directorio);

                            f.SaveAs(rutaAbsoluta);

                            evidenciasEscritas.Add(new TicketEvidencia
                            {
                                Id = 0,
                                TicketId = ticketId,
                                EmpresaId = empresaId,
                                NombreArchivo = f.FileName,
                                RutaArchivo = rutaRelativa,
                                FechaSubida = DateTime.Now
                            });

                            var nuevoId = _dbWrapper.GuardarEvidencia(ticketId, f.FileName, rutaRelativa, usuario);
                            if (nuevoId == 0)
                                throw new InvalidOperationException("No se pudo registrar la evidencia en la base de datos.");
                        }
                    }

                    _dbWrapper.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _dbWrapper.RollbackTransaction();
                    LimpiarArchivosEnDisco(evidenciasEscritas);
                    Log.Error(ex, "Error en TicketService.GuardarTicketConEvidencias para usuario {Usuario}", usuario);
                    return new ModelResponse<Ticket>
                    {
                        IsSuccess = false,
                        Message = "No se pudo guardar el ticket ni sus evidencias. No se guardó ningún dato."
                    };
                }

                var result = new ModelResponse<Ticket>
                {
                    IsSuccess = true,
                    Response = ticket,
                    Message = "Ticket guardado correctamente."
                };
                Log.Information("TicketService.GuardarTicketConEvidencias RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.GuardarTicketConEvidencias SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarTicketConEvidencias para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.GuardarTicketConEvidencias para usuario {Usuario}", usuario);
                return new ModelResponse<Ticket> { IsSuccess = false, Message = "Ocurrió un error al guardar el ticket." };
            }
        }

        public ModelResponse EliminarTicket(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("TicketService.EliminarTicket para Id {Id} usuario {Usuario}", id, usuario);
                Log.Information("TicketService.EliminarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { id, modificadoPor, fechaModificacion, usuario }));

                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarTicket(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("TicketService.EliminarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.EliminarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.EliminarTicket para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar el ticket."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorArea(long areaId, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketsPorArea para AreaId {AreaId} usuario {Usuario}", areaId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorArea ENTRADA: {Json}", LogSanitizer.ToJson(new { areaId, usuario }));

                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorArea(areaId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketsPorArea SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por área."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUsuario(string creadoPor, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketsPorUsuario para CreadoPor {CreadoPor} usuario {Usuario}", creadoPor, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUsuario ENTRADA: {Json}", LogSanitizer.ToJson(new { creadoPor, usuario }));

                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorUsuario(creadoPor, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketsPorUsuario SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por usuario."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorUrgencia(int urgencia, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketsPorUrgencia para Urgencia {Urgencia} usuario {Usuario}", urgencia, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUrgencia ENTRADA: {Json}", LogSanitizer.ToJson(new { urgencia, usuario }));

                if (urgencia <= 0 || urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorUrgencia(urgencia, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUrgencia RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketsPorUrgencia SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorUrgencia para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorUrgencia para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por urgencia."
                };
            }
        }

        public ModelResponse<List<TicketDTO>> ObtenerTicketsPorEstatus(int ticketEstatusId, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketsPorEstatus para TicketEstatusId {TicketEstatusId} usuario {Usuario}", ticketEstatusId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorEstatus ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketEstatusId, usuario }));

                if (ticketEstatusId <= 0) { throw new ArgumentException("El ID del estatus es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorEstatus(ticketEstatusId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorEstatus RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketsPorEstatus SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketsPorEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketsPorEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los tickets por estatus."
                };
            }
        }

        public ModelResponse<List<TicketEstatus>> ObtenerTicketEstatus()
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketEstatus");
                Log.Information("TicketService.ObtenerTicketEstatus ENTRADA: {Json}", LogSanitizer.ToJson(new { }));

                var result = _dbWrapper.ObtenerTicketEstatus();
                Log.Information("TicketService.ObtenerTicketEstatus RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketEstatus SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketEstatus");
                return new ModelResponse<List<TicketEstatus>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los estatus de tickets."
                };
            }
        }

        public ModelResponse TomarTicket(long ticketId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.TomarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.TomarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.TomarTicket(ticketId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Tomar", comentario);
                }
                Log.Information("TicketService.TomarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.TomarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en TomarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.TomarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al tomar el ticket." };
            }
        }

        public ModelResponse ReasignarTicket(long ticketId, long nuevoUsuarioId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.ReasignarTicket para TicketId {TicketId}, NuevoUsuarioId {NuevoUsuarioId}, usuario {Usuario}", ticketId, nuevoUsuarioId, usuario);
                Log.Information("TicketService.ReasignarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, nuevoUsuarioId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (nuevoUsuarioId <= 0) { throw new ArgumentException("El nuevo agente es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(comentario) || comentario.Length > 300)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El comentario de reasignación es requerido (máx 300 caracteres)."
                    };
                }

                var result = _dbWrapper.ReasignarTicket(ticketId, nuevoUsuarioId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Reasignar", comentario);
                }
                Log.Information("TicketService.ReasignarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ReasignarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ReasignarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ReasignarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al reasignar el ticket." };
            }
        }

        public ModelResponse<List<TicketAsignacionDTO>> ObtenerTicketAsignaciones(long ticketId)
        {
            try
            {
                Log.Information("TicketService.ObtenerTicketAsignaciones para TicketId {TicketId}", ticketId);
                Log.Information("TicketService.ObtenerTicketAsignaciones ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }

                var result = _dbWrapper.ObtenerTicketAsignaciones(ticketId);
                Log.Information("TicketService.ObtenerTicketAsignaciones RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerTicketAsignaciones SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTicketAsignaciones");
                return new ModelResponse<List<TicketAsignacionDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerTicketAsignaciones para ticket {TicketId}", ticketId);
                return new ModelResponse<List<TicketAsignacionDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las asignaciones del ticket."
                };
            }
        }

        public ModelResponse ResolverTicket(long ticketId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.ResolverTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.ResolverTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(comentario) || comentario.Length > 300)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El comentario de resolución es requerido (máx 300 caracteres)."
                    };
                }

                var result = _dbWrapper.ResolverTicket(ticketId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Resolver", comentario);
                }
                Log.Information("TicketService.ResolverTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ResolverTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ResolverTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ResolverTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al resolver el ticket." };
            }
        }

        public ModelResponse RechazarTicket(long ticketId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.RechazarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.RechazarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(comentario) || comentario.Length > 300)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El comentario de rechazo es requerido (máx 300 caracteres)."
                    };
                }

                var result = _dbWrapper.RechazarTicket(ticketId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Rechazar", comentario);
                }
                Log.Information("TicketService.RechazarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.RechazarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en RechazarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.RechazarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al rechazar el ticket." };
            }
        }

        public ModelResponse CerrarTicket(long ticketId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.CerrarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.CerrarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(comentario) || comentario.Length > 300)
                {
                    return new ModelResponse
                    {
                        IsSuccess = false,
                        Message = "El comentario de cierre es requerido (máx 300 caracteres)."
                    };
                }

                var result = _dbWrapper.CerrarTicket(ticketId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Cerrar", comentario);
                }
                Log.Information("TicketService.CerrarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.CerrarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en CerrarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.CerrarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al cerrar el ticket." };
            }
        }

        public ModelResponse RetomarTicket(long ticketId, string usuario)
        {
            try
            {
                Log.Information("TicketService.RetomarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.RetomarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.RetomarTicket(ticketId, usuario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Retomar", null);
                }
                Log.Information("TicketService.RetomarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.RetomarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en RetomarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.RetomarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al retomar el ticket." };
            }
        }

        public ModelResponse PausarTicket(long ticketId, string usuario, string comentario, DateTime? fechaEstimada, string tipoMovimiento)
        {
            try
            {
                Log.Information("TicketService.PausarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.PausarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario, fechaEstimada, tipoMovimiento }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (tipoMovimiento != "PendienteMateriales" && tipoMovimiento != "EnEsperaTerceros")
                {
                    return new ModelResponse { IsSuccess = false, Message = "El motivo de la pausa no es válido." };
                }
                if (string.IsNullOrWhiteSpace(comentario) || comentario.Length > 300)
                {
                    return new ModelResponse { IsSuccess = false, Message = "El comentario de la pausa es requerido (máx 300 caracteres)." };
                }

                var result = _dbWrapper.PausarTicket(ticketId, usuario, comentario, fechaEstimada, tipoMovimiento);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, tipoMovimiento, comentario, fechaEstimada);
                }
                Log.Information("TicketService.PausarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.PausarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en PausarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.PausarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al pausar el ticket." };
            }
        }

        public ModelResponse ReanudarTicket(long ticketId, string usuario, string comentario)
        {
            try
            {
                Log.Information("TicketService.ReanudarTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);
                Log.Information("TicketService.ReanudarTicket ENTRADA: {Json}", LogSanitizer.ToJson(new { ticketId, usuario, comentario }));

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ReanudarTicket(ticketId, usuario, comentario);
                if (result != null && result.IsSuccess)
                {
                    NotificarCambioEstatus(ticketId, usuario, "Reanudar", comentario);
                }
                Log.Information("TicketService.ReanudarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ReanudarTicket SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ReanudarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ReanudarTicket para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al reanudar el ticket." };
            }
        }

        public ModelResponse<List<UsuarioDTO>> ObtenerUsuariosArea(long areaId, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerUsuariosArea para AreaId {AreaId} usuario {Usuario}", areaId, usuario);
                Log.Information("TicketService.ObtenerUsuariosArea ENTRADA: {Json}", LogSanitizer.ToJson(new { areaId, usuario }));

                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerUsuariosArea(areaId, usuario);
                Log.Information("TicketService.ObtenerUsuariosArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                Log.Information("TicketService.ObtenerUsuariosArea SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerUsuariosArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<UsuarioDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TicketService.ObtenerUsuariosArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<UsuarioDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los usuarios del área."
                };
            }
        }

        /// <summary>
        /// Notifica por correo al creador del ticket cada vez que cambia de estatus.
        /// Best-effort: cualquier fallo se registra en el log y NO rompe la transición.
        /// </summary>
        private void NotificarCambioEstatus(long ticketId, string usuario, string tipoMovimiento, string comentario, DateTime? fechaEstimada = null)
        {
            try
            {
                var ticketResp = _dbWrapper.ObtenerTicketPorId(ticketId, usuario);
                if (ticketResp == null || !ticketResp.IsSuccess || ticketResp.Response == null)
                {
                    Log.Warning("No se pudo obtener el ticket {TicketId} para notificar el cambio de estatus.", ticketId);
                    return;
                }

                var ticket = ticketResp.Response;

                string correoCreador = null;
                string nombreCreador = ticket.CreadoPor;
                var creadorResp = _dbWrapper.ObtenerUsuarioPorNombreUsuario(ticket.CreadoPor, usuario);
                if (creadorResp != null && creadorResp.IsSuccess && creadorResp.Response != null)
                {
                    correoCreador = creadorResp.Response.Correo;
                    nombreCreador = $"{creadorResp.Response.Nombre} {creadorResp.Response.Apellido}".Trim();
                }

                if (string.IsNullOrWhiteSpace(correoCreador))
                {
                    Log.Warning("El creador del ticket {TicketId} no tiene correo; no se envía notificación.", ticketId);
                    return;
                }

                var templatePath = HostingEnvironment.MapPath("~/Template/Template_CambioEstatusTicket.html");
                if (string.IsNullOrEmpty(templatePath) || !File.Exists(templatePath))
                {
                    Log.Error("No se encontró la plantilla de notificación de cambio de estatus en {TemplatePath}.", templatePath);
                    return;
                }

                string mensaje, nota;
                ObtenerMensajeYNota(tipoMovimiento, comentario, fechaEstimada, ticket.EstatusNombre, out mensaje, out nota);

                var colorEstatus = string.IsNullOrWhiteSpace(ticket.EstatusColor) ? "#4e73df" : ticket.EstatusColor;
                var baseUri = ConfigurationManager.AppSettings["BaseUri"] ?? string.Empty;

                var html = File.ReadAllText(templatePath)
                    .Replace("{{NombreUsuario}}", string.IsNullOrWhiteSpace(nombreCreador) ? ticket.CreadoPor : nombreCreador)
                    .Replace("{{MensajeEstatus}}", mensaje)
                    .Replace("{{ColorEstatus}}", colorEstatus)
                    .Replace("{{NumeroTicket}}", string.IsNullOrWhiteSpace(ticket.Folio) ? ticket.Id.ToString() : ticket.Folio)
                    .Replace("{{TituloTicket}}", ticket.Titulo ?? string.Empty)
                    .Replace("{{Categoria}}", ticket.CategoriaNombre ?? string.Empty)
                    .Replace("{{ColorPrioridad}}", ObtenerPrioridadColor(ticket.Urgencia))
                    .Replace("{{Prioridad}}", ObtenerPrioridadTexto(ticket.Urgencia))
                    .Replace("{{FechaCreacion}}", ticket.FechaCreacion.ToString("dd/MM/yyyy HH:mm"))
                    .Replace("{{Estatus}}", ticket.EstatusNombre ?? string.Empty)
                    .Replace("{{DescripcionTicket}}", ticket.Descripcion ?? string.Empty)
                    .Replace("{{NotaAdicional}}", nota)
                    .Replace("{{UrlTicket}}", $"{baseUri}Ticket/Index")
                    .Replace("{{UrlTerminos}}", $"{baseUri}Home/Terminos")
                    .Replace("{{UrlPrivacidad}}", $"{baseUri}Home/Privacidad");

                var asunto = $"Ticket {(string.IsNullOrWhiteSpace(ticket.Folio) ? "#" + ticket.Id : ticket.Folio)} - Estatus: {ticket.EstatusNombre}";

                EmailHelper.EnvioEmaiil(new List<string> { correoCreador }, asunto, html, false);
                Log.Information("Notificación de cambio de estatus ({TipoMovimiento}) enviada para ticket {TicketId} a {Correo}.", tipoMovimiento, ticketId, correoCreador);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al notificar el cambio de estatus del ticket {TicketId}.", ticketId);
            }
        }

        /// <summary>
        /// Devuelve el mensaje principal y la nota adicional del correo según el movimiento.
        /// </summary>
        private static void ObtenerMensajeYNota(string tipoMovimiento, string comentario, DateTime? fechaEstimada, string estatusNombre, out string mensaje, out string nota)
        {
            var c = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim();

            switch (tipoMovimiento)
            {
                case "Tomar":
                    mensaje = "Tu ticket fue tomado por un agente y está siendo atendido.";
                    nota = c ?? "El ticket pasó a \"En Progreso\".";
                    break;
                case "Resolver":
                    mensaje = "El agente marcó tu ticket como <strong>Resuelto</strong>. Revisa la solución y, si estás de acuerdo, ciérralo.";
                    nota = c ?? "El ticket fue resuelto.";
                    break;
                case "Rechazar":
                    mensaje = "Tu ticket fue marcado como <strong>Rechazado</strong>. Un agente puede retomarlo.";
                    nota = c ?? "El ticket fue rechazado.";
                    break;
                case "Cerrar":
                    mensaje = "Tu ticket fue <strong>Cerrado</strong>. ¡Gracias por confirmar!";
                    nota = c ?? "El ticket fue cerrado.";
                    break;
                case "Retomar":
                    mensaje = "El agente retomó tu ticket rechazado y vuelve a estar <strong>En Progreso</strong>.";
                    nota = c ?? "El ticket fue retomado.";
                    break;
                case "Reasignar":
                    mensaje = "Tu ticket fue reasignado a otro agente y sigue <strong>En Progreso</strong>.";
                    nota = c ?? "El ticket fue reasignado.";
                    break;
                case "PendienteMateriales":
                    mensaje = "Tu ticket está <strong>Pendiente de Materiales</strong>: el avance depende de una compra, refacción o mantenimiento de un tercero. El ticket sigue abierto.";
                    nota = c ?? "El ticket está en espera de materiales.";
                    if (fechaEstimada.HasValue) { nota += $" Fecha estimada de respuesta: {fechaEstimada.Value:dd/MM/yyyy}."; }
                    break;
                case "EnEsperaTerceros":
                    mensaje = "Tu ticket está <strong>En Espera de Terceros</strong>: el avance depende de un proveedor externo. El ticket sigue abierto.";
                    nota = c ?? "El ticket está en espera de un tercero.";
                    if (fechaEstimada.HasValue) { nota += $" Fecha estimada de respuesta: {fechaEstimada.Value:dd/MM/yyyy}."; }
                    break;
                case "Reanudar":
                    mensaje = "Tu ticket se <strong>reanudó</strong> y vuelve a estar <strong>En Progreso</strong>.";
                    nota = c ?? "El ticket fue reanudado.";
                    break;
                default:
                    mensaje = $"El estatus de tu ticket cambió a <strong>{estatusNombre}</strong>.";
                    nota = c ?? "El ticket cambió de estatus.";
                    break;
            }
        }

        private static string ObtenerPrioridadTexto(int urgencia)
        {
            switch (urgencia)
            {
                case 1: return "Baja";
                case 2: return "Media";
                case 3: return "Alta";
                case 4: return "Crítica";
                default: return "No definida";
            }
        }

        private static string ObtenerPrioridadColor(int urgencia)
        {
            switch (urgencia)
            {
                case 1: return "#1cc88a";
                case 2: return "#36b9cc";
                case 3: return "#f6c23e";
                case 4: return "#e74a3b";
                default: return "#858796";
            }
        }

        private EvidenciaConfigDTO ObtenerConfiguracionEvidencias()
        {
            var config = new EvidenciaConfigDTO();

            int maxArchivos;
            if (!int.TryParse(ConfigurationManager.AppSettings["EvidenciasMaxArchivos"], out maxArchivos)) maxArchivos = 3;
            config.MaxArchivos = maxArchivos;

            int maxTamanoMB;
            if (!int.TryParse(ConfigurationManager.AppSettings["EvidenciasMaxTamanoMB"], out maxTamanoMB)) maxTamanoMB = 3;
            config.MaxTamanoMB = maxTamanoMB;

            var extensiones = ConfigurationManager.AppSettings["EvidenciasExtensionesPermitidas"];
            if (string.IsNullOrWhiteSpace(extensiones)) extensiones = "pdf,jpg,png";
            config.ExtensionesPermitidas = extensiones
                .Split(',')
                .Select(e => e.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            return config;
        }

        private void LimpiarArchivosEnDisco(IEnumerable<TicketEvidencia> evidencias)
        {
            if (evidencias == null) return;

            foreach (var evidencia in evidencias)
            {
                try
                {
                    if (evidencia == null || string.IsNullOrWhiteSpace(evidencia.RutaArchivo)) continue;

                    var rutaAbsoluta = HostingEnvironment.MapPath("~/" + evidencia.RutaArchivo);
                    if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                        File.Delete(rutaAbsoluta);
                }
                catch
                {
                    // No bloquear el flujo por un fallo de limpieza.
                }
            }
        }
    }
}
