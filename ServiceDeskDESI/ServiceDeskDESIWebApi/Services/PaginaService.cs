using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;

namespace ServiceDeskDESIWebApi.Services
{
    public class PaginaService
    {
        private readonly DbWrapper _dbWrapper;

        public PaginaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerPaginasPorUsuario(string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerPaginasPorUsuario(usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPaginasPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginasPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }

        public ModelResponse ValidarAccesoPagina(string usuario, string direccion)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }
                if (string.IsNullOrWhiteSpace(direccion)) { throw new ArgumentException("La dirección de la página es requerida."); }

                return _dbWrapper.ValidarAccesoPagina(usuario, direccion);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ValidarAccesoPagina para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ValidarAccesoPagina para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al validar el acceso." };
            }
        }

        public ModelResponse ObtenerPaginaPorNombre(string nombre)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(nombre)) { throw new ArgumentException("El nombre de la página es requerido."); }

                return _dbWrapper.ObtenerPaginaPorNombre(nombre);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPaginaPorNombre para nombre {Nombre}", nombre);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginaPorNombre para nombre {Nombre}", nombre);
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener la página." };
            }
        }

        public ModelResponse ObtenerPaginas()
        {
            try
            {
                return _dbWrapper.ObtenerPaginas();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginas");
                return new ModelResponse { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }
    }
}