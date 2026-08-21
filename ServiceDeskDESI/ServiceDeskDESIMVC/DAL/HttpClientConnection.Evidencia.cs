using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<EvidenciaConfigDTO>> ObtenerConfiguracionEvidencias()
        {
            return await RequestAsync<EvidenciaConfigDTO>("api/Evidencia/Configuracion", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<TicketEvidencia>>> ObtenerEvidenciasPorTicket(long ticketId)
        {
            return await RequestAsync<List<TicketEvidencia>>("api/Evidencia/PorTicket/" + ticketId, HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<EvidenciaDescargaDTO> ObtenerEvidenciaDescarga(long id)
        {
            return await RequestFileAsync("api/Evidencia/Descargar/" + id, token.Token.access_token);
        }
    }
}
