using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.DAL
{
    public partial class HttpClientConnection
    {
        public async Task<ModelResponse<FoliadorDTO>> ConsultarFoliador(string nombre = "Ticket")
        {
            return await RequestAsync<FoliadorDTO>($"api/Foliador/Consultar?nombre={nombre}", HttpMethod.Get, null, token.Token.access_token);
        }
    }
}
