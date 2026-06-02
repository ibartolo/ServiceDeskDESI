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
        public ModelResponse ObtenerAreas(long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var areas = GetObjects("ObtenerAreas", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Area>((reader) =>
                    {
                        var area = LlenarEntidad<Area>(reader);
                        return area;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = areas;
                modelResponse.Message = "Áreas obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las áreas";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerAreaPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var area = GetObject("ObtenerAreaPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el área";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarArea(Area a, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(a.Nombre)) { throw new ArgumentException("El nombre del área es requerido."); }
                if (a.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (a.Descripcion != null && a.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (a.Correo != null && a.Correo.Length > 100) { throw new ArgumentException("El correo no puede exceder los 100 caracteres."); }
                if (string.IsNullOrWhiteSpace(a.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

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
                    EmpresaId = empresaId
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el área";
            }

            return modelResponse;
        }

        public ModelResponse EliminarArea(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = ExecuteScalar("EliminarArea", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
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
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el área";
            }

            return modelResponse;
        }
    }
}