using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class CompaniaService
    {
        private readonly DbWrapper _dbWrapper;

        public CompaniaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Compania>> ObtenerCompanias(string usuario)
        {
            try
            {
                Log.Information("CompaniaService.ObtenerCompanias para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCompanias(usuario);
                Log.Information("CompaniaService.ObtenerCompanias RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCompanias para usuario {Usuario}", usuario);
                return new ModelResponse<List<Compania>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CompaniaService.ObtenerCompanias para usuario {Usuario}", usuario);
                return new ModelResponse<List<Compania>> { IsSuccess = false, Message = "Ocurrió un error al obtener las compañías." };
            }
        }

        public ModelResponse<Compania> ObtenerCompaniaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("CompaniaService.ObtenerCompaniaPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la compañía es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerCompaniaPorId(id, usuario);
                Log.Information("CompaniaService.ObtenerCompaniaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerCompaniaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Compania> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CompaniaService.ObtenerCompaniaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Compania> { IsSuccess = false, Message = "Ocurrió un error al obtener la compañía." };
            }
        }

        public ModelResponse<Compania> GuardarOActualizarCompania(Compania compania, string usuario)
        {
            try
            {
                Log.Information("CompaniaService.GuardarOActualizarCompania para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(compania.Nombre)) { throw new ArgumentException("El nombre de la compañía es requerido."); }
                if (compania.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (compania.Acronimo != null && compania.Acronimo.Length > 50) { throw new ArgumentException("El acrónimo no puede exceder los 50 caracteres."); }
                if (compania.RFC != null && compania.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (compania.Direccion != null && compania.Direccion.Length > 250) { throw new ArgumentException("La dirección no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(compania.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarCompania(compania, usuario);
                Log.Information("CompaniaService.GuardarOActualizarCompania RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarCompania para usuario {Usuario}", usuario);
                return new ModelResponse<Compania> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CompaniaService.GuardarOActualizarCompania para usuario {Usuario}", usuario);
                return new ModelResponse<Compania> { IsSuccess = false, Message = "Ocurrió un error al guardar la compañía." };
            }
        }

        public ModelResponse EliminarCompania(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("CompaniaService.EliminarCompania para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la compañía es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarCompania(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("CompaniaService.EliminarCompania RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarCompania para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en CompaniaService.EliminarCompania para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar la compañía." };
            }
        }
    }
}