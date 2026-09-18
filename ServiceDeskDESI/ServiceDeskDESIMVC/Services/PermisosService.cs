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
    public class PermisosService
    {
        private readonly HttpClientConnection _httpClient;

        public PermisosService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<PermisosViewModel>>> ObtenerPermisosPorUsuario()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerPermisosPorUsuario ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerPermisosPorUsuario();
            Log.Information("PermisosService.ObtenerPermisosPorUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<bool>> ValidarPermisoUsuario(string nombrePagina, string accion)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ValidarPermisoUsuario ENTRADA: NombrePagina={NombrePagina}, Accion={Accion}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                nombrePagina, accion, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
            Log.Information("PermisosService.ValidarPermisoUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Pagina>>> ObtenerPaginas()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerPaginas ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerPaginas();
            Log.Information("PermisosService.ObtenerPaginas SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<RolPaginaAccionDTO>>> ObtenerPermisosPorRol(long rolId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerPermisosPorRol ENTRADA: RolId={RolId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                rolId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerPermisosPorRol(rolId);
            Log.Information("PermisosService.ObtenerPermisosPorRol SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> GuardarPermisosRol(GuardarPermisosRequest request)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.GuardarPermisosRol ENTRADA: RolId={RolId}, PaginaId={PaginaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                request?.RolId, request?.PaginaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarPermisosRol(request);
            Log.Information("PermisosService.GuardarPermisosRol SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> GuardarPermisosRolMasivo(GuardarPermisosMasivoRequest request)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.GuardarPermisosRolMasivo ENTRADA: RolId={RolId}, Permisos={Permisos}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                request?.RolId, request?.Permisos?.Count, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarPermisosRolMasivo(request);
            Log.Information("PermisosService.GuardarPermisosRolMasivo SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<List<PermisosViewModel>> ObtenerPermisosParaPagina(string nombrePagina)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerPermisosParaPagina ENTRADA: NombrePagina={NombrePagina}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                nombrePagina, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerPermisosPorUsuario();
            if (response.IsSuccess && response.Response != null)
            {
                var permisos = response.Response.Where(p => p.PaginaNombre == nombrePagina).ToList();
                Log.Information("PermisosService.ObtenerPermisosParaPagina SALIDA: Permisos={Permisos}", permisos.Count);
                return permisos;
            }
            Log.Information("PermisosService.ObtenerPermisosParaPagina SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return new List<PermisosViewModel>();
        }

        public async Task<bool> TienePermiso(string nombrePagina, string accion)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.TienePermiso ENTRADA: NombrePagina={NombrePagina}, Accion={Accion}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                nombrePagina, accion, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ValidarPermisoUsuario(nombrePagina, accion);
            if (response.IsSuccess)
            {
                Log.Information("PermisosService.TienePermiso SALIDA: TienePermiso={TienePermiso}", response.Response);
                return response.Response;
            }
            Log.Information("PermisosService.TienePermiso SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return false;
        }

        public async Task<object> ObtenerPermisosParaPermisos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerPermisosParaPermisos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Permisos");
                Log.Information("PermisosService.ObtenerPermisosParaPermisos SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("PermisosService.ObtenerPermisosParaPermisos SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }

        public async Task<ModelResponse<List<RolConteoPaginasDTO>>> ObtenerConteoPaginasPorRol()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PermisosService.ObtenerConteoPaginasPorRol ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerConteoPaginasPorRol();
            Log.Information("PermisosService.ObtenerConteoPaginasPorRol SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
