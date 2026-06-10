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
        public async Task <ModelResponse> ObtenerTodosLosModelos(long empresaId)
        {
            var result = await RequestAsync<object>($"api/Modelo/List/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> GuardarActualizarModelos (Modelo m, long empresaId)
        {
            MappingColumSecurity(m);
            var result = await RequestAsync<object>($"api/Modelo/Guardar/{empresaId}", HttpMethod.Post, m,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
        public async Task<ModelResponse> ObtenerModelosPorId (long id,long empresaId)
        {
            var result = await RequestAsync<object>($"api/Modelo/{id}/{empresaId}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }
        public async Task<ModelResponse> EliminarModelos(Modelo m, long empresaId)
        {
            MappingColumSecurity(m);
                var result = await RequestAsync<object>($"api/Modelo/Eliminar/{empresaId}", HttpMethod.Delete, m,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
}