using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PersonaService
    {
        private readonly HttpClientConnection _httpClient;

        public PersonaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PersonaDTO> ObtenerPersonaPorId(long id)
        {
            var response = await _httpClient.ObtenerPersonaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Persona>> GuardarOActualizarPersona(Persona persona)
        {
            return await _httpClient.GuardarOActualizarPersona(persona);
        }

        public async Task<ModelResponse> EliminarPersona(Persona persona)
        {
            return await _httpClient.EliminarPersona(persona);
        }

        public async Task<ModelResponse> VincularPersonaUsuario(long personaId, long usuarioId)
        {
            return await _httpClient.VincularPersonaUsuario(personaId, usuarioId);
        }

        public async Task<ModelResponse> DesvincularPersonaUsuario(long personaId)
        {
            return await _httpClient.DesvincularPersonaUsuario(personaId);
        }

        public async Task<ModelResponse<List<PersonaDTO>>> ConsultarTodasLasPersonas()
        {
            return await _httpClient.ObtenerTodasLasPersonas();
        }

        public async Task<object> ObtenerPermisosParaPersona()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Personas");
            }
            return null;
        }
    }
}
