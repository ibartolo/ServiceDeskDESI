using Newtonsoft.Json;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
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

        public async Task<ModelResponse> ObtenerTodosLosRoles()
        {
            return await _httpClient.ObtenerTodosLosRoles();
        }

        public async Task<ModelResponse> ObtenerRolPorId(long id)
        {
            return await _httpClient.ObtenerRolPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarRol(Rol rol)
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

        public async Task<ModelResponse> ObtenerRolesPorUsuario(long usuarioId)
        {
            return await _httpClient.ObtenerRolesPorUsuario(usuarioId);
        }

        public async Task<ModelResponse> EliminarRolUsuario(long usuarioRolId)
        {
            return await _httpClient.EliminarRolUsuario(usuarioRolId);
        }
    }
}