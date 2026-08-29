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
        public async Task<ModelResponse<List<Compania>>> ObtenerTodasCompanias()
        {
            return await RequestAsync<List<Compania>>($"api/Compania/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Compania>> ObtenerCompaniaPorId(long id)
        {
            return await RequestAsync<Compania>($"api/Compania/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Compania>> GuardarActualizarCompania(Compania compania)
        {
            MappingColumSecurity(compania);
            return await RequestAsync<Compania>($"api/Compania/Guardar", HttpMethod.Post, compania, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarCompania(Compania compania)
        {
            MappingColumSecurity(compania);
            var result = await RequestAsync<object>($"api/Compania/Eliminar", HttpMethod.Delete, compania,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
