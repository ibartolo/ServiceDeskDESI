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
        public async Task<ModelResponse> ObtenerTodasCompanias(long empresaId)
        {
            var result = await RequestAsync($"api/Compania/List/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }

        public async Task<ModelResponse> GuardarActualizarCompania(Compania c, long empresaId)
        {
            MappingColumSecurity(c);
           var result = await RequestAsync<object>($"api/Compania/Guardar/{empresaId}", HttpMethod.Post, c,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerCompaniaPorId(long id, long empresaId)
        {
            var result = await RequestAsync<object>($"api/Compania/{id}/{empresaId}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;

        }

        public async Task<ModelResponse> EliminarCompania(Compania c, long empresaId )
        {
            MappingColumSecurity(c);
            var result = await RequestAsync<object>($"api/Compania/Compania/{empresaId}", HttpMethod.Delete, c,
                new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }

    }
}