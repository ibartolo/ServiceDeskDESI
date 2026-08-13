using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class CategoriaService
    {
        private readonly HttpClientConnection _httpClient;

        public CategoriaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse> ObtenerCategoriaPorId(long id)
        {
            return await _httpClient.ObtenerCategoriaPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarCategoria(Categoria categoria)
        {
            return await _httpClient.GuardarOActualizarCategoria(categoria);
        }

        public async Task<ModelResponse> EliminarCategoria(Categoria categoria)
        {
            return await _httpClient.EliminarCategoria(categoria);
        }

        public async Task<ModelResponse> ConsultarTodasCategorias()
        {
            return await _httpClient.ObtenerCategorias();
        }

        public async Task<object> ObtenerPermisosParaCategoria()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Categorías");
            }
            return null;
        }

        public async Task<ModelResponse> ObtenerCategoriasPorArea(long areaId)
        {
            return await _httpClient.ObtenerCategoriasPorArea(areaId);
        }

        public async Task<ModelResponse> ObtenerCategoriasPorPadre(long categoriaPadreId)
        {
            return await _httpClient.ObtenerCategoriasPorPadre(categoriaPadreId);
        }
    }
}