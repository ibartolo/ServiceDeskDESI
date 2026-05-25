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
        public ModelResponse ObtenerTodasRelaciones()
        {
            var modelResponse = new ModelResponse();
            try 
            {
                var relacion = GetObjects("ObtenerRelacion", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                  new Func<IDataReader, Relacion>((reader) =>
                  {
                      var relaciones = LlenarEntidad<Relacion>(reader);
                      return relaciones;
                  }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = relacion;
                modelResponse.Message = "Relacion obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }
        public ModelResponse GuardarOActualizarRelacion(Relacion r)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(r).ToArray();
                var companiaId = ExecuteScalar("GuardarOActualizarRelacion", CommandType.StoredProcedure, parametros);
                r.Id = Convert.ToInt64(companiaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = r;
                modelResponse.Message = "Relacion Guardado correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerRelacionPorId(long id)
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

                var result = GetObject("ObtenerRelacionPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Relacion>((reader) =>
                    {
                        var r = LlenarEntidad<Relacion>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Relacion obtenido correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse EliminarRelacion(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarRelacion", CommandType.StoredProcedure, new SqlParameter[]
                {
                     new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Relacion eliminado correctamente";
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