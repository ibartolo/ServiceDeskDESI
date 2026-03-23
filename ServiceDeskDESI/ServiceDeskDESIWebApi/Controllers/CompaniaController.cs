using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [RoutePrefix("api/compania")]
    public class CompaniaController : ApiController
    {
        private DbWrapper dbWrapper;
        public CompaniaController()
        {
            dbWrapper = new DbWrapper();
        }

        [HttpGet, Route("List")]
        public ModelResponse ObtenerCompania()
        {
            var result = dbWrapper.ObtenerCompania();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerCompaniaPorId(long id)
        {
            var result = dbWrapper.ObtenerCompaniaPorId(id);
            return result;
        }

        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarCompania(Compania c)
        {
            var result = dbWrapper.GuardarOActualizarCompania(c);
            return result;
        }

        [HttpDelete, Route("")]
        public ModelResponse EliminarCompania(Compania c)
        {
            c.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarCompania(c.Id, c.ModificadoPor, c.FechaModificacion.Value);
            return result;
        }

    }
}

