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
        public async Task <ModelResponse> ObtenerTodasLasSucursales()
        {
            var result = await RequestAsync($"api/sucursales/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }),
                token.Token.access_token);
            var modelresponse = JsonConvert.DeserializeObject<ModelResponse>(result);
            return modelresponse;
        }

        public async Task<ModelResponse> GuardarActualizarSucursales(Sucursal s)
        {
            MappingColumSecurity(s);
            var result = await RequestAsync<object>($"api/sucursales/Guardar", HttpMethod.Post, s,
               new Func<string, string>((responseString) =>
               {
                   return responseString;
               }), 
               token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerSucursalesPorId(long id)
        {
            var result = await RequestAsync<object>($"api/sucursales/{id}", HttpMethod.Get, null,
            new Func<string, string>((responseString) =>
            {
                return responseString;
            }));
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse>EliminarSucursal (Sucursal s)
        {
            MappingColumSecurity(s);
            var result= await RequestAsync<object>($"api/sucursales/Eliminar", HttpMethod.Delete, s,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), 
                token.Token.access_token);
            var modelreponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelreponse;
        }
    }
} 