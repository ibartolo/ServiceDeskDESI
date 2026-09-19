using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Evidencia")]
    public class EvidenciaController : BaseController
    {
        private readonly EvidenciaService _evidenciaService;

        public EvidenciaController()
        {
            _evidenciaService = new EvidenciaService();
        }

        /// <summary>
        /// Obtiene la configuración de evidencias (límites y extensiones permitidas).
        /// </summary>
        [HttpGet, Route("Configuracion")]
        public ModelResponse<EvidenciaConfigDTO> Configuracion()
        {
            var config = _evidenciaService.ObtenerConfiguracion();
            return new ModelResponse<EvidenciaConfigDTO>
            {
                IsSuccess = true,
                Response = config,
                Message = "Configuración obtenida correctamente."
            };
        }

        /// <summary>
        /// Guarda uno o más archivos de evidencia asociados a un ticket (multipart/form-data).
        /// </summary>
        [Permiso("Tickets", "Leer")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<List<TicketEvidencia>> Guardar()
        {
            var usuario = User.Identity.Name;
            var empresaId = ObtenerEmpresaIdDesdeClaim();

            var files = HttpContext.Current.Request.Files;
            var ticketIdStr = HttpContext.Current.Request.Form["ticketId"];

            long ticketId;
            if (!long.TryParse(ticketIdStr, out ticketId))
                return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "TicketId requerido." };

            return _evidenciaService.GuardarEvidencias(ticketId, files, usuario, empresaId);
        }

        /// <summary>
        /// Obtiene las evidencias de un ticket.
        /// </summary>
        [Permiso("Tickets", "Leer")]
        [HttpGet, Route("PorTicket/{ticketId:long}")]
        public ModelResponse<List<TicketEvidencia>> PorTicket(long ticketId)
        {
            var usuario = User.Identity.Name;
            return _evidenciaService.ObtenerEvidenciasPorTicket(ticketId, usuario);
        }

        /// <summary>
        /// Descarga una evidencia como archivo adjunto (attachment).
        /// </summary>
        [Permiso("Tickets", "Leer")]
        [HttpGet, Route("Descargar/{id:long}")]
        public HttpResponseMessage Descargar(long id)
        {
            var usuario = User.Identity.Name;
            var result = _evidenciaService.ObtenerEvidenciaParaDescarga(id, usuario);

            if (!result.IsSuccess || result.Response == null)
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, result.Message ?? "Evidencia no encontrada.");

            var dto = result.Response;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(dto.Contenido)
            };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(dto.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = dto.NombreArchivo
            };

            return response;
        }
    }
}
