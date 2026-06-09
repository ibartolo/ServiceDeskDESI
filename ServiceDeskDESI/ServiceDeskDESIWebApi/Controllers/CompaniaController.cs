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
    [RoutePrefix("api/Compania")]
    public class CompaniaController : BaseController
    {
        //private DbWrapper dbWrapper;
        //public CompaniaController()
        //{
        //    dbWrapper = new DbWrapper();
        //}

        [HttpGet, Route("List/{empresaId:long}")]
        public ModelResponse ObtenerCompania(long empresaId)
        {
            var result = dbWrapper.ObtenerCompania(empresaId);
            return result;
        }
        [HttpGet, Route("{id:long}/{empresaId:long}")]
        public ModelResponse ObtenerCompaniaPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerCompaniaPorId(id,empresaId);
            return result;
        }

        [HttpPost, Route("Guardar/{empresaId:long}")]
        public ModelResponse GuardarOActualizarCompania(Compania c, long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarCompania(c,empresaId);
            return result;
        }

        [HttpDelete, Route("Compania")]
        public ModelResponse EliminarCompania(Compania c, long empresaId)
        {
            c.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarCompania(c.Id,c.ModificadoPor,c.FechaModificacion.Value,empresaId);
            return result;
        }

    }
}

