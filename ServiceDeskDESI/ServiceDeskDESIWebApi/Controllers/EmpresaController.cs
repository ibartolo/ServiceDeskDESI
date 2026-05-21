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
    [RoutePrefix("api/Empresas")]
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
        [HttpPost, Route("RFC")]
        public ModelResponse ObtenerEmpresasPorRFC(Empresa empresa)
        {
            var result = dbWrapper.ObtenerEmpresaPorRFC(empresa.RFC);
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
