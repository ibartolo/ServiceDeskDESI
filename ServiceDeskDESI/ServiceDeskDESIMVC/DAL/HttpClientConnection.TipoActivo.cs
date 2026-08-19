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
        public async Task<ModelResponse<List<TipoActivo>>> ObtenerTodosLosTipoActivos()
        {
            return await RequestAsync<List<TipoActivo>>($"api/TipoActivo/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<TipoActivo>> ObtenerTipoActivoPorId(long id)
        {
            return await RequestAsync<TipoActivo>($"api/TipoActivo/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<TipoActivo>> GuardarOActualizarTipoActivo(TipoActivo tipoActivo)
        {
            MappingColumSecurity(tipoActivo);
            return await RequestAsync<TipoActivo>($"api/TipoActivo/Guardar", HttpMethod.Post, tipoActivo, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarTipoActivo(TipoActivo tipoActivo)
        {
            MappingColumSecurity(tipoActivo);
            var result = await RequestAsync<object>($"api/TipoActivo/Eliminar", HttpMethod.Delete, tipoActivo,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
