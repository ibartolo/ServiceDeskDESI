using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class HorarioLaboralService
    {
        private readonly DbWrapper _dbWrapper;

        public HorarioLaboralService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<HorarioLaboral>> ObtenerHorarioLaboral(string usuario)
        {
            try
            {
                Log.Information("HorarioLaboralService.ObtenerHorarioLaboral para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerHorarioLaboral(usuario);
                Log.Information("HorarioLaboralService.ObtenerHorarioLaboral RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerHorarioLaboral para usuario {Usuario}", usuario);
                return new ModelResponse<List<HorarioLaboral>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en HorarioLaboralService.ObtenerHorarioLaboral para usuario {Usuario}", usuario);
                return new ModelResponse<List<HorarioLaboral>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener el horario laboral."
                };
            }
        }

        public ModelResponse GuardarHorarioLaboral(string usuario, List<HorarioLaboral> horario)
        {
            try
            {
                Log.Information("HorarioLaboralService.GuardarHorarioLaboral para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (horario == null || horario.Count != 7) { throw new ArgumentException("Debe proporcionar los 7 días de la semana."); }

                foreach (var dia in horario)
                {
                    if (dia.DiaSemana < 1 || dia.DiaSemana > 7)
                    {
                        throw new ArgumentException("El día de la semana debe estar entre 1 (lunes) y 7 (domingo).");
                    }

                    if (dia.Estatus)
                    {
                        if (dia.HoraInicio == null || dia.HoraFin == null)
                        {
                            throw new ArgumentException($"El día {dia.DiaSemana} está marcado como laborable y requiere hora de inicio y fin.");
                        }
                        if (dia.HoraFin.Value <= dia.HoraInicio.Value)
                        {
                            throw new ArgumentException($"El día {dia.DiaSemana} debe tener una hora de fin mayor a la hora de inicio.");
                        }
                    }
                    else
                    {
                        dia.HoraInicio = null;
                        dia.HoraFin = null;
                    }
                }

                var result = _dbWrapper.GuardarHorarioLaboral(usuario, horario);
                Log.Information("HorarioLaboralService.GuardarHorarioLaboral RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarHorarioLaboral para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en HorarioLaboralService.GuardarHorarioLaboral para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el horario laboral."
                };
            }
        }
    }
}
