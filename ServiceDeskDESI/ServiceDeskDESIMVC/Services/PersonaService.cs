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
    public class PersonaService
    {
        private readonly HttpClientConnection _httpClient;

        public PersonaService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<PersonaDTO> ObtenerPersonaPorId(long id)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.ObtenerPersonaPorId ENTRADA: Id={Id}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var response = await _httpClient.ObtenerPersonaPorId(id);
            if (response.IsSuccess && response.Response != null)
            {
                Log.Information("PersonaService.ObtenerPersonaPorId SALIDA: Id={Id}", response.Response.Id);
                return response.Response;
            }
            Log.Information("PersonaService.ObtenerPersonaPorId SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                response?.IsSuccess, response?.Message);
            return null;
        }

        public async Task<ModelResponse<Persona>> GuardarOActualizarPersona(Persona persona)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.GuardarOActualizarPersona ENTRADA: PersonaId={PersonaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                persona?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.GuardarOActualizarPersona(persona);
            Log.Information("PersonaService.GuardarOActualizarPersona SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> EliminarPersona(Persona persona)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.EliminarPersona ENTRADA: PersonaId={PersonaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                persona?.Id, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.EliminarPersona(persona);
            Log.Information("PersonaService.EliminarPersona SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> VincularPersonaUsuario(long personaId, long usuarioId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.VincularPersonaUsuario ENTRADA: PersonaId={PersonaId}, UsuarioId={UsuarioId}, Usuario={Usuario}, EmpresaId={EmpresaId}",
                personaId, usuarioId, sesion?.UserName, sesion?.EmpresaID);
            var resultado = await _httpClient.VincularPersonaUsuario(personaId, usuarioId);
            Log.Information("PersonaService.VincularPersonaUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> DesvincularPersonaUsuario(long personaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.DesvincularPersonaUsuario ENTRADA: PersonaId={PersonaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                personaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.DesvincularPersonaUsuario(personaId);
            Log.Information("PersonaService.DesvincularPersonaUsuario SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<PersonaDTO>>> ConsultarTodasLasPersonas()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.ConsultarTodasLasPersonas ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerTodasLasPersonas();
            Log.Information("PersonaService.ConsultarTodasLasPersonas SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<object> ObtenerPermisosParaPersona()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaService.ObtenerPermisosParaPersona ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var permisosResponse = await _httpClient.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "Personas");
                Log.Information("PersonaService.ObtenerPermisosParaPersona SALIDA: PermisoEncontrado={PermisoEncontrado}",
                    permiso != null);
                return permiso;
            }
            Log.Information("PersonaService.ObtenerPermisosParaPersona SALIDA: sin resultado, IsSuccess={IsSuccess}, Message={Message}",
                permisosResponse?.IsSuccess, permisosResponse?.Message);
            return null;
        }
    }
}
