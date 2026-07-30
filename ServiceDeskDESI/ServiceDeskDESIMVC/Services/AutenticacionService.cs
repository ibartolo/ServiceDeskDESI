using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class AutenticacionService
    {
        private readonly HttpClientConnection _httpClient;

        public AutenticacionService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse> AutenticarUsuario(Usuario usuario)
        {
            return await _httpClient.AutenticarUsuario(usuario);
        }

        public async Task<ModelResponse> ActualizarPerfilUsuario(Usuario usuario)
        {
            return await _httpClient.ActualizarPerfilUsuario(usuario);
        }

        public async Task<ModelResponse> ValidarTokenRecuperacion(string token)
        {
            return await _httpClient.ValidarTokenRecuperacion(token);
        }

        public async Task<ModelResponse> RestablecerContrasenia(string token, string nuevaContrasena)
        {
            return await _httpClient.RestablecerContrasenia(token, nuevaContrasena);
        }

        public async Task<ModelResponse> ValidarRecetearContrasenia(Usuario usuario)
        {
            return await _httpClient.ValidarRecetearContrasenia(usuario);
        }

        public async Task<ModelResponse> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            return await _httpClient.GuardarOActualizarUsuarioAdmin(usuario);
        }
    }
}