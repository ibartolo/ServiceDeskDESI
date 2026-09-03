using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Services;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Foliador")]
    public class FoliadorController : BaseController
    {
        private readonly FoliadorService _foliadorService;

        public FoliadorController()
        {
            _foliadorService = new FoliadorService(dbWrapper);
        }

        /// <summary>
        /// Consulta el foliador por Nombre para la empresa del usuario autenticado.
        /// Expone ÚNICAMENTE la consulta (el incremento es interno y no tiene endpoint).
        /// </summary>
        [HttpGet, Route("Consultar")]
        public ModelResponse<FoliadorDTO> Consultar(string nombre)
        {
            var usuario = User.Identity.Name;
            return _foliadorService.ConsultarConsecutivo(nombre, usuario);
        }
    }
}
