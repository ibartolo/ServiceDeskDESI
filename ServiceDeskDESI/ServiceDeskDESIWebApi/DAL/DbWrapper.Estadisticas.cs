using ServiceDeskDESIEntities.Tickets;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Obtiene el resumen de KPIs de estadísticas (multi-tenant, por usuario autenticado).
        /// </summary>
        public MetricasResumenDTO ObtenerMetricasResumen(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            return GetObject("ObtenerMetricasResumen", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin)
                },
                new Func<IDataReader, MetricasResumenDTO>((reader) => LlenarEntidad<MetricasResumenDTO>(reader)));
        }

        /// <summary>
        /// Obtiene la distribución de tickets por estatus (gráfica de pastel).
        /// </summary>
        public List<DistribucionEstatusDTO> ObtenerDistribucionEstatus(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            return GetObjects("ObtenerDistribucionEstatus", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin)
                },
                new Func<IDataReader, DistribucionEstatusDTO>((reader) => LlenarEntidad<DistribucionEstatusDTO>(reader))).ToList();
        }

        /// <summary>
        /// Obtiene la evolución diaria de tickets creados y resueltos.
        /// </summary>
        public List<EvolucionDiariaDTO> ObtenerEvolucionDiaria(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            return GetObjects("ObtenerEvolucionDiaria", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin)
                },
                new Func<IDataReader, EvolucionDiariaDTO>((reader) => LlenarEntidad<EvolucionDiariaDTO>(reader))).ToList();
        }

        /// <summary>
        /// Obtiene el ranking de áreas con tickets creados en el rango.
        /// </summary>
        public List<RankingAreaDTO> ObtenerRankingAreas(string usuario, DateTime fechaInicio, DateTime fechaFin)
        {
            return GetObjects("ObtenerRankingAreas", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin)
                },
                new Func<IDataReader, RankingAreaDTO>((reader) => LlenarEntidad<RankingAreaDTO>(reader))).ToList();
        }

        /// <summary>
        /// Obtiene el ranking de reasignaciones (Reciben / Quitan) limitado por TopN.
        /// </summary>
        public List<RankingReasignacionDTO> ObtenerRankingReasignaciones(string usuario, DateTime fechaInicio, DateTime fechaFin, int topN)
        {
            return GetObjects("ObtenerRankingReasignaciones", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@FechaInicio", fechaInicio),
                    new SqlParameter("@FechaFin", fechaFin),
                    new SqlParameter("@TopN", topN)
                },
                new Func<IDataReader, RankingReasignacionDTO>((reader) => LlenarEntidad<RankingReasignacionDTO>(reader))).ToList();
        }
    }
}
