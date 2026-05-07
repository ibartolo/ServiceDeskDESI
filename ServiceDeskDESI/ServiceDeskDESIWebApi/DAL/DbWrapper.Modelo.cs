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
        public ModelResponse ObtenerTodosLosModelos()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var modelos = GetObjects("ObtenerModelo", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                   new Func<IDataReader, Modelo>((reader) =>
                   {
                       var activo = LlenarEntidad<Modelo>(reader);
                       return activo;
                   }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = modelos;
                modelResponse.Message = "Modelos Obtenidos Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }

        public ModelResponse GuardarOActualizarModelo(Modelo m)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(m).ToArray();
                var modeloId = ExecuteScalar("GuardarOActualizarModelo", CommandType.StoredProcedure, parametros);
                m.Id = Convert.ToInt64(modeloId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Modelos Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse ObtnerModeloPorId(long id)
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

                var result = GetObject("ObtenerModeloPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Modelo>((reader) =>
                    {
                        var r = LlenarEntidad<Modelo>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Modelo Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;

        }
        public ModelResponse EliminarModelo (Modelo m)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarModelo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", m.Id),
                    new SqlParameter("@ModificadoPor", m.ModificadoPor),
                    new SqlParameter("@FechaModificacion",m.FechaModificacion)
                });
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Modelo Eliminado Correctamente";
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