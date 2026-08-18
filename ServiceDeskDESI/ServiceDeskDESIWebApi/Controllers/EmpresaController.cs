using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
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

        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerEmpresasPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _empresaService.ObtenerEmpresaPorId(id, usuario);
            return result;
        }

        [Permiso("Compañías")]
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarEmpresa(Empresa e)
        {
            var usuario = User.Identity.Name;
            var result = _empresaService.GuardarOActualizarEmpresa(e, usuario);
            return result;
        }

        [Permiso("Compañías", "Crear")]
        [HttpPost, Route("Nueva")]
        public ModelResponse GuardarNuevaEmpresa(Empresa e)
        {
            var result = _empresaService.GuardarNuevaEmpresa(e);
            return result;
        }

        [Permiso("Compañías", "Crear")]
        [HttpPost, Route("NuevaCompleta")]
        public ModelResponse GuardarNuevaEmpresaCompleta(Empresa e)
        {
            var result = _empresaService.GuardarNuevaEmpresaConDatosIniciales(e);
            return result;
        }

        /// <summary>
        /// Registro de empresa pre-login (anónimo). Valida campos y unicidad server-side y
        /// crea la empresa con sus datos iniciales (sucursal, área, usuario admin, roles y permisos).
        /// </summary>
        /// <param name="e">Objeto empresa con los datos de registro</param>
        /// <returns>Resultado del registro</returns>
        [AllowAnonymous]
        [HttpPost, Route("Registrar")]
        public ModelResponse Registrar(Empresa e)
        {
            return _empresaService.RegistrarEmpresa(e);
        }

        [Permiso("Compañías", "Eliminar")]
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