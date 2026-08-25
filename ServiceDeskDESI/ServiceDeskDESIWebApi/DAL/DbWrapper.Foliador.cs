using ServiceDeskDESIEntities.Catalogos;
using System;
using System.Data;
using System.Data.SqlClient;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        /// <summary>
        /// Consulta el foliador por Nombre para la empresa del usuario.
        /// Devuelve null si no existe fila (resultado vacío, sin error).
        /// </summary>
        public Foliador ConsultarFoliador(string nombre, string usuario)
        {
            return GetObject("ConsultarFoliador", CommandType.StoredProcedure,
                new[] {
                    new SqlParameter("@Nombre", nombre),
                    new SqlParameter("@Usuario", usuario)
                },
                new Func<IDataReader, Foliador>((reader) =>
                {
                    var f = LlenarEntidad<Foliador>(reader);
                    return f;
                }));
        }

        /// <summary>
        /// Incrementa el consecutivo del foliador atómicamente y devuelve el nuevo valor.
        /// Devuelve 0 si no se pudo resolver la empresa del usuario.
        /// </summary>
        public int ActualizarFoliador(string nombre, string usuario)
        {
            var result = ExecuteScalar("ActualizarFoliador", CommandType.StoredProcedure, new SqlParameter[]
            {
                new SqlParameter("@Nombre", nombre),
                new SqlParameter("@Usuario", usuario)
            });

            return result == null || result is DBNull ? 0 : Convert.ToInt32(result);
        }
    }
}
