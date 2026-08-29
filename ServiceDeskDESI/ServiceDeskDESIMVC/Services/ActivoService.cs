using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class ActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public ActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ActivoDTO> ObtenerActivoPorId(long id)
        {
            var response = await _httpClient.ObtenerActivoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Activo>> GuardarOActualizarActivo(Activo activo)
        {
            return await _httpClient.GuardarOActualizarActivo(activo);
        }

        public async Task<ModelResponse> EliminarActivo(Activo activo)
        {
            return await _httpClient.EliminarActivo(activo);
        }

        public async Task<ModelResponse<List<ActivoDTO>>> ConsultarTodosLosActivos()
        {
            return await _httpClient.ObtenerTodosLosActivos();
        }

        public async Task<object> ObtenerPermisosParaActivo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Activos");
            }
            return null;
        }
    }
}
