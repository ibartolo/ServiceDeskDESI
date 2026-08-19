using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class SucursalService
    {
        private readonly HttpClientConnection _httpClient;

        public SucursalService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Sucursal> ObtenerSucursalPorId(long id)
        {
            var response = await _httpClient.ObtenerSucursalPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                return response.Response;
            }
            return null;
        }

        public async Task<ModelResponse<Sucursal>> GuardarOActualizarSucursal(Sucursal sucursal)
        {
            return await _httpClient.GuardarActualizarSucursal(sucursal);
        }

        public async Task<ModelResponse> EliminarSucursal(Sucursal sucursal)
        {
            return await _httpClient.EliminarSucursal(sucursal);
        }

        public async Task<ModelResponse<List<Sucursal>>> ConsultarTodasSucursales()
        {
            return await _httpClient.ObtenerTodasLasSucursales();
        }

        public async Task<object> ObtenerPermisosParaSucursal()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                return permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Sucursales");
            }
            return null;
        }
    }
}
