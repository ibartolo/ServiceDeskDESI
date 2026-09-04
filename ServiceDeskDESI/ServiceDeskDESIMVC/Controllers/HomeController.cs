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
        private readonly PersonaActivoService _personaActivoService;
        private readonly AreaService _areaService;

        public HomeController()
        {
            _autenticacionService = new AutenticacionService(httpClientConnection);
            _empresaService = new EmpresaService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _dashboardService = new DashboardService(httpClientConnection);
            _personaActivoService = new PersonaActivoService(httpClientConnection);
            _areaService = new AreaService(httpClientConnection);
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
        [HttpGet]
        public async Task<ActionResult> VerAsignacion(string token, string accion = null)
        {
            // Página anónima (standalone). Si el token no es un GUID válido, se muestra el mensaje limpio.
            if (!Guid.TryParse(token, out Guid tokenGuid))
            {
                ViewBag.ErrorMessage = "El enlace de asignación no es válido o ha sido alterado.";
                ViewBag.Detalle = null;
                ViewBag.Accion = null;
                ViewBag.Token = token;
                return View();
            }

            var detalleResponse = await _personaActivoService.AsignacionPorToken(tokenGuid);

            ViewBag.Detalle = detalleResponse != null ? detalleResponse.Response : null;
            ViewBag.Accion = string.Equals(accion, "desvincular", StringComparison.OrdinalIgnoreCase) ? "desvincular" : "aceptar";
            ViewBag.Token = token;
            ViewBag.ErrorMessage = (detalleResponse == null || !detalleResponse.IsSuccess)
                ? (detalleResponse?.Message ?? "El enlace de asignación no es válido o ha sido alterado.")
                : null;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AceptarAsignacion(string token)
        {
            if (!Guid.TryParse(token, out Guid tokenGuid))
            {
                return Json(new ModelResponse { IsSuccess = false, Message = "El enlace de asignación no es válido o ha sido alterado." });
            }

            var result = await _personaActivoService.ConfirmarRecepcion(tokenGuid);
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> DesvincularAsignacion(string token)
        {
            if (!Guid.TryParse(token, out Guid tokenGuid))
            {
                return Json(new ModelResponse { IsSuccess = false, Message = "El enlace de desvinculación no es válido o ha sido alterado." });
            }

            var result = await _personaActivoService.DesvincularConfirmacion(tokenGuid);
            return Json(result);
        }

        public ActionResult MisActivos()
        {
            return View();
        }

        public async Task<string> ObtenerMisActivos()
        {
            var response = await _personaActivoService.MisActivos();
            return JsonConvert.SerializeObject(response);
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

        public async Task<ActionResult> Configuration () {

            var tokenCookie = SessionHelper.GetSessionUser();

            // Determinar si el usuario es jefe de departamento (responsable de al menos un área)
            bool esJefeArea = false;
            if (tokenCookie != null && tokenCookie.UserID > 0)
            {
                var areasResponse = await _areaService.ConsultarTodasAreas();
                if (areasResponse.IsSuccess && areasResponse.Response != null)
                {
                    esJefeArea = areasResponse.Response.Any(a => a.UsuarioResponsableId == tokenCookie.UserID);
                }
            }

            ViewBag.EsJefeArea = esJefeArea;

            // Si es jefe, obtener la vigencia de la licencia de su empresa
            if (esJefeArea && tokenCookie != null && tokenCookie.EmpresaID > 0)
            {
                var empresa = await _empresaService.ObtenerEmpresaPorId(tokenCookie.EmpresaID);
                if (empresa != null)
                {
                    ViewBag.EmpresaNombre = empresa.NombreComercial;
                    ViewBag.FechaVigenciaFin = empresa.FechaVigenciaFin;
                    ViewBag.FechaVigenciaInicio = empresa.FechaVigenciaInicio;
                    ViewBag.EsPeriodoPrueba = empresa.EsPeriodoPrueba;

                    var diasRestantes = (empresa.FechaVigenciaFin.Date - DateTime.Now.Date).Days;
                    ViewBag.DiasRestantes = diasRestantes > 0 ? diasRestantes : 0;
                }
            }

            return View();
        }

        /// <summary>
        /// Manual de usuario / Ayuda. Requiere sesión (el filtro global AuthenticationFilter
        /// redirige a Home/Autentication si no hay sesión; aquí se refuerza igual que MyProfile).
        /// </summary>
        public ActionResult Ayuda()
        {
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            return View();
        }

        /// <summary>
        /// Guarda la preferencia de tema (light/dark) en una cookie propia del usuario
        /// (no es la cookie de sesión). Expira en 1 año y se renueva cada vez que cambia el tema.
        /// Antes de guardar elimina cualquier cookie de tema previa del usuario para evitar
        /// que quede una versión vieja en el navegador (causaba que el cambio solo se viera
        /// hasta borrar cookies manualmente).
        /// </summary>
        [HttpPost]
        public ActionResult GuardarTema(string tema)
        {
            if (tema != "light" && tema != "dark")
            {
                tema = "light";
            }

            var tokenCookie = SessionHelper.GetSessionUser();
            var cookieName = tokenCookie != null && tokenCookie.UserID > 0
                ? $"TemaUsuario_{tokenCookie.UserID}"
                : "TemaUsuario";

            // Forzar la expiración de cualquier cookie de tema previa de este usuario
            // (incluye la genérica "TemaUsuario" por si quedó de una versión anterior).
            foreach (string nombre in Request.Cookies.AllKeys)
            {
                if (nombre == cookieName || nombre == "TemaUsuario")
                {
                    var cookieVieja = new HttpCookie(nombre)
                    {
                        Expires = DateTime.Now.AddDays(-1),
                        Path = "/"
                    };
                    Response.Cookies.Add(cookieVieja);
                }
            }

            var cookie = new HttpCookie(cookieName, tema)
            {
                Expires = DateTime.Now.AddYears(1), // cookie siempre viva: cada cambio renueva +1 año
                HttpOnly = true,
                Path = "/"
            };
            Response.Cookies.Add(cookie);

            return Json(new { IsSuccess = true, Tema = tema });
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