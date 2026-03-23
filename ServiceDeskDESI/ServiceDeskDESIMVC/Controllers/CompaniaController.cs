using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIMVC.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;


namespace ServiceDeskDESIMVC.Controllers
{
    public class CompaniaController : Controller
    {
        // GET: Compania
        public ActionResult Index()
        {
            return View();
        }

        //public async  Task<ActionResult> AltaEdicion( long id = 0)
        //{
        //    var compania = new Compania();
        //    if (id !=0)
        //    {
        //        var result = await httpClientConnection.ObtenerCompaniaPorId(id);
        //        compania = JsonConvert.DeserializeObject<Compania>(result.Response.ToString());
        //    }

        //    return View();
        //}

        //public async Task<string> ObtnerTodasLasCompanias()
        //{
        //    var token = Helpers.SessionHelper.GetSessionUser();
        //    var result = await httpClientConnection.ObtenerCompanias(token.Token.access_token);
        //    return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        //}

        //public string GuaradarActualizarCompanias(Compania c)
        //{
            
        //}


    }
}