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
    public partial class DbWrapper
    {
        public ModelResponse ObtenerTodosLosTipoActivos(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                //if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var tipoActivos = GetObjects("ObtenerTipoActivo", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, TipoActivo>((reader) =>
                    {
                        var tipoActivo = LlenarEntidad<TipoActivo>(reader);
                        return tipoActivo;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = tipoActivos;
                modelResponse.Message = "Tipos de activo obtenidos correctamente";
            }
            //catch (ArgumentException ex)
            //{
            //    modelResponse.IsSuccess = false;
            //    modelResponse.Message = ex.Message;
            //}
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tipos de activo para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los tipos de activo";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerTipoActivoPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                //if (id <= 0) { throw new ArgumentException("El ID del tipo de activo es requerido."); }
                //if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var tipoActivo = GetObject("ObtenerTipoActivoPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, TipoActivo>((reader) =>
                    {
                        var ta = LlenarEntidad<TipoActivo>(reader);
                        return ta;
                    }));

                if (tipoActivo == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el tipo de activo especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = tipoActivo;
                modelResponse.Message = "Tipo de activo obtenido correctamente";
            }
            //catch (ArgumentException ex)
            //{
            //    modelResponse.IsSuccess = false;
            //    modelResponse.Message = ex.Message;
            //}
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener tipo de activo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el tipo de activo";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarTipoActivo(TipoActivo ta)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                //if (string.IsNullOrWhiteSpace(ta.Nombre)) { throw new ArgumentException("El nombre del tipo de activo es requerido."); }
                //if (ta.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                //if (ta.Descripcion != null && ta.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                //if (string.IsNullOrWhiteSpace(ta.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                //if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var parametrosObj = new
                {
                    ta.Id,
                    ta.Nombre,
                    ta.Descripcion,
                    ta.CreadoPor,
                    ta.FechaCreacion,
                    ta.ModificadoPor,
                    ta.FechaModificacion,
                    ta.Estatus,
                    Usuario = ta.CreadoPor
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var tipoActivoId = ExecuteScalar("GuardarOActualizarTipoActivo", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(tipoActivoId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                ta.Id = Convert.ToInt64(tipoActivoId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = ta;
                modelResponse.Message = "Tipo de activo guardado correctamente";
            }
            //catch (ArgumentException ex)
            //{
            //    modelResponse.IsSuccess = false;
            //    modelResponse.Message = ex.Message;
            //}
            //catch (Exception ex)
            //{
            //    Log.Error(ex, "Error al guardar tipo de activo para usuario {Usuario}", usuario);
            //    modelResponse.IsSuccess = false;
            //    modelResponse.Message = "Ocurrió un error al guardar el tipo de activo";
            //}
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar área");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el área";
            }


            return modelResponse;
        }

        public ModelResponse EliminarTipoActivo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                //if (id <= 0) { throw new ArgumentException("El ID del tipo de activo es requerido."); }
                //if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                //if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = ExecuteScalar("EliminarTipoActivo", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este tipo de activo.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Tipo de activo eliminado correctamente";
            }
            //catch (ArgumentException ex)
            //{
            //    modelResponse.IsSuccess = false;
            //    modelResponse.Message = ex.Message;
            //}
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar tipo de activo {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el tipo de activo";
            }

            return modelResponse;
        }
    }
}