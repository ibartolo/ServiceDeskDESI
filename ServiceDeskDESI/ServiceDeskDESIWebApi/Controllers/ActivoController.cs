using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [RoutePrefix("api/activos")]
    public class ActivoController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerActivos()
        {
            var result = dbWrapper.ObtenerTodosLosActivos();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerActivosPorId(long id)
        {
            var result = dbWrapper.ObtenerActivoPorId(id);
            return result;
        }
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarActivos(Activo a)
        {
            var result = dbWrapper.GuardarOActualizarActivo(a);
            return result;
        }
        [HttpDelete, Route("")]
        public ModelResponse EliminarActivos(Activo a)
        {
            a.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarActivo(a.Id,a.ModificadoPor,a.FechaModificacion.Value);
            return result;
        }
    }
}