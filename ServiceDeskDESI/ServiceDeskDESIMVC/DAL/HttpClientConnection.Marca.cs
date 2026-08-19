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
        public async Task<ModelResponse<List<Marca>>> ObtenerTodosLasMarcas()
        {
            return await RequestAsync<List<Marca>>($"api/Marca/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Marca>> ObtenerMarcaPorId(long id)
        {
            return await RequestAsync<Marca>($"api/Marca/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Marca>> GuardarOActualizarMarca(Marca marca)
        {
            MappingColumSecurity(marca);
            return await RequestAsync<Marca>($"api/Marca/Guardar", HttpMethod.Post, marca, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarMarca(Marca marca)
        {
            MappingColumSecurity(marca);
            var result = await RequestAsync<object>($"api/Marca/Eliminar", HttpMethod.Delete, marca,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
