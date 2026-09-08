using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class MantenimientoService
    {
        private readonly DbWrapper _dbWrapper;

        public MantenimientoService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Mantenimiento>> ObtenerMantenimientosPorActivo(long activoId, string usuario)
        {
            try
            {
                Log.Information("MantenimientoService.ObtenerMantenimientosPorActivo para ActivoId {ActivoId} usuario {Usuario}", activoId, usuario);

                if (activoId <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerMantenimientosPorActivo(activoId, usuario);
                Log.Information("MantenimientoService.ObtenerMantenimientosPorActivo RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerMantenimientosPorActivo para usuario {Usuario}", usuario);
                return new ModelResponse<List<Mantenimiento>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MantenimientoService.ObtenerMantenimientosPorActivo para usuario {Usuario}", usuario);
                return new ModelResponse<List<Mantenimiento>> { IsSuccess = false, Message = "Ocurrió un error al obtener los mantenimientos." };
            }
        }

        public ModelResponse GuardarMantenimiento(Mantenimiento mantenimiento, string usuario)
        {
            try
            {
                Log.Information("MantenimientoService.GuardarMantenimiento para usuario {Usuario}", usuario);

                if (mantenimiento.ActivoId <= 0) { throw new ArgumentException("El ID del activo es requerido."); }
                if (string.IsNullOrWhiteSpace(mantenimiento.Comentario)) { throw new ArgumentException("El comentario es requerido."); }
                if (mantenimiento.Comentario.Length > 500) { throw new ArgumentException("El comentario no puede exceder los 500 caracteres."); }
                if (string.IsNullOrWhiteSpace(mantenimiento.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarMantenimiento(mantenimiento, usuario);
                Log.Information("MantenimientoService.GuardarMantenimiento RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarMantenimiento para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MantenimientoService.GuardarMantenimiento para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al guardar el mantenimiento." };
            }
        }
    }
}
