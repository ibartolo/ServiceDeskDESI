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
        public async Task  <ModelResponse> ObtenerTodosLosActivos (long empresaId)
        {
            var result = await RequestAsync<object>($"api/Activos/List{empresaId}", HttpMethod.Get, null,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelresponse =JsonConvert.DeserializeObject <ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> GuardarActualizarActivos(Activo a,long empresaId)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/Activos/{empresaId}", HttpMethod.Post, a,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> ObtenerActivoPorId(long id, long empresaId)
        {
            var result = await RequestAsync<object>($"api/Activos/{id}/{empresaId}", HttpMethod.Get, null,
                 new Func<string, string>((responseString) =>
                 {
                     return responseString;
                 }),
                 token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
        public async Task<ModelResponse> EliminarActivos(Activo a,long empresaId)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/Activos/{empresaId}", HttpMethod.Delete, a,
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