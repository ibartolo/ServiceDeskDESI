using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
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
        public async Task<ModelResponse> AutenticarUsuario(Usuario usuario)
        {
            var result = await RequestAsync<object>($"api/Autentication/autenticar", HttpMethod.Post, usuario,
                new Func<string, string>((responseString) =>
                {
                    return responseString;
                })); // No necesita token porque es el login

            var modelResponse = JsonConvert.DeserializeObject<ModelResponse>(result.ToString());

            return modelResponse;
        }
    }
}