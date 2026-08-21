using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
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
            return await _httpClient.ObtenerConfiguracionEvidencias();
        }

        public async Task<ModelResponse<List<TicketEvidencia>>> GuardarEvidencias(long ticketId, HttpFileCollectionBase files)
        {
            using (var form = new MultipartFormDataContent())
            {
                form.Add(new StringContent(ticketId.ToString()), "ticketId");

                if (files != null)
                {
                    foreach (string key in files.AllKeys)
                    {
                        var file = files[key];
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

                return await _httpClient.PostMultipartAsync<List<TicketEvidencia>>("api/Evidencia/Guardar", form);
            }
        }

        public async Task<ModelResponse<List<TicketEvidencia>>> ObtenerEvidenciasPorTicket(long ticketId)
        {
            return await _httpClient.ObtenerEvidenciasPorTicket(ticketId);
        }

        public async Task<EvidenciaDescargaDTO> ObtenerEvidenciaDescarga(long id)
        {
            return await _httpClient.ObtenerEvidenciaDescarga(id);
        }
    }
}
