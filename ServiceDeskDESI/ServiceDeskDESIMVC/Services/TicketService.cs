using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            return await _httpClient.GuardarOActualizarTicket(ticket);
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
