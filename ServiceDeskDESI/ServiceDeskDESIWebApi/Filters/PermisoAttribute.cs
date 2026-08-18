using ServiceDeskDESIWebApi.Services;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace ServiceDeskDESIWebApi.Filters
{
    /// <summary>
    /// Atributo que fuerza la validación de permisos contra RolPaginaAccion.
    /// La autenticación (401) la garantiza el [Authorize] a nivel de controller;
    /// aquí solo se valida el permiso (pagina/accion) vía PermisosService.ValidarPermisoUsuario,
    /// en la fase de action filter (posterior al model binding, para poder leer el Id de la entidad).
    /// </summary>
    public class PermisoAttribute : ActionFilterAttribute
    {
        private readonly string _pagina;
        private readonly string _accion;

        /// <summary>
        /// Constructor con acción explícita (p. ej. "Crear", "Editar", "Eliminar").
        /// </summary>
        /// <param name="pagina">Nombre exacto de la página (Pagina.Nombre).</param>
        /// <param name="accion">Acción requerida (Crear/Editar/Eliminar/Leer/Exportar).</param>
        public PermisoAttribute(string pagina, string accion)
        {
            _pagina = pagina;
            _accion = accion;
        }

        /// <summary>
        /// Constructor sin acción: la acción se auto-detecta en OnActionExecuting.
        /// DELETE -> "Eliminar"; entidad bound con Id == 0 -> "Crear"; con Id > 0 -> "Editar".
        /// </summary>
        /// <param name="pagina">Nombre exacto de la página (Pagina.Nombre).</param>
        public PermisoAttribute(string pagina) : this(pagina, null)
        {
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var usuario = actionContext.RequestContext.Principal.Identity.Name;
            var accion = ResolverAccion(actionContext);

            var resultado = new PermisosService().ValidarPermisoUsuario(usuario, _pagina, accion);

            if (resultado == null || !resultado.IsSuccess || !(resultado.Response is bool) || !(bool)resultado.Response)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.Forbidden,
                    "No tiene permiso para realizar esta acción.");
            }
        }

        /// <summary>
        /// Resuelve la acción a validar. Prioridad: acción explícita > DELETE ("Eliminar")
        /// > entidad bound con Id numérico (0 -> "Crear", > 0 -> "Editar") > null (se deniega).
        /// </summary>
        private string ResolverAccion(HttpActionContext actionContext)
        {
            if (!string.IsNullOrWhiteSpace(_accion))
            {
                return _accion;
            }

            if (actionContext.Request.Method == HttpMethod.Delete)
            {
                return "Eliminar";
            }

            foreach (var argumento in actionContext.ActionArguments.Values)
            {
                if (argumento == null)
                {
                    continue;
                }

                var propiedadId = argumento.GetType().GetProperty("Id");
                if (propiedadId != null && EsNumerico(propiedadId.PropertyType))
                {
                    var idValor = Convert.ToInt64(propiedadId.GetValue(argumento));
                    return idValor == 0 ? "Crear" : "Editar";
                }
            }

            return null;
        }

        private static bool EsNumerico(Type tipo)
        {
            if (tipo == null)
            {
                return false;
            }

            var tipoBase = Nullable.GetUnderlyingType(tipo) ?? tipo;

            return tipoBase == typeof(byte) ||
                   tipoBase == typeof(sbyte) ||
                   tipoBase == typeof(short) ||
                   tipoBase == typeof(ushort) ||
                   tipoBase == typeof(int) ||
                   tipoBase == typeof(uint) ||
                   tipoBase == typeof(long) ||
                   tipoBase == typeof(ulong);
        }
    }
}
