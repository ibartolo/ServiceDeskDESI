using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
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
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("FoliadorService.ConsultarFolioSiguiente ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ConsultarFoliador("Ticket");
            Log.Information("FoliadorService.ConsultarFolioSiguiente SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
