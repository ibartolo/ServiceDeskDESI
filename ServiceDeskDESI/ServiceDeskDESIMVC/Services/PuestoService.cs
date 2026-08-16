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

        public async Task<ModelResponse> ObtenerPuestoPorId(long id)
        {
            return await _httpClient.ObtenerPuestoPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarPuesto(Puesto puesto)
        {
            return await _httpClient.GuardarOActualizarPuesto(puesto);
        }

        public async Task<ModelResponse> EliminarPuesto(Puesto puesto)
        {
            return await _httpClient.EliminarPuesto(puesto);
        }

        public async Task<ModelResponse> ConsultarTodosLosPuestos()
        {
            return await _httpClient.ObtenerTodosLosPuestos();
        }

        public async Task<object> ObtenerPermisosParaPuesto()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Puesto");
            }
            return null;
        }
    }
}