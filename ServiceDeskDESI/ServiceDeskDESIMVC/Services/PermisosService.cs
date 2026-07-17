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

        public async Task<ModelResponse> ObtenerPermisosPorUsuario()
        {
            return await _httpClient.ObtenerPermisosPorUsuario();
        }

        public async Task<ModelResponse> ValidarPermisoUsuario(string nombrePagina, string accion)
        {
            return await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
        }

        public async Task<ModelResponse> ObtenerPaginas()
        {
            return await _httpClient.ObtenerPaginas();
        }

        public async Task<ModelResponse> ObtenerPermisosPorRol(long rolId)
        {
            return await _httpClient.ObtenerPermisosPorRol(rolId);
        }

        public async Task<ModelResponse> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            return await _httpClient.GuardarPermisosRol(request);
        }

        public async Task<List<PermisosViewModel>> ObtenerPermisosParaPagina(string nombrePagina)
        {
            var response = await _httpClient.ObtenerPermisosPorUsuario();
            if (response.IsSuccess && response.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(response.Response.ToString());
                return listaPermisos.Where(p => p.PaginaNombre == nombrePagina).ToList();
            }
            return new List<PermisosViewModel>();
        }

        public async Task<bool> TienePermiso(string nombrePagina, string accion)
        {
            var response = await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
            if (response.IsSuccess && response.Response != null)
            {
                return (bool)response.Response;
            }
            return false;
        }
    }
}