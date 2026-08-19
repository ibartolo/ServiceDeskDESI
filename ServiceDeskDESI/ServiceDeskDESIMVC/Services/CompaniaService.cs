using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class CompaniaService
    {
        private readonly HttpClientConnection _httpClient;

        public CompaniaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Compania> ObtenerCompaniaPorId(long id)
        {
            var response = await _httpClient.ObtenerCompaniaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Compania>> GuardarOActualizarCompania(Compania compania)
        {
            return await _httpClient.GuardarActualizarCompania(compania);
        }

        public async Task<ModelResponse> EliminarCompania(Compania compania)
        {
            return await _httpClient.EliminarCompania(compania);
        }

        public async Task<ModelResponse<List<Compania>>> ConsultarTodasCompanias()
        {
            return await _httpClient.ObtenerTodasCompanias();
        }

        public async Task<object> ObtenerPermisosParaCompania()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Compañías");
            }
            return null;
        }
    }
}
