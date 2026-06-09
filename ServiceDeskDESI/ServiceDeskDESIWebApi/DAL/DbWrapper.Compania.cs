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
        public ModelResponse ObtenerCompania(long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var companias = GetObjects("ObtenerCompanias", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Compania>((reader) =>
                    {
                        var compania = LlenarEntidad<Compania>(reader);
                        return compania;
                    }));
                modelResponse.IsSuccess = true;
                modelResponse.Response = companias;
                modelResponse.Message = "Companias obtenidas correctamente";

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

        public ModelResponse GuardarOActualizarCompania(Compania c, long empresaId)
        {
            var modelResponse = new ModelResponse();
            try
            {
                // validaciones
                if (string.IsNullOrWhiteSpace(c.Nombre)) { throw new ArgumentException("El nombre del área es requerido."); }
                if (c.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (c.Acronimo != null && c.Acronimo.Length > 500) { throw new ArgumentException("El Acronimo no puede exceder los 500 caracteres."); }
                if (c.RFC != null && c.RFC.Length > 100) { throw new ArgumentException("El RFC no puede exceder los 100 caracteres."); }
                if (c.Direccion != null && c.Direccion.Length > 100) { throw new ArgumentException("La Direccion no puede exceder los 100 caracteres."); }
                if (string.IsNullOrWhiteSpace(c.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var parametrosObj = new
                {
                    c.Id,
                    c.Nombre,
                    c.Acronimo,
                    c.RFC,
                    c.Direccion,
                    c.CreadoPor,
                    c.FechaCreacion,
                    c.ModificadoPor,
                    c.FechaModificacion,
                    c.Estatus,
                    EmpresaId = empresaId

                };
               var parametros = ObtenerParametrosSQL(c).ToArray();
                var companiaId = ExecuteScalar("GuardarOActualizarCompania", CommandType.StoredProcedure, parametros);
                if (Convert.ToInt64(companiaId)== 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }
                c.Id = Convert.ToInt64(companiaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = c;
                modelResponse.Message = "Compania Guardado correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el Compania";
            }
            return modelResponse;
        }
        public ModelResponse ObtenerCompaniaPorId (long id, long empresaId)
        {
            var modelResponse = new ModelResponse();
           try
           {
                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                var result = GetObject("ObtenerCompaniaPorId", CommandType.StoredProcedure,
                      new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },

                    new Func<IDataReader, Compania>((reader) =>
                    {
                        var r = LlenarEntidad<Compania>(reader);
                        return r;
                    }));
                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la Compania Especificada.";
                    return modelResponse;
                }
                modelResponse.Response = result;
                modelResponse.IsSuccess = true;
                modelResponse.Message = "Compania obtenido correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener Compania";
            }

            return modelResponse;

        }
        public ModelResponse EliminarCompania(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)

        {
            var modelResponse = new ModelResponse();
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la Compania es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

              var result=ExecuteScalar  ("EliminarCompania", CommandType.StoredProcedure, new SqlParameter[]
                
              {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
              });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta Compania.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Compania eliminada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la Compània";
            }

            return modelResponse;
        }

    }

}
