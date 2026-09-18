using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class MantenimientoService
    {
        private readonly HttpClientConnection _httpClient;

        public MantenimientoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Mantenimiento>>> ObtenerMantenimientosPorActivo(long activoId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MantenimientoService.ObtenerMantenimientosPorActivo ENTRADA: ActivoId={ActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                activoId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerMantenimientosPorActivo(activoId);
            Log.Information("MantenimientoService.ObtenerMantenimientosPorActivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> GuardarMantenimiento(Mantenimiento mantenimiento)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("MantenimientoService.GuardarMantenimiento ENTRADA: MantenimientoId={MantenimientoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                mantenimiento?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarMantenimiento(mantenimiento);
            Log.Information("MantenimientoService.GuardarMantenimiento SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
