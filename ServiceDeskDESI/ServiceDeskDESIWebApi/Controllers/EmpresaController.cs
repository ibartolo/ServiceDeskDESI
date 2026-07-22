using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Services;
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
        private readonly EmpresaService _empresaService;

        public EmpresaController()
        {
            _empresaService = new EmpresaService();
        }

        [AllowAnonymous]
        [HttpGet, Route("List")]
        public ModelResponse ObtenerEmpresas()
        {
            var result = _empresaService.ObtenerTodasLasEmpresas();
            return result;
        }

        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerEmpresasPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _empresaService.ObtenerEmpresaPorId(id, usuario);
            return result;
        }

        [HttpPost, Route("RFC")]
        public ModelResponse ObtenerEmpresasPorRFC(Empresa empresa)
        {
            var result = _empresaService.ObtenerEmpresaPorRFC(empresa.RFC);
            return result;
        }

        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarEmpresa(Empresa e)
        {
            var usuario = User.Identity.Name;
            var result = _empresaService.GuardarOActualizarEmpresa(e, usuario);
            return result;
        }

        [AllowAnonymous]
        [HttpPost, Route("Nueva")]
        public ModelResponse GuardarNuevaEmpresa(Empresa e)
        {
            var result = _empresaService.GuardarNuevaEmpresa(e);
            return result;
        }

        [AllowAnonymous]
        [HttpPost, Route("NuevaCompleta")]
        public ModelResponse GuardarNuevaEmpresaCompleta(Empresa e)
        {
            var result = _empresaService.GuardarNuevaEmpresaConDatosIniciales(e);
            return result;
        }

        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarEmpresa(Empresa e)
        {
            var usuario = User.Identity.Name;
            e.FechaModificacion = DateTime.Now;
            var result = _empresaService.EliminarEmpresa(e.Id, e.ModificadoPor, e.FechaModificacion.Value, usuario);
            return result;
        }
    }
}