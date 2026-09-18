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
    public class PuestoService
    {
        private readonly HttpClientConnection _httpClient;

        public PuestoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Puesto> ObtenerPuestoPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PuestoService.ObtenerPuestoPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerPuestoPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("PuestoService.ObtenerPuestoPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("PuestoService.ObtenerPuestoPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Puesto>> GuardarOActualizarPuesto(Puesto puesto)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PuestoService.GuardarOActualizarPuesto ENTRADA: PuestoId={PuestoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                puesto?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarPuesto(puesto);
            Log.Information("PuestoService.GuardarOActualizarPuesto SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarPuesto(Puesto puesto)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PuestoService.EliminarPuesto ENTRADA: PuestoId={PuestoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                puesto?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarPuesto(puesto);
            Log.Information("PuestoService.EliminarPuesto SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Puesto>>> ConsultarTodosLosPuestos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PuestoService.ConsultarTodosLosPuestos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosPuestos();
            Log.Information("PuestoService.ConsultarTodosLosPuestos SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaPuesto()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PuestoService.ObtenerPermisosParaPuesto ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Puestos");
                Log.Information("PuestoService.ObtenerPermisosParaPuesto SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("PuestoService.ObtenerPermisosParaPuesto SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
