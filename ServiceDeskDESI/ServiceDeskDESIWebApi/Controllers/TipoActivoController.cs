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
    [RoutePrefix("api/TipoActivo")]
    public class TipoActivoController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerTipoActivo(long empresaId)
        {
            var result = dbWrapper.ObtenerTodosTipoActivos(empresaId);
            return result;

        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerTipoActivoPorId(long id,long empresaId)
        {
            var result = dbWrapper.ObtenerTipoActivoPorId(id,empresaId);
            return result;
        }
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarTipoActivo(TipoActivo t)
        {
            var result = dbWrapper.GuardarOActualizarTipoActivo(t);
            return result;
        }

        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarTipoActivo(TipoActivo t)
        {
            t.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarTipoActivo(t);
            return result;
        }

    }
}
