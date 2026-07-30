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

        public async Task<ModelResponse> ObtenerMarcaPorId(long id)
        {
            return await _httpClient.ObtenerMarcaPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarMarca(Marca marca)
        {
            return await _httpClient.GuardarOActualizarMarca(marca);
        }

        public async Task<ModelResponse> EliminarMarca(Marca marca)
        {
            return await _httpClient.EliminarMarca(marca);
        }

        public async Task<ModelResponse> ConsultarTodosLasMarcas()
        {
            return await _httpClient.ObtenerTodosLasMarcas();
        }

        public async Task<object> ObtenerPermisosParaMarca()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Marcas");
            }
            return null;
        }
    }
}