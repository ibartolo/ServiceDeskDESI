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

        public async Task<ModelResponse> ObtenerActivoPorId(long id)
        {
            return await _httpClient.ObtenerActivoPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarActivo(Activo activo)
        {
            return await _httpClient.GuardarOActualizarActivo(activo);
        }

        public async Task<ModelResponse> EliminarActivo(Activo activo)
        {
            return await _httpClient.EliminarActivo(activo);
        }

        public async Task<ModelResponse> ConsultarTodosLosActivos()
        {
            return await _httpClient.ObtenerTodosLosActivos();
        }

        public async Task<object> ObtenerPermisosParaActivo()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Activos");
            }
            return null;
        }
    }
}