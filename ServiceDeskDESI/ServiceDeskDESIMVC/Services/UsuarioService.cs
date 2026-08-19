using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class UsuarioService
    {
        private readonly HttpClientConnection _httpClient;

        public UsuarioService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Usuario> ObtenerUsuarioPorId(long id)
        {
            var response = await _httpClient.ObtenerUsuarioPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuarios()
        {
            return await _httpClient.ObtenerUsuarios();
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuario(Usuario usuario)
        {
            return await _httpClient.GuardarOActualizarUsuario(usuario);
        }

        public async Task<ModelResponse<Usuario>> GuardarUsuarioEmpresa(Usuario usuario)
        {
            return await _httpClient.GuardarUsuarioEmpresa(usuario);
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            return await _httpClient.GuardarOActualizarUsuarioAdmin(usuario);
        }

        public async Task<ModelResponse> EliminarUsuario(Usuario usuario)
        {
            return await _httpClient.EliminarUsuario(usuario);
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ConsultarTodosLosUsuarios()
        {
            return await _httpClient.ObtenerUsuarios();
        }

        public async Task<object> ObtenerPermisosParaUsuario()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Usuarios");
            }
            return null;
        }
    }
}
