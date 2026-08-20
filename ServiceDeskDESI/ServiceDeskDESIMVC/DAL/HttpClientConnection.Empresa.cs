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
        public async Task<ModelResponse<Empresa>> ObtenerEmpresaPorId(long id)
        {
            return await RequestAsync<Empresa>($"api/Empresas/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Empresa>> GuardarOActualizarEmpresa(Empresa empresa)
        {
            return await RequestAsync<Empresa>($"api/Empresas/Guardar", HttpMethod.Post, empresa, token.Token.access_token);
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresa(Empresa empresa)
        {
            return await RequestAsync<Empresa>($"api/Empresas/Registrar", HttpMethod.Post, empresa);
        }

        public async Task<ModelResponse<Empresa>> RegistrarEmpresa(Empresa empresa)
        {
            return await RequestAsync<Empresa>($"api/Empresas/Registrar", HttpMethod.Post, empresa);
        }

        public async Task<ModelResponse<Empresa>> GuardarNuevaEmpresaCompleta(Empresa empresa)
        {
            return await RequestAsync<Empresa>($"api/Empresas/NuevaCompleta", HttpMethod.Post, empresa);
        }

        public async Task<ModelResponse> EliminarEmpresa(Empresa empresa)
        {
            var result = await RequestAsync<object>($"api/Empresas/Eliminar", HttpMethod.Delete, empresa,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
