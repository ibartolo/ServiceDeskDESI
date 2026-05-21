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
        public ModelResponse ObtenerTodasLasEmpresas ()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var empresa = GetObjects("ObtenerEmpresas", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
               new Func<IDataReader, Empresa>((reader) =>
               {
                   var empresas = LlenarEntidad<Empresa>(reader);
                   return empresas;
               }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = empresa;
                modelResponse.Message = "Empresas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }

        public ModelResponse GuardarOActualizarEmpresas(Empresa e)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(e).ToArray();
                var empresaId = ExecuteScalar("GuardarOActualizarEmpresa", CommandType.StoredProcedure, parametros);
                e.Id = Convert.ToInt64(empresaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = e;
                modelResponse.Message = "Empresas Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerEmpresaPorRFC(string rfc)
        {
            var modelResponse = new ModelResponse();
            try
            {
                modelResponse.IsSuccess = true;
                var parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter()
                {
                    Value = rfc,
                    IsNullable = true,
                    ParameterName = "@RFC",
                    SqlDbType = System.Data.SqlDbType.Int
                });

                var result = GetObject("ObtenerEmpresaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerEmpresasPorId(long id)
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

                var result = GetObject("ObtenerEmpresaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Empresa>((reader) =>
                    {
                        var r = LlenarEntidad<Empresa>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse EliminarEmpresa(long id, string modificadoPor, DateTime fechaModificacion)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarEmpresa", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Empresa Eliminado Correctamente";
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