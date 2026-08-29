using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class TipoActivoService
    {
        private readonly DbWrapper _dbWrapper;

        public TipoActivoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<TipoActivo>> ObtenerTodosLosTipoActivos(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTodosLosTipoActivos(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodosLosTipoActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<TipoActivo>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TipoActivoService.ObtenerTodosLosTipoActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<TipoActivo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los tipos de activo." };
            }
        }

        public ModelResponse<TipoActivo> ObtenerTipoActivoPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del tipo de activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTipoActivoPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTipoActivoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TipoActivo> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TipoActivoService.ObtenerTipoActivoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<TipoActivo> { IsSuccess = false, Message = "Ocurrió un error al obtener el tipo de activo." };
            }
        }

        public ModelResponse<TipoActivo> GuardarOActualizarTipoActivo(TipoActivo tipoActivo, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoActivo.Nombre)) { throw new ArgumentException("El nombre del tipo de activo es requerido."); }
                if (tipoActivo.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (tipoActivo.Descripcion != null && tipoActivo.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(tipoActivo.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarTipoActivo(tipoActivo, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarTipoActivo para usuario {Usuario}", usuario);
                return new ModelResponse<TipoActivo> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TipoActivoService.GuardarOActualizarTipoActivo para usuario {Usuario}", usuario);
                return new ModelResponse<TipoActivo> { IsSuccess = false, Message = "Ocurrió un error al guardar el tipo de activo." };
            }
        }

        public ModelResponse EliminarTipoActivo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del tipo de activo es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarTipoActivo(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarTipoActivo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en TipoActivoService.EliminarTipoActivo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el tipo de activo." };
            }
        }
    }
}
