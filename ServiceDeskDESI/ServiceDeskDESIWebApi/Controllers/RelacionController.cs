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
    [Authorize]
    [RoutePrefix("api/Relaciones")]
    public class RelacionController : BaseController
    {
        [AllowAnonymous]
        [HttpGet, Route("List")]
        public ModelResponse ObtenerRelaciones ()
        {
            var result = dbWrapper.ObtenerTodasRelaciones();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerRelacionesPorId (long id)
        {
            var result = dbWrapper.ObtenerRelacionPorId(id);
            return result;
        }
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarRelaciones(Relacion r)
        {
            var result = dbWrapper.GuardarOActualizarRelacion(r);
            return result;
        }
        public ModelResponse EliminarRelaciones (Relacion r)
        {
            var result = dbWrapper.EliminarEmpresa(r.Id, r.ModificadoPor, r.FechaModificacion.Value);
            return result;
        }
    }
}
