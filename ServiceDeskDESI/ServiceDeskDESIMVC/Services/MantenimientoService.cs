using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class MantenimientoService
    {
        private readonly HttpClientConnection _httpClient;

        public MantenimientoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Mantenimiento>>> ObtenerMantenimientosPorActivo(long activoId)
        {
            return await _httpClient.ObtenerMantenimientosPorActivo(activoId);
        }

        public async Task<ModelResponse> GuardarMantenimiento(Mantenimiento mantenimiento)
        {
            return await _httpClient.GuardarMantenimiento(mantenimiento);
        }
    }
}
