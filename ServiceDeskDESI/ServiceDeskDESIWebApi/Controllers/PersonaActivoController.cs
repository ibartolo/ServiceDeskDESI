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
}
