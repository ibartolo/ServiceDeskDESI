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
        public ModelResponse<List<Compania>> ObtenerCompanias(string usuario)
        {
            var modelResponse = new ModelResponse<List<Compania>>();

            try
            {
                var companias = GetObjects("ObtenerCompanias", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Compania>((reader) =>
                    {
                        var compania = LlenarEntidad<Compania>(reader);
                        return compania;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = companias.ToList();
                modelResponse.Message = "Compañías obtenidas correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener compañías para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener las compañías";
            }

            return modelResponse;
        }

        public ModelResponse<Compania> ObtenerCompaniaPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse<Compania>();

            try
            {
                var result = GetObject("ObtenerCompaniaPorId", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@Id", id),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Compania>((reader) =>
                    {
                        var r = LlenarEntidad<Compania>(reader);
                        return r;
                    }));

                if (result == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la compañía especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = result;
                modelResponse.Message = "Compañía obtenida correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener compañía {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la compañía";
            }

            return modelResponse;
        }

        public ModelResponse<Compania> GuardarOActualizarCompania(Compania c, string usuario)
        {
            var modelResponse = new ModelResponse<Compania>();

            try
            {
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
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var companiaId = ExecuteScalar("GuardarOActualizarCompania", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(companiaId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                c.Id = Convert.ToInt64(companiaId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = c;
                modelResponse.Message = "Compañía guardada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar compañía");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar la compañía";
            }

            return modelResponse;
        }

        public ModelResponse EliminarCompania(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarCompania", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@ModificadoPor", modificadoPor),
                    new SqlParameter("@FechaModificacion", fechaModificacion),
                    new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar esta compañía.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Compañía eliminada correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar compañía {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar la compañía";
            }

            return modelResponse;
        }
    }
}