using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class AreaService
    {
        private readonly DbWrapper _dbWrapper;

        public AreaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Area>> ObtenerAreas(string usuario)
        {
            try
            {
                Log.Information("AreaService.ObtenerAreas para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerAreas(usuario);
                Log.Information("AreaService.ObtenerAreas RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerAreas para usuario {Usuario}", usuario);
                return new ModelResponse<List<Area>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.ObtenerAreas para usuario {Usuario}", usuario);
                return new ModelResponse<List<Area>> { IsSuccess = false, Message = "Ocurrió un error al obtener las áreas." };
            }
        }

        public ModelResponse<Area> ObtenerAreaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("AreaService.ObtenerAreaPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerAreaPorId(id, usuario);
                Log.Information("AreaService.ObtenerAreaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerAreaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Area> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.ObtenerAreaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Area> { IsSuccess = false, Message = "Ocurrió un error al obtener el área." };
            }
        }

        public ModelResponse<Area> GuardarOActualizarArea(Area area, string usuario)
        {
            try
            {
                Log.Information("AreaService.GuardarOActualizarArea para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(area.Nombre)) { throw new ArgumentException("El nombre del área es requerido."); }
                if (area.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (area.Descripcion != null && area.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (area.Correo != null && area.Correo.Length > 100) { throw new ArgumentException("El correo no puede exceder los 100 caracteres."); }
                if (string.IsNullOrWhiteSpace(area.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarArea(area);
                Log.Information("AreaService.GuardarOActualizarArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarArea para usuario {Usuario}", usuario);
                return new ModelResponse<Area> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.GuardarOActualizarArea para usuario {Usuario}", usuario);
                return new ModelResponse<Area> { IsSuccess = false, Message = "Ocurrió un error al guardar el área." };
            }
        }

        public ModelResponse EliminarArea(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("AreaService.EliminarArea para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID del área es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarArea(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("AreaService.EliminarArea RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarArea para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en AreaService.EliminarArea para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el área." };
            }
        }
    }
}