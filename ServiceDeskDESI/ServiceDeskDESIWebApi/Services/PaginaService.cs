using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;

namespace ServiceDeskDESIWebApi.Services
{
    public class PaginaService
    {
        private readonly DbWrapper _dbWrapper;

        public PaginaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<List<Pagina>> ObtenerPaginasPorUsuario(string usuario)
        {
            try
            {
                Log.Information("PaginaService.ObtenerPaginasPorUsuario para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerPaginasPorUsuario(usuario);
                Log.Information("PaginaService.ObtenerPaginasPorUsuario RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPaginasPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<Pagina>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginasPorUsuario para usuario {Usuario}", usuario);
                return new ModelResponse<List<Pagina>> { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }

        public ModelResponse<Pagina> ObtenerPaginaPorNombre(string nombre)
        {
            try
            {
                Log.Information("PaginaService.ObtenerPaginaPorNombre para Nombre {Nombre}", nombre);

                if (string.IsNullOrWhiteSpace(nombre)) { throw new ArgumentException("El nombre de la página es requerido."); }

                var result = _dbWrapper.ObtenerPaginaPorNombre(nombre);
                Log.Information("PaginaService.ObtenerPaginaPorNombre RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerPaginaPorNombre para nombre {Nombre}", nombre);
                return new ModelResponse<Pagina> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginaPorNombre para nombre {Nombre}", nombre);
                return new ModelResponse<Pagina> { IsSuccess = false, Message = "Ocurrió un error al obtener la página." };
            }
        }

        public ModelResponse<List<Pagina>> ObtenerPaginas()
        {
            try
            {
                Log.Information("PaginaService.ObtenerPaginas");

                var result = _dbWrapper.ObtenerPaginas();
                Log.Information("PaginaService.ObtenerPaginas RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en ObtenerPaginas");
                return new ModelResponse<List<Pagina>> { IsSuccess = false, Message = "Ocurrió un error al obtener las páginas." };
            }
        }
    }
}