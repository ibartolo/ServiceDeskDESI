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
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerCompanias(usuario);
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
                if (id <= 0) { throw new ArgumentException("El ID de la compañía es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerCompaniaPorId(id, usuario);
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
                if (string.IsNullOrWhiteSpace(compania.Nombre)) { throw new ArgumentException("El nombre de la compañía es requerido."); }
                if (compania.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (compania.Acronimo != null && compania.Acronimo.Length > 50) { throw new ArgumentException("El acrónimo no puede exceder los 50 caracteres."); }
                if (compania.RFC != null && compania.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (compania.Direccion != null && compania.Direccion.Length > 250) { throw new ArgumentException("La dirección no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(compania.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarCompania(compania, usuario);
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
                if (id <= 0) { throw new ArgumentException("El ID de la compañía es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarCompania(id, modificadoPor, fechaModificacion, usuario);
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