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
        public async Task<ModelResponse> ObtenerResponsablesPorCategoria(long categoriaId)
        {
            var result = await RequestAsync<object>($"api/Catalogs/CategoriaResponsable/{categoriaId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerCategoriasPorResponsable(long usuarioId)
        {
            var result = await RequestAsync<object>($"api/Catalogs/CategoriaResponsable/Usuario/{usuarioId}", HttpMethod.Get, null,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable)
        {
            MappingColumSecurity(categoriaResponsable);
            var result = await RequestAsync<object>($"api/Catalogs/CategoriaResponsable", HttpMethod.Post, categoriaResponsable,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
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