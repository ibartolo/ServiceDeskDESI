using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
using System;

namespace ServiceDeskDESIWebApi.Services
{
    public class DashboardService
    {
        private readonly DbWrapper _dbWrapper;

        public DashboardService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<DashboardIndicadoresDTO> ObtenerIndicadores(string usuario)
        {
            try
            {
                Log.Information("DashboardService.ObtenerIndicadores para usuario {Usuario}", usuario);

                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var indicadores = _dbWrapper.ObtenerIndicadoresDashboard(usuario);

                var result = new ModelResponse<DashboardIndicadoresDTO>
                {
                    IsSuccess = true,
                    Response = indicadores ?? new DashboardIndicadoresDTO(),
                    Message = "Indicadores obtenidos correctamente."
                };

                Log.Information("DashboardService.ObtenerIndicadores RESULTADO: IsSuccess={IsSuccess}", result.IsSuccess);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerIndicadores para usuario {Usuario}", usuario);
                return new ModelResponse<DashboardIndicadoresDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en DashboardService.ObtenerIndicadores para usuario {Usuario}", usuario);
                return new ModelResponse<DashboardIndicadoresDTO> { IsSuccess = false, Message = "Ocurrió un error al obtener los indicadores." };
            }
        }
    }
}
