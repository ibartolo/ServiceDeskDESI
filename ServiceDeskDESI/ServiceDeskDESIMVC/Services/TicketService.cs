using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.Services
{
    public class TicketService
    {
        private readonly HttpClientConnection _httpClient;

        public TicketService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTickets()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTickets ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTickets();
            Log.Information("TicketService.ObtenerTickets SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<TicketDTO>> ObtenerTicketPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketPorId(id);
            Log.Information("TicketService.ObtenerTicketPorId SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Ticket>> GuardarOActualizarTicket(Ticket ticket)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.GuardarOActualizarTicket ENTRADA: TicketId={TicketId}, AreaId={AreaId}, CategoriaId={CategoriaId}, Titulo={Titulo}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticket?.Id, ticket?.AreaId, ticket?.CategoriaId, ticket?.Titulo, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var validacion = ValidarTicket(ticket);
            if (validacion != null)
            {
                Log.Information("TicketService.GuardarOActualizarTicket SALIDA: validacion fallida, Message={Message}",
                    validacion.Message);
                return validacion;
            }

            var resultado = await _httpClient.GuardarOActualizarTicket(ticket);
            Log.Information("TicketService.GuardarOActualizarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Ticket>> GuardarTicketConEvidencias(Ticket ticket, HttpFileCollectionBase files)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.GuardarTicketConEvidencias ENTRADA: TicketId={TicketId}, AreaId={AreaId}, CategoriaId={CategoriaId}, Archivos={Archivos}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticket?.Id, ticket?.AreaId, ticket?.CategoriaId, files?.Count, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var validacion = ValidarTicket(ticket);
            if (validacion != null)
            {
                Log.Information("TicketService.GuardarTicketConEvidencias SALIDA: validacion fallida, Message={Message}",
                    validacion.Message);
                return validacion;
            }

            // Columnas de auditoría (CreadoPor/FechaCreacion o ModificadoPor/FechaModificacion).
            _httpClient.MappingColumSecurity(ticket);

            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(ticket.Id.ToString()), "Id");
                form.Add(new StringContent(ticket.AreaId.ToString()), "AreaId");
                form.Add(new StringContent(ticket.CategoriaId.ToString()), "CategoriaId");
                form.Add(new StringContent(ticket.SubcategoriaId.HasValue ? ticket.SubcategoriaId.Value.ToString() : string.Empty), "SubcategoriaId");
                form.Add(new StringContent(ticket.Urgencia.ToString()), "Urgencia");
                form.Add(new StringContent(ticket.Titulo ?? string.Empty), "Titulo");
                form.Add(new StringContent(ticket.Descripcion ?? string.Empty), "Descripcion");
                form.Add(new StringContent(ticket.TicketEstatusId.ToString()), "TicketEstatusId");
                form.Add(new StringContent(ticket.CreadoPor ?? string.Empty), "CreadoPor");
                form.Add(new StringContent(ticket.FechaCreacion.ToString("o")), "FechaCreacion");
                form.Add(new StringContent(ticket.ModificadoPor ?? string.Empty), "ModificadoPor");
                form.Add(new StringContent(ticket.FechaModificacion.HasValue ? ticket.FechaModificacion.Value.ToString("o") : string.Empty), "FechaModificacion");
                form.Add(new StringContent(ticket.Estatus.ToString()), "Estatus");
                form.Add(new StringContent(ticket.Folio ?? string.Empty), "Folio");

                if (files != null)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        if (file == null) continue;

                        byte[] bytes;
                        using (var ms = new MemoryStream())
                        {
                            if (file.InputStream.CanSeek) file.InputStream.Position = 0;
                            file.InputStream.CopyTo(ms);
                            bytes = ms.ToArray();
                        }

                        var content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue(
                            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

                        form.Add(content, "archivos", file.FileName);
                    }
                }

                var resultado = await _httpClient.PostMultipartAsync<Ticket>("api/Ticket/GuardarConEvidencias", form);
                Log.Information("TicketService.GuardarTicketConEvidencias SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                    resultado?.IsSuccess, resultado?.Message);
                return resultado;
            }
        }

        /// <summary>
        /// Validaciones de ticket en la capa MVC (espejo de la capa de servicio de la WebApi),
        /// para rechazar datos inválidos antes de enviarlos a la API.
        /// </summary>
        private ModelResponse<Ticket> ValidarTicket(Ticket ticket)
        {
            if (ticket == null) return ErrorTicket("El ticket es requerido.");
            if (ticket.AreaId <= 0) return ErrorTicket("El área es requerida.");
            if (ticket.CategoriaId <= 0) return ErrorTicket("La categoría es requerida.");
            if (ticket.Urgencia <= 0 || ticket.Urgencia > 4) return ErrorTicket("La urgencia debe ser un valor entre 1 y 4.");
            if (string.IsNullOrWhiteSpace(ticket.Titulo)) return ErrorTicket("El título es requerido.");
            if (ticket.Titulo.Length > 250) return ErrorTicket("El título no puede exceder los 250 caracteres.");
            if (string.IsNullOrWhiteSpace(ticket.Descripcion)) return ErrorTicket("La descripción es requerida.");
            if (ticket.TicketEstatusId <= 0) return ErrorTicket("El estatus del ticket es requerido.");
            return null;
        }

        private ModelResponse<Ticket> ErrorTicket(string message)
        {
            return new ModelResponse<Ticket> { IsSuccess = false, Message = message };
        }

        public async Task<ModelResponse> EliminarTicket(Ticket ticket)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.EliminarTicket ENTRADA: TicketId={TicketId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticket?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarTicket(ticket);
            Log.Information("TicketService.EliminarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorArea(long areaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketsPorArea ENTRADA: AreaId={AreaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                areaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketsPorArea(areaId);
            Log.Information("TicketService.ObtenerTicketsPorArea SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUsuario(string creadoPor)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketsPorUsuario ENTRADA: CreadoPor={CreadoPor}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                creadoPor, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketsPorUsuario(creadoPor);
            Log.Information("TicketService.ObtenerTicketsPorUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUrgencia(int urgencia)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketsPorUrgencia ENTRADA: Urgencia={Urgencia}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                urgencia, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketsPorUrgencia(urgencia);
            Log.Information("TicketService.ObtenerTicketsPorUrgencia SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorEstatus(int ticketEstatusId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketsPorEstatus ENTRADA: TicketEstatusId={TicketEstatusId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketEstatusId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketsPorEstatus(ticketEstatusId);
            Log.Information("TicketService.ObtenerTicketsPorEstatus SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketEstatus>>> ObtenerTicketEstatus()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketEstatus ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketEstatus();
            Log.Information("TicketService.ObtenerTicketEstatus SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> TomarTicket(long ticketId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.TomarTicket ENTRADA: TicketId={TicketId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.TomarTicket(ticketId, comentario);
            Log.Information("TicketService.TomarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ReasignarTicket(long ticketId, long nuevoUsuarioId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ReasignarTicket ENTRADA: TicketId={TicketId}, NuevoUsuarioId={NuevoUsuarioId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, nuevoUsuarioId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ReasignarTicket(ticketId, nuevoUsuarioId, comentario);
            Log.Information("TicketService.ReasignarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketAsignacionDTO>>> ObtenerTicketAsignaciones(long ticketId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerTicketAsignaciones ENTRADA: TicketId={TicketId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTicketAsignaciones(ticketId);
            Log.Information("TicketService.ObtenerTicketAsignaciones SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ResolverTicket(long ticketId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ResolverTicket ENTRADA: TicketId={TicketId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ResolverTicket(ticketId, comentario);
            Log.Information("TicketService.ResolverTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> RechazarTicket(long ticketId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.RechazarTicket ENTRADA: TicketId={TicketId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.RechazarTicket(ticketId, comentario);
            Log.Information("TicketService.RechazarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> CerrarTicket(long ticketId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.CerrarTicket ENTRADA: TicketId={TicketId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.CerrarTicket(ticketId, comentario);
            Log.Information("TicketService.CerrarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> RetomarTicket(long ticketId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.RetomarTicket ENTRADA: TicketId={TicketId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.RetomarTicket(ticketId);
            Log.Information("TicketService.RetomarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> PausarTicket(long ticketId, string tipoPausa, string comentario, DateTime? fechaEstimada)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.PausarTicket ENTRADA: TicketId={TicketId}, TipoPausa={TipoPausa}, Comentario={Comentario}, FechaEstimada={FechaEstimada}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, tipoPausa, comentario, fechaEstimada, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.PausarTicket(ticketId, tipoPausa, comentario, fechaEstimada);
            Log.Information("TicketService.PausarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ReanudarTicket(long ticketId, string comentario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ReanudarTicket ENTRADA: TicketId={TicketId}, Comentario={Comentario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, comentario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ReanudarTicket(ticketId, comentario);
            Log.Information("TicketService.ReanudarTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuariosArea(long areaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerUsuariosArea ENTRADA: AreaId={AreaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                areaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerUsuariosArea(areaId);
            Log.Information("TicketService.ObtenerUsuariosArea SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaTicket()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TicketService.ObtenerPermisosParaTicket ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Tickets");
                Log.Information("TicketService.ObtenerPermisosParaTicket SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("TicketService.ObtenerPermisosParaTicket SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
