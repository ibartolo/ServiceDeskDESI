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

        public async Task<ModelResponse> ObtenerUsuarioPorId(long id)
        {
            return await _httpClient.ObtenerUsuarioPorId(id);
        }

        public async Task<ModelResponse> ObtenerUsuarios()
        {
            return await _httpClient.ObtenerUsuarios();
        }

        public async Task<ModelResponse> GuardarOActualizarUsuario(Usuario usuario)
        {
            return await _httpClient.GuardarOActualizarUsuario(usuario);
        }

        public async Task<ModelResponse> GuardarUsuarioEmpresa(Usuario usuario)
        {
            return await _httpClient.GuardarUsuarioEmpresa(usuario);
        }

        public async Task<ModelResponse> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            return await _httpClient.GuardarOActualizarUsuarioAdmin(usuario);
        }

        public async Task<ModelResponse> EliminarUsuario(Usuario usuario)
        {
            return await _httpClient.EliminarUsuario(usuario);
        }

        public async Task<ModelResponse> ConsultarTodosLosUsuarios()
        {
            return await _httpClient.ObtenerUsuarios();
        }

        public async Task<object> ObtenerPermisosParaUsuario()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Usuarios");
            }
            return null;
        }
    }
}