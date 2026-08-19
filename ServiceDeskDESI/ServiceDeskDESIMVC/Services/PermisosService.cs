using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PermisosService
    {
        private readonly HttpClientConnection _httpClient;

        public PermisosService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<PermisosViewModel>>> ObtenerPermisosPorUsuario()
        {
            return await _httpClient.ObtenerPermisosPorUsuario();
        }

        public async Task<ModelResponse<bool>> ValidarPermisoUsuario(string nombrePagina, string accion)
        {
            return await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
        }

        public async Task<ModelResponse<List<Pagina>>> ObtenerPaginas()
        {
            return await _httpClient.ObtenerPaginas();
        }

        public async Task<ModelResponse<List<RolPaginaAccionDTO>>> ObtenerPermisosPorRol(long rolId)
        {
            return await _httpClient.ObtenerPermisosPorRol(rolId);
        }

        public async Task<ModelResponse> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            return await _httpClient.GuardarPermisosRol(request);
        }

        public async Task<ModelResponse> GuardarPermisosRolMasivo(GuardarPermisosMasivoRequest request)
        {
            return await _httpClient.GuardarPermisosRolMasivo(request);
        }

        public async Task<List<PermisosViewModel>> ObtenerPermisosParaPagina(string nombrePagina)
        {
            var response = await _httpClient.ObtenerPermisosPorUsuario();
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response.Where(p => p.PaginaNombre == nombrePagina).ToList();
            }
            return new List<PermisosViewModel>();
        }

        public async Task<bool> TienePermiso(string nombrePagina, string accion)
        {
            var response = await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
            if (response.IsSuccess)
            {
                return response.Response;
            }
            return false;
        }

        public async Task<object> ObtenerPermisosParaPermisos()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Permisos");
            }
            return null;
        }
    }
}
