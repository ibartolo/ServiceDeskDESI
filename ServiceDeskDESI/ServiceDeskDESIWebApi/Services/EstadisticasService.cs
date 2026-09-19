using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace ServiceDeskDESIWebApi.Services
{
    public class EstadisticasService
    {
        private readonly DbWrapper _dbWrapper;

        public EstadisticasService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<MetricasResumenDTO> ObtenerResumen(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                Log.Information("EstadisticasService.ObtenerResumen para usuario {Usuario} ({FechaInicio} - {FechaFin})", usuario, fechaInicio, fechaFin);
                Log.Information("EstadisticasService.ObtenerResumen ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario, fechaInicio, fechaFin }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var resumen = _dbWrapper.ObtenerMetricasResumen(usuario, fechaInicio, fechaFin);

                var result = new ModelResponse<MetricasResumenDTO>
                {
                    IsSuccess = true,
                    Response = resumen ?? new MetricasResumenDTO(),
                    Message = "Resumen de métricas obtenido correctamente."
                };

                Log.Information("EstadisticasService.ObtenerResumen RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                Log.Information("EstadisticasService.ObtenerResumen SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerResumen para usuario {Usuario}", usuario);
                return new ModelResponse<MetricasResumenDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EstadisticasService.ObtenerResumen para usuario {Usuario}", usuario);
                return new ModelResponse<MetricasResumenDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener el resumen de métricas." };
            }
        }

        public ModelResponse<List<DistribucionEstatusDTO>> ObtenerDistribucionEstatus(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                Log.Information("EstadisticasService.ObtenerDistribucionEstatus para usuario {Usuario} ({FechaInicio} - {FechaFin})", usuario, fechaInicio, fechaFin);
                Log.Information("EstadisticasService.ObtenerDistribucionEstatus ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario, fechaInicio, fechaFin }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var distribucion = _dbWrapper.ObtenerDistribucionEstatus(usuario, fechaInicio, fechaFin);

                var result = new ModelResponse<List<DistribucionEstatusDTO>>
                {
                    IsSuccess = true,
                    Response = distribucion ?? new List<DistribucionEstatusDTO>(),
                    Message = "Distribución de estatus obtenida correctamente."
                };

                Log.Information("EstadisticasService.ObtenerDistribucionEstatus RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                Log.Information("EstadisticasService.ObtenerDistribucionEstatus SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerDistribucionEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<DistribucionEstatusDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EstadisticasService.ObtenerDistribucionEstatus para usuario {Usuario}", usuario);
                return new ModelResponse<List<DistribucionEstatusDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener la distribución de estatus." };
            }
        }

        public ModelResponse<List<EvolucionDiariaDTO>> ObtenerEvolucionDiaria(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                Log.Information("EstadisticasService.ObtenerEvolucionDiaria para usuario {Usuario} ({FechaInicio} - {FechaFin})", usuario, fechaInicio, fechaFin);
                Log.Information("EstadisticasService.ObtenerEvolucionDiaria ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario, fechaInicio, fechaFin }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var evolucion = _dbWrapper.ObtenerEvolucionDiaria(usuario, fechaInicio, fechaFin);

                var result = new ModelResponse<List<EvolucionDiariaDTO>>
                {
                    IsSuccess = true,
                    Response = evolucion ?? new List<EvolucionDiariaDTO>(),
                    Message = "Evolución diaria obtenida correctamente."
                };

                Log.Information("EstadisticasService.ObtenerEvolucionDiaria RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                Log.Information("EstadisticasService.ObtenerEvolucionDiaria SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEvolucionDiaria para usuario {Usuario}", usuario);
                return new ModelResponse<List<EvolucionDiariaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EstadisticasService.ObtenerEvolucionDiaria para usuario {Usuario}", usuario);
                return new ModelResponse<List<EvolucionDiariaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener la evolución diaria." };
            }
        }

        public ModelResponse<List<RankingAreaDTO>> ObtenerRankingAreas(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                Log.Information("EstadisticasService.ObtenerRankingAreas para usuario {Usuario} ({FechaInicio} - {FechaFin})", usuario, fechaInicio, fechaFin);
                Log.Information("EstadisticasService.ObtenerRankingAreas ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario, fechaInicio, fechaFin }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var ranking = _dbWrapper.ObtenerRankingAreas(usuario, fechaInicio, fechaFin);

                var result = new ModelResponse<List<RankingAreaDTO>>
                {
                    IsSuccess = true,
                    Response = ranking ?? new List<RankingAreaDTO>(),
                    Message = "Ranking de áreas obtenido correctamente."
                };

                Log.Information("EstadisticasService.ObtenerRankingAreas RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                Log.Information("EstadisticasService.ObtenerRankingAreas SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerRankingAreas para usuario {Usuario}", usuario);
                return new ModelResponse<List<RankingAreaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EstadisticasService.ObtenerRankingAreas para usuario {Usuario}", usuario);
                return new ModelResponse<List<RankingAreaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener el ranking de áreas." };
            }
        }

        public ModelResponse<List<RankingReasignacionDTO>> ObtenerRankingReasignaciones(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                Log.Information("EstadisticasService.ObtenerRankingReasignaciones para usuario {Usuario} ({FechaInicio} - {FechaFin})", usuario, fechaInicio, fechaFin);
                Log.Information("EstadisticasService.ObtenerRankingReasignaciones ENTRADA: {Json}", LogSanitizer.ToJson(new { usuario, fechaInicio, fechaFin }));

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                int topN;
                if (!int.TryParse(ConfigurationManager.AppSettings["EstadisticasTopAgentes"], out topN))
                    topN = 5;

                var ranking = _dbWrapper.ObtenerRankingReasignaciones(usuario, fechaInicio, fechaFin, topN);

                var result = new ModelResponse<List<RankingReasignacionDTO>>
                {
                    IsSuccess = true,
                    Response = ranking ?? new List<RankingReasignacionDTO>(),
                    Message = "Ranking de reasignaciones obtenido correctamente."
                };

                Log.Information("EstadisticasService.ObtenerRankingReasignaciones RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                Log.Information("EstadisticasService.ObtenerRankingReasignaciones SALIDA: {Json}", LogSanitizer.ToJson(result));
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerRankingReasignaciones para usuario {Usuario}", usuario);
                return new ModelResponse<List<RankingReasignacionDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EstadisticasService.ObtenerRankingReasignaciones para usuario {Usuario}", usuario);
                return new ModelResponse<List<RankingReasignacionDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener el ranking de reasignaciones." };
            }
        }
    }
}
