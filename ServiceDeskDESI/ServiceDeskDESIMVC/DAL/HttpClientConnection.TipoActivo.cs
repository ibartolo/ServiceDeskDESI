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
        public async Task<ModelResponse> ObtenerTodosLosTipoActivos()
        {
            var result = await RequestAsync($"api/TipoActivo/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result);
            return modelresponse;
        }

        public async Task<ModelResponse> GuardarOActualizarTipoActivo (TipoActivo t)
        {
            MappingColumSecurity(t);
            var result = await RequestAsync<object>($"api/TipoActivo/Guardar", HttpMethod.Post, t,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerTipoActivoPorId(long id)
        {
            var result = await RequestAsync<object>($"api/TipoActivo/{id}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse> EliminarTipoActivo (TipoActivo t)
        {
            MappingColumSecurity(t);
            var result = await RequestAsync<object>($"api/TipoActivo/Eliminar", HttpMethod.Delete, t,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
}