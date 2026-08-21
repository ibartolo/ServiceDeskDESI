using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class PersonaService
    {
        private readonly DbWrapper _dbWrapper;

        public PersonaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<PersonaDTO>> ObtenerTodasLasPersonas(string usuario)
        {
            try
            {
                Log.Information("PersonaService.ObtenerTodasLasPersonas para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTodasLasPersonas(usuario);
                Log.Information("PersonaService.ObtenerTodasLasPersonas RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodasLasPersonas para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaService.ObtenerTodasLasPersonas para usuario {Usuario}", usuario);
                return new ModelResponse<List<PersonaDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener las personas." };
            }
        }

        public ModelResponse<PersonaDTO> ObtenerPersonaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("PersonaService.ObtenerPersonaPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerPersonaPorId(id, usuario);
                Log.Information("PersonaService.ObtenerPersonaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPersonaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<PersonaDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaService.ObtenerPersonaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<PersonaDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener la persona." };
            }
        }

        public ModelResponse<Persona> GuardarOActualizarPersona(Persona persona, string usuario)
        {
            try
            {
                Log.Information("PersonaService.GuardarOActualizarPersona para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(persona.Nombre)) { throw new ArgumentException("El nombre de la persona es requerido."); }
                if (persona.Nombre.Length > 150) { throw new ArgumentException("El nombre no puede exceder los 150 caracteres."); }
                if (string.IsNullOrWhiteSpace(persona.Apellido)) { throw new ArgumentException("El apellido de la persona es requerido."); }
                if (persona.Apellido.Length > 250) { throw new ArgumentException("El apellido no puede exceder los 250 caracteres."); }
                if (persona.Correo != null && persona.Correo.Length > 250) { throw new ArgumentException("El correo no puede exceder los 250 caracteres."); }
                if (persona.Telefono != null && persona.Telefono.Length > 50) { throw new ArgumentException("El teléfono no puede exceder los 50 caracteres."); }
                if (persona.PuestoId <= 0) { throw new ArgumentException("El puesto es requerido."); }
                if (string.IsNullOrWhiteSpace(persona.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarPersona(persona, usuario);
                Log.Information("PersonaService.GuardarOActualizarPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarPersona para usuario {Usuario}", usuario);
                return new ModelResponse<Persona> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaService.GuardarOActualizarPersona para usuario {Usuario}", usuario);
                return new ModelResponse<Persona> { IsSuccess = false, Message = "Ocurrió un error al guardar la persona." };
            }
        }

        public ModelResponse EliminarPersona(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("PersonaService.EliminarPersona para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la persona es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarPersona(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("PersonaService.EliminarPersona RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PersonaService.EliminarPersona para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar la persona." };
            }
        }
    }
}
