using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class SucursalService
    {
        private readonly DbWrapper _dbWrapper;

        public SucursalService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Sucursal>> ObtenerSucursales(string usuario)
        {
            try
            {
                Log.Information("SucursalService.ObtenerSucursales para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerSucursales(usuario);
                Log.Information("SucursalService.ObtenerSucursales RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerSucursales para usuario {Usuario}", usuario);
                return new ModelResponse<List<Sucursal>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en SucursalService.ObtenerSucursales para usuario {Usuario}", usuario);
                return new ModelResponse<List<Sucursal>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las sucursales."
                };
            }
        }

        public ModelResponse<Sucursal> ObtenerSucursalPorId(long id, string usuario)
        {
            try
            {
                Log.Information("SucursalService.ObtenerSucursalPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la sucursal es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerSucursalPorId(id, usuario);
                Log.Information("SucursalService.ObtenerSucursalPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerSucursalPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Sucursal> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en SucursalService.ObtenerSucursalPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Sucursal>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la sucursal."
                };
            }
        }

        public ModelResponse<Sucursal> GuardarOActualizarSucursal(Sucursal sucursal, string usuario)
        {
            try
            {
                Log.Information("SucursalService.GuardarOActualizarSucursal para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(sucursal.Nombre)) { throw new ArgumentException("El nombre de la sucursal es requerido."); }
                if (sucursal.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (sucursal.Descripcion != null && sucursal.Descripcion.Length > 500) { throw new ArgumentException("La descripción no puede exceder los 500 caracteres."); }
                if (sucursal.Calle != null && sucursal.Calle.Length > 100) { throw new ArgumentException("La calle no puede exceder los 100 caracteres."); }
                if (sucursal.Ciudad != null && sucursal.Ciudad.Length > 100) { throw new ArgumentException("La ciudad no puede exceder los 100 caracteres."); }
                if (sucursal.Colonia != null && sucursal.Colonia.Length > 100) { throw new ArgumentException("La colonia no puede exceder los 100 caracteres."); }
                if (sucursal.CodigoPostal != null && sucursal.CodigoPostal.Length > 10) { throw new ArgumentException("El código postal no puede exceder los 10 caracteres."); }
                if (string.IsNullOrWhiteSpace(sucursal.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarSucursal(sucursal, usuario);
                Log.Information("SucursalService.GuardarOActualizarSucursal RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarSucursal para usuario {Usuario}", usuario);
                return new ModelResponse<Sucursal> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en SucursalService.GuardarOActualizarSucursal para usuario {Usuario}", usuario);
                return new ModelResponse<Sucursal>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la sucursal."
                };
            }
        }

        public ModelResponse EliminarSucursal(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("SucursalService.EliminarSucursal para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la sucursal es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarSucursal(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("SucursalService.EliminarSucursal RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarSucursal para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en SucursalService.EliminarSucursal para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar la sucursal."
                };
            }
        }
    }
}
