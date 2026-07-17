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
        public ModelResponse ObtenerAreas(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(usuario))
                {
                    modelResponse.IsSuccess = true;
                    modelResponse.Response = new List<Area>();
                    modelResponse.Message = "No hay usuario autenticado";
                    return modelResponse;
                }

                var areas = GetObjects("ObtenerAreas", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Area>((reader) =>
                    {
                        var area = LlenarEntidad<Area>(reader);
                        return area;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = areas;
                modelResponse.Message = "Áreas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener áreas para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las áreas";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerAreaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El ID del área es requerido.";
                    return modelResponse;
                }

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre de usuario es requerido.";
                    return modelResponse;
                }

                var area = GetObject("ObtenerAreaPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Area>((reader) =>
                    {
                        var a = LlenarEntidad<Area>(reader);
                        return a;
                    }));

                if (area == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el área especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = area;
                modelResponse.Message = "Área obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener área {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el área";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarArea(Area a)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Solo validaciones de datos nulos (reglas de base de datos)
                if (string.IsNullOrWhiteSpace(a.Nombre))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre del área es requerido.";
                    return modelResponse;
                }

                if (a.Nombre.Length > 250)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre no puede exceder los 250 caracteres.";
                    return modelResponse;
                }

                if (a.Descripcion != null && a.Descripcion.Length > 500)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La descripción no puede exceder los 500 caracteres.";
                    return modelResponse;
                }

                if (a.Correo != null && a.Correo.Length > 100)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El correo no puede exceder los 100 caracteres.";
                    return modelResponse;
                }

                if (string.IsNullOrWhiteSpace(a.CreadoPor))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario creador es requerido.";
                    return modelResponse;
                }

                var parametrosObj = new
                {
                    a.Id,
                    a.Nombre,
                    a.Descripcion,
                    a.Correo,
                    a.CreadoPor,
                    a.FechaCreacion,
                    a.ModificadoPor,
                    a.FechaModificacion,
                    a.Estatus,
                    Usuario = a.CreadoPor
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var areaId = ExecuteScalar("GuardarOActualizarArea", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(areaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                a.Id = Convert.ToInt64(areaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = a;
                modelResponse.Message = "Área guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar área");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el área";
            }

            return modelResponse;
        }

        public ModelResponse EliminarArea(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El ID del área es requerido.";
                    return modelResponse;
                }

                if (string.IsNullOrWhiteSpace(modificadoPor))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario modificador es requerido.";
                    return modelResponse;
                }

                if (string.IsNullOrWhiteSpace(usuario))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre de usuario es requerido.";
                    return modelResponse;
                }

                var result = ExecuteScalar("EliminarArea", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta área.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Área eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar área {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el área";
            }

            return modelResponse;
        }

        public ModelResponse GuardarNuevaAreaParaEmpresa(Area area)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (string.IsNullOrWhiteSpace(area.Nombre))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre del área es requerido.";
                    return modelResponse;
                }

                if (area.Nombre.Length > 250)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre no puede exceder los 250 caracteres.";
                    return modelResponse;
                }

                if (area.Descripcion != null && area.Descripcion.Length > 500)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La descripción no puede exceder los 500 caracteres.";
                    return modelResponse;
                }

                if (area.Correo != null && area.Correo.Length > 100)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El correo no puede exceder los 100 caracteres.";
                    return modelResponse;
                }

                if (string.IsNullOrWhiteSpace(area.CreadoPor))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El usuario creador es requerido.";
                    return modelResponse;
                }

                var parametrosObj = new
                {
                    area.Nombre,
                    area.Descripcion,
                    area.Correo,
                    area.CreadoPor,
                    area.FechaCreacion
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var areaId = ExecuteScalar("GuardarNuevaAreaParaEmpresa", CommandType.StoredProcedure, parametros);
                area.Id = Convert.ToInt64(areaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = area;
                modelResponse.Message = "Área creada exitosamente para la nueva empresa.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear área para nueva empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al crear el área para la empresa.";
            }

            return modelResponse;
        }
    }
}