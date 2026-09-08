using ServiceDeskDESIEntities.Tickets;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Obtiene los 4 indicadores del dashboard del agente (multi-tenant, por usuario autenticado).
        /// </summary>
        public DashboardIndicadoresDTO ObtenerIndicadoresDashboard(string usuario)
        {
            return GetObject("ObtenerIndicadoresDashboard", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Usuario", usuario)
                },
                new Func<IDataReader, DashboardIndicadoresDTO>((reader) =>
                {
                    var d = LlenarEntidad<DashboardIndicadoresDTO>(reader);
                    return d;
                }));
        }
    }
}
