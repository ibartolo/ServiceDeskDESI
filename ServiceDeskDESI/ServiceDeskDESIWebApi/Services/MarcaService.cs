using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class MarcaService
    {
        private readonly DbWrapper _dbWrapper;

        public MarcaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Marca>> ObtenerMarcas(string usuario)
        {
            try
            {
                Log.Information("MarcaService.ObtenerMarcas para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerMarcas(usuario);
                Log.Information("MarcaService.ObtenerMarcas RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerMarcas para usuario {Usuario}", usuario);
                return new ModelResponse<List<Marca>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MarcaService.ObtenerMarcas para usuario {Usuario}", usuario);
                return new ModelResponse<List<Marca>> { IsSuccess = false, Message = "Ocurrió un error al obtener las marcas." };
            }
        }

        public ModelResponse<Marca> ObtenerMarcaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("MarcaService.ObtenerMarcaPorId para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la marca es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerMarcaPorId(id, usuario);
                Log.Information("MarcaService.ObtenerMarcaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerMarcaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Marca> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MarcaService.ObtenerMarcaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Marca> { IsSuccess = false, Message = "Ocurrió un error al obtener la marca." };
            }
        }

        public ModelResponse<Marca> GuardarOActualizarMarca(Marca marca, string usuario)
        {
            try
            {
                Log.Information("MarcaService.GuardarOActualizarMarca para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(marca.Nombre)) { throw new ArgumentException("El nombre de la marca es requerido."); }
                if (marca.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (marca.Descripcion != null && marca.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(marca.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.GuardarOActualizarMarca(marca);
                Log.Information("MarcaService.GuardarOActualizarMarca RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarMarca para usuario {Usuario}", usuario);
                return new ModelResponse<Marca> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MarcaService.GuardarOActualizarMarca para usuario {Usuario}", usuario);
                return new ModelResponse<Marca> { IsSuccess = false, Message = "Ocurrió un error al guardar la marca." };
            }
        }

        public ModelResponse EliminarMarca(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                Log.Information("MarcaService.EliminarMarca para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la marca es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarMarca(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("MarcaService.EliminarMarca RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarMarca para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en MarcaService.EliminarMarca para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar la marca." };
            }
        }
    }
}
