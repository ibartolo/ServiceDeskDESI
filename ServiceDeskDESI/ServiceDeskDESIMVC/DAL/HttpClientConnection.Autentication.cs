using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using Serilog;
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
        public async Task<ModelResponse<UsuarioDTO>> AutenticarUsuario(Usuario usuario)
        {
            // No necesita token porque es el login
            var result = await RequestAsync<UsuarioDTO>($"api/Autentication/autenticar", HttpMethod.Post, usuario);
            Log.Information("AutenticarUsuario (MVC DAL) para {Usuario}: IsSuccess={IsSuccess}, Message={Message}", usuario.NombreUsuario, result?.IsSuccess, result?.Message);
            return result;
        }

        public async Task<ModelResponse<Usuario>> ActualizarPerfilUsuario(Usuario usuario)
        {
            MappingColumSecurity(usuario);
            return await RequestAsync<Usuario>($"api/Autentication/ActualizarPerfil", HttpMethod.Post, usuario, token.Token.access_token);
        }

        public async Task<ModelResponse> ValidarTokenRecuperacion(string token)
        {
            var result = await RequestAsync<object>($"api/Autentication/validarToken/{token}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                })); // No necesita token porque es el login

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse> RestablecerContrasenia(string token, string nuevaContrasena)
        {
            var request = new
            {
                Token = token,
                NuevaContrasena = nuevaContrasena
            };

            var result = await RequestAsync<object>($"api/Autentication/restablecerContrasenia", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }));

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse> ValidarRecetearContrasenia(Usuario usuario)
        {
            var result = await RequestAsync<object>($"api/Autentication/ValidarRecetearContrasenia", HttpMethod.Post, usuario,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }));

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            MappingColumSecurity(usuario);
            return await RequestAsync<Usuario>($"api/Autentication/Admin/Usuario", HttpMethod.Post, usuario, token.Token.access_token);
        }
    }
}
