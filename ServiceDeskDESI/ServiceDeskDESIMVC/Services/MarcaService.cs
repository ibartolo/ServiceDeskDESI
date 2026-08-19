using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class MarcaService
    {
        private readonly HttpClientConnection _httpClient;

        public MarcaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Marca> ObtenerMarcaPorId(long id)
        {
            var response = await _httpClient.ObtenerMarcaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Marca>> GuardarOActualizarMarca(Marca marca)
        {
            return await _httpClient.GuardarOActualizarMarca(marca);
        }

        public async Task<ModelResponse> EliminarMarca(Marca marca)
        {
            return await _httpClient.EliminarMarca(marca);
        }

        public async Task<ModelResponse<List<Marca>>> ConsultarTodosLasMarcas()
        {
            return await _httpClient.ObtenerTodosLasMarcas();
        }

        public async Task<object> ObtenerPermisosParaMarca()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Marcas");
            }
            return null;
        }
    }
}
