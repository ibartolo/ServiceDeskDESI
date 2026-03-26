using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Helpers.FiltersHelper;

namespace ServiceDeskDESIMVC.Controllers
{
    public class HomeController : BaseController
    {
        [NoAutenticated]
        public ActionResult Autentication()
        {
            return View();
        }
        [Autenticated]
        public ActionResult Index()
        {
            return View();
        }
        [NoAutenticated]
        public ActionResult RecoverPassword(string id)
        {
            ViewBag.Token = id;
            return View();
        }

        #region Data Access
        [HttpPost]
        public async Task<string> LogIn(string user, string pass)
        {
            var mr = new ModelResponse();

            try
            {
                Token token = await httpClientConnection.GetToken(user, Cryptography.Encrypt(pass));

                if (token != null)
                {
                    var response = await httpClientConnection.AutenticarUsuario(new Usuario()
                    {
                        NombreUsuario = user,
                        Contrasena = Cryptography.Encrypt(pass)
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
                            UserName = user,
                            ProfileImage = usuarioAutenticado.ImagenPerfil
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

        public async Task<string> ValidarToken(string id)
        { 
            var response = await httpClientConnection.ValidarTokenRecuperacion(id);
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> RestablecerContrasenia(string token, string nuevaContrasena)
        {
            var response = await httpClientConnection.RestablecerContrasenia(token, Cryptography.Encrypt(nuevaContrasena));
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> ValidarRecetearContrasenia(Usuario usuario)
        {
            var response = await httpClientConnection.ValidarRecetearContrasenia(usuario);
            return JsonConvert.SerializeObject(response);
        }

        #endregion
    }
}