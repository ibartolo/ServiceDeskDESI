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
        public async Task<ModelResponse> ObtenerTodasLasEmpresas()
        {
            var result = await RequestAsync<object>($"api/Empresas/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }));

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerEmpresasPorId(long id, long empresaId)
        {
            var result = await RequestAsync<object>($"api/Empresas/{id}/{empresaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerEmpresasPorRFC(string rfc)
        {
            var empresa = new Empresa { RFC = rfc };

            var result = await RequestAsync<object>($"api/Empresas/RFC", HttpMethod.Post, empresa,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarOActualizarEmpresa(Empresa empresa,long empresaId)
        {
            var result = await RequestAsync<object>($"api/Empresas/{empresaId}", HttpMethod.Post, empresa,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarNuevaEmpresa(Empresa empresa)
        {
            var result = await RequestAsync<object>($"api/Empresas/Nueva", HttpMethod.Post, empresa,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }));

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> EliminarEmpresa(Empresa empresa,long empresaId)
        {
            var result = await RequestAsync<object>($"api/Empresas/{empresaId}", HttpMethod.Delete, empresa,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}