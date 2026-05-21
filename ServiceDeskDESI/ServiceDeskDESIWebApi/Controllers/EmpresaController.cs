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
    [RoutePrefix("api/empresas")]
    public class EmpresaController : BaseController
    {
        [HttpGet, Route("List")]
        public ModelResponse ObtenerEmpresas()
        {
            var result = dbWrapper.ObtenerTodasLasEmpresas();
            return result;
        }
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerEmpresasPorId(long id)
        {
            var result = dbWrapper.ObtenerEmpresasPorId(id);
            return result;
        }
        [HttpGet, Route("{id:string}")]
        public ModelResponse ObtenerEmpresasPorRFC(string rfc)
        {
            var result = dbWrapper.ObtenerEmpresaPorRFC(rfc);
            return result;
        }
        [HttpPost, Route("")]
        public ModelResponse GuardarOActualizarEmpresas(Empresa e)
        {
            var result = dbWrapper.GuardarOActualizarEmpresas(e);
            return result;
        }
        [HttpDelete, Route("")]
        public ModelResponse EliminarEmpresas(Empresa e)
        {
            e.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarEmpresa(e.Id, e.ModificadoPor, e.FechaModificacion.Value);
            return result;
        }
    }
}
