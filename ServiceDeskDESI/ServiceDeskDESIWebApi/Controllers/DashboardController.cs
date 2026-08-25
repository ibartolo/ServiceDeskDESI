using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.Services;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Dashboard")]
    public class DashboardController : BaseController
    {
        private readonly DashboardService _dashboardService;

        public DashboardController()
        {
            _dashboardService = new DashboardService();
        }

        /// <summary>
        /// Obtiene los 4 indicadores del dashboard (del usuario autenticado y su empresa).
        /// </summary>
        [HttpGet, Route("Indicadores")]
        public ModelResponse<DashboardIndicadoresDTO> Indicadores()
        {
            var usuario = User.Identity.Name;
            return _dashboardService.ObtenerIndicadores(usuario);
        }
    }
}
