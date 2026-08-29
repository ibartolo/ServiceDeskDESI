using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class ActivoService
    {
        private readonly DbWrapper _dbWrapper;

        public ActivoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<ActivoDTO>> ObtenerTodosLosActivos(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTodosLosActivos(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodosLosActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<ActivoDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ActivoService.ObtenerTodosLosActivos para usuario {Usuario}", usuario);
                return new ModelResponse<List<ActivoDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los activos." };
            }
        }

        public ModelResponse<ActivoDTO> ObtenerActivoPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerActivoPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerActivoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<ActivoDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ActivoService.ObtenerActivoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<ActivoDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener el activo." };
            }
        }

        public ModelResponse<Activo> GuardarOActualizarActivo(Activo activo, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(activo.Nombre)) { throw new ArgumentException("El nombre del activo es requerido."); }
                if (activo.Nombre.Length > 50) { throw new ArgumentException("El nombre no puede exceder los 50 caracteres."); }
                if (activo.Descripcion != null && activo.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if ((activo.TipoActivoId ?? 0) <= 0) { throw new ArgumentException("El tipo de activo es requerido."); }
                if ((activo.MarcaId ?? 0) <= 0) { throw new ArgumentException("La marca es requerida."); }
                if ((activo.ModeloId ?? 0) <= 0) { throw new ArgumentException("El modelo es requerido."); }
                if (activo.Serial != null && activo.Serial.Length > 50) { throw new ArgumentException("El serial no puede exceder los 50 caracteres."); }
                if (activo.Notas != null && activo.Notas.Length > 250) { throw new ArgumentException("Las notas no pueden exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(activo.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarActivo(activo, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarActivo para usuario {Usuario}", usuario);
                return new ModelResponse<Activo> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ActivoService.GuardarOActualizarActivo para usuario {Usuario}", usuario);
                return new ModelResponse<Activo> { IsSuccess = false, Message = "Ocurrió un error al guardar el activo." };
            }
        }

        public ModelResponse EliminarActivo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarActivo(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarActivo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ActivoService.EliminarActivo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el activo." };
            }
        }
    }
}
