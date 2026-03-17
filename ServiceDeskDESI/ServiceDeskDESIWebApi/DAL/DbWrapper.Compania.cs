using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse ObtenerCompania()
        {
            var modelResponse = new ModelResponse();
            try
            {
                modelResponse.IsSuccess = true;
                var parameters = new List<SqlParameter>();
                var result = GetObject("ObtenerCompanias", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Compania>((reader) =>
                    {
                        var r = MapearPorpiedades<Compania>(reader);
                        return r;
                    }));
                modelResponse.Response = result;

            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = true;
                modelResponse.Message ="Companias obtenidas correctamente";
                modelResponse.Response = ex;
            }
            return modelResponse;
        }

        public ModelResponse GuardarOActualizarCompania(Compania c)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(c).ToArray();
                var companiaId = ExecuteScalar("GuardarOActualizarCompania", CommandType.StoredProcedure, parametros);
                c.Id = Convert.ToInt64(companiaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = c;
                modelResponse.Message = "Compania Guardado correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
         public ModelResponse EliminarCompania(long id, string modificadoPor, DateTime fechaModificacion)

         {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarCompania", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Compania eliminado correctamente";
                modelResponse.Response = null;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
         }
        public ModelResponse ObtenerCompaniaPorId (long id)
        {
            var modelResponse = new ModelResponse();
           try
           {
                modelResponse.IsSuccess = true;
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    Value = id,
                    IsNullable = true,
                    ParameterName = "@Id",
                    SqlDbType = System.Data.SqlDbType.Int
                });

                var result = GetObject("ObtenerCompaniaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Compania>((reader) =>
                    {
                        var r = LlenarEntidad<Compania>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Compania obtenido correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;

        }

    }

}
