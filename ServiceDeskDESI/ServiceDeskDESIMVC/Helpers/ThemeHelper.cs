using ServiceDeskDESIEntities.Seguridad;
using System.Web;

namespace ServiceDeskDESIMVC.Helpers
{
    /// <summary>
    /// Helper para la preferencia de tema (claro/oscuro) almacenada en una cookie propia del usuario.
    /// No es la cookie de sesión; expira en 1 año y se renueva cada vez que el usuario cambia el tema.
    /// </summary>
    public static class ThemeHelper
    {
        public const string CookiePrefix = "TemaUsuario_";

        /// <summary>
        /// Devuelve el nombre de la cookie de tema para el usuario indicado.
        /// </summary>
        public static string GetCookieName(TokenCookie tokenCookie)
        {
            if (tokenCookie != null && tokenCookie.UserID > 0)
                return $"{CookiePrefix}{tokenCookie.UserID}";
            return CookiePrefix;
        }

        /// <summary>
        /// Lee el tema guardado en la cookie ('light' o 'dark'). Si no existe, devuelve 'light'.
        /// </summary>
        public static string GetTema(HttpRequestBase request, TokenCookie tokenCookie)
        {
            var cookieName = GetCookieName(tokenCookie);
            var cookie = request?.Cookies[cookieName];
            if (cookie != null && (cookie.Value == "light" || cookie.Value == "dark"))
            {
                return cookie.Value;
            }
            return "light";
        }

        /// <summary>
        /// Devuelve la clase CSS a aplicar en el body según el tema ('dark-theme' o string vacío).
        /// </summary>
        public static string GetTemaClase(HttpRequestBase request, TokenCookie tokenCookie)
        {
            return GetTema(request, tokenCookie) == "dark" ? "dark-theme" : "";
        }
    }
}
