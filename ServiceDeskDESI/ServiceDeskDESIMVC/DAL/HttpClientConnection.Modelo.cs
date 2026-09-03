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
        public async Task<ModelResponse<List<ModeloDTO>>> ObtenerTodosLosModelos()
        {
            return await RequestAsync<List<ModeloDTO>>($"api/Modelo/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<ModeloDTO>> ObtenerModeloPorId(long id)
        {
            return await RequestAsync<ModeloDTO>($"api/Modelo/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<Modelo>>> ObtenerModelosPorMarca(long marcaId)
        {
            return await RequestAsync<List<Modelo>>($"api/Modelo/PorMarca/{marcaId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Modelo>> GuardarOActualizarModelo(Modelo modelo)
        {
            MappingColumSecurity(modelo);
            return await RequestAsync<Modelo>($"api/Modelo/Guardar", HttpMethod.Post, modelo, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarModelo(Modelo modelo)
        {
            MappingColumSecurity(modelo);
            var result = await RequestAsync<object>($"api/Modelo/Eliminar", HttpMethod.Delete, modelo,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
