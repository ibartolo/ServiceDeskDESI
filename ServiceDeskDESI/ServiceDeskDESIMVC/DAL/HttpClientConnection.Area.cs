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
        public async Task<ModelResponse<List<Area>>> ObtenerAreas()
        {
            return await RequestAsync<List<Area>>($"api/Area/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Area>> ObtenerAreaPorId(long id)
        {
            return await RequestAsync<Area>($"api/Area/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Area>> GuardarOActualizarArea(Area area)
        {
            MappingColumSecurity(area);
            return await RequestAsync<Area>($"api/Area/Guardar", HttpMethod.Post, area, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarArea(Area area)
        {
            MappingColumSecurity(area);
            var result = await RequestAsync<object>($"api/Area/Eliminar", HttpMethod.Delete, area,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
