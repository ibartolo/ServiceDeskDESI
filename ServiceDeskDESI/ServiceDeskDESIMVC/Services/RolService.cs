using Newtonsoft.Json;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class RolService
    {
        private readonly HttpClientConnection _httpClient;

        public RolService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<Rol>>> ObtenerTodosLosRoles()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.ObtenerTodosLosRoles ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodosLosRoles();
            Log.Information("RolService.ObtenerTodosLosRoles SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<Rol> ObtenerRolPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.ObtenerRolPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerRolPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("RolService.ObtenerRolPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("RolService.ObtenerRolPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Rol>> GuardarOActualizarRol(Rol rol)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.GuardarOActualizarRol ENTRADA: RolId={RolId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                rol?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarRol(rol);
            Log.Information("RolService.GuardarOActualizarRol SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarRol(Rol rol)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.EliminarRol ENTRADA: RolId={RolId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                rol?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarRol(rol);
            Log.Information("RolService.EliminarRol SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> AsignarRolUsuario(long usuarioId, long rolId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.AsignarRolUsuario ENTRADA: UsuarioId={UsuarioId}, RolId={RolId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuarioId, rolId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.AsignarRolUsuario(usuarioId, rolId);
            Log.Information("RolService.AsignarRolUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Rol>>> ObtenerRolesPorUsuario(long usuarioId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.ObtenerRolesPorUsuario ENTRADA: UsuarioId={UsuarioId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuarioId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ObtenerRolesPorUsuario(usuarioId);
            Log.Information("RolService.ObtenerRolesPorUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<UsuarioRol>>> ObtenerUsuarioRolesPorUsuario(long usuarioId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.ObtenerUsuarioRolesPorUsuario ENTRADA: UsuarioId={UsuarioId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuarioId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ObtenerUsuarioRolesPorUsuario(usuarioId);
            Log.Information("RolService.ObtenerUsuarioRolesPorUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarRolUsuario(long usuarioRolId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.EliminarRolUsuario ENTRADA: UsuarioRolId={UsuarioRolId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                usuarioRolId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarRolUsuario(usuarioRolId);
            Log.Information("RolService.EliminarRolUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaRol()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("RolService.ObtenerPermisosParaRol ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Roles");
                Log.Information("RolService.ObtenerPermisosParaRol SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("RolService.ObtenerPermisosParaRol SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
