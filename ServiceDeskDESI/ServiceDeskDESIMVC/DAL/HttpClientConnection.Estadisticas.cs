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
        /// <summary>
        /// Formatea la fecha en ISO (yyyy-MM-dd) para el query string de los endpoints de estadísticas.
        /// </summary>
        private static string FormatearFechaEstadisticas(DateTime fecha)
        {
            return fecha.ToString("yyyy-MM-dd");
        }

        public async Task<ModelResponse<MetricasResumenDTO>> ObtenerResumenEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await RequestAsync<MetricasResumenDTO>(
                $"api/Estadisticas/Resumen?fechaInicio={FormatearFechaEstadisticas(fechaInicio)}&fechaFin={FormatearFechaEstadisticas(fechaFin)}",
                HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<DistribucionEstatusDTO>>> ObtenerDistribucionEstatusEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await RequestAsync<List<DistribucionEstatusDTO>>(
                $"api/Estadisticas/DistribucionEstatus?fechaInicio={FormatearFechaEstadisticas(fechaInicio)}&fechaFin={FormatearFechaEstadisticas(fechaFin)}",
                HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<EvolucionDiariaDTO>>> ObtenerEvolucionDiariaEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await RequestAsync<List<EvolucionDiariaDTO>>(
                $"api/Estadisticas/EvolucionDiaria?fechaInicio={FormatearFechaEstadisticas(fechaInicio)}&fechaFin={FormatearFechaEstadisticas(fechaFin)}",
                HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<RankingAreaDTO>>> ObtenerRankingAreasEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await RequestAsync<List<RankingAreaDTO>>(
                $"api/Estadisticas/RankingAreas?fechaInicio={FormatearFechaEstadisticas(fechaInicio)}&fechaFin={FormatearFechaEstadisticas(fechaFin)}",
                HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<RankingReasignacionDTO>>> ObtenerRankingReasignacionesEstadisticas(DateTime fechaInicio, DateTime fechaFin)
        {
            return await RequestAsync<List<RankingReasignacionDTO>>(
                $"api/Estadisticas/RankingReasignaciones?fechaInicio={FormatearFechaEstadisticas(fechaInicio)}&fechaFin={FormatearFechaEstadisticas(fechaFin)}",
                HttpMethod.Get, null, token.Token.access_token);
        }
    }
}
