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
    public class ActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public ActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ActivoDTO> ObtenerActivoPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ActivoService.ObtenerActivoPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerActivoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("ActivoService.ObtenerActivoPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("ActivoService.ObtenerActivoPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Activo>> GuardarOActualizarActivo(Activo activo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ActivoService.GuardarOActualizarActivo ENTRADA: ActivoId={ActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                activo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarActivo(activo);
            Log.Information("ActivoService.GuardarOActualizarActivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarActivo(Activo activo)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ActivoService.EliminarActivo ENTRADA: ActivoId={ActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                activo?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarActivo(activo);
            Log.Information("ActivoService.EliminarActivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<ActivoDTO>>> ConsultarTodosLosActivos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ActivoService.ConsultarTodosLosActivos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosActivos();
            Log.Information("ActivoService.ConsultarTodosLosActivos SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaActivo()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("ActivoService.ObtenerPermisosParaActivo ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Activos");
                Log.Information("ActivoService.ObtenerPermisosParaActivo SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("ActivoService.ObtenerPermisosParaActivo SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
