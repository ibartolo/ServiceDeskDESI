using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
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
            return await _httpClient.ObtenerHorarioLaboral();
        }

        public async Task<ModelResponse> GuardarHorarioLaboral(List<HorarioLaboral> horario)
        {
            return await _httpClient.GuardarHorarioLaboral(horario);
        }
    }
}
