using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
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
        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuarios()
        {
            return await RequestAsync<List<UsuarioDTO>>($"api/Autentication/User/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<UsuarioDTO>> ObtenerUsuarioPorId(long id)
        {
            return await RequestAsync<UsuarioDTO>($"api/Autentication/User/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuario(Usuario usuario)
        {
            return await RequestAsync<Usuario>($"api/Autentication/User", HttpMethod.Post, usuario, token.Token.access_token);
        }

        public async Task<ModelResponse<Usuario>> GuardarUsuarioEmpresa(Usuario usuario)
        {
            return await RequestAsync<Usuario>($"api/Autentication/User/Empresa", HttpMethod.Post, usuario);
        }

        public async Task<ModelResponse> EliminarUsuario(Usuario usuario)
        {
            var result = await RequestAsync<object>($"api/Autentication/User", HttpMethod.Delete, usuario,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerSucursales()
        {
            var result = await RequestAsync<object>($"api/Sucursales/List", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }
    }
}
