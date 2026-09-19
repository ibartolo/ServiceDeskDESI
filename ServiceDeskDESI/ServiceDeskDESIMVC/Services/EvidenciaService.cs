using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace ServiceDeskDESIMVC.Services
{
    public class EvidenciaService
    {
        private readonly HttpClientConnection _httpClient;

        public EvidenciaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<EvidenciaConfigDTO>> ObtenerConfiguracion()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EvidenciaService.ObtenerConfiguracion ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerConfiguracionEvidencias();
            Log.Information("EvidenciaService.ObtenerConfiguracion SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TicketEvidencia>>> GuardarEvidencias(long ticketId, HttpFileCollectionBase files)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EvidenciaService.GuardarEvidencias ENTRADA: TicketId={TicketId}, Archivos={Archivos}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, files?.Count, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(ticketId.ToString()), "ticketId");

                if (files != null)
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        if (file == null) continue;

                        byte[] bytes;
                        using (var ms = new MemoryStream())
                        {
                            if (file.InputStream.CanSeek) file.InputStream.Position = 0;
                            file.InputStream.CopyTo(ms);
                            bytes = ms.ToArray();
                        }

                        var content = new ByteArrayContent(bytes);
                        content.Headers.ContentType = new MediaTypeHeaderValue(
                            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

                        form.Add(content, "archivos", file.FileName);
                    }
                }

                var resultado = await _httpClient.PostMultipartAsync<List<TicketEvidencia>>("api/Evidencia/Guardar", form);
                Log.Information("EvidenciaService.GuardarEvidencias SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                    resultado?.IsSuccess, resultado?.Message);
                return resultado;
            }
        }

        public async Task<ModelResponse<List<TicketEvidencia>>> ObtenerEvidenciasPorTicket(long ticketId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EvidenciaService.ObtenerEvidenciasPorTicket ENTRADA: TicketId={TicketId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                ticketId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerEvidenciasPorTicket(ticketId);
            Log.Information("EvidenciaService.ObtenerEvidenciasPorTicket SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<EvidenciaDescargaDTO> ObtenerEvidenciaDescarga(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("EvidenciaService.ObtenerEvidenciaDescarga ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerEvidenciaDescarga(id);
            Log.Information("EvidenciaService.ObtenerEvidenciaDescarga SALIDA: ResultadoObtenido={ResultadoObtenido}",
                resultado != null);
            return resultado;
        }
    }
}
