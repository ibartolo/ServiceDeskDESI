using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class CategoriaService
    {
        private readonly DbWrapper _dbWrapper;

        public CategoriaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<CategoriaDTO>> ObtenerCategorias(string usuario)
        {
            try
            {
                Log.Information("CategoriaService.ObtenerCategorias para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCategorias(usuario);
                Log.Information("CategoriaService.ObtenerCategorias RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCategorias para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.ObtenerCategorias para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las categorías."
                };
            }
        }

        public ModelResponse<List<CategoriaDTO>> ObtenerCategoriasPorArea(long areaId, string usuario)
        {
            try
            {
                Log.Information("CategoriaService.ObtenerCategoriasPorArea para AreaId {AreaId} usuario {Usuario}", areaId, usuario);

                if (areaId <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCategoriasPorArea(areaId, usuario);
                Log.Information("CategoriaService.ObtenerCategoriasPorArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCategoriasPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.ObtenerCategoriasPorArea para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las categorías por área."
                };
            }
        }

        public ModelResponse<CategoriaDTO> ObtenerCategoriaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("CategoriaService.ObtenerCategoriaPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCategoriaPorId(id, usuario);
                Log.Information("CategoriaService.ObtenerCategoriaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCategoriaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<CategoriaDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.ObtenerCategoriaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<CategoriaDTO>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la categoría."
                };
            }
        }

        public ModelResponse<List<CategoriaDTO>> ObtenerCategoriasPorPadre(long categoriaPadreId, string usuario)
        {
            try
            {
                Log.Information("CategoriaService.ObtenerCategoriasPorPadre para CategoriaPadreId {CategoriaPadreId} usuario {Usuario}", categoriaPadreId, usuario);

                if (categoriaPadreId <= 0) { throw new ArgumentException("El ID de la categoría padre es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCategoriasPorPadre(categoriaPadreId, usuario);
                Log.Information("CategoriaService.ObtenerCategoriasPorPadre RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCategoriasPorPadre para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.ObtenerCategoriasPorPadre para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las subcategorías."
                };
            }
        }

        public ModelResponse<Categoria> GuardarOActualizarCategoria(Categoria categoria, string usuario)
        {
            try
            {
                Log.Information("CategoriaService.GuardarOActualizarCategoria para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(categoria.Nombre)) { throw new ArgumentException("El nombre de la categoría es requerido."); }
                if (categoria.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (categoria.Descripcion != null && categoria.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (categoria.AreaId <= 0) { throw new ArgumentException("El área es requerida."); }
                if (categoria.CategoriaPadreId != null && categoria.CategoriaPadreId == categoria.Id) { throw new ArgumentException("La categoría no puede ser padre de sí misma."); }
                if (string.IsNullOrWhiteSpace(categoria.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarCategoria(categoria, usuario);
                Log.Information("CategoriaService.GuardarOActualizarCategoria RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarCategoria para usuario {Usuario}", usuario);
                return new ModelResponse<Categoria> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.GuardarOActualizarCategoria para usuario {Usuario}", usuario);
                return new ModelResponse<Categoria>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la categoría."
                };
            }
        }

        public ModelResponse EliminarCategoria(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("CategoriaService.EliminarCategoria para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarCategoria(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("CategoriaService.EliminarCategoria RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarCategoria para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaService.EliminarCategoria para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar la categoría."
                };
            }
        }
    }
}
