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
        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerResponsablesPorCategoria(long categoriaId)
        {
            return await RequestAsync<List<CategoriaResponsableDTO>>($"api/Catalogs/CategoriaResponsable/{categoriaId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerCategoriasPorResponsable(long usuarioId)
        {
            return await RequestAsync<List<CategoriaResponsableDTO>>($"api/Catalogs/CategoriaResponsable/Usuario/{usuarioId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<CategoriaResponsableDTO>>> ObtenerTodosLosResponsables()
        {
            return await RequestAsync<List<CategoriaResponsableDTO>>($"api/Catalogs/CategoriaResponsable/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<CategoriaResponsable>> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            MappingColumSecurity(categoriaResponsable);
            return await RequestAsync<CategoriaResponsable>($"api/Catalogs/CategoriaResponsable", HttpMethod.Post, categoriaResponsable, token.Token.access_token);
        }

        public async Task<ModelResponse> EliminarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            MappingColumSecurity(categoriaResponsable);
            var result = await RequestAsync<object>($"api/Catalogs/CategoriaResponsable", HttpMethod.Delete, categoriaResponsable,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }
    }
}
