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
        public async Task<ModelResponse> ObtenerTodasCompanias(string token)
        {
            var result = await RequestAsync<object>("api/compania/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelresponse;
        }

        public async Task<ModelResponse> GuardarActualizarCompania(Compania c)
        {
           var result = await RequestAsync<object>("api/compania/Guardar", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }));

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerCompaniaPorId(long id)
        {
            var result = await RequestAsync<object>($"api/compania/{id}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;

        }

    }
}