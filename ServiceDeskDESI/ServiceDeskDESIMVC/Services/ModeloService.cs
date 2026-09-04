using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class ModeloService
    {
        private readonly HttpClientConnection _httpClient;

        public ModeloService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModeloDTO> ObtenerModeloPorId(long id)
        {
            var response = await _httpClient.ObtenerModeloPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Modelo>> GuardarOActualizarModelo(Modelo modelo)
        {
            return await _httpClient.GuardarOActualizarModelo(modelo);
        }

        public async Task<ModelResponse> EliminarModelo(Modelo modelo)
        {
            return await _httpClient.EliminarModelo(modelo);
        }

        public async Task<ModelResponse<List<ModeloDTO>>> ConsultarTodosLosModelos()
        {
            return await _httpClient.ObtenerTodosLosModelos();
        }

        public async Task<object> ObtenerPermisosParaModelo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Modelos");
            }
            return null;
        }
    }
}
