using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class DashboardService
    {
        private readonly HttpClientConnection _httpClient;

        public DashboardService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<DashboardIndicadoresDTO>> ObtenerIndicadores()
        {
            return await _httpClient.ObtenerIndicadoresDashboard();
        }
    }
}
