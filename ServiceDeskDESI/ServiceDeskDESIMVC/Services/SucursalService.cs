using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
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
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("SucursalService.ObtenerSucursalPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerSucursalPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("SucursalService.ObtenerSucursalPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("SucursalService.ObtenerSucursalPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Sucursal>> GuardarOActualizarSucursal(Sucursal sucursal)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("SucursalService.GuardarOActualizarSucursal ENTRADA: SucursalId={SucursalId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sucursal?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarActualizarSucursal(sucursal);
            Log.Information("SucursalService.GuardarOActualizarSucursal SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarSucursal(Sucursal sucursal)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("SucursalService.EliminarSucursal ENTRADA: SucursalId={SucursalId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sucursal?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarSucursal(sucursal);
            Log.Information("SucursalService.EliminarSucursal SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Sucursal>>> ConsultarTodasSucursales()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("SucursalService.ConsultarTodasSucursales ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodasLasSucursales();
            Log.Information("SucursalService.ConsultarTodasSucursales SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaSucursal()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("SucursalService.ObtenerPermisosParaSucursal ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Sucursales");
                Log.Information("SucursalService.ObtenerPermisosParaSucursal SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("SucursalService.ObtenerPermisosParaSucursal SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
