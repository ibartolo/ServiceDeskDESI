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
    [RoutePrefix("api/Empresas")]
    public class EmpresaController : BaseController
    {
        [AllowAnonymous]
        [HttpGet, Route("List/{empresaId:long}")]
        public ModelResponse ObtenerEmpresas(long empresaId)
        {
            var result = dbWrapper.ObtenerTodasLasEmpresas(empresaId);
            return result;
        }
        [HttpGet, Route("{id:long}/{empresaId:long}")]
        public ModelResponse ObtenerEmpresasPorId(long id,long empresaId)
        {
            var result = dbWrapper.ObtenerEmpresasPorId(id,empresaId);
            return result;
        }
        [HttpPost, Route("RFC")]
        public ModelResponse ObtenerEmpresasPorRFC(Empresa empresa)
        {
            var result = dbWrapper.ObtenerEmpresaPorRFC(empresa.RFC);
            return result;
        }
        [HttpPost, Route("Guardar/{empresaId:long}")]
        public ModelResponse GuardarOActualizarEmpresas(Empresa e,long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarEmpresas(e,empresaId);
            return result;
        }
        [AllowAnonymous]
        [HttpPost, Route("Nueva")]
        public ModelResponse GuardarNuevaEmpresas(Empresa e)
        {
            var result = dbWrapper.GuardarNuevaEmpresaConDatosIniciales(e);
            return result;
        }
        [HttpDelete, Route("Eliminar/{empresaId:long}")]
        public ModelResponse EliminarEmpresas(Empresa e, long empresaId)
        {
            e.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarEmpresa(e.Id, e.ModificadoPor, e.FechaModificacion.Value,empresaId);
            return result;
        }
    }
}
