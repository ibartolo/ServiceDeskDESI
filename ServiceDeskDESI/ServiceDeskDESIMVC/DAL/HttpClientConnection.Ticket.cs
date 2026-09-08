using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTickets()
        {
            return await RequestAsync<List<TicketDTO>>($"api/Ticket/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<TicketDTO>> ObtenerTicketPorId(long id)
        {
            return await RequestAsync<TicketDTO>($"api/Ticket/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Ticket>> GuardarOActualizarTicket(Ticket ticket)
        {
            MappingColumSecurity(ticket);

            return await RequestAsync<Ticket>($"api/Ticket/Guardar", HttpMethod.Post, ticket, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarTicket(Ticket ticket)
        {
            MappingColumSecurity(ticket);

            var result = await RequestAsync<object>($"api/Ticket/Eliminar", HttpMethod.Delete, ticket,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorArea(long areaId)
        {
            return await RequestAsync<List<TicketDTO>>($"api/Ticket/Area/{areaId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUsuario(string creadoPor)
        {
            return await RequestAsync<List<TicketDTO>>($"api/Ticket/Usuario/{creadoPor}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorUrgencia(int urgencia)
        {
            return await RequestAsync<List<TicketDTO>>($"api/Ticket/Urgencia/{urgencia}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<TicketDTO>>> ObtenerTicketsPorEstatus(int ticketEstatusId)
        {
            return await RequestAsync<List<TicketDTO>>($"api/Ticket/Estatus/{ticketEstatusId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<TicketEstatus>>> ObtenerTicketEstatus()
        {
            return await RequestAsync<List<TicketEstatus>>($"api/Ticket/Estatus/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse> TomarTicket(long ticketId, string comentario)
        {
            var request = new
            {
                TicketId = ticketId,
                Comentario = comentario
            };

            var result = await RequestAsync<object>($"api/Ticket/Tomar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ReasignarTicket(long ticketId, long nuevoUsuarioId, string comentario)
        {
            var request = new
            {
                TicketId = ticketId,
                NuevoUsuarioId = nuevoUsuarioId,
                Comentario = comentario
            };

            var result = await RequestAsync<object>($"api/Ticket/Reasignar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse<List<TicketAsignacionDTO>>> ObtenerTicketAsignaciones(long ticketId)
        {
            return await RequestAsync<List<TicketAsignacionDTO>>($"api/Ticket/Asignaciones/{ticketId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse> ResolverTicket(long ticketId, string comentario)
        {
            var request = new
            {
                TicketId = ticketId,
                Comentario = comentario
            };

            var result = await RequestAsync<object>($"api/Ticket/Resolver", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> RechazarTicket(long ticketId, string comentario)
        {
            var request = new
            {
                TicketId = ticketId,
                Comentario = comentario
            };

            var result = await RequestAsync<object>($"api/Ticket/Rechazar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> CerrarTicket(long ticketId, string comentario)
        {
            var request = new
            {
                TicketId = ticketId,
                Comentario = comentario
            };

            var result = await RequestAsync<object>($"api/Ticket/Cerrar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> RetomarTicket(long ticketId)
        {
            var request = new
            {
                TicketId = ticketId
            };

            var result = await RequestAsync<object>($"api/Ticket/Retomar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuariosArea(long areaId)
        {
            return await RequestAsync<List<UsuarioDTO>>($"api/Ticket/UsuariosArea/{areaId}", HttpMethod.Get, null, token.Token.access_token);
        }

    }
}
