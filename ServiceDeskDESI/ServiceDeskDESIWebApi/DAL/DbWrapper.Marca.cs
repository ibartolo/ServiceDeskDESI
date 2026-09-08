using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse<List<Marca>> ObtenerMarcas(string usuario)
        {
            var modelResponse = new ModelResponse<List<Marca>>();

            try
            {
                var marcas = GetObjects("ObtenerMarca", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var marca = LlenarEntidad<Marca>(reader);
                        return marca;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = marcas.ToList();
                modelResponse.Message = "Marcas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener marcas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las marcas";
            }

            return modelResponse;
        }

        public ModelResponse<Marca> ObtenerMarcaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<Marca>();

            try
            {
                var marca = GetObject("ObtenerMarcaPorId", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Marca>((reader) =>
                    {
                        var m = LlenarEntidad<Marca>(reader);
                        return m;
                    }));

                if (marca == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la marca especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = marca;
                modelResponse.Message = "Marca obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener marca {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la marca";
            }

            return modelResponse;
        }

        public ModelResponse<Marca> GuardarOActualizarMarca(Marca m)
        {
            var modelResponse = new ModelResponse<Marca>();

            try
            {
                var parametrosObj = new
                {
                    m.Id,
                    m.Nombre,
                    m.Descripcion,
                    m.CreadoPor,
                    m.FechaCreacion,
                    m.ModificadoPor,
                    m.FechaModificacion,
                    m.Estatus,
                    Usuario = m.CreadoPor
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var marcaId = ExecuteScalar("GuardarOActualizarMarca", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(marcaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                m.Id = Convert.ToInt64(marcaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Marca guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar marca");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la marca";
            }

            return modelResponse;
        }

        public ModelResponse EliminarMarca(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarMarca", CommandType.StoredProcedure, new SqlParameter[]
                {
                new SqlParameter("@Id", id),
                new SqlParameter("@ModificadoPor", modificadoPor),
                new SqlParameter("@FechaModificacion", fechaModificacion),
                new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta marca.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Marca eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar marca {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la marca";
            }

            return modelResponse;
        }
    }
}
