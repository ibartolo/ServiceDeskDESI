using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Services;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace ServiceDeskDESIMVC.Filters
{
    /// <summary>
    /// Atributo MVC que fuerza la validación de permisos contra RolPaginaAccion.
    /// La sesión la garantiza el filtro global AuthenticationFilter (autorización);
    /// aquí se valida el permiso y, si falta, se redirige a Home/AccesoDenegado.
    /// </summary>
    public class PermisoAttribute : ActionFilterAttribute
    {
        private readonly string _pagina;
        private readonly string _accion;

        /// <summary>
        /// Constructor con acción explícita (p. ej. "Eliminar", "Editar").
        /// </summary>
        public PermisoAttribute(string pagina, string accion)
        {
            _pagina = pagina;
            _accion = accion;
        }

        /// <summary>
        /// Constructor sin acción: la acción se auto-detecta en OnActionExecuting
        /// (DELETE -> "Eliminar"; Id == 0 -> "Crear"; Id > 0 -> "Editar").
        /// </summary>
        public PermisoAttribute(string pagina) : this(pagina, null)
        {
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var accion = ResolverAccion(filterContext);

            // IMPORTANTE: construir el service en el hilo de la petición.
            // HttpClientConnection (en su constructor) lee el token de sesión desde
            // HttpContext.Current, que es null en un hilo del thread-pool (Task.Run).
            // Por eso se construye AQUÍ y solo se ejecuta la espera async dentro de
            // Task.Run (que ya no toca HttpContext.Current), evitando también el deadlock.
            var permisosService = new PermisosService(new HttpClientConnection());

            var permitido = Task.Run(() => permisosService.TienePermiso(_pagina, accion))
                .GetAwaiter()
                .GetResult();

            if (!permitido)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "AccesoDenegado"
                }));
            }
        }

        /// <summary>
        /// Resuelve la acción a validar. Prioridad: acción explícita > DELETE ("Eliminar")
        /// > entidad en ActionParameters con Id numérico (0 -> "Crear", > 0 -> "Editar") > null (se deniega).
        /// </summary>
        private string ResolverAccion(ActionExecutingContext filterContext)
        {
            if (!string.IsNullOrWhiteSpace(_accion))
            {
                return _accion;
            }

            if (string.Equals(filterContext.HttpContext.Request.HttpMethod, "DELETE", StringComparison.OrdinalIgnoreCase))
            {
                return "Eliminar";
            }

            foreach (var parametro in filterContext.ActionParameters.Values)
            {
                if (parametro == null)
                {
                    continue;
                }

                var propiedadId = parametro.GetType().GetProperty("Id");
                if (propiedadId != null && EsNumerico(propiedadId.PropertyType))
                {
                    var idValor = Convert.ToInt64(propiedadId.GetValue(parametro));
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
