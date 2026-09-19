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
        public async Task<ModelResponse<List<Mantenimiento>>> ObtenerMantenimientosPorActivo(long activoId)
        {
            return await RequestAsync<List<Mantenimiento>>($"api/Mantenimiento/PorActivo/{activoId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse> GuardarMantenimiento(Mantenimiento mantenimiento)
        {
            MappingColumSecurity(mantenimiento);

            var result = await RequestAsync<object>($"api/Mantenimiento/Guardar", HttpMethod.Post, mantenimiento,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
