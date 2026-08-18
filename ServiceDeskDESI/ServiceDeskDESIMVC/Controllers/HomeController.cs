using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Controllers
{
    public class HomeController : BaseController
    {
        private readonly AutenticacionService _autenticacionService;
        private readonly EmpresaService _empresaService;

        public HomeController()
        {
            _autenticacionService = new AutenticacionService(httpClientConnection);
            _empresaService = new EmpresaService(httpClientConnection);
        }

        #region Views
        public ActionResult Autentication()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult RecoverPassword(string id)
        {
            ViewBag.Token = id;
            return View();
        }
        public ActionResult NewCompany()
        {
            return View();
        }
        public async Task<ActionResult> MenusUser()
        {
            var paginasResponse = await httpClientConnection.ObtenerPaginasPorUsuario();
            var paginas = new List<Pagina>();
            if (paginasResponse.IsSuccess && paginasResponse.Response != null)
            {
                paginas = JsonConvert.DeserializeObject<List<Pagina>>(paginasResponse.Response.ToString());
            }
            return PartialView(paginas);
        }

        public ActionResult AccesoDenegado()
        {
            return View();
        }

        public ActionResult Configuration () { 

            return View();
        }


        #endregion

        #region Data Access
        [HttpPost]
        public async Task<string> LogIn(string user, string pass)
        {
            var mr = new ModelResponse();

            try
            {
                Token token = await httpClientConnection.GetToken(user, pass);

                if (token != null)
                {
                    var response = await _autenticacionService.AutenticarUsuario(new Usuario()
                    {
                        NombreUsuario = user,
                        Contrasena = pass
                    });

                    if (response.IsSuccess && response.Response != null)
                    {
                        var usuarioAutenticado = JsonConvert.DeserializeObject<Usuario>(response.Response.ToString());

                        token.ExpirationDate = DateTime.Now.AddSeconds(token.expires_in);
                        mr.IsSuccess = true;
                        mr.Message = "Ok";

                        var tokenCookie = new TokenCookie()
                        {
                            Token = token,
                            UserID = usuarioAutenticado.Id,
                            EmpresaID = usuarioAutenticado.Empresa != null ? usuarioAutenticado.Empresa.Id : 0,
                            UserName = user,
                            ProfileImage = usuarioAutenticado.ImagenPerfil,
                            UserAvatar = GenerarAvatarIniciales(usuarioAutenticado.NombreUsuario)
                        };

                        SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));
                    }
                    else
                    {
                        mr.IsSuccess = false;
                        mr.Message = response.Message ?? "Error al obtener datos del usuario";
                    }
                }
                else
                {
                    mr.IsSuccess = false;
                    mr.Message = "Error de usuario o contraseña";
                }
            }
            catch (Exception ex)
            {
                mr.IsSuccess = false;
                mr.Message = ex.Message;
            }

            return JsonConvert.SerializeObject(mr);
        }

        public ActionResult LogOut()
        {
            SessionHelper.CloseSession();
            if (Request.Cookies["ConfigMenu"] != null)
            {
                var c = new HttpCookie("ConfigMenu")
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Add(c);
            }

            return RedirectToAction("Autentication");
        }

        public async Task<string> ValidarToken(string id)
        { 
            var response = await _autenticacionService.ValidarTokenRecuperacion(id);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> RestablecerContrasenia(string token, string nuevaContrasena)
        {
            var response = await _autenticacionService.RestablecerContrasenia(token, nuevaContrasena);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ValidarRecetearContrasenia(Usuario usuario)
        {
            var response = await _autenticacionService.ValidarRecetearContrasenia(usuario);
            return JsonConvert.SerializeObject(response);
        }
        [HttpPost]
        public async Task<string> GuardarNuevaEmpresa(Empresa empresa)
        {
            var response = await _empresaService.RegistrarEmpresa(empresa);
            return JsonConvert.SerializeObject(response);
        }

        #endregion
    }
}