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
        public ModelResponse ObtenerTodosTipoActivos()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var TipoActivos = GetObjects("ObtenerTipoActivo", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, TipoActivo>((reader) =>
                    {
                        var activo = LlenarEntidad<TipoActivo>(reader);
                        return activo;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = TipoActivos;
                modelResponse.Message = "TiposActivos obtenidos correctamente";

            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }
        public ModelResponse GuardarOActualizarTipoActivo(TipoActivo ta)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(ta).ToArray();
                var activoId = ExecuteScalar("GuardarOActualizarTipoActivo", CommandType.StoredProcedure, parametros);
                ta.Id = Convert.ToInt64(activoId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = ta;
                modelResponse.Message = "TipoActivos Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTipoActivoPorId (long id)
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

                var result = GetObject("ObtenerTipoActivoPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, TipoActivo>((reader) =>
                    {
                        var r = LlenarEntidad<TipoActivo>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "TipoActivo Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse EliminarTipoActivo (TipoActivo t)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarTipoActivo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", t.Id),
                    new SqlParameter("@ModificadoPor", t.ModificadoPor),
                    new SqlParameter("@FechaModificacion",t.FechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Tipo Activo Eliminado Correctamente";
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