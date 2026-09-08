using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class FoliadorService
    {
        private readonly HttpClientConnection _httpClient;

        public FoliadorService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Devuelve el folio "current+1" formateado para la vista previa de captura.
        /// </summary>
        public async Task<ModelResponse<FoliadorDTO>> ConsultarFolioSiguiente()
        {
            return await _httpClient.ConsultarFoliador("Ticket");
        }
    }
}
