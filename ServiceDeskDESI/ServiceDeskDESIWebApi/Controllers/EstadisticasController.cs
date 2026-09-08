using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Estadisticas")]
    public class EstadisticasController : BaseController
    {
        private readonly EstadisticasService _service = new EstadisticasService();

        [HttpGet, Route("Resumen")]
        [Permiso("Estadisticas", "Leer")]
        public ModelResponse<MetricasResumenDTO> Resumen(DateTime fechaInicio, DateTime fechaFin)
            => _service.ObtenerResumen(User.Identity.Name, fechaInicio, fechaFin);

        [HttpGet, Route("DistribucionEstatus")]
        [Permiso("Estadisticas", "Leer")]
        public ModelResponse<List<DistribucionEstatusDTO>> DistribucionEstatus(DateTime fechaInicio, DateTime fechaFin)
            => _service.ObtenerDistribucionEstatus(User.Identity.Name, fechaInicio, fechaFin);

        [HttpGet, Route("EvolucionDiaria")]
        [Permiso("Estadisticas", "Leer")]
        public ModelResponse<List<EvolucionDiariaDTO>> EvolucionDiaria(DateTime fechaInicio, DateTime fechaFin)
            => _service.ObtenerEvolucionDiaria(User.Identity.Name, fechaInicio, fechaFin);

        [HttpGet, Route("RankingAreas")]
        [Permiso("Estadisticas", "Leer")]
        public ModelResponse<List<RankingAreaDTO>> RankingAreas(DateTime fechaInicio, DateTime fechaFin)
            => _service.ObtenerRankingAreas(User.Identity.Name, fechaInicio, fechaFin);

        [HttpGet, Route("RankingReasignaciones")]
        [Permiso("Estadisticas", "Leer")]
        public ModelResponse<List<RankingReasignacionDTO>> RankingReasignaciones(DateTime fechaInicio, DateTime fechaFin)
            => _service.ObtenerRankingReasignaciones(User.Identity.Name, fechaInicio, fechaFin);
    }
}
