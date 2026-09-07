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
        public ModelResponse<List<HorarioLaboral>> ObtenerHorarioLaboral(string usuario)
        {
            var modelResponse = new ModelResponse<List<HorarioLaboral>>();

            try
            {
                var horario = GetObjects("ObtenerHorarioLaboral", CommandType.StoredProcedure,
                    new[] { new SqlParameter("@Usuario", usuario) },
                    new Func<IDataReader, HorarioLaboral>((reader) => LlenarEntidad<HorarioLaboral>(reader)));

                modelResponse.IsSuccess = true;
                modelResponse.Response = horario.ToList();
                modelResponse.Message = "Horario laboral obtenido correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener horario laboral para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al obtener el horario laboral.";
            }

            return modelResponse;
        }

        public ModelResponse GuardarHorarioLaboral(string usuario, List<HorarioLaboral> horario)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Transacción sobre una única conexión (evita escalación a MSDTC).
                BeginTransaction();

                if (horario != null)
                {
                    foreach (var dia in horario)
                    {
                        var resultadoDia = GuardarHorarioLaboralDia(usuario, dia.DiaSemana, dia.HoraInicio, dia.HoraFin, dia.Estatus);
                        if (!resultadoDia.IsSuccess)
                        {
                            throw new Exception($"Error al guardar el horario del día {dia.DiaSemana}.");
                        }
                    }
                }

                CommitTransaction();

                modelResponse.IsSuccess = true;
                modelResponse.Message = "Horario laboral guardado correctamente.";
            }
            catch (Exception ex)
            {
                RollbackTransaction();
                Log.Error(ex, "Error al guardar horario laboral para usuario {Usuario}", usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el horario laboral.";
            }

            return modelResponse;
        }

        public ModelResponse GuardarHorarioLaboralDia(string usuario, int diaSemana, DateTime? horaInicio, DateTime? horaFin, bool esLaboral)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var resultado = ExecuteScalar("GuardarHorarioLaboralDia", CommandType.StoredProcedure, new SqlParameter[]
                {
                    new SqlParameter("@Usuario", usuario),
                    new SqlParameter("@DiaSemana", diaSemana),
                    new SqlParameter("@HoraInicio", (object)horaInicio ?? DBNull.Value),
                    new SqlParameter("@HoraFin", (object)horaFin ?? DBNull.Value),
                    new SqlParameter("@EsLaboral", esLaboral)
                });

                if (Convert.ToInt32(resultado) == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo determinar la empresa del usuario.";
                    return modelResponse;
                }

                modelResponse.IsSuccess = true;
                modelResponse.Response = resultado;
                modelResponse.Message = "Día del horario laboral guardado correctamente.";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar horario laboral (día {DiaSemana}) para usuario {Usuario}", diaSemana, usuario);
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el día del horario laboral.";
            }

            return modelResponse;
        }
    }
}
