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
        public async Task  <ModelResponse> ObtenerTodosLosActivos ()
        {
            var result = await RequestAsync<object>($"api/activos/List", HttpMethod.Get, null,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelresponse =JsonConvert.DeserializeObject <ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> GuardarActualizarActivos(Activo a)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/activos", HttpMethod.Post, a,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> ObtenerActivoPorId(long id)
        {
            var result = await RequestAsync<object>($"api/activos/{id}", HttpMethod.Get, null,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
        public async Task<ModelResponse> EliminarActivos(Activo a)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/activos", HttpMethod.Delete, a,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }),
                 token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }

    }
}