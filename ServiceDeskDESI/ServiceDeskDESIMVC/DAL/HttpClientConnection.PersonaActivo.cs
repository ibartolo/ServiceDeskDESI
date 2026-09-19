using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<PersonaActivoDTO>>> ObtenerActivosPorPersona(long personaId)
        {
            return await RequestAsync<List<PersonaActivoDTO>>($"api/PersonaActivo/ActivosPorPersona/{personaId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<Activo>>> ObtenerActivosDisponibles()
        {
            return await RequestAsync<List<Activo>>($"api/PersonaActivo/Disponibles", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<PersonaActivoDTO>>> MisActivos()
        {
            return await RequestAsync<List<PersonaActivoDTO>>($"api/PersonaActivo/MisActivos", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<AsignacionActivoDetalleDTO>> AsignacionPorToken(Guid token)
        {
            return await RequestAsync<AsignacionActivoDetalleDTO>($"api/PersonaActivo/AsignacionPorToken/{token}", HttpMethod.Get, null, string.Empty);
        }

        public async Task<ModelResponse> AsignarActivoPersona(long personaId, long activoId)
        {
            var request = new
            {
                PersonaId = personaId,
                ActivoId = activoId
            };

            var result = await RequestAsync<object>($"api/PersonaActivo/Asignar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> DesvincularActivoPersona(long personaActivoId)
        {
            var request = new
            {
                PersonaActivoId = personaActivoId
            };

            var result = await RequestAsync<object>($"api/PersonaActivo/Desvincular", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> IniciarDesvinculacion(long personaActivoId)
        {
            var request = new
            {
                PersonaActivoId = personaActivoId
            };

            var result = await RequestAsync<object>($"api/PersonaActivo/IniciarDesvinculacion", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ConfirmarRecepcion(Guid tokenGuid)
        {
            var request = new { Token = tokenGuid };

            var result = await RequestAsync<object>($"api/PersonaActivo/confirmarRecepcion", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> DesvincularConfirmacion(Guid tokenGuid)
        {
            var request = new { Token = tokenGuid };

            var result = await RequestAsync<object>($"api/PersonaActivo/desvincularConfirmacion", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
