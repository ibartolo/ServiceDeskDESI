using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class TipoActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public TipoActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TipoActivo> ObtenerTipoActivoPorId(long id)
        {
            var response = await _httpClient.ObtenerTipoActivoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<TipoActivo>> GuardarOActualizarTipoActivo(TipoActivo tipoActivo)
        {
            return await _httpClient.GuardarOActualizarTipoActivo(tipoActivo);
        }

        public async Task<ModelResponse> EliminarTipoActivo(TipoActivo tipoActivo)
        {
            return await _httpClient.EliminarTipoActivo(tipoActivo);
        }

        public async Task<ModelResponse<List<TipoActivo>>> ConsultarTodosLosTipoActivos()
        {
            return await _httpClient.ObtenerTodosLosTipoActivos();
        }

        public async Task<object> ObtenerPermisosParaTipoActivo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Tipo Activo");
            }
            return null;
        }
    }
}
