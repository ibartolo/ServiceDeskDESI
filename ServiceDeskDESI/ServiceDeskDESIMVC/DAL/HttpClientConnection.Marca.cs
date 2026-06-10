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
        public async Task<ModelResponse> ObtenerTodosLasMarcas(long empresaId)
        {
            var result = await RequestAsync($"api/Marca/List/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }
        public async Task<ModelResponse> GuardarOActualizarMarca(Marca m,long empresaId)
        {
            MappingColumSecurity(m);
            var result = await RequestAsync<object>($"api/Marca/Guardar/{empresaId}", HttpMethod.Post, m,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
        public async Task<ModelResponse> ObtenerMarcaPorId(long id,long empresaId)
        {
            var result = await RequestAsync<object>($"api/Marca/{id}/{empresaId}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }
        public async Task<ModelResponse> EliminarMarcas(Marca m,long empresaId)
        {
            MappingColumSecurity(m);
            var result = await RequestAsync<object>($"api/Marca/Eliminar/{empresaId}", HttpMethod.Delete, m,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
}