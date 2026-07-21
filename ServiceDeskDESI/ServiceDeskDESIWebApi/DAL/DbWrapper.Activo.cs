using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ServiceDeskDESIWebApi.DAL
{
    public partial class DbWrapper
    {
        public ModelResponse ObtenerTodosLosActivos(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var activos = GetObjects("ObtenerActivos", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Activo>((reader) =>
                    {
                        var activo = LlenarEntidad<Activo>(reader);

                        activo.TipoActivo = new TipoActivo()
                        {
                            Id = MapearPorpiedades<long>(reader["TipoActivoID"]),
                            Nombre = MapearPorpiedades<string>(reader["TipoActivoNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["TipoActivoDescripcion"])
                        };

                        activo.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaID"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        activo.Modelo = new Modelo()
                        {
                            Id = MapearPorpiedades<long>(reader["ModeloID"]),
                            Nombre = MapearPorpiedades<string>(reader["ModeloNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["ModeloDescripcion"])
                        };

                        return activo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = activos;
                modelResponse.Message = "Activos obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener activos para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los activos";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerActivoPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var activo = GetObject("ObtenerActivoPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Activo>((reader) =>
                    {
                        var a = LlenarEntidad<Activo>(reader);

                        a.TipoActivo = new TipoActivo()
                        {
                            Id = MapearPorpiedades<long>(reader["TipoActivoID"]),
                            Nombre = MapearPorpiedades<string>(reader["TipoActivoNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["TipoActivoDescripcion"])
                        };

                        a.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaID"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        a.Modelo = new Modelo()
                        {
                            Id = MapearPorpiedades<long>(reader["ModeloID"]),
                            Nombre = MapearPorpiedades<string>(reader["ModeloNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["ModeloDescripcion"])
                        };

                        return a;
                    }));

                if (activo == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el activo especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = activo;
                modelResponse.Message = "Activo obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener activo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el activo";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarActivo(Activo a, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    a.Id,
                    a.Nombre,
                    a.Descripcion,
                    TipoActivoID = a.TipoActivo.Id,
                    a.Serial,
                    MarcaID = a.Marca.Id,
                    ModeloID = a.Modelo.Id,
                    a.Notas,
                    a.FechaCompra,
                    a.CreadoPor,
                    a.FechaCreacion,
                    a.ModificadoPor,
                    a.FechaModificacion,
                    a.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var activoId = ExecuteScalar("GuardarOActualizarActivo", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(activoId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                a.Id = Convert.ToInt64(activoId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = a;
                modelResponse.Message = "Activo guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar activo para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el activo";
            }

            return modelResponse;
        }

        public ModelResponse EliminarActivo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarActivo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este activo.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Activo eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar activo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el activo";
            }

            return modelResponse;
        }
    }
}