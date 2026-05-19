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
        public ModelResponse ObtenerTodosLasSucursales()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var sucursales = GetObjects("ObtenerSucursales", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                new Func<IDataReader, Sucursal>((reader) =>
                 {
                     var sucursal = LlenarEntidad<Sucursal>(reader);
                     return sucursal;
                 }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursales;
                modelResponse.Message= "Sucursales obtenidos correctamente";
            }
            catch(Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }

        public ModelResponse GuardarOActualizarSucursales(Sucursal s)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(s).ToArray();
                var sucursalId = ExecuteScalar("GuardarOActualizarSucursal", CommandType.StoredProcedure, parametros);
                s.Id = Convert.ToInt64(sucursalId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = s;
                modelResponse.Message = "Sucursales Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerSucursalesPorId(long id)
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

                var result = GetObject("ObtenerSucursalPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Sucursal>((reader) =>
                    {
                        var r = LlenarEntidad<Sucursal>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Sucursal Obtenido Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse EliminarSucursales (Sucursal s)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarSucursal", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", s.Id),
                    new SqlParameter("@ModificadoPor", s.ModificadoPor),
                    new SqlParameter("@FechaModificacion",s.FechaModificacion)
                });

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Sucursal Eliminado Correctamente";
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