using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PuestoService
    {
        private readonly HttpClientConnection _httpClient;

        public PuestoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Puesto> ObtenerPuestoPorId(long id)
        {
            var response = await _httpClient.ObtenerPuestoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Puesto>> GuardarOActualizarPuesto(Puesto puesto)
        {
            return await _httpClient.GuardarOActualizarPuesto(puesto);
        }

        public async Task<ModelResponse> EliminarPuesto(Puesto puesto)
        {
            return await _httpClient.EliminarPuesto(puesto);
        }

        public async Task<ModelResponse<List<Puesto>>> ConsultarTodosLosPuestos()
        {
            return await _httpClient.ObtenerTodosLosPuestos();
        }

        public async Task<object> ObtenerPermisosParaPuesto()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Tipped");
            }
            return null;
        }
    }
}
