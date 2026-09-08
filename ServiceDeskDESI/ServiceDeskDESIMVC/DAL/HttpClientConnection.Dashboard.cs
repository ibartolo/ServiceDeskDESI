using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<DashboardIndicadoresDTO>> ObtenerIndicadoresDashboard()
        {
            return await RequestAsync<DashboardIndicadoresDTO>("api/Dashboard/Indicadores", HttpMethod.Get, null, token.Token.access_token);
        }
    }
}
