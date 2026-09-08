using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.Filters;
using ServiceDeskDESIWebApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceDeskDESIWebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/PersonaActivo")]
    public class PersonaActivoController : BaseController
    {
        private readonly PersonaActivoService _personaActivoService;

        public PersonaActivoController()
        {
            _personaActivoService = new PersonaActivoService();
        }

        /// <summary>
        /// Obtiene los activos asignados vigentes de una persona.
        /// </summary>
        [HttpGet, Route("ActivosPorPersona/{personaId:long}")]
        [Permiso("Personas", "Leer")]
        public ModelResponse<List<PersonaActivoDTO>> ObtenerActivosPorPersona(long personaId)
        {
            var usuario = User.Identity.Name;
            var result = _personaActivoService.ObtenerActivosPorPersona(personaId, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene los activos sin asignación vigente (disponibles para asignar).
        /// </summary>
        [HttpGet, Route("Disponibles")]
        [Permiso("Personas", "Leer")]
        public ModelResponse<List<Activo>> ObtenerActivosDisponibles()
        {
            var usuario = User.Identity.Name;
            var result = _personaActivoService.ObtenerActivosDisponibles(usuario);
            return result;
        }

        /// <summary>
        /// "Mis Activos": activos del usuario autenticado (sin [Permiso]).
        /// Deriva PersonaId desde Usuarios.PersonaId del usuario autenticado.
        /// </summary>
        [HttpGet, Route("MisActivos")]
        public ModelResponse<List<PersonaActivoDTO>> ObtenerMisActivos()
        {
            var usuario = User.Identity.Name;
            var result = _personaActivoService.ObtenerMisActivos(usuario);
            return result;
        }

        /// <summary>
        /// Asigna un activo a una persona.
        /// </summary>
        [HttpPost, Route("Asignar")]
        [Permiso("Personas", "Editar")]
        public ModelResponse AsignarActivoPersona([FromBody] AsignarActivoRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _personaActivoService.AsignarActivoPersona(request.PersonaId, request.ActivoId, usuario);
            return result;
        }

        /// <summary>
        /// Desvincula un activo de una persona (finaliza la asignación vigente).
        /// </summary>
        [HttpPost, Route("Desvincular")]
        [Permiso("Personas", "Editar")]
        public ModelResponse DesvincularActivoPersona([FromBody] DesvincularActivoRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _personaActivoService.DesvincularActivoPersona(request.PersonaActivoId, usuario);
            return result;
        }

        /// <summary>
        /// Obtiene la asignación por token (anónimo, para render de la página VerAsignacion).
        /// </summary>
        [AllowAnonymous]
        [HttpGet, Route("AsignacionPorToken/{token:guid}")]
        public ModelResponse<AsignacionActivoDetalleDTO> ObtenerAsignacionPorToken(Guid token)
        {
            return _personaActivoService.ObtenerAsignacionPorToken(token);
        }

        /// <summary>
        /// Confirma la recepción de un activo (AUTENTICADO; valida titularidad vía Usuarios.PersonaId).
        /// </summary>
        [HttpPost, Route("confirmarRecepcion")]
        public ModelResponse ConfirmarRecepcion([FromBody] ConfirmarRecepcionRequest request)
        {
            var usuario = User.Identity.Name;
            return _personaActivoService.ConfirmarRecepcion(request.Token, usuario);
        }

        /// <summary>
        /// Desvincula un activo confirmando con su token (AUTENTICADO; valida titularidad).
        /// </summary>
        [HttpPost, Route("desvincularConfirmacion")]
        public ModelResponse DesvincularConfirmacion([FromBody] ConfirmarRecepcionRequest request)
        {
            var usuario = User.Identity.Name;
            return _personaActivoService.DesvincularConfirmacion(request.Token, usuario);
        }

        /// <summary>
        /// Inicia la desvinculación (lado admin): envía correo al usuario para que confirme.
        /// </summary>
        [HttpPost, Route("IniciarDesvinculacion")]
        [Permiso("Personas", "Editar")]
        public ModelResponse IniciarDesvinculacion([FromBody] DesvincularActivoRequest request)
        {
            var usuario = User.Identity.Name;
            return _personaActivoService.IniciarDesvinculacion(request.PersonaActivoId, usuario);
        }
    }

    public class AsignarActivoRequest
    {
        public long PersonaId { get; set; }
        public long ActivoId { get; set; }
    }

    public class DesvincularActivoRequest
    {
        public long PersonaActivoId { get; set; }
    }

    public class ConfirmarRecepcionRequest
    {
        public Guid Token { get; set; }
    }
}
