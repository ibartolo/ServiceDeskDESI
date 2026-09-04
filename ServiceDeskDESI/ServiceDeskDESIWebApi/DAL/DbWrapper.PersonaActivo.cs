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

                if (resultadoLong == -2)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La persona no tiene un usuario vinculado.";
                    return modelResponse;
                }

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

        public ModelResponse GenerarTokenConfirmacion(long personaActivoId, Guid token)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("GenerarTokenConfirmacion", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@PersonaActivoId", personaActivoId),
                    new SqlParameter("@TokenConfirmacion", token)
                });

                var rowCount = Convert.ToInt64(resultado);

                modelResponse.IsSuccess = (rowCount > 0);
                modelResponse.Response = rowCount;
                modelResponse.Message = modelResponse.IsSuccess
                    ? "Token de confirmación generado correctamente."
                    : "No se pudo generar el token de confirmación.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al generar token de confirmación para PersonaActivoId {PersonaActivoId}", personaActivoId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al generar el token de confirmación.";
            }

            return modelResponse;
        }

        public ModelResponse ConfirmarRecepcionActivo(Guid token, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("ConfirmarRecepcionActivo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@TokenConfirmacion", token),
                    new SqlParameter("@Usuario", usuario)
                });

                var estado = Convert.ToInt64(resultado);

                modelResponse.Response = estado;
                modelResponse.IsSuccess = (estado == 1 || estado == 2);
                modelResponse.Message = estado == 1
                    ? "Recepción confirmada correctamente."
                    : estado == 2
                        ? "La recepción de este activo ya fue confirmada anteriormente."
                        : estado == 3
                            ? "La asignación no corresponde a su usuario."
                            : "El enlace de confirmación no es válido o ha sido alterado.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al confirmar recepción de activo por token para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al confirmar la recepción del activo.";
            }

            return modelResponse;
        }

        public ModelResponse DesvincularActivoPersonaConfirmacion(Guid token, string usuario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("DesvincularActivoPersonaConfirmacion", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@TokenConfirmacion", token),
                    new SqlParameter("@Usuario", usuario)
                });

                var estado = Convert.ToInt64(resultado);

                modelResponse.Response = estado;
                modelResponse.IsSuccess = (estado == 1);
                modelResponse.Message = estado == 1
                    ? "Activo desvinculado correctamente."
                    : estado == 3
                        ? "La asignación no corresponde a su usuario."
                        : "El enlace de desvinculación no es válido o la asignación ya fue desvinculada.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al desvincular activo por token para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al desvincular el activo.";
            }

            return modelResponse;
        }

        public ModelResponse<AsignacionActivoDetalleDTO> ObtenerAsignacionPorToken(Guid token)
        {
            var modelResponse = new ModelResponse<AsignacionActivoDetalleDTO>();

            try
            {
                var detalle = GetObject("ObtenerAsignacionPorToken", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@TokenConfirmacion", token) },
                    new Func<IDataReader, AsignacionActivoDetalleDTO>((reader) =>
                    {
                        var d = LlenarEntidad<AsignacionActivoDetalleDTO>(reader);
                        return d;
                    }));

                if (detalle == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El enlace de asignación no es válido o ha sido alterado.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = detalle;
                modelResponse.Message = "Asignación obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener asignación por token");
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la asignación.";
            }

            return modelResponse;
        }

        public ModelResponse<long?> ObtenerPersonaIdPorUsuario(string usuario)
        {
            var modelResponse = new ModelResponse<long?>();

            try
            {
                var resultado = ExecuteScalar("ObtenerPersonaIdPorUsuario", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) });

                if (resultado == null || resultado is DBNull)
                {
                    modelResponse.IsSuccess = true;
                    modelResponse.Response = null;
                    modelResponse.Message = "El usuario no tiene persona vinculada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = Convert.ToInt64(resultado);
                modelResponse.Message = "PersonaId obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener PersonaId para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la persona vinculada.";
            }

            return modelResponse;
        }

        public ModelResponse<PersonaActivo> ObtenerPersonaActivoPorId(long personaActivoId)
        {
            var modelResponse = new ModelResponse<PersonaActivo>();

            try
            {
                var pa = GetObject("ObtenerPersonaActivoPorId", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Id", personaActivoId) },
                    new Func<IDataReader, PersonaActivo>((reader) =>
                    {
                        var e = LlenarEntidad<PersonaActivo>(reader);
                        return e;
                    }));

                if (pa == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se encontró la asignación especificada.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = pa;
                modelResponse.Message = "Asignación obtenida correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener asignación {PersonaActivoId}", personaActivoId);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener la asignación.";
            }

            return modelResponse;
        }

        public ModelResponse RegistrarBitacoraCorreo(string tipo, string destinatario, string asunto, string estado, string error, long? referenciaId)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("RegistrarBitacoraCorreo", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@TipoCorreo", tipo),
                    new SqlParameter("@Destinatario", destinatario ?? string.Empty),
                    new SqlParameter("@Asunto", asunto),
                    new SqlParameter("@Estado", estado),
                    new SqlParameter("@Error", (object)error ?? DBNull.Value),
                    new SqlParameter("@ReferenciaId", (object)referenciaId ?? DBNull.Value)
                });

                var bitacoraId = Convert.ToInt64(resultado);

                modelResponse.IsSuccess = (bitacoraId > 0);
                modelResponse.Response = bitacoraId;
                modelResponse.Message = modelResponse.IsSuccess
                    ? "Bitácora de correo registrada correctamente."
                    : "No se pudo registrar la bitácora de correo.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al registrar bitácora de correo de tipo {TipoCorreo}", tipo);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al registrar la bitácora de correo.";
            }

            return modelResponse;
        }
    }
}
