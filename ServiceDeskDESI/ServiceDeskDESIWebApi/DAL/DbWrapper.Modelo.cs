using Serilog;
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
    //MarcaId = m.Marca.Id,
    public partial class DbWrapper
    {
        public ModelResponse ObtenerModelos(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var modelos = GetObjects("ObtenerModelo", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Modelo>((reader) =>
                    {
                        var modelo = LlenarEntidad<Modelo>(reader);
                        return modelo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = modelos;
                modelResponse.Message = "Modelos obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener modelos para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los modelos";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerModeloPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var modelo = GetObject("ObtenerModeloPorId", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Modelo>((reader) =>
                    {
                        var m = LlenarEntidad<Modelo>(reader);
                        return m;
                    }));

                if (modelo == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el modelo especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = modelo;
                modelResponse.Message = "Modelo obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener modelo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el modelo";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarModelo(Modelo m)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    m.Id,
                    m.Nombre,
                    m.Descripcion,
                    MarcaId = m.Marca.Id,
                    m.CreadoPor,
                    m.FechaCreacion,
                    m.ModificadoPor,
                    m.FechaModificacion,
                    m.Estatus,
                    Usuario = m.CreadoPor
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var modeloId = ExecuteScalar("GuardarOActualizarModelo", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(modeloId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                m.Id = Convert.ToInt64(modeloId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = m;
                modelResponse.Message = "Modelo guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar modelo");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el modelo";
            }

            return modelResponse;
        }

        public ModelResponse EliminarModelo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarModelo", CommandType.StoredProcedure, new SqlParameter[]
                {
                new SqlParameter("@Id", id),
                new SqlParameter("@ModificadoPor", modificadoPor),
                new SqlParameter("@FechaModificacion", fechaModificacion),
                new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este modelo.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Modelo eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar modelo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el modelo";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerModelosPorMarcaId(long marcaId, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var modelos = GetObjects("ObtenerModelosPorMarcaId", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@MarcaId", marcaId),
                    new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Modelo>((reader) =>
                    {
                        var modelo = LlenarEntidad<Modelo>(reader);
                        return modelo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = modelos;
                modelResponse.Message = "Modelos obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener modelos por marca {MarcaId} para usuario {Usuario}", marcaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los modelos";
            }

            return modelResponse;
        }
    }
}