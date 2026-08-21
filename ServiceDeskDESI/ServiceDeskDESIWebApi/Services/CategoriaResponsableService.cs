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
                Log.Information("CategoriaResponsableService.ObtenerResponsablesPorCategoria para CategoriaId {CategoriaId} usuario {Usuario}", categoriaId, usuario);

                if (categoriaId <= 0) { throw new ArgumentException("El ID de la categoría es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerResponsablesPorCategoria(categoriaId, usuario);
                Log.Information("CategoriaResponsableService.ObtenerResponsablesPorCategoria RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("CategoriaResponsableService.ObtenerCategoriasPorResponsable para UsuarioId {UsuarioId} usuario {Usuario}", usuarioId, usuario);

                if (usuarioId <= 0) { throw new ArgumentException("El ID del usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCategoriasPorResponsable(usuarioId, usuario);
                Log.Information("CategoriaResponsableService.ObtenerCategoriasPorResponsable RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("CategoriaResponsableService.GuardarOActualizarCategoriaResponsable para usuario {Usuario}", usuario);

                if (categoriaResponsable.CategoriaId <= 0) { throw new ArgumentException("La categoría es requerida."); }
                if (categoriaResponsable.UsuarioId <= 0) { throw new ArgumentException("El usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(categoriaResponsable.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarCategoriaResponsable(categoriaResponsable, usuario);
                Log.Information("CategoriaResponsableService.GuardarOActualizarCategoriaResponsable RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
                Log.Information("CategoriaResponsableService.EliminarCategoriaResponsable para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del responsable es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarCategoriaResponsable(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("CategoriaResponsableService.EliminarCategoriaResponsable RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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

        public ModelResponse<List<CategoriaResponsableDTO>> ObtenerTodosLosResponsables(string usuario)
        {
            try
            {
                Log.Information("CategoriaResponsableService.ObtenerTodosLosResponsables para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTodosLosResponsables(usuario);
                Log.Information("CategoriaResponsableService.ObtenerTodosLosResponsables RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodosLosResponsables para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CategoriaResponsableService.ObtenerTodosLosResponsables para usuario {Usuario}", usuario);
                return new ModelResponse<List<CategoriaResponsableDTO>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener los responsables."
                };
            }
        }
    }
}
