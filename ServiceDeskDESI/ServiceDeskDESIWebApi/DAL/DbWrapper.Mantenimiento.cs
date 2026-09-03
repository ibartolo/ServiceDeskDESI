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
        public ModelResponse GuardarMantenimiento(Mantenimiento m, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("GuardarMantenimiento", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@ActivoId", m.ActivoId),
                    new SqlParameter("@Comentario", m.Comentario),
                    new SqlParameter("@CreadoPor", m.CreadoPor),
                    new SqlParameter("@FechaCreacion", m.FechaCreacion),
                    new SqlParameter("@Usuario", usuario)
                });

                var mantenimientoId = Convert.ToInt64(resultado);

                if (mantenimientoId == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No tiene permisos para realizar esta operación.";
                    return modelResponse;
                }

                m.Id = mantenimientoId;

                modelResponse.IsSuccess = true;
                modelResponse.Response = mantenimientoId;
                modelResponse.Message = "Mantenimiento guardado correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar mantenimiento del activo {ActivoId} para usuario {Usuario}", m.ActivoId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el mantenimiento";
            }

            return modelResponse;
        }

        public ModelResponse<List<Mantenimiento>> ObtenerMantenimientosPorActivo(long activoId, string usuario)
        {
            var modelResponse = new ModelResponse<List<Mantenimiento>>();

            try
            {
                var mantenimientos = GetObjects("ObtenerMantenimientosPorActivo", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@ActivoId", activoId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, Mantenimiento>((reader) =>
                    {
                        var m = LlenarEntidad<Mantenimiento>(reader);
                        return m;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = mantenimientos.ToList();
                modelResponse.Message = "Mantenimientos del activo obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener mantenimientos del activo {ActivoId} para usuario {Usuario}", activoId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los mantenimientos";
            }

            return modelResponse;
        }
    }
}
