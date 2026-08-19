using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class CategoriaResponsableService
    {
        private readonly DbWrapper _dbWrapper;

        public CategoriaResponsableService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerResponsablesPorCategoria(long categoriaId, string usuario)
        {
            try
            {
                if (categoriaId <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerResponsablesPorCategoria(categoriaId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerResponsablesPorCategoria para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaResponsableService.ObtenerResponsablesPorCategoria para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los responsables."
                };
            }
        }

        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerCategoriasPorResponsable(long usuarioId, string usuario)
        {
            try
            {
                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerCategoriasPorResponsable(usuarioId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCategoriasPorResponsable para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaResponsableService.ObtenerCategoriasPorResponsable para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las categorías."
                };
            }
        }

        public ModelResponse<CategoriaResponsable> GuardarOActualizarCategoriaResponsable(CategoriaResponsable categoriaResponsable, string usuario)
        {
            try
            {
                if (categoriaResponsable.CategoriaId <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (categoriaResponsable.UsuarioId <= 0) { throw new ArgumentException("El usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(categoriaResponsable.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarCategoriaResponsable(categoriaResponsable, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarCategoriaResponsable para usuario {Usuario}", usuario);
                return new ModelResponse<CategoriaResponsable> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaResponsableService.GuardarOActualizarCategoriaResponsable para usuario {Usuario}", usuario);
                return new ModelResponse<CategoriaResponsable>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar el responsable."
                };
            }
        }

        public ModelResponse EliminarCategoriaResponsable(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del responsable es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarCategoriaResponsable(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarCategoriaResponsable para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaResponsableService.EliminarCategoriaResponsable para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar el responsable."
                };
            }
        }
    }
}
