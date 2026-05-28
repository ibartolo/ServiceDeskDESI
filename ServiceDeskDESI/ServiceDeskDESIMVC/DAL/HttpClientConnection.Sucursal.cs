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
        public async Task<ModelResponse> ObtenerTodasLasSucursales(long empresaId)
        {
            var result = await RequestAsync<object>($"api/Sucursales/List/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }),
                token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerSucursalPorId(long id, long empresaId)
        {
            var result = await RequestAsync<object>($"api/Sucursales/{id}/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarActualizarSucursal(Sucursal sucursal, long empresaId)
        {
            MappingColumSecurity(sucursal);
            var result = await RequestAsync<object>($"api/Sucursales/Guardar/{empresaId}", HttpMethod.Post, sucursal,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> EliminarSucursal(Sucursal sucursal, long empresaId)
        {
            MappingColumSecurity(sucursal);
            var result = await RequestAsync<object>($"api/Sucursales/Eliminar/{empresaId}", HttpMethod.Delete, sucursal,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);
            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}