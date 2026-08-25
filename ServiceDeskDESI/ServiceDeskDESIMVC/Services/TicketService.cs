using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
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
            return await _httpClient.ObtenerTickets();
        }

        public async Task<ModelResponse<TicketDTO>> ObtenerTicketPorId(long id)
        {
            return await _httpClient.ObtenerTicketPorId(id);
        }

        public async Task<ModelResponse<Ticket>> GuardarOActualizarTicket(Ticket ticket)
        {
            var validacion = ValidarTicket(ticket);
            if (validacion != null) return validacion;

            return await _httpClient.GuardarOActualizarTicket(ticket);
        }

        public async Task<ModelResponse<Ticket>> GuardarTicketConEvidencias(Ticket ticket, HttpFileCollectionBase files)
        {
            var validacion = ValidarTicket(ticket);
            if (validacion != null) return validacion;

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

                return await _httpClient.PostMultipartAsync<Ticket>("api/Ticket/GuardarConEvidencias", form);
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
            return await _httpClient.EliminarTicket(ticket);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorArea(long areaId)
        {
            return await _httpClient.ObtenerTicketsPorArea(areaId);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUsuario(string creadoPor)
        {
            return await _httpClient.ObtenerTicketsPorUsuario(creadoPor);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUrgencia(int urgencia)
        {
            return await _httpClient.ObtenerTicketsPorUrgencia(urgencia);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorEstatus(int ticketEstatusId)
        {
            return await _httpClient.ObtenerTicketsPorEstatus(ticketEstatusId);
        }

        public async Task<ModelResponse<List<TicketEstatus>>> ObtenerTicketEstatus()
        {
            return await _httpClient.ObtenerTicketEstatus();
        }

        public async Task<ModelResponse> TomarTicket(long ticketId, string comentario)
        {
            return await _httpClient.TomarTicket(ticketId, comentario);
        }

        public async Task<ModelResponse> ReasignarTicket(long ticketId, long nuevoUsuarioId, string comentario)
        {
            return await _httpClient.ReasignarTicket(ticketId, nuevoUsuarioId, comentario);
        }

        public async Task<ModelResponse<List<TicketAsignacionDTO>>> ObtenerTicketAsignaciones(long ticketId)
        {
            return await _httpClient.ObtenerTicketAsignaciones(ticketId);
        }

        public async Task<ModelResponse> ResolverTicket(long ticketId, string comentario)
        {
            return await _httpClient.ResolverTicket(ticketId, comentario);
        }

        public async Task<ModelResponse> RechazarTicket(long ticketId, string comentario)
        {
            return await _httpClient.RechazarTicket(ticketId, comentario);
        }

        public async Task<ModelResponse> CerrarTicket(long ticketId, string comentario)
        {
            return await _httpClient.CerrarTicket(ticketId, comentario);
        }

        public async Task<ModelResponse> RetomarTicket(long ticketId)
        {
            return await _httpClient.RetomarTicket(ticketId);
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuariosArea(long areaId)
        {
            return await _httpClient.ObtenerUsuariosArea(areaId);
        }

        public async Task<object> ObtenerPermisosParaTicket()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Tickets");
            }
            return null;
        }
    }
}
