using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse> ObtenerTodosLosPuestos()
        {
            var result = await RequestAsync<object>($"api/Puesto/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerPuestoPorId(long id)
        {
            var result = await RequestAsync<object>($"api/Puesto/{id}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarOActualizarPuesto(Puesto puesto)
        {
            MappingColumSecurity(puesto);
            var result = await RequestAsync<object>($"api/Puesto/Guardar", HttpMethod.Post, puesto,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> EliminarPuesto(Puesto puesto)
        {
            MappingColumSecurity(puesto);
            var result = await RequestAsync<object>($"api/Puesto/Eliminar", HttpMethod.Delete, puesto,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}