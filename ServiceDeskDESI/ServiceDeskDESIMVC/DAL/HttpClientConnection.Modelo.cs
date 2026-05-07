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
        public async Task <ModelResponse> ObtenerTodosLosModelos()
        {
            var result = await RequestAsync($"api/modelo/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result);
            return modelresponse;
        }
        public async Task<ModelResponse> GuardarActualizarModelos (Modelo m)
        {
            MappingColumSecurity(m);
            var result = await RequestAsync<object>($"api/modelo/Guardar", HttpMethod.Post, m,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
        public async Task<ModelResponse> ObtenerModelosPorId (long id)
        {
            var result = await RequestAsync<object>($"api/modelo/{id}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }
        public async Task<ModelResponse> EliminarModelos(Modelo m)
        {
            MappingColumSecurity(m);
            var result = await RequestAsync<object>($"api/modelo/Eliminar", HttpMethod.Delete, m,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
}