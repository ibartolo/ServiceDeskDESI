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
        public ModelResponse ObtenerTodosLosPuestos(string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var puestos = GetObjects("ObtenerPuesto", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Puesto>((reader) =>
                    {
                        var puesto = LlenarEntidad<Puesto>(reader);
                        return puesto;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = puestos;
                modelResponse.Message = "Puestos obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener puestos para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los puestos";
            }

            return modelResponse;
        }

        public ModelResponse ObtenerPuestoPorId(long id, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var puesto = GetObject("ObtenerPuestoPorId", CommandType.StoredProcedure,
                    new[] {
                    new SqlParameter("@Id", id),
                    new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Puesto>((reader) =>
                    {
                        var p = LlenarEntidad<Puesto>(reader);
                        return p;
                    }));

                if (puesto == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró el puesto especificado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = puesto;
                modelResponse.Message = "Puesto obtenido correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener puesto {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el puesto";
            }

            return modelResponse;
        }

        public ModelResponse GuardarOActualizarPuesto(Puesto p, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var parametrosObj = new
                {
                    p.Id,
                    p.Nombre,
                    p.Descripcion,
                    p.CreadoPor,
                    p.FechaCreacion,
                    p.ModificadoPor,
                    p.FechaModificacion,
                    p.Estatus,
                    Usuario = usuario
                };

                var parametros = ObtenerParametrosSQL(parametrosObj).ToArray();
                var puestoId = ExecuteScalar("GuardarOActualizarPuesto", CommandType.StoredProcedure, parametros);

                if (Convert.ToInt64(puestoId) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                p.Id = Convert.ToInt64(puestoId);

                modelResponse.IsSuccess = true;
                modelResponse.Response = p;
                modelResponse.Message = "Puesto guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar puesto");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el puesto";
            }

            return modelResponse;
        }

        public ModelResponse EliminarPuesto(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var result = ExecuteScalar("EliminarPuesto", CommandType.StoredProcedure, new SqlParameter[]
                {
                new SqlParameter("@Id", id),
                new SqlParameter("@ModificadoPor", modificadoPor),
                new SqlParameter("@FechaModificacion", fechaModificacion),
                new SqlParameter("@Usuario", usuario)
                });

                if (Convert.ToInt64(result) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para eliminar este puesto.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Puesto eliminado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al eliminar puesto {Id} para usuario {Usuario}", id, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al eliminar el puesto";
            }

            return modelResponse;
        }
    }
}