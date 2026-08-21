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
                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerActivosPorPersona(personaId, usuario);
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
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerActivosDisponibles(usuario);
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
                if (personaId <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (activoId <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.AsignarActivoPersona(personaId, activoId, usuario);
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
                if (personaActivoId <= 0) { throw new ArgumentException("El ID de la asignación es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.DesvincularActivoPersona(personaActivoId, usuario);
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
