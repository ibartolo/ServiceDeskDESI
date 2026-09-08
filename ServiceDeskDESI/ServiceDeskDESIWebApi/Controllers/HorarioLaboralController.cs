using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System.Collections.Generic;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/HorarioLaboral")]
    public class HorarioLaboralController : BaseController
    {
        private readonly HorarioLaboralService _service = new HorarioLaboralService();

        [HttpGet, Route("Obtener")]
        [Permiso("ConfiguracionEmpresa", "Leer")]
        public ModelResponse<List<HorarioLaboral>> Obtener()
            => _service.ObtenerHorarioLaboral(User.Identity.Name);

        [HttpPost, Route("Guardar")]
        [Permiso("ConfiguracionEmpresa", "Editar")]
        public ModelResponse Guardar(List<HorarioLaboral> horario)
            => _service.GuardarHorarioLaboral(User.Identity.Name, horario);
    }
}
