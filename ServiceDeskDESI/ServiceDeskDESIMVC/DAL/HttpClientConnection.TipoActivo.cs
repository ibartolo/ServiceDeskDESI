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
        public async Task<ModelResponse> ObtenerTodosLosTipoActivos(long empresaId)
        {
            var result = await RequestAsync($"api/TipoActivo/List/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }

        public async Task<ModelResponse> GuardarOActualizarTipoActivo (TipoActivo t,long empresaId)
        {
            MappingColumSecurity(t);
            var result = await RequestAsync<object>($"api/TipoActivo/Guardar/{empresaId}", HttpMethod.Post, t,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerTipoActivoPorId(long id, long empresaId)
        {
            var result = await RequestAsync<object>($"api/TipoActivo/{id}/{empresaId}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse> EliminarTipoActivo (TipoActivo t, long empresaId)
        {
            MappingColumSecurity(t);
            var result = await RequestAsync<object>($"api/TipoActivo/Eliminar/{empresaId}", HttpMethod.Delete, t,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
}