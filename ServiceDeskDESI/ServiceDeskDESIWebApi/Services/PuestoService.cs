using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class PuestoService
    {
        private readonly DbWrapper _dbWrapper;

        public PuestoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Puesto>> ObtenerTodosLosPuestos(string usuario)
        {
            try
            {
                Log.Information("PuestoService.ObtenerTodosLosPuestos para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerTodosLosPuestos(usuario);
                Log.Information("PuestoService.ObtenerTodosLosPuestos RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerTodosLosPuestos para usuario {Usuario}", usuario);
                return new ModelResponse<List<Puesto>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.ObtenerTodosLosPuestos para usuario {Usuario}", usuario);
                return new ModelResponse<List<Puesto>> { IsSuccess = false, Message = "Ocurrió un error al obtener los puestos." };
            }
        }

        public ModelResponse<Puesto> ObtenerPuestoPorId(long id, string usuario)
        {
            try
            {
                Log.Information("PuestoService.ObtenerPuestoPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del puesto es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerPuestoPorId(id, usuario);
                Log.Information("PuestoService.ObtenerPuestoPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPuestoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Puesto> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.ObtenerPuestoPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Puesto> { IsSuccess = false, Message = "Ocurrió un error al obtener el puesto." };
            }
        }

        public ModelResponse<Puesto> GuardarOActualizarPuesto(Puesto puesto, string usuario)
        {
            try
            {
                Log.Information("PuestoService.GuardarOActualizarPuesto para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(puesto.Nombre)) { throw new ArgumentException("El nombre del puesto es requerido."); }
                if (puesto.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (puesto.Descripcion != null && puesto.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(puesto.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarPuesto(puesto, usuario);
                Log.Information("PuestoService.GuardarOActualizarPuesto RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse<Puesto> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en PuestoService.GuardarOActualizarPuesto para usuario {Usuario}", usuario);
                return new ModelResponse<Puesto> { IsSuccess = false, Message = "Ocurrió un error al guardar el puesto." };
            }
        }

        public ModelResponse EliminarPuesto(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("PuestoService.EliminarPuesto para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del puesto es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarPuesto(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("PuestoService.EliminarPuesto RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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
