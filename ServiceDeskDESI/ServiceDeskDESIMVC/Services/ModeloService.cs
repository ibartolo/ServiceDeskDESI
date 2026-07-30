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

        public async Task<ModelResponse> ObtenerModeloPorId(long id)
        {
            return await _httpClient.ObtenerModeloPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarModelo(Modelo modelo)
        {
            return await _httpClient.GuardarOActualizarModelo(modelo);
        }

        public async Task<ModelResponse> EliminarModelo(Modelo modelo)
        {
            return await _httpClient.EliminarModelo(modelo);
        }

        public async Task<ModelResponse> ConsultarTodosLosModelos()
        {
            return await _httpClient.ObtenerTodosLosModelos();
        }

        public async Task<object> ObtenerPermisosParaModelo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Modelos");
            }
            return null;
        }
    }
}