using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Services
{
    public class AreaService
    {
        private readonly HttpClientConnection _httpClient;

        public AreaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Area> ObtenerAreaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AreaService.ObtenerAreaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerAreaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("AreaService.ObtenerAreaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("AreaService.ObtenerAreaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Area>> GuardarOActualizarArea(Area area)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AreaService.GuardarOActualizarArea ENTRADA: AreaId={AreaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                area?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarArea(area);
            Log.Information("AreaService.GuardarOActualizarArea SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarArea(Area area)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AreaService.EliminarArea ENTRADA: AreaId={AreaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                area?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarArea(area);
            Log.Information("AreaService.EliminarArea SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Area>>> ConsultarTodasAreas()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AreaService.ConsultarTodasAreas ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerAreas();
            Log.Information("AreaService.ConsultarTodasAreas SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaArea()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AreaService.ObtenerPermisosParaArea ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Áreas");
                Log.Information("AreaService.ObtenerPermisosParaArea SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("AreaService.ObtenerPermisosParaArea SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
