using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<HorarioLaboral>>> ObtenerHorarioLaboral()
        {
            return await RequestAsync<List<HorarioLaboral>>("api/HorarioLaboral/Obtener", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse> GuardarHorarioLaboral(List<HorarioLaboral> horario)
        {
            var result = await RequestAsync<object>("api/HorarioLaboral/Guardar", HttpMethod.Post, horario,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
