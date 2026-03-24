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
    [RoutePrefix("api/Catalogs")]
    public class CatalogsController : BaseController
    {
        /// <summary>
        /// Obtiene todas las áreas activas
        /// </summary>
        /// <returns>Lista de áreas</returns>
        [HttpGet, Route("Area/Lista")]
        public ModelResponse ObtenerAreas()
        {
            var result = dbWrapper.ObtenerAreas();
            return result;
        }

        /// <summary>
        /// Obtiene un área por su ID
        /// </summary>
        /// <param name="id">ID del área</param>
        /// <returns>Área encontrada</returns>
        [HttpGet, Route("Area/{id:long}")]
        public ModelResponse ObtenerAreaPorId(long id)
        {
            var result = dbWrapper.ObtenerAreaPorId(id);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza un área
        /// </summary>
        /// <param name="a">Objeto área con los datos</param>
        /// <returns>Área guardada con su ID actualizado</returns>
        [HttpPost, Route("Area")]
        public ModelResponse GuardarOActualizarArea(Area a)
        {
            var result = dbWrapper.GuardarOActualizarArea(a);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente un área
        /// </summary>
        /// <param name="a">Área a eliminar (debe incluir Id, ModificadoPor y FechaModificacion)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Area")]
        public ModelResponse EliminarArea(Area a)
        {
            a.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarArea(a);
            return result;
        }
    }
}
