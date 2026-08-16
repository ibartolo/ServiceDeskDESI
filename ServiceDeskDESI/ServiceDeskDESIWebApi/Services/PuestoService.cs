using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;

namespace ServiceDeskDESIWebApi.Services
{
    public class PuestoService
    {
        private readonly DbWrapper _dbWrapper;

        public PuestoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerTodosLosPuestos(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerTodosLosPuestos(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodosLosPuestos para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.ObtenerTodosLosPuestos para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener los puestos." };
            }
        }

        public ModelResponse ObtenerPuestoPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del puesto es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerPuestoPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPuestoPorId para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.ObtenerPuestoPorId para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener el puesto." };
            }
        }

        public ModelResponse GuardarOActualizarPuesto(Puesto puesto, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(puesto.Nombre)) { throw new ArgumentException("El nombre del puesto es requerido."); }
                if (puesto.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (puesto.Descripcion != null && puesto.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(puesto.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarPuesto(puesto, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.GuardarOActualizarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el puesto." };
            }
        }

        public ModelResponse EliminarPuesto(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del puesto es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarPuesto(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.EliminarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el puesto." };
            }
        }
    }
}