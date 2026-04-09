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
                var companias = GetObjects("ObtenerCompanias", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, Compania>((reader) =>
                    {
                        var compania = LlenarEntidad<Compania>(reader);
                        return compania;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = companias;
                modelResponse.Message = "Companias obtenidas correctamente";

            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
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
        public ModelResponse EliminarCompania(Compania c)

        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarCompania", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", c.Id),
                    new SqlParameter("@ModificadoPor", c.ModificadoPor),
                    new SqlParameter("@FechaModificacion",c.FechaModificacion)
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

    }

}
