using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
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
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EstadisticasService.ObtenerResumen ENTRADA: FechaInicio={FechaInicio}, FechaFin={FechaFin}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                fechaInicio, fechaFin, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerResumenEstadisticas(fechaInicio, fechaFin);
            Log.Information("EstadisticasService.ObtenerResumen SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<DistribucionEstatusDTO>>> ObtenerDistribucionEstatus(DateTime fechaInicio, DateTime fechaFin)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EstadisticasService.ObtenerDistribucionEstatus ENTRADA: FechaInicio={FechaInicio}, FechaFin={FechaFin}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                fechaInicio, fechaFin, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerDistribucionEstatusEstadisticas(fechaInicio, fechaFin);
            Log.Information("EstadisticasService.ObtenerDistribucionEstatus SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<EvolucionDiariaDTO>>> ObtenerEvolucionDiaria(DateTime fechaInicio, DateTime fechaFin)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EstadisticasService.ObtenerEvolucionDiaria ENTRADA: FechaInicio={FechaInicio}, FechaFin={FechaFin}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                fechaInicio, fechaFin, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerEvolucionDiariaEstadisticas(fechaInicio, fechaFin);
            Log.Information("EstadisticasService.ObtenerEvolucionDiaria SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<RankingAreaDTO>>> ObtenerRankingAreas(DateTime fechaInicio, DateTime fechaFin)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EstadisticasService.ObtenerRankingAreas ENTRADA: FechaInicio={FechaInicio}, FechaFin={FechaFin}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                fechaInicio, fechaFin, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerRankingAreasEstadisticas(fechaInicio, fechaFin);
            Log.Information("EstadisticasService.ObtenerRankingAreas SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<RankingReasignacionDTO>>> ObtenerRankingReasignaciones(DateTime fechaInicio, DateTime fechaFin)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EstadisticasService.ObtenerRankingReasignaciones ENTRADA: FechaInicio={FechaInicio}, FechaFin={FechaFin}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                fechaInicio, fechaFin, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerRankingReasignacionesEstadisticas(fechaInicio, fechaFin);
            Log.Information("EstadisticasService.ObtenerRankingReasignaciones SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
