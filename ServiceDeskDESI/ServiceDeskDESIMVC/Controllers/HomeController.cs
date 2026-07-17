using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
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
        #region Views
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
        #endregion

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

        [Autenticated]
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
        [HttpPost]
        public async Task<string> GuardarNuevaEmpresa(Empresa empresa)
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(empresa.NombreComercial))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El nombre comercial es requerido";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                if (string.IsNullOrWhiteSpace(empresa.RazonSocial))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La razón social es requerida";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                if (string.IsNullOrWhiteSpace(empresa.RFC))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El RFC es requerido";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                if (string.IsNullOrWhiteSpace(empresa.Responsable))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El responsable es requerido";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                if (string.IsNullOrWhiteSpace(empresa.Direccion))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La dirección es requerida";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "El correo de contacto es requerido";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Obtener todas las empresas
                //  ING AQUI TAMBIEN LE PUESE 0 
                var empresasResponse = await httpClientConnection.ObtenerTodasLasEmpresas();

                if (!empresasResponse.IsSuccess)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Error al validar los datos de la empresa";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                var empresas = JsonConvert.DeserializeObject<List<Empresa>>(empresasResponse.Response.ToString());

                // Validar si ya existe una empresa con el mismo RFC
                var existePorRFC = empresas.Any(e => e.RFC == empresa.RFC);
                if (existePorRFC)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe una empresa registrada con estos datos.";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Validar si ya existe una empresa con el mismo correo de contacto
                var existePorCorreo = empresas.Any(e => e.CorreoContacto == empresa.CorreoContacto);
                if (existePorCorreo)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe una empresa registrada con estos datos.";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Validar si ya existe una empresa con el mismo nombre comercial
                var existePorNombreComercial = empresas.Any(e => e.NombreComercial == empresa.NombreComercial);
                if (existePorNombreComercial)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe una empresa registrada con estos datos.";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Validar si ya existe una empresa con la misma razón social
                var existePorRazonSocial = empresas.Any(e => e.RazonSocial == empresa.RazonSocial);
                if (existePorRazonSocial)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Ya existe una empresa registrada con estos datos.";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Establecer fechas de vigencia (prueba gratis 30 días)
                empresa.FechaVigenciaInicio = DateTime.Now;
                empresa.FechaVigenciaFin = DateTime.Now.AddDays(30);
                empresa.EsPeriodoPrueba = true;
                empresa.CreadoPor = "system.register";
                empresa.FechaCreacion = DateTime.Now;
                empresa.Estatus = true;

                // Guardar la empresa
                var response = await httpClientConnection.GuardarNuevaEmpresa(empresa);

                if (response.IsSuccess && response.Response != null)
                {
                    var empresaGuardada = JsonConvert.DeserializeObject<Empresa>(response.Response.ToString());

                    // Aquí puedes agregar lógica adicional como:
                    // - Enviar correo de bienvenida con las credenciales
                    // - Crear roles por defecto para la empresa

                    modelResponse.IsSuccess = true;
                    modelResponse.Message = "Empresa registrada correctamente. Se ha enviado un correo con las credenciales de acceso.";
                    modelResponse.Response = empresaGuardada;
                }
                else
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = response.Message ?? "Error al registrar la empresa";
                }
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al procesar la solicitud";
            }

            return JsonConvert.SerializeObject(modelResponse);
        }

        #endregion
    }
}