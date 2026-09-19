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
    public class TipoActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public TipoActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<TipoActivo> ObtenerTipoActivoPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TipoActivoService.ObtenerTipoActivoPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerTipoActivoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("TipoActivoService.ObtenerTipoActivoPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("TipoActivoService.ObtenerTipoActivoPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<TipoActivo>> GuardarOActualizarTipoActivo(TipoActivo tipoActivo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TipoActivoService.GuardarOActualizarTipoActivo ENTRADA: TipoActivoId={TipoActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                tipoActivo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarTipoActivo(tipoActivo);
            Log.Information("TipoActivoService.GuardarOActualizarTipoActivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarTipoActivo(TipoActivo tipoActivo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TipoActivoService.EliminarTipoActivo ENTRADA: TipoActivoId={TipoActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                tipoActivo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarTipoActivo(tipoActivo);
            Log.Information("TipoActivoService.EliminarTipoActivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<TipoActivo>>> ConsultarTodosLosTipoActivos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TipoActivoService.ConsultarTodosLosTipoActivos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosTipoActivos();
            Log.Information("TipoActivoService.ConsultarTodosLosTipoActivos SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaTipoActivo()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("TipoActivoService.ObtenerPermisosParaTipoActivo ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Tipo Activo");
                Log.Information("TipoActivoService.ObtenerPermisosParaTipoActivo SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("TipoActivoService.ObtenerPermisosParaTipoActivo SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
