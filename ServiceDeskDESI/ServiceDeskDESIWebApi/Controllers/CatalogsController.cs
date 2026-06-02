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

        /// <summary>
        /// Obtiene todas las categorías con jerarquía (ordenadas: padres primero, luego hijas)
        /// </summary>
        /// <returns>Lista de categorías ordenadas jerárquicamente</returns>
        [HttpGet, Route("Categoria/List")]
        public ModelResponse ObtenerCategorias()
        {
            var result = dbWrapper.ObtenerCategorias();
            return result;
        }
    }
}
