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
        public ModelResponse<List<PersonaActivoDTO>> ObtenerActivosPorPersona(long personaId, string usuario)
        {
            var modelResponse = new ModelResponse<List<PersonaActivoDTO>>();

            try
            {
                var activos = GetObjects("ObtenerActivosPorPersona", CommandType.StoredProcedure,
                    new[] {
                        new SqlParameter("@PersonaId", personaId),
                        new SqlParameter("@Usuario", usuario)
                    },
                    new Func<IDataReader, PersonaActivoDTO>((reader) =>
                    {
                        var pa = LlenarEntidad<PersonaActivoDTO>(reader);
                        return pa;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = activos.ToList();
                modelResponse.Message = "Activos de la persona obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener activos de la persona {PersonaId} para usuario {Usuario}", personaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los activos de la persona";
            }

            return modelResponse;
        }

        public ModelResponse<List<Activo>> ObtenerActivosDisponibles(string usuario)
        {
            var modelResponse = new ModelResponse<List<Activo>>();

            try
            {
                var activos = GetObjects("ObtenerActivosDisponibles", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, Activo>((reader) =>
                    {
                        var a = LlenarEntidad<Activo>(reader);
                        return a;
                    }));

                modelResponse.IsSuccess = true;
                modelResponse.Response = activos.ToList();
                modelResponse.Message = "Activos disponibles obtenidos correctamente";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener activos disponibles para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener los activos disponibles";
            }

            return modelResponse;
        }

        public ModelResponse AsignarActivoPersona(long personaId, long activoId, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("AsignarActivoPersona", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@PersonaId", personaId),
                    new SqlParameter("@ActivoId", activoId),
                    new SqlParameter("@Usuario", usuario)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong <= -1)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El activo ya está asignado a otra persona.";
                    return modelResponse;
                }

                if (resultadoLong <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo asignar el activo.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Activo asignado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al asignar activo {ActivoId} a persona {PersonaId} para usuario {Usuario}", activoId, personaId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al asignar el activo.";
            }

            return modelResponse;
        }

        public ModelResponse DesvincularActivoPersona(long personaActivoId, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("DesvincularActivoPersona", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@PersonaActivoId", personaActivoId),
                    new SqlParameter("@Usuario", usuario)
                });

                var resultadoLong = Convert.ToInt64(resultado);

                if (resultadoLong <= 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo desvincular el activo.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultadoLong;
                modelResponse.Message = "Activo desvinculado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al desvincular activo {PersonaActivoId} para usuario {Usuario}", personaActivoId, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al desvincular el activo.";
            }

            return modelResponse;
        }
    }
}
