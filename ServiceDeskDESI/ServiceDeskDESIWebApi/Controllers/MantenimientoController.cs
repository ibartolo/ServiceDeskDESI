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
    [RoutePrefix("api/Mantenimiento")]
    public class MantenimientoController : BaseController
    {
        private readonly MantenimientoService _mantenimientoService;

        public MantenimientoController()
        {
            _mantenimientoService = new MantenimientoService();
        }

        /// <summary>
        /// Obtiene el historial de mantenimientos de un activo (reutiliza permiso "Activos").
        /// </summary>
        [HttpGet, Route("PorActivo/{activoId:long}")]
        [Permiso("Activos", "Leer")]
        public ModelResponse<List<Mantenimiento>> ObtenerMantenimientosPorActivo(long activoId)
        {
            var usuario = User.Identity.Name;
            var result = _mantenimientoService.ObtenerMantenimientosPorActivo(activoId, usuario);
            return result;
        }

        /// <summary>
        /// Guarda un mantenimiento (reutiliza permiso "Activos" - Editar).
        /// La fecha y la empresa las asigna el SP (GETDATE() y derivación por @Usuario).
        /// </summary>
        [HttpPost, Route("Guardar")]
        [Permiso("Activos", "Editar")]
        public ModelResponse GuardarMantenimiento(Mantenimiento m)
        {
            var usuario = User.Identity.Name;
            var result = _mantenimientoService.GuardarMantenimiento(m, usuario);
            return result;
        }
    }
}
