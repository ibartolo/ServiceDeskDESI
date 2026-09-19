using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<PersonaDTO>>> ObtenerTodasLasPersonas()
        {
            return await RequestAsync<List<PersonaDTO>>($"api/Persona/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<PersonaDTO>> ObtenerPersonaPorId(long id)
        {
            return await RequestAsync<PersonaDTO>($"api/Persona/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Persona>> GuardarOActualizarPersona(Persona persona)
        {
            MappingColumSecurity(persona);
            return await RequestAsync<Persona>($"api/Persona/Guardar", HttpMethod.Post, persona, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarPersona(Persona persona)
        {
            MappingColumSecurity(persona);
            var result = await RequestAsync<object>($"api/Persona/Eliminar", HttpMethod.Delete, persona,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> VincularPersonaUsuario(long personaId, long usuarioId)
        {
            var request = new
            {
                PersonaId = personaId,
                UsuarioId = usuarioId
            };

            var result = await RequestAsync<object>($"api/Persona/VincularUsuario", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> DesvincularPersonaUsuario(long personaId)
        {
            var request = new
            {
                PersonaId = personaId
            };

            var result = await RequestAsync<object>($"api/Persona/DesvincularUsuario", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
