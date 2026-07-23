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
        public async Task<ModelResponse> ObtenerCompaniaPorId(long id)
        {
            return await _httpClient.ObtenerCompaniaPorId(id);
        }
        public async Task<ModelResponse> GuardarOActualizarCompania(Compania compania)
        {
            return await _httpClient.GuardarActualizarCompania(compania);
        }

        public async Task<ModelResponse> EliminarCompania(Compania compania)
        {
            return await _httpClient.EliminarCompania(compania);
        }

        public async Task<ModelResponse> ConsultarTodasCompanias()
        {
            return await _httpClient.ObtenerTodasCompanias();
        }

        public async Task<object> ObtenerPermisosParaCompania()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Compañías");
            }
            return null;
        }
    }
}