using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PersonaActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public PersonaActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<PersonaActivoDTO>>> ObtenerActivosPorPersona(long personaId)
        {
            return await _httpClient.ObtenerActivosPorPersona(personaId);
        }

        public async Task<ModelResponse<List<Activo>>> ObtenerActivosDisponibles()
        {
            return await _httpClient.ObtenerActivosDisponibles();
        }

        public async Task<ModelResponse<List<PersonaActivoDTO>>> MisActivos()
        {
            return await _httpClient.MisActivos();
        }

        public async Task<ModelResponse<AsignacionActivoDetalleDTO>> AsignacionPorToken(Guid token)
        {
            return await _httpClient.AsignacionPorToken(token);
        }

        public async Task<ModelResponse> AsignarActivoPersona(long personaId, long activoId)
        {
            return await _httpClient.AsignarActivoPersona(personaId, activoId);
        }

        public async Task<ModelResponse> DesvincularActivoPersona(long personaActivoId)
        {
            return await _httpClient.DesvincularActivoPersona(personaActivoId);
        }

        public async Task<ModelResponse> IniciarDesvinculacion(long personaActivoId)
        {
            return await _httpClient.IniciarDesvinculacion(personaActivoId);
        }

        public async Task<ModelResponse> ConfirmarRecepcion(Guid token)
        {
            return await _httpClient.ConfirmarRecepcion(token);
        }

        public async Task<ModelResponse> DesvincularConfirmacion(Guid token)
        {
            return await _httpClient.DesvincularConfirmacion(token);
        }
    }
}
