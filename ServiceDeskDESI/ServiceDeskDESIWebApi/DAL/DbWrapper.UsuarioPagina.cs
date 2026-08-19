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
        public ModelResponse<List<UsuarioPagina>> ObtenerUsuarioPagina(string usuario)
        {
            var modelResponse = new ModelResponse<List<UsuarioPagina>>();
            try
            {
                var usuarioResp = GetObjects("ObtenerUsuarioPagina", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                  new Func<IDataReader, UsuarioPagina>((reader) =>
                  {
                      var usuarioPagina = LlenarEntidad<UsuarioPagina>(reader);
                      return usuarioPagina;
                  }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = usuarioResp.ToList();
                modelResponse.Message = "UsuarioPagina obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }
        public ModelResponse<UsuarioPagina> GuardarOActualizarUsuarioPagina(UsuarioPagina u)
        {
            var modelResponse = new ModelResponse<UsuarioPagina>();
            try
            {
                var parametros = ObtenerParametrosSQL(u).ToArray();
                var usuarioP = ExecuteScalar("GuardarOActualizarUsuarioPagina", CommandType.StoredProcedure, parametros);
                u.Id = Convert.ToInt64(usuarioP);

                modelResponse.IsSuccess = true;
                modelResponse.Response = u;
                modelResponse.Message = "UsuarioPagina Guardado correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse<UsuarioPagina> ObtenerUsuarioPaginaPorId(long id, string usuario)
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
                parameters.Add(new SqlParameter("@Usuario", usuario));

                var result = GetObject("ObtenerUsuarioPaginaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, UsuarioPagina>((reader) =>
                    {
                        var r = LlenarEntidad<UsuarioPagina>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "ObtenerUsuarioPaginaPorId obtenido correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse EliminarUsuarioPagina(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarUsuarioPagina", CommandType.StoredProcedure, new SqlParameter[]
                {
                     new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "UsuarioPagina eliminado correctamente";
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