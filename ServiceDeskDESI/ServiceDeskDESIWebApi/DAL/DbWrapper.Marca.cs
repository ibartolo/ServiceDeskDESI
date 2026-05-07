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
        public ModelResponse ObtenerTodasLasMarcas()
        {
            var modelResponse = new ModelResponse();
            try
            {
                var marcas = GetObjects("ObtenerMarca", CommandType.StoredProcedure, Enumerable.Empty<SqlParameter>(),
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var marca = LlenarEntidad<Marca>(reader);
                        return marca;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = marcas;
                modelResponse.Message = "Marcas Obtenidos Correctamente";

            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }
            return modelResponse;
        }
        public ModelResponse GuardarOActualizarMarca (Marca m)
        {
            var modelResponse = new ModelResponse();
            try
            {
                var parametros = ObtenerParametrosSQL(m).ToArray();
                var marcaId = ExecuteScalar("GuardarOActualizarMarca", CommandType.StoredProcedure, parametros);
                m.Id = Convert.ToInt64(marcaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Marcas Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse ObtenerMarcasPorId(long id)
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

                var result = GetObject("ObtenerMarcaPorId", System.Data.CommandType.StoredProcedure,
                    parameters, new Func<System.Data.IDataReader, Marca>((reader) =>
                    {
                        var r = LlenarEntidad<Marca>(reader);
                        return r;
                    }));
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca Obtenida Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
        public ModelResponse EliminarMarca (Marca m)
        {
            var modelResponse = new ModelResponse();
            try
            {
                ExecuteNonQuery("EliminarMarca", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", m.Id),
                    new SqlParameter("@ModificadoPor", m.ModificadoPor),
                    new SqlParameter("@FechaModificacion",m.FechaModificacion)
                });
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca Eliminado Correctamente";
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