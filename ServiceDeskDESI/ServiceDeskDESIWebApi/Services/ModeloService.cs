using Serilog;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class ModeloService
    {
        private readonly DbWrapper _dbWrapper;

        public ModeloService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<ModeloDTO>> ObtenerModelos(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerModelos(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerModelos para usuario {Usuario}", usuario);
                return new ModelResponse<List<ModeloDTO>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ModeloService.ObtenerModelos para usuario {Usuario}", usuario);
                return new ModelResponse<List<ModeloDTO>> { IsSuccess = false, Message = "Ocurrió un error al obtener los modelos." };
            }
        }

        public ModelResponse<ModeloDTO> ObtenerModeloPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del modelo es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerModeloPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerModeloPorId para usuario {Usuario}", usuario);
                return new ModelResponse<ModeloDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ModeloService.ObtenerModeloPorId para usuario {Usuario}", usuario);
                return new ModelResponse<ModeloDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener el modelo." };
            }
        }

        public ModelResponse<Modelo> GuardarOActualizarModelo(Modelo modelo, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(modelo.Nombre)) { throw new ArgumentException("El nombre del modelo es requerido."); }
                if (modelo.Nombre.Length > 250) { throw new ArgumentException("El nombre no puede exceder los 250 caracteres."); }
                if (modelo.Descripcion != null && modelo.Descripcion.Length > 250) { throw new ArgumentException("La descripción no puede exceder los 250 caracteres."); }
                //if (modelo.MarcaId == null || modelo.MarcaId <= 0) { throw new ArgumentException("La marca es requerida."); }
                if (string.IsNullOrWhiteSpace(modelo.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarModelo(modelo);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarModelo para usuario {Usuario}", usuario);
                return new ModelResponse<Modelo> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ModeloService.GuardarOActualizarModelo para usuario {Usuario}", usuario);
                return new ModelResponse<Modelo> { IsSuccess = false, Message = "Ocurrió un error al guardar el modelo." };
            }
        }

        public ModelResponse EliminarModelo(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID del modelo es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarModelo(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarModelo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ModeloService.EliminarModelo para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al eliminar el modelo." };
            }
        }

        public ModelResponse<List<Modelo>> ObtenerModelosPorMarcaId(long marcaId, string usuario)
        {
            try
            {
                if (marcaId <= 0) { throw new ArgumentException("El ID de la marca es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerModelosPorMarcaId(marcaId, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerModelosPorMarcaId para usuario {Usuario}", usuario);
                return new ModelResponse<List<Modelo>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ModeloService.ObtenerModelosPorMarcaId para usuario {Usuario}", usuario);
                return new ModelResponse<List<Modelo>> { IsSuccess = false, Message = "Ocurrió un error al obtener los modelos por marca." };
            }
        }
    }
}
