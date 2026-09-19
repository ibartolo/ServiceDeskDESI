using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class UsuarioService
    {
        private readonly HttpClientConnection _httpClient;

        public UsuarioService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Usuario> ObtenerUsuarioPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.ObtenerUsuarioPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerUsuarioPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("UsuarioService.ObtenerUsuarioPorId SALIDA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}",
                    response.Response.Id, response.Response.NombreUsuario);
                return response.Response;
            }
            Log.Information("UsuarioService.ObtenerUsuarioPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ObtenerUsuarios()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.ObtenerUsuarios ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerUsuarios();
            Log.Information("UsuarioService.ObtenerUsuarios SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuario(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.GuardarOActualizarUsuario ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.GuardarOActualizarUsuario(usuario);
            Log.Information("UsuarioService.GuardarOActualizarUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Usuario>> GuardarUsuarioEmpresa(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.GuardarUsuarioEmpresa ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.GuardarUsuarioEmpresa(usuario);
            Log.Information("UsuarioService.GuardarUsuarioEmpresa SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.GuardarOActualizarUsuarioAdmin ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.GuardarOActualizarUsuarioAdmin(usuario);
            Log.Information("UsuarioService.GuardarOActualizarUsuarioAdmin SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarUsuario(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.EliminarUsuario ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.EliminarUsuario(usuario);
            Log.Information("UsuarioService.EliminarUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<UsuarioDTO>>> ConsultarTodosLosUsuarios()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.ConsultarTodosLosUsuarios ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerUsuarios();
            Log.Information("UsuarioService.ConsultarTodosLosUsuarios SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaUsuario()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("UsuarioService.ObtenerPermisosParaUsuario ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Usuarios");
                Log.Information("UsuarioService.ObtenerPermisosParaUsuario SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("UsuarioService.ObtenerPermisosParaUsuario SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
