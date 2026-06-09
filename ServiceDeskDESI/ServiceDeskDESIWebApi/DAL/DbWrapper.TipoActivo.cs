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
        public ModelResponse ObtenerTodosTipoActivos(long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var TipoActivos = GetObjects("ObtenerTipoActivo", CommandType.StoredProcedure,
                     new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, TipoActivo>((reader) =>
                    {
                        var activo = LlenarEntidad<TipoActivo>(reader);
                        return activo;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = TipoActivos;
                modelResponse.Message = "TiposActivos obtenidos correctamente";

            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener TipoActivo";
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

        public ModelResponse ObtenerTipoActivoPorId (long id, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var result = GetObject("ObtenerTipoActivoPorId", CommandType.StoredProcedure,
                 new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                  new Func<IDataReader, TipoActivo>((reader) =>
                  {
                      var r = LlenarEntidad<TipoActivo>(reader);
                      return r;
                  }));
                if (result ==null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el TipoActivo especificado.";
                    return modelResponse;
                }
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "TipoActivo Obtenido Correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el TipoActivo";
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