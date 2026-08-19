using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class CategoriaResponsableService
    {
        private readonly HttpClientConnection _httpClient;

        public CategoriaResponsableService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerResponsablesPorCategoria(long categoriaId)
        {
            return await _httpClient.ObtenerResponsablesPorCategoria(categoriaId);
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerCategoriasPorResponsable(long usuarioId)
        {
            return await _httpClient.ObtenerCategoriasPorResponsable(usuarioId);
        }

        public async Task<ModelResponse<CategoriaResponsable>> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            return await _httpClient.GuardarOActualizarCategoriaResponsable(categoriaResponsable);
        }

        public async Task<ModelResponse> EliminarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            return await _httpClient.EliminarCategoriaResponsable(categoriaResponsable);
        }
    }
}
