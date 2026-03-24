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
        public async Task<ModelResponse> ObtenerAreas()
        {
            var result = await RequestAsync($"api/Catalogs/Area/Lista", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result);

            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerAreaPorId(long id)
        {
            var result = await RequestAsync($"api/Catalogs/Area/{id}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result);
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarOActualizarArea(Area a)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/Catalogs/Area", HttpMethod.Post, a,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
        public async Task<ModelResponse> EliminarArea(Area a)
        {
            MappingColumSecurity(a);
            var result = await RequestAsync<object>($"api/Catalogs/Area", HttpMethod.Delete, a,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}