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

        public async Task<ModelResponse> ObtenerTipoActivoPorId(long id)
        {
            return await _httpClient.ObtenerTipoActivoPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarTipoActivo(TipoActivo tipoActivo)
        {
            return await _httpClient.GuardarOActualizarTipoActivo(tipoActivo);
        }

        public async Task<ModelResponse> EliminarTipoActivo(TipoActivo tipoActivo)
        {
            return await _httpClient.EliminarTipoActivo(tipoActivo);
        }

        public async Task<ModelResponse> ConsultarTodosLosTipoActivos()
        {
            return await _httpClient.ObtenerTodosLosTipoActivos();
        }

        public async Task<object> ObtenerPermisosParaTipoActivo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Tipo Activo");
            }
            return null;
        }
    }
}