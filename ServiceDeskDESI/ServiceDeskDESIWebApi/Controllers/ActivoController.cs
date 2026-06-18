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
    [RoutePrefix("api/Activos")]
    public class ActivoController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerActivos(long empresaId)
        {
            var result = dbWrapper.ObtenerTodosLosActivos(empresaId);
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerActivosPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerActivoPorId(id,empresaId);
            return result;
        }
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarActivos(Activo a)
        {
            var result = dbWrapper.GuardarOActualizarActivo(a);
            return result;
        }
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarActivos(Activo a, long empresaId)
        {
            a.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarActivo(a.Id,a.ModificadoPor,a.FechaModificacion.Value,empresaId);
            return result;
        }
    }
}