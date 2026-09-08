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
        public ModelResponse<List<UsuarioPagina>> ObtenerTodasRelaciones()
        {
            var modelResponse = new ModelResponse<List<UsuarioPagina>>();
            try 
            {
                var relacion = GetObjects("ObtenerRelacion", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                  new Func<IDataReader, UsuarioPagina>((reader) =>
                  {
                      var relaciones = LlenarEntidad<UsuarioPagina>(reader);
                      return relaciones;
                  }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = relacion.ToList();
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
        public ModelResponse<UsuarioPagina> GuardarOActualizarRelacion(UsuarioPagina r)
        {
            var modelResponse = new ModelResponse<UsuarioPagina>();
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
        public ModelResponse<UsuarioPagina> ObtenerRelacionPorId(long id)
        {
            var modelResponse = new ModelResponse<UsuarioPagina>();
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
                    parameters, new Func<System.Data.IDataReader, UsuarioPagina>((reader) =>
                    {
                        var r = LlenarEntidad<UsuarioPagina>(reader);
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