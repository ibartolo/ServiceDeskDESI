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
        public async Task<ModelResponse<List<ActivoDTO>>> ObtenerTodosLosActivos()
        {
            return await RequestAsync<List<ActivoDTO>>($"api/Activo/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<ActivoDTO>> ObtenerActivoPorId(long id)
        {
            return await RequestAsync<ActivoDTO>($"api/Activo/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Activo>> GuardarOActualizarActivo(Activo activo)
        {
            MappingColumSecurity(activo);
            return await RequestAsync<Activo>($"api/Activo/Guardar", HttpMethod.Post, activo, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarActivo(Activo activo)
        {
            MappingColumSecurity(activo);
            var result = await RequestAsync<object>($"api/Activo/Eliminar", HttpMethod.Delete, activo,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
