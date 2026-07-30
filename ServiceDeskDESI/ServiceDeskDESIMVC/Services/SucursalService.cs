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

        public async Task<ModelResponse> ObtenerSucursalPorId(long id)
        {
            return await _httpClient.ObtenerSucursalPorId(id);
        }

        public async Task<ModelResponse> GuardarOActualizarSucursal(Sucursal sucursal)
        {
            return await _httpClient.GuardarActualizarSucursal(sucursal);
        }

        public async Task<ModelResponse> EliminarSucursal(Sucursal sucursal)
        {
            return await _httpClient.EliminarSucursal(sucursal);
        }

        public async Task<ModelResponse> ConsultarTodasSucursales()
        {
            return await _httpClient.ObtenerTodasLasSucursales();
        }

        public async Task<object> ObtenerPermisosParaSucursal()
        {
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var listaPermisos = JsonConvert.DeserializeObject<List<PermisosViewModel>>(permisosResponse.Response.ToString());
                return listaPermisos.FirstOrDefault(p => p.PaginaNombre == "Sucursales");
            }
            return null;
        }
    }
}