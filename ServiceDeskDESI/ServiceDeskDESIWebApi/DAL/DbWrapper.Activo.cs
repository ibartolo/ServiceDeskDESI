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
        public ModelResponse ObtenerTodosLosActivos(long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var activos = GetObjects("ObtenerActivos", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Activo>((reader) =>
                    {
                        var activo = LlenarEntidad<Activo>(reader);

                        activo.TipoActivo = new TipoActivo()
                        {
                            Id = MapearPorpiedades<long>(reader["TipoActivoId"]),
                            Nombre = MapearPorpiedades<string>(reader["TipoActivoNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["TipoActivoDescripcion"])
                        };

                        activo.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaId"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        activo.Modelo = new Modelo()
                        {
                            Id = MapearPorpiedades<long>(reader["ModeloId"]),
                            Nombre = MapearPorpiedades<string>(reader["ModeloNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["ModeloDescripcion"])
                        };

                        return activo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = activos;
                modelResponse.Message = "Activos obtenidos correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las activos";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarActivo(Activo a)
        {
            var modelResponse = new ModelResponse();
            try
            {
                // Validaciones

                var parametros = ObtenerParametrosSQL(a).ToArray();
                var activoId = ExecuteScalar("GuardarOActualizarActivo", CommandType.StoredProcedure, parametros);
                a.Id = Convert.ToInt64(activoId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = a;
                modelResponse.Message = "Activos Guardados Correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public ModelResponse ObtenerActivoPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var activo = GetObject("ObtenerActivoPorId", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", id),
                     new SqlParameter("@EmpresaId", empresaId)
                    },

                    new Func<IDataReader, Activo>((reader) =>
                    {
                        var a = LlenarEntidad<Activo>(reader);

                        a.TipoActivo = new TipoActivo()
                        {
                            Id = MapearPorpiedades<long>(reader["TipoActivoId"]),
                            Nombre = MapearPorpiedades<string>(reader["TipoActivoNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["TipoActivoDescripcion"])
                        };

                        a.Marca = new Marca()
                        {
                            Id = MapearPorpiedades<long>(reader["MarcaId"]),
                            Nombre = MapearPorpiedades<string>(reader["MarcaNombre"]),
                            Descripcion = MapearPorpiedades<string>(reader["MarcaDescripcion"])
                        };

                        a.Modelo = new Modelo()
                        {
                            Id = MapearPorpiedades<long>(reader["ModeloId"]),
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el activo";
            }

            return modelResponse;
        }

        public ModelResponse EliminarActivo(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del Modelo es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El activo modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result =  ExecuteNonQuery("EliminarActivo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion)
                });
                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este Activo.";
                    return modelResponse;
                }

                  modelResponse.IsSuccess = true;
                modelResponse.Message = "Activo eliminado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la Activo";
            }
            return modelResponse;
        }
    }
}