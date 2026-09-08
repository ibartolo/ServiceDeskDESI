using Newtonsoft.Json;
using ServiceDeskDESIMVC.Filters;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Services;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Controllers
{
    public class EstadisticasController : BaseController
    {
        private readonly EstadisticasService _estadisticasService;
        private readonly RolService _rolService;
        private readonly AreaService _areaService;

        public EstadisticasController()
        {
            _estadisticasService = new EstadisticasService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _areaService = new AreaService(httpClientConnection);
        }

        /// <summary>
        /// Vista principal del módulo "Estadísticas" (solo lectura).
        /// Gate de acceso Opción A: requiere sesión y (Administrador | Supervisor | Jefe de área).
        /// El jefe de área se modela como Area.UsuarioResponsableId == UserID.
        /// </summary>
        [Permiso("Estadisticas", "Leer")]
        public async Task<ActionResult> Index()
        {
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID <= 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            bool esAdmin = false;
            bool esSupervisor = false;
            bool esJefeArea = false;

            try
            {
                var rolesResponse = await _rolService.ObtenerRolesPorUsuario(tokenCookie.UserID);
                esAdmin = rolesResponse.IsSuccess && rolesResponse.Response != null
                       && rolesResponse.Response.Any(r => r.Nombre == "Administrador");
                esSupervisor = rolesResponse.IsSuccess && rolesResponse.Response != null
                            && rolesResponse.Response.Any(r => r.Nombre == "Supervisor");

                var areasResponse = await _areaService.ConsultarTodasAreas();
                esJefeArea = areasResponse.IsSuccess && areasResponse.Response != null
                          && areasResponse.Response.Any(a => a.UsuarioResponsableId == tokenCookie.UserID);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al validar el acceso al módulo Estadísticas para el usuario {Usuario}", tokenCookie.UserName);
                return RedirectToAction("AccesoDenegado", "Home");
            }

            // Opción A (decisión del usuario): el rol Supervisor SIEMPRE entra (ve todas las áreas en fase 1).
            if (!esAdmin && !esSupervisor && !esJefeArea)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            ViewBag.FechaInicio = new DateTime(DateTime.Now.Year, 1, 1);
            ViewBag.FechaFin = DateTime.Today;

            return View();
        }

        [Permiso("Estadisticas", "Leer")]
        public async Task<string> ObtenerResumen(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = await _estadisticasService.ObtenerResumen(fechaInicio, fechaFin);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Estadisticas", "Leer")]
        public async Task<string> ObtenerDistribucionEstatus(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = await _estadisticasService.ObtenerDistribucionEstatus(fechaInicio, fechaFin);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Estadisticas", "Leer")]
        public async Task<string> ObtenerEvolucionDiaria(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = await _estadisticasService.ObtenerEvolucionDiaria(fechaInicio, fechaFin);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Estadisticas", "Leer")]
        public async Task<string> ObtenerRankingAreas(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = await _estadisticasService.ObtenerRankingAreas(fechaInicio, fechaFin);
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Estadisticas", "Leer")]
        public async Task<string> ObtenerRankingReasignaciones(DateTime fechaInicio, DateTime fechaFin)
        {
            var response = await _estadisticasService.ObtenerRankingReasignaciones(fechaInicio, fechaFin);
            return JsonConvert.SerializeObject(response);
        }
    }
}
