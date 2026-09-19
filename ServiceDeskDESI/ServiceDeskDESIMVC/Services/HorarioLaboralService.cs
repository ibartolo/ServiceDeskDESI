using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class HorarioLaboralService
    {
        private readonly HttpClientConnection _httpClient;

        public HorarioLaboralService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<HorarioLaboral>>> ObtenerHorarioLaboral()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("HorarioLaboralService.ObtenerHorarioLaboral ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerHorarioLaboral();
            Log.Information("HorarioLaboralService.ObtenerHorarioLaboral SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> GuardarHorarioLaboral(List<HorarioLaboral> horario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("HorarioLaboralService.GuardarHorarioLaboral ENTRADA: Horarios={Horarios}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                horario?.Count, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarHorarioLaboral(horario);
            Log.Information("HorarioLaboralService.GuardarHorarioLaboral SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
