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
    [RoutePrefix("api/Persona")]
    public class PersonaController : BaseController
    {
        private readonly PersonaService _personaService;

        public PersonaController()
        {
            _personaService = new PersonaService();
        }

        /// <summary>
        /// Obtiene todas las personas de la empresa del usuario autenticado
        /// </summary>
        /// <returns>Lista de personas</returns>
        [HttpGet, Route("List")]
        public ModelResponse<List<PersonaDTO>> ObtenerTodasLasPersonas()
        {
            var usuario = User.Identity.Name;
            var result = _personaService.ObtenerTodasLasPersonas(usuario);
            return result;
        }

        /// <summary>
        /// Obtiene una persona por su ID
        /// </summary>
        /// <param name="id">ID de la persona</param>
        /// <returns>Persona encontrada</returns>
        [HttpGet, Route("{id:long}")]
        public ModelResponse<PersonaDTO> ObtenerPersonaPorId(long id)
        {
            var usuario = User.Identity.Name;
            var result = _personaService.ObtenerPersonaPorId(id, usuario);
            return result;
        }

        /// <summary>
        /// Guarda o actualiza una persona
        /// </summary>
        /// <param name="persona">Objeto persona con los datos</param>
        /// <returns>Persona guardada con su ID actualizado</returns>
        [Permiso("Personas")]
        [HttpPost, Route("Guardar")]
        public ModelResponse<Persona> GuardarOActualizarPersona(Persona persona)
        {
            var usuario = User.Identity.Name;
            var result = _personaService.GuardarOActualizarPersona(persona, usuario);
            return result;
        }

        /// <summary>
        /// Elimina lógicamente una persona
        /// </summary>
        /// <param name="persona">Persona a eliminar (debe incluir Id y ModificadoPor)</param>
        /// <returns>Resultado de la operación</returns>
        [Permiso("Personas", "Eliminar")]
        [HttpDelete, Route("Eliminar")]
        public ModelResponse EliminarPersona(Persona persona)
        {
            var usuario = User.Identity.Name;
            persona.FechaModificacion = DateTime.Now;
            var result = _personaService.EliminarPersona(persona.Id, persona.ModificadoPor, persona.FechaModificacion.Value, usuario);
            return result;
        }

        /// <summary>
        /// Vincula una Persona a un Usuario existente (sincroniza datos de la Persona desde el Usuario).
        /// </summary>
        [Permiso("Personas", "Editar")]
        [HttpPost, Route("VincularUsuario")]
        public ModelResponse VincularPersonaUsuario([FromBody] VincularUsuarioRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _personaService.VincularPersonaUsuario(request.PersonaId, request.UsuarioId, usuario);
            return result;
        }

        /// <summary>
        /// Desvincula la Persona de su Usuario (Usuarios.PersonaId = NULL).
        /// </summary>
        [Permiso("Personas", "Editar")]
        [HttpPost, Route("DesvincularUsuario")]
        public ModelResponse DesvincularPersonaUsuario([FromBody] DesvincularUsuarioRequest request)
        {
            var usuario = User.Identity.Name;
            var result = _personaService.DesvincularPersonaUsuario(request.PersonaId, usuario);
            return result;
        }
    }

    public class VincularUsuarioRequest
    {
        public long PersonaId { get; set; }
        public long UsuarioId { get; set; }
    }

    public class DesvincularUsuarioRequest
    {
        public long PersonaId { get; set; }
    }
}
    