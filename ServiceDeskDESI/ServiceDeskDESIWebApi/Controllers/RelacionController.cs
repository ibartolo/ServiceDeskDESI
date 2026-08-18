using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
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
        [Permiso("Responsables por Categoría")]
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarRelaciones(UsuarioPagina r)
        {
            var result = dbWrapper.GuardarOActualizarRelacion(r);
            return result;
        }
    }
}
