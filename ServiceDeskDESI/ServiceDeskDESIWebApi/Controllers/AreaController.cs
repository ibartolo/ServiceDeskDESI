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
    [RoutePrefix("api/Area")]
    public class AreaController : BaseController
    {
        /// <summary>
        /// Obtiene todas las áreas de la empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Lista de áreas</returns>
        [HttpGet, Route("List/{empresaId:long}")]
        public ModelResponse ObtenerAreas(long empresaId)
        {
            var result = dbWrapper.ObtenerAreas(empresaId);
            return result;
        }

        /// <summary>
        /// Obtiene un área por su ID
        /// </summary>
        /// <param name="id">ID del área</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Área encontrada</returns>
        [HttpGet, Route("{id:long}/{empresaId:long}")]
        public ModelResponse ObtenerAreaPorId(long id, long empresaId)
        {
            var result = dbWrapper.ObtenerAreaPorId(id, empresaId);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un área
        /// </summary>
        /// <param name="area">Objeto área con los datos</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Área guardada con su ID actualizado</returns>
        [HttpPost, Route("Guardar/{empresaId:long}")]
        public ModelResponse GuardarOActualizarArea(Area area, long empresaId)
        {
            var result = dbWrapper.GuardarOActualizarArea(area, empresaId);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un área
        /// </summary>
        /// <param name="area">Área a eliminar (debe incluir Id, ModificadoPor)</param>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Eliminar/{empresaId:long}")]
        public ModelResponse EliminarArea(Area area, long empresaId)
        {
            area.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarArea(area.Id, area.ModificadoPor, area.FechaModificacion.Value, empresaId);
            return result;
        }
    }
}