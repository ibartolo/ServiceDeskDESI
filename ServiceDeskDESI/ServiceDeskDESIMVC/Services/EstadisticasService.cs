using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class EstadisticasService
    {
        private readonly HttpClientConnection _httpClient;

        public EstadisticasService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<MetricasResumenDTO>> ObtenerResumen(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _httpClient.ObtenerResumenEstadisticas(fechaInicio, fechaFin);
        }

        public async Task<ModelResponse<List<DistribucionEstatusDTO>>> ObtenerDistribucionEstatus(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _httpClient.ObtenerDistribucionEstatusEstadisticas(fechaInicio, fechaFin);
        }

        public async Task<ModelResponse<List<EvolucionDiariaDTO>>> ObtenerEvolucionDiaria(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _httpClient.ObtenerEvolucionDiariaEstadisticas(fechaInicio, fechaFin);
        }

        public async Task<ModelResponse<List<RankingAreaDTO>>> ObtenerRankingAreas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _httpClient.ObtenerRankingAreasEstadisticas(fechaInicio, fechaFin);
        }

        public async Task<ModelResponse<List<RankingReasignacionDTO>>> ObtenerRankingReasignaciones(DateTime fechaInicio, DateTime fechaFin)
        {
            return await _httpClient.ObtenerRankingReasignacionesEstadisticas(fechaInicio, fechaFin);
        }
    }
}
