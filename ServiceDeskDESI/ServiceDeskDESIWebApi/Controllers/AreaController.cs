using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Area")]
    public class AreaController : BaseController
    {
        private readonly AreaService _areaService;

        public AreaController()
        {
            _areaService = new AreaService();
        }

        /// <summary>
        /// Obtiene todas las áreas de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de áreas</returns>
        [HttpGet, Route("List")]
        public ModelResponse ObtenerAreas()
        {
            var usuario = User.Identity.Name;
            var result = _areaService.ObtenerAreas(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene un área por su ID
        /// </summary>
        /// <param name="id">ID del área</param>
        /// <returns>Área encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse ObtenerAreaPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _areaService.ObtenerAreaPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un área
        /// </summary>
        /// <param name="area">Objeto área con los datos</param>
        /// <returns>Área guardada con su ID actualizado</returns>
        [HttpPost, Route("Guardar")]
        public ModelResponse GuardarOActualizarArea(Area area)
        {
            var usuario = User.Identity.Name;
            var result = _areaService.GuardarOActualizarArea(area, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un área
        /// </summary>
        /// <param name="area">Área a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarArea(Area area)
        {
            var usuario = User.Identity.Name;
            area.FechaModificacion = DateTime.Now;
            var result = _areaService.EliminarArea(area.Id, area.ModificadoPor, area.FechaModificacion.Value, usuario);
            return result;
        }
    }
}