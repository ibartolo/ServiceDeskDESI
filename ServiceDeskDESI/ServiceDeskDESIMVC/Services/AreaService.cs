using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Services
{
    public class AreaService
    {
        private readonly HttpClientConnection _httpClient;

        public AreaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Area> ObtenerAreaPorId(long id)
        {
            var response = await _httpClient.ObtenerAreaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return JsonConvert.DeserializeObject<Area>(response.Response.ToString());
            }
            return null;
        }

        public async Task<object> ObtenerPermisosParaArea()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Áreas");
            }
            return null;
        }

        public async Task<bool> TienePermisoLectura()
        {
            var permisos = await ObtenerPermisosParaArea();
            if (permisos != null)
            {
                var permiso = (PermisosViewModel)permisos;
                return permiso.PuedeLeer;
            }
            return false;
        }

        public async Task<ModelResponse> GuardarOActualizarArea(Area area)
        {
            return await _httpClient.GuardarOActualizarArea(area);
        }

        public async Task<ModelResponse> EliminarArea(Area area)
        {
            return await _httpClient.EliminarArea(area);
        }

        public async Task<ModelResponse> ConsultarTodasAreas()
        {
            return await _httpClient.ObtenerAreas();
        }
    }
}