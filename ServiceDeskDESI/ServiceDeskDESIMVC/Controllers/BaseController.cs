using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Controllers
{
    public class BaseController : Controller
    {
        public HttpClientConnection httpClientConnection;
        public Usuario usuarioAutenticado;
        public ModelResponse mr { get; set; }

        public BaseController()
        {
            httpClientConnection = new HttpClientConnection();
            mr = new ModelResponse();
            var token = SessionHelper.GetSessionUser();

            if (token?.Token?.ExpirationDate <= DateTime.Now)
            {
                SessionHelper.CloseSession();
                Redirect("~/Home/Autenticacion");
            }
        }
        public List<SelectListItem> MappingPropertiToDropDownList<T>(IEnumerable<T> items, string value, string title, string prefix = "")
        {
            List<SelectListItem> list = new List<SelectListItem>();

            foreach (var r in items)
            {
                var id = r.GetType().GetProperty(value);
                var nombre = r.GetType().GetProperty(title);

                PropertyInfo segundoNombre = null;
                if (!string.IsNullOrEmpty(prefix))
                    segundoNombre = r.GetType().GetProperty(prefix);

                list.Add(new SelectListItem()
                {
                    Value = id.GetValue(r).ToString(),
                    Text = (string.IsNullOrEmpty(prefix) && segundoNombre == null) ? nombre.GetValue(r).ToString() : $"{segundoNombre.GetValue(r).ToString()}-{nombre.GetValue(r).ToString()}"
                });
            }

            return list;
        }
        public string GenerarAvatarIniciales(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return "??";

            // Expresión regular para obtener las iniciales
            var regex = new System.Text.RegularExpressions.Regex(@"^([a-zA-Z])[a-zA-Z]*\.?([a-zA-Z])");
            var match = regex.Match(nombreUsuario);

            if (match.Success && match.Groups.Count > 2)
            {
                string primera = match.Groups[1].Value.ToUpper();
                string segunda = match.Groups[2].Value.ToUpper();
                return $"{primera}{segunda}";
            }

            // Si no encuentra el patrón con punto, toma primera y última letra
            if (nombreUsuario.Length >= 2)
            {
                return $"{nombreUsuario[0].ToString().ToUpper()}{nombreUsuario[1].ToString().ToUpper()}";
            }

            return nombreUsuario.Length == 1 ? nombreUsuario.ToUpper() : "??";
        }
    }
}