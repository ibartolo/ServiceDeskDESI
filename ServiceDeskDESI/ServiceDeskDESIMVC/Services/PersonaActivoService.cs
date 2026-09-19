using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceDeskDESIMVC.Services
{
    public class PersonaActivoService
    {
        private readonly HttpClientConnection _httpClient;

        public PersonaActivoService(HttpClientConnection httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ModelResponse<List<PersonaActivoDTO>>> ObtenerActivosPorPersona(long personaId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.ObtenerActivosPorPersona ENTRADA: PersonaId={PersonaId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                personaId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerActivosPorPersona(personaId);
            Log.Information("PersonaActivoService.ObtenerActivosPorPersona SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<Activo>>> ObtenerActivosDisponibles()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.ObtenerActivosDisponibles ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ObtenerActivosDisponibles();
            Log.Information("PersonaActivoService.ObtenerActivosDisponibles SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<List<PersonaActivoDTO>>> MisActivos()
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.MisActivos ENTRADA: Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.MisActivos();
            Log.Information("PersonaActivoService.MisActivos SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse<AsignacionActivoDetalleDTO>> AsignacionPorToken(Guid token)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.AsignacionPorToken ENTRADA: Token={Token}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                token == Guid.Empty ? "(vacio)" : "(presente)", sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.AsignacionPorToken(token);
            Log.Information("PersonaActivoService.AsignacionPorToken SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> AsignarActivoPersona(long personaId, long activoId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.AsignarActivoPersona ENTRADA: PersonaId={PersonaId}, ActivoId={ActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                personaId, activoId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.AsignarActivoPersona(personaId, activoId);
            Log.Information("PersonaActivoService.AsignarActivoPersona SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> DesvincularActivoPersona(long personaActivoId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.DesvincularActivoPersona ENTRADA: PersonaActivoId={PersonaActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                personaActivoId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.DesvincularActivoPersona(personaActivoId);
            Log.Information("PersonaActivoService.DesvincularActivoPersona SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> IniciarDesvinculacion(long personaActivoId)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.IniciarDesvinculacion ENTRADA: PersonaActivoId={PersonaActivoId}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                personaActivoId, sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.IniciarDesvinculacion(personaActivoId);
            Log.Information("PersonaActivoService.IniciarDesvinculacion SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> ConfirmarRecepcion(Guid token)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.ConfirmarRecepcion ENTRADA: Token={Token}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                token == Guid.Empty ? "(vacio)" : "(presente)", sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.ConfirmarRecepcion(token);
            Log.Information("PersonaActivoService.ConfirmarRecepcion SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }

        public async Task<ModelResponse> DesvincularConfirmacion(Guid token)
        {
            var sesion = SessionHelper.GetSessionUser();
            Log.Information("PersonaActivoService.DesvincularConfirmacion ENTRADA: Token={Token}, Usuario={Usuario}, EmpresaId={EmpresaId}, UserId={UserId}",
                token == Guid.Empty ? "(vacio)" : "(presente)", sesion?.UserName, sesion?.EmpresaID, sesion?.UserID);
            var resultado = await _httpClient.DesvincularConfirmacion(token);
            Log.Information("PersonaActivoService.DesvincularConfirmacion SALIDA: IsSuccess={IsSuccess}, Message={Message}",
                resultado?.IsSuccess, resultado?.Message);
            return resultado;
        }
    }
}
