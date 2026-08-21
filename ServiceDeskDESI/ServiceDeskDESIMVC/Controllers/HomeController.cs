using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIMVC.DAL;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Services;
using Serilog;
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
        private readonly RolService _rolService;
        private readonly DashboardService _dashboardService;

        public HomeController()
        {
            _autenticacionService = new AutenticacionService(httpClientConnection);
            _empresaService = new EmpresaService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _dashboardService = new DashboardService(httpClientConnection);
        }

        #region Views
        public ActionResult Autentication()
        {
            return View();
        }
        public async Task<ActionResult> Index()
        {
            bool esAgente = false;
            DashboardIndicadoresDTO indicadores = null;

            try
            {
                var tokenCookie = SessionHelper.GetSessionUser();
                if (tokenCookie != null && tokenCookie.UserID > 0)
                {
                    var rolesResponse = await _rolService.ObtenerRolesPorUsuario(tokenCookie.UserID);
                    esAgente = rolesResponse.IsSuccess && rolesResponse.Response != null && rolesResponse.Response.Any(r => r.PuedeAtenderTickets);

                    if (esAgente)
                    {
                        var indResponse = await _dashboardService.ObtenerIndicadores();
                        if (indResponse.IsSuccess && indResponse.Response != null)
                        {
                            indicadores = indResponse.Response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al cargar indicadores del dashboard");
            }

            ViewBag.EsAgente = esAgente;
            ViewBag.Indicadores = indicadores;

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
                paginas = paginasResponse.Response;
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

            Log.Information("LogIn iniciado para usuario {Usuario}", user);

            try
            {
                // 1. Autenticar primero para obtener el mensaje específico del backend
                //    (contraseña incorrecta, trial expirado, etc.). El /token no devuelve ese detalle.
                var response = await _autenticacionService.AutenticarUsuario(new Usuario()
                {
                    NombreUsuario = user,
                    Contrasena = pass
                });
                Log.Information("LogIn paso 1 (autenticar) para {Usuario}: IsSuccess={IsSuccess}, Message={Message}", user, response?.IsSuccess, response?.Message);

                if (!response.IsSuccess || response.Response == null)
                {
                    mr.IsSuccess = false;
                    mr.Message = response?.Message ?? "Usuario o contraseña incorrectos";
                    return JsonConvert.SerializeObject(mr);
                }

                // 2. Obtener el token (solo si la autenticación fue exitosa)
                Token token = await httpClientConnection.GetToken(user, pass);

                if (token == null)
                {
                    Log.Warning("LogIn paso 2 (token) FALLÓ para {Usuario}: token nulo", user);
                    mr.IsSuccess = false;
                    mr.Message = "Error de usuario o contraseña";
                    return JsonConvert.SerializeObject(mr);
                }

                var usuarioAutenticado = response.Response;

                token.ExpirationDate = DateTime.Now.AddSeconds(token.expires_in);
                mr.IsSuccess = true;
                mr.Message = "Ok";

                var tokenCookie = new TokenCookie()
                {
                    Token = token,
                    UserID = usuarioAutenticado.Id,
                    EmpresaID = usuarioAutenticado.EmpresaId ?? 0,
                    UserName = user,
                    ProfileImage = usuarioAutenticado.ImagenPerfil,
                    UserAvatar = GenerarAvatarIniciales(usuarioAutenticado.NombreUsuario)
                };

                SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));

                Log.Information("LogIn exitoso para {Usuario}", user);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LogIn error para {Usuario}", user);
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