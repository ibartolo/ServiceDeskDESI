using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using static ServiceDeskDESIMVC.Controllers.CatalogsController;

namespace ServiceDeskDESIMVC.Controllers
{
    public class UserController : BaseController
    {
        public async Task<ActionResult> MyProfile()
        {
            // Obtener el ID del usuario desde la sesión
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            var usuario = new Usuario();
            var response = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID, tokenCookie.EmpresaID);

            if (response.IsSuccess && response.Response != null)
            {
                usuario = JsonConvert.DeserializeObject<Usuario>(response.Response.ToString());
            }
            else
            {
                ViewBag.ErrorMessage = response.Message;
            }

            // Cargar listas para los dropdowns
            //var sucursalesResponse = await httpClientConnection.ObtenerSucursales();
            var sucursalesResponse = await ObtenerSucursales();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                ViewBag.Sucursales = sucursalesResponse.Response;
            }

            //var areasResponse = await httpClientConnection.ObtenerAreas();
            var areasResponse = await ObtenerAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            return View(usuario);
        }

        public async Task<ActionResult> PartialChangePass()
        {
            return PartialView();
        }
        public async Task<string> GuardarPerfil(Usuario usuario, HttpPostedFileBase file)
        {
            var modelResponse = new ModelResponse();

            try
            {

                // Manejar imagen de perfil
                //var file = Request.Files["ImagenPerfil"];
                if (file != null && file.ContentLength > 0)
                {
                    var name = Guid.NewGuid().ToString();
                    var extension = Path.GetExtension(file.FileName);
                    var fileName = $"{name}{extension}";
                    var pathTemplate = $"Uploads/Perfiles/{usuario.Id}/";

                    // Validar carpeta
                    if (!Directory.Exists(Server.MapPath("~/" + pathTemplate)))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/" + pathTemplate));
                    }

                    var path = Path.Combine(Server.MapPath("~/" + pathTemplate), fileName);
                    file.SaveAs(path);
                    usuario.ImagenPerfil = $"/{pathTemplate}{fileName}";
                }

                var response = await httpClientConnection.GuardarOActualizarUsuario(usuario);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el perfil";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }

        public async Task<string> CambiarContrasena([FromBody] CambioContrasenaRequest request)
        {
            var modelResponse = new ModelResponse();

            try
            {
                var tokenCookie = SessionHelper.GetSessionUser();
                if (tokenCookie == null || tokenCookie.UserID == 0)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "Sesión no válida";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                // Validar contraseña actual
                var usuarioResponse = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID, tokenCookie.EmpresaID);

                if (!usuarioResponse.IsSuccess || usuarioResponse.Response == null)
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "No se pudo obtener la información del usuario";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                var usuario = JsonConvert.DeserializeObject<Usuario>(usuarioResponse.Response.ToString());

                if (usuario.Contrasena != Cryptography.Encrypt(request.ContrasenaActual))
                {
                    modelResponse.IsSuccess = false;
                    modelResponse.Message = "La contraseña actual es incorrecta";
                    return JsonConvert.SerializeObject(modelResponse);
                }

                usuario.Contrasena = Cryptography.Encrypt(request.NuevaContrasena);

                // Actualizar contraseña
                //usuario.Contrasena = request.NuevaContrasena;
                //usuario.ModificadoPor = tokenCookie.UserName;
                //usuario.FechaModificacion = DateTime.Now;
                //
                //var response = await httpClientConnection.ActualizarContrasena(usuario);
                var response = await httpClientConnection.GuardarOActualizarUsuario(usuario);
                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al cambiar la contraseña";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }




        public async Task<ModelResponse> ObtenerSucursales()
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Lista temporal de sucursales
                var sucursales = new List<Sucursal>
        {
            new Sucursal { Id = 1, Nombre = "Matriz", Descripcion = "Oficina principal", Calle = "Av. Reforma #123", Ciudad = "Ciudad de México", Colonia = "Centro", CodigoPostal = "06000", Estatus = true },
            new Sucursal { Id = 2, Nombre = "Poza Rica", Descripcion = "Sucursal Veracruz", Calle = "Av. Reforma #123", Ciudad = "Poza Rica", Colonia = "Centro", CodigoPostal = "93210", Estatus = true }
        };

                modelResponse.IsSuccess = true;
                modelResponse.Response = sucursales;
                modelResponse.Message = "Sucursales obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }

        public async Task<ModelResponse> ObtenerAreas()
        {
            var modelResponse = new ModelResponse();

            try
            {
                // Lista temporal de áreas
                var areas = new List<Area>
        {
            new Area { Id = 1, Nombre = "Sistemas", Descripcion = "Área de tecnología", Correo = "sistemas@empresa.com", Estatus = true },
            new Area { Id = 2, Nombre = "Recursos Humanos", Descripcion = "Gestión de personal", Correo = "rh@empresa.com", Estatus = true }
        };

                modelResponse.IsSuccess = true;
                modelResponse.Response = areas;
                modelResponse.Message = "Áreas obtenidas correctamente";
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
                modelResponse.Response = null;
            }

            return modelResponse;
        }
    }
}