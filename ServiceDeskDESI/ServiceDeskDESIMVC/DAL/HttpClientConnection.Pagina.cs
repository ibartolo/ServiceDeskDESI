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
        public async Task<ModelResponse<List<Pagina>>> ObtenerPaginasPorUsuario()
        {
            return await RequestAsync<List<Pagina>>($"api/Pagina/List", HttpMethod.Get, null, token.Token.access_token);
        }
    }
}