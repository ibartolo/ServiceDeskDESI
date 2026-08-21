using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class PersonaActivoService
    {
        private readonly DbWrapper _dbWrapper;

        public PersonaActivoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<PersonaActivoDTO>> ObtenerActivosPorPersona(long personaId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerActivosPorPersona para PersonaId {PersonaId} usuario {Usuario}", personaId, usuario);

                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerActivosPorPersona(personaId, usuario);
                Log.Information("PersonaActivoService.ObtenerActivosPorPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerActivosPorPersona para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerActivosPorPersona para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaActivoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los activos de la persona." };
            }
        }

        public ModelResponse<List<Activo>> ObtenerActivosDisponibles(string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.ObtenerActivosDisponibles para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerActivosDisponibles(usuario);
                Log.Information("PersonaActivoService.ObtenerActivosDisponibles RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerActivosDisponibles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Activo>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.ObtenerActivosDisponibles para usuario {Usuario}", usuario);
                return new ModelResponse<List<Activo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los activos disponibles." };
            }
        }

        public ModelResponse AsignarActivoPersona(long personaId, long activoId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.AsignarActivoPersona para PersonaId {PersonaId} ActivoId {ActivoId} usuario {Usuario}", personaId, activoId, usuario);

                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (activoId <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.AsignarActivoPersona(personaId, activoId, usuario);
                Log.Information("PersonaActivoService.AsignarActivoPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en AsignarActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.AsignarActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al asignar el activo." };
            }
        }

        public ModelResponse DesvincularActivoPersona(long personaActivoId, string usuario)
        {
            try
            {
                Log.Information("PersonaActivoService.DesvincularActivoPersona para PersonaActivoId {PersonaActivoId} usuario {Usuario}", personaActivoId, usuario);

                if (personaActivoId <= 0) { throw new ArgumentException("El ID de la asignación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.DesvincularActivoPersona(personaActivoId, usuario);
                Log.Information("PersonaActivoService.DesvincularActivoPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en DesvincularActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaActivoService.DesvincularActivoPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al desvincular el activo." };
            }
        }
    }
}
