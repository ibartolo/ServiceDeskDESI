using Newtonsoft.Json;
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

        public async Task<ModelResponse> ObtenerTickets()
        {
            return await _httpClient.ObtenerTickets();
        }

        public async Task<ModelResponse> ObtenerTicketPorId(long id)
        {
            return await _httpClient.ObtenerTicketPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarTicket(Ticket ticket)
        {
            return await _httpClient.GuardarOActualizarTicket(ticket);
        }

        public async Task<ModelResponse> EliminarTicket(Ticket ticket)
        {
            return await _httpClient.EliminarTicket(ticket);
        }

        public async Task<ModelResponse> ObtenerTicketsPorArea(long areaId)
        {
            return await _httpClient.ObtenerTicketsPorArea(areaId);
        }

        public async Task<ModelResponse> ObtenerTicketsPorUsuario(string creadoPor)
        {
            return await _httpClient.ObtenerTicketsPorUsuario(creadoPor);
        }

        public async Task<ModelResponse> ObtenerTicketsPorUrgencia(int urgencia)
        {
            return await _httpClient.ObtenerTicketsPorUrgencia(urgencia);
        }

        public async Task<ModelResponse> ObtenerTicketsPorEstatus(int ticketEstatusId)
        {
            return await _httpClient.ObtenerTicketsPorEstatus(ticketEstatusId);
        }

        public async Task<ModelResponse> ObtenerTicketEstatus()
        {
            return await _httpClient.ObtenerTicketEstatus();
        }

        public async Task<object> ObtenerPermisosParaTicket()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Tickets");
            }
            return null;
        }
    }
}