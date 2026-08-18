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
        public ModelResponse ObtenerSucursales(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var sucursales = GetObjects("ObtenerSucursales", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Sucursal>((reader) =>
                    {
                        var sucursal = LlenarEntidad<Sucursal>(reader);
                        return sucursal;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursales;
                modelResponse.Message = "Sucursales obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener sucursales para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las sucursales";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerSucursalPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var sucursal = GetObject("ObtenerSucursalPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener sucursal {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la sucursal";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarSucursal(Sucursal s, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
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
                    Usuario = usuario
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar sucursal para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la sucursal";
            }

            return modelResponse;
        }

        public ModelResponse EliminarSucursal(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarSucursal", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
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
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar sucursal {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la sucursal";
            }

            return modelResponse;
        }

        public ModelResponse GuardarNuevaSucursalParaEmpresa(Sucursal sucursal, long empresaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    sucursal.Nombre,
                    sucursal.Descripcion,
                    sucursal.Calle,
                    sucursal.Ciudad,
                    sucursal.Colonia,
                    sucursal.CodigoPostal,
                    sucursal.CreadoPor,
                    sucursal.FechaCreacion,
                    EmpresaId = empresaId
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var sucursalId = ExecuteScalar("GuardarNuevaSucursalParaEmpresa", CommandType.StoredProcedure, parametros);
                sucursal.Id = Convert.ToInt64(sucursalId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursal;
                modelResponse.Message = "Sucursal creada exitosamente para la nueva empresa.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear sucursal para nueva empresa");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al crear la sucursal para la empresa.";
            }

            return modelResponse;
        }
    }
}