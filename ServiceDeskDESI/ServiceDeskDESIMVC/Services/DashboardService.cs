using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
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
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("DashboardService.ObtenerIndicadores ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerIndicadoresDashboard();
            Log.Information("DashboardService.ObtenerIndicadores SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
