using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
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
    [RoutePrefix("api/Catalogs")]
    public class CatalogsController : BaseController
    {
        private readonly CategoriaService _categoriaService;

        public CatalogsController()
        {
            _categoriaService = new CategoriaService();
        }

        /// <summary>
        /// Obtiene todas las categorías con jerarquía (ordenadas: padres primero, luego hijas)
        /// </summary>
        /// <returns>Lista de categorías ordenadas jerárquicamente</returns>
        [HttpGet, Route("Categoria/List")]
        public ModelResponse ObtenerCategorias()
        {
            var usuario = User.Identity.Name;
            var result = _categoriaService.ObtenerCategorias(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene todas las categorías por área
        /// </summary>
        /// <param name="areaId">ID del área</param>
        /// <returns>Lista de categorías con jerarquía</returns>
        [HttpGet, Route("Categoria/Lista/{areaId:long}")]
        public ModelResponse ObtenerCategoriasPorArea(long areaId)
        {
            var usuario = User.Identity.Name;
            var result = _categoriaService.ObtenerCategoriasPorArea(areaId, usuario);
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
            var usuario = User.Identity.Name;
            var result = _categoriaService.ObtenerCategoriaPorId(id, usuario);
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
            var usuario = User.Identity.Name;
            var result = _categoriaService.ObtenerCategoriasPorPadre(categoriaPadreId, usuario);
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
            var usuario = User.Identity.Name;
            var result = _categoriaService.GuardarOActualizarCategoria(categoria, usuario);
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
            var usuario = User.Identity.Name;
            categoria.FechaModificacion = DateTime.Now;
            var result = _categoriaService.EliminarCategoria(categoria.Id, categoria.ModificadoPor, categoria.FechaModificacion.Value, usuario);
            return result;
        }
    }
}