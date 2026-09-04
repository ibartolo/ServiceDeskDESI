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
        public async Task<ModelResponse<List<PermisosViewModel>>> ObtenerPermisosPorUsuario()
        {
            return await RequestAsync<List<PermisosViewModel>>($"api/Permisos/List", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<bool>> ValidarPermisoUsuario(string nombrePagina, string accion)
        {
            var request = new
            {
                NombrePagina = nombrePagina,
                Accion = accion
            };

            return await RequestAsync<bool>($"api/Permisos/Validar", HttpMethod.Post, request, token.Token.access_token);
        }

        public async Task<ModelResponse<List<Pagina>>> ObtenerPaginas()
        {
            return await RequestAsync<List<Pagina>>($"api/Permisos/Paginas", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse<List<RolPaginaAccionDTO>>> ObtenerPermisosPorRol(long rolId)
        {
            return await RequestAsync<List<RolPaginaAccionDTO>>($"api/Permisos/Rol/{rolId}", HttpMethod.Get, null, token.Token.access_token);
        }

        public async Task<ModelResponse> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            var result = await RequestAsync<object>($"api/Permisos/Guardar", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse> GuardarPermisosRolMasivo(GuardarPermisosMasivoRequest request)
        {
            var result = await RequestAsync<object>($"api/Permisos/GuardarMasivo", HttpMethod.Post, request,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                }), token.Token.access_token);

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
            return modelResponse;
        }

        public async Task<ModelResponse<List<RolConteoPaginasDTO>>> ObtenerConteoPaginasPorRol()
        {
            return await RequestAsync<List<RolConteoPaginasDTO>>($"api/Permisos/ConteoPaginasPorRol", HttpMethod.Get, null, token.Token.access_token);
        }
    }
}
