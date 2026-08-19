using Newtonsoft.Json;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class RolService
    {
        private readonly HttpClientConnection _httpClient;

        public RolService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Rol>>> ObtenerTodosLosRoles()
        {
            return await _httpClient.ObtenerTodosLosRoles();
        }

        public async Task<Rol> ObtenerRolPorId(long id)
        {
            var response = await _httpClient.ObtenerRolPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Rol>> GuardarOActualizarRol(Rol rol)
        {
            return await _httpClient.GuardarOActualizarRol(rol);
        }

        public async Task<ModelResponse> EliminarRol(Rol rol)
        {
            return await _httpClient.EliminarRol(rol);
        }

        public async Task<ModelResponse> AsignarRolUsuario(long usuarioId, long rolId)
        {
            return await _httpClient.AsignarRolUsuario(usuarioId, rolId);
        }

        public async Task<ModelResponse<List<Rol>>> ObtenerRolesPorUsuario(long usuarioId)
        {
            return await _httpClient.ObtenerRolesPorUsuario(usuarioId);
        }

        public async Task<ModelResponse> EliminarRolUsuario(long usuarioRolId)
        {
            return await _httpClient.EliminarRolUsuario(usuarioRolId);
        }

        public async Task<object> ObtenerPermisosParaRol()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Roles");
            }
            return null;
        }
    }
}
