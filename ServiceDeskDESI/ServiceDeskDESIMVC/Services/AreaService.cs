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
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Area>> GuardarOActualizarArea(Area area)
        {
            return await _httpClient.GuardarOActualizarArea(area);
        }

        public async Task<ModelResponse> EliminarArea(Area area)
        {
            return await _httpClient.EliminarArea(area);
        }

        public async Task<ModelResponse<List<Area>>> ConsultarTodasAreas()
        {
            return await _httpClient.ObtenerAreas();
        }

        public async Task<object> ObtenerPermisosParaArea()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Áreas");
            }
            return null;
        }
    }
}
