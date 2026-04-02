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
    [Authorize]
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

        // =========================================
        // CATEGORÍAS
        // =========================================

        /// <summary>
        /// Obtiene todas las categorías por área
        /// </summary>
        /// <param name="areaId">ID del área</param>
        /// <returns>Lista de categorías con jerarquía</returns>
        [HttpGet, Route("Categoria/Lista/{areaId:long}")]
        public ModelResponse ObtenerCategoriasPorArea(long areaId)
        {
            var result = dbWrapper.ObtenerCategoriasPorArea(areaId);
            return result;
        }

        /// <summary>
        /// Obtiene una categoría por su ID
        /// </summary>
        /// <param name="id">ID de la categoría</param>
        /// <returns>Categoría encontrada</returns>
        [HttpGet, Route("Categoria/{id:long}")]
        public ModelResponse ObtenerCategoriaPorId(long id)
        {
            var result = dbWrapper.ObtenerCategoriaPorId(id);
            return result;
        }

        /// <summary>
        /// Obtiene subcategorías por categoría padre
        /// </summary>
        /// <param name="categoriaPadreId">ID de la categoría padre</param>
        /// <returns>Lista de subcategorías</returns>
        [HttpGet, Route("Categoria/Subcategorias/{categoriaPadreId:long}")]
        public ModelResponse ObtenerCategoriasPorPadre(long categoriaPadreId)
        {
            var result = dbWrapper.ObtenerCategoriasPorPadre(categoriaPadreId);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una categoría
        /// </summary>
        /// <param name="categoria">Objeto categoría con los datos</param>
        /// <returns>Categoría guardada con su ID actualizado</returns>
        [HttpPost, Route("Categoria")]
        public ModelResponse GuardarOActualizarCategoria(Categoria categoria)
        {
            var result = dbWrapper.GuardarOActualizarCategoria(categoria);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una categoría
        /// </summary>
        /// <param name="categoria">Categoría a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete, Route("Categoria")]
        public ModelResponse EliminarCategoria(Categoria categoria)
        {
            categoria.FechaModificacion = DateTime.Now;
            var result = dbWrapper.EliminarCategoria(categoria.Id, categoria.ModificadoPor, categoria.FechaModificacion.Value);
            return result;
        }
    }
}
