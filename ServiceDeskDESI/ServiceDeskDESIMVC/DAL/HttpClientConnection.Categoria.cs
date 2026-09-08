using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<List<CategoriaDTO>>> ObtenerCategorias()
        {
            return await RequestAsync<List<CategoriaDTO>>($"api/Catalogs/Categoria/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<CategoriaDTO>>> ObtenerCategoriasPorArea(long areaId)
        {
            return await RequestAsync<List<CategoriaDTO>>($"api/Catalogs/Categoria/Lista/{areaId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<CategoriaDTO>> ObtenerCategoriaPorId(long id)
        {
            return await RequestAsync<CategoriaDTO>($"api/Catalogs/Categoria/{id}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<CategoriaDTO>>> ObtenerCategoriasPorPadre(long categoriaPadreId)
        {
            return await RequestAsync<List<CategoriaDTO>>($"api/Catalogs/Categoria/Subcategorias/{categoriaPadreId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<Categoria>> GuardarOActualizarCategoria(Categoria categoria)
        {
            MappingColumSecurity(categoria);
            return await RequestAsync<Categoria>($"api/Catalogs/Categoria", HttpMethod.Post, categoria, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarCategoria(Categoria categoria)
        {
            MappingColumSecurity(categoria);
            var result = await RequestAsync<object>($"api/Catalogs/Categoria", HttpMethod.Delete, categoria,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }


    }
}
