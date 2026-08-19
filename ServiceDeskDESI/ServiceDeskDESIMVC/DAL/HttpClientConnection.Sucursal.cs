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
        public async Task<ModelResponse<List<Sucursal>>> ObtenerTodasLasSucursales()
        {
            return await RequestAsync<List<Sucursal>>($"api/Sucursales/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Sucursal>> ObtenerSucursalPorId(long id)
        {
            return await RequestAsync<Sucursal>($"api/Sucursales/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Sucursal>> GuardarActualizarSucursal(Sucursal sucursal)
        {
            MappingColumSecurity(sucursal);
            return await RequestAsync<Sucursal>($"api/Sucursales/Guardar", HttpMethod.Post, sucursal, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarSucursal(Sucursal sucursal)
        {
            MappingColumSecurity(sucursal);
            var result = await RequestAsync<object>($"api/Sucursales/Eliminar", HttpMethod.Delete, sucursal,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
