using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace ServiceDeskDESIMVC.App_Start
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new AuthenticationFilter());
        }
    }

    /// <summary>
    /// Filtro de autorización global ("seguro por defecto").
    /// Permite únicamente las acciones públicas de la allowlist; el resto requiere
    /// una sesión válida (redirige a Home/Autentication si no la hay).
    /// El enforcement de permisos de escritura lo impone el atributo [Permiso] por acción.
    /// </summary>
    public class AuthenticationFilter : IAuthorizationFilter
    {
        private static readonly HashSet<string> PublicActions =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Home.Autentication",
                "Home.LogIn",
                "Home.RecoverPassword",
                "Home.VerAsignacion",
                "Home.ValidarToken",
                "Home.RestablecerContrasenia",
                "Home.ValidarRecetearContrasenia",
                "Home.NewCompany",
                "Home.GuardarNuevaEmpresa",
                "Home.AccesoDenegado"
            };

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            if (filterContext.ActionDescriptor == null)
            {
                return;
            }

            var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var actionName = filterContext.ActionDescriptor.ActionName;
            var key = controllerName + "." + actionName;

            if (PublicActions.Contains(key))
            {
                return;
            }

            if (!SessionHelper.EixstSession())
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Autentication"
                }));
            }
        }
    }
}
