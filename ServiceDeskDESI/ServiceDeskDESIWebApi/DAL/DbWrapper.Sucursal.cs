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
        public ModelResponse ObtenerSucursales(long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var sucursales = GetObjects("ObtenerSucursales", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@EmpresaId", empresaId) },
                    new Func<IDataReader, Sucursal>((reader) =>
                    {
                        var sucursal = LlenarEntidad<Sucursal>(reader);
                        return sucursal;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursales;
                modelResponse.Message = "Sucursales obtenidas correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las sucursales";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerSucursalPorId(long id, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la sucursal es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var sucursal = GetObject("ObtenerSucursalPorId", CommandType.StoredProcedure,
                    new[] {
                new SqlParameter("@Id", id),
                new SqlParameter("@EmpresaId", empresaId)
                    },
                    new Func<IDataReader, Sucursal>((reader) =>
                    {
                        var s = LlenarEntidad<Sucursal>(reader);
                        return s;
                    }));

                if (sucursal == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la sucursal especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursal;
                modelResponse.Message = "Sucursal obtenida correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la sucursal";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarSucursal(Sucursal s, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validaciones
                if (string.IsNullOrWhiteSpace(s.Nombre)) { throw new ArgumentException("El nombre de la sucursal es requerido."); }
                if (s.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (s.Descripcion != null && s.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (s.Calle != null && s.Calle.Length > 100) { throw new ArgumentException("La calle no puede exceder los 100 caracteres."); }
                if (s.Ciudad != null && s.Ciudad.Length > 100) { throw new ArgumentException("La ciudad no puede exceder los 100 caracteres."); }
                if (s.Colonia != null && s.Colonia.Length > 100) { throw new ArgumentException("La colonia no puede exceder los 100 caracteres."); }
                if (s.CodigoPostal != null && s.CodigoPostal.Length > 10) { throw new ArgumentException("El código postal no puede exceder los 10 caracteres."); }
                if (string.IsNullOrWhiteSpace(s.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var parametrosObj = new
                {
                    s.Id,
                    s.Nombre,
                    s.Descripcion,
                    s.Calle,
                    s.Ciudad,
                    s.Colonia,
                    s.CodigoPostal,
                    s.CreadoPor,
                    s.FechaCreacion,
                    s.ModificadoPor,
                    s.FechaModificacion,
                    s.Estatus,
                    EmpresaId = empresaId
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var sucursalId = ExecuteScalar("GuardarOActualizarSucursal", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(sucursalId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                s.Id = Convert.ToInt64(sucursalId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = s;
                modelResponse.Message = "Sucursal guardada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la sucursal";
            }

            return modelResponse;
        }

        public ModelResponse EliminarSucursal(long id, string modificadoPor, DateTime fechaModificacion, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la sucursal es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (empresaId <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }

                var result = ExecuteScalar("EliminarSucursal", CommandType.StoredProcedure, new SqlParameter[]
                {
            new SqlParameter("@Id", id),
            new SqlParameter("@ModificadoPor", modificadoPor),
            new SqlParameter("@FechaModificacion", fechaModificacion),
            new SqlParameter("@EmpresaId", empresaId)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta sucursal.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Sucursal eliminada correctamente";
            }
            catch (ArgumentException ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la sucursal";
            }

            return modelResponse;
        }

    }
}