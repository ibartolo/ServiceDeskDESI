using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
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

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTickets(usuario);
                Log.Information("TicketService.ObtenerTickets RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketPorId(id, usuario);
                Log.Information("TicketService.ObtenerTicketPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (id <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarTicket(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("TicketService.EliminarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorArea(areaId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (string.IsNullOrWhiteSpace(creadoPor)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorUsuario(creadoPor, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (urgencia <= 0 || urgencia > 4) { throw new ArgumentException("La urgencia debe ser un valor entre 1 y 4."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorUrgencia(urgencia, usuario);
                Log.Information("TicketService.ObtenerTicketsPorUrgencia RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (ticketEstatusId <= 0) { throw new ArgumentException("El ID del estatus es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTicketsPorEstatus(ticketEstatusId, usuario);
                Log.Information("TicketService.ObtenerTicketsPorEstatus RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                var result = _dbWrapper.ObtenerTicketEstatus();
                Log.Information("TicketService.ObtenerTicketEstatus RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.TomarTicket(ticketId, usuario, comentario);
                Log.Information("TicketService.TomarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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
                Log.Information("TicketService.ReasignarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }

                var result = _dbWrapper.ObtenerTicketAsignaciones(ticketId);
                Log.Information("TicketService.ObtenerTicketAsignaciones RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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
                Log.Information("TicketService.ResolverTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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
                Log.Information("TicketService.RechazarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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
                Log.Information("TicketService.CerrarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

                if (ticketId <= 0) { throw new ArgumentException("El ID del ticket es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.RetomarTicket(ticketId, usuario);
                Log.Information("TicketService.RetomarTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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

        public ModelResponse<List<UsuarioDTO>> ObtenerUsuariosArea(long areaId, string usuario)
        {
            try
            {
                Log.Information("TicketService.ObtenerUsuariosArea para AreaId {AreaId} usuario {Usuario}", areaId, usuario);

                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerUsuariosArea(areaId, usuario);
                Log.Information("TicketService.ObtenerUsuariosArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
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
