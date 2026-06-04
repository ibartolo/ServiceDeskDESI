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
        public ModelResponse ObtenerTodasLasMarcas(long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var marcas = GetObjects("ObtenerMarca", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var marca = LlenarEntidad<Marca>(reader);
                        return marca;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = marcas;
                modelResponse.Message = "Marcas Obtenidos Correctamente";

            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las Marcas";
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
        public ModelResponse ObtenerMarcasPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la Marca es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var marca = GetObject("ObtenerMarcaPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var a = LlenarEntidad<Marca>(reader);
                        return a;
                    }));
                if (marca == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la Marca especificada.";
                    return modelResponse;
                }
                modelResponse.IsSuccess = true;
                modelResponse.Response = marca;
                modelResponse.Message = "Marca obtenida correctamente";
            }

            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener Marca";
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