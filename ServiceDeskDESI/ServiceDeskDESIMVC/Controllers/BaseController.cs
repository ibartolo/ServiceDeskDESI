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
        Usuario usuarioAutenticado;
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
    }
}