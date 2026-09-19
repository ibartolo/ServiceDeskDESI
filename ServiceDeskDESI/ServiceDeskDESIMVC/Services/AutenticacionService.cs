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
    public class AutenticacionService
    {
        private readonly HttpClientConnection _httpClient;

        public AutenticacionService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<UsuarioDTO>> AutenticarUsuario(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.AutenticarUsuario ENTRADA: NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.AutenticarUsuario(usuario);
            Log.Information("AutenticacionService.AutenticarUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Usuario>> ActualizarPerfilUsuario(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.ActualizarPerfilUsuario ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ActualizarPerfilUsuario(usuario);
            Log.Information("AutenticacionService.ActualizarPerfilUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<bool>> ExisteNombreUsuario(string nombreUsuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.ExisteNombreUsuario ENTRADA: NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                nombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ExisteNombreUsuario(nombreUsuario);
            Log.Information("AutenticacionService.ExisteNombreUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ValidarTokenRecuperacion(string token)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.ValidarTokenRecuperacion ENTRADA: TokenRecuperacion={TokenRecuperacion}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                string.IsNullOrWhiteSpace(token) ? "(vacio)" : "(presente)", sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ValidarTokenRecuperacion(token);
            Log.Information("AutenticacionService.ValidarTokenRecuperacion SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> RestablecerContrasenia(string token, string nuevaContrasena)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.RestablecerContrasenia ENTRADA: Token={Token}, NuevaContrasena=(omitida), Usuario={Usuario}, EmpresaId={EmpresaId}",
                string.IsNullOrWhiteSpace(token) ? "(vacio)" : "(presente)", sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.RestablecerContrasenia(token, nuevaContrasena);
            Log.Information("AutenticacionService.RestablecerContrasenia SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ValidarRecetearContrasenia(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.ValidarRecetearContrasenia ENTRADA: NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.ValidarRecetearContrasenia(usuario);
            Log.Information("AutenticacionService.ValidarRecetearContrasenia SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<Usuario>> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("AutenticacionService.GuardarOActualizarUsuarioAdmin ENTRADA: UsuarioId={UsuarioId}, NombreUsuario={NombreUsuario}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                usuario?.Id, usuario?.NombreUsuario, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.GuardarOActualizarUsuarioAdmin(usuario);
            Log.Information("AutenticacionService.GuardarOActualizarUsuarioAdmin SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
