using Newtonsoft.Json;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Services;
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
        private readonly AreaService _areaService;
        private readonly RolService _rolService;

        public UserController()
        {
            _areaService = new AreaService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
        }

        public async Task<ActionResult> MyProfile()
        {
            // Obtener el ID del usuario desde la sesión
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            var usuario = new Usuario();
            var response = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID);

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
            var sucursalesResponse = await httpClientConnection.ObtenerSucursales();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                ViewBag.Sucursales = sucursalesResponse.Response;
            }

            //var areasResponse = await httpClientConnection.ObtenerAreas();
            var areasResponse = await _areaService.ConsultarTodasAreas();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                ViewBag.Areas = areasResponse.Response;
            }

            var rolesResponse = await _rolService.ObtenerTodosLosRoles();
            if (rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                ViewBag.Roles = rolesResponse.Response;
            }

            return View(usuario);
        }

        public async Task<ActionResult> PartialChangePass()
        {
            return PartialView();
        }
        public async Task<string> ActualizarPerfilUsuario(Usuario usuario, HttpPostedFileBase file)
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

                var response = await httpClientConnection.ActualizarPerfilUsuario(usuario);
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
                var usuarioResponse = await httpClientConnection.ObtenerUsuarioPorId(tokenCookie.UserID);

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


        #region Catelogo de usuarios
        public async Task<ActionResult> Users(long id = 0)
        {
            var usuario = new Usuario();

            // Cargar listas para los dropdowns
            var sucursalesResponse = await httpClientConnection.ObtenerSucursales();
            var sucursalesList = new List<Sucursal>();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                sucursalesList = JsonConvert.DeserializeObject<List<Sucursal>>(sucursalesResponse.Response.ToString());
            }

            var areasResponse = await httpClientConnection.ObtenerAreas();
            var areasList = new List<Area>();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                areasList = JsonConvert.DeserializeObject<List<Area>>(areasResponse.Response.ToString());
            }

            // Cargar roles
            var rolesResponse = await _rolService.ObtenerTodosLosRoles();
            var rolesList = new List<Rol>();
            long rolSeleccionadoId = 0;

            if (rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                rolesList = JsonConvert.DeserializeObject<List<Rol>>(rolesResponse.Response.ToString());
            }

            if (id > 0)
            {
                var response = await httpClientConnection.ObtenerUsuarioPorId(id);

                if (response.IsSuccess && response.Response != null)
                {
                    usuario = JsonConvert.DeserializeObject<Usuario>(response.Response.ToString());

                    // Desencriptar la contraseña para mostrarla en el input
                    if (!string.IsNullOrEmpty(usuario.Contrasena))
                    {
                        usuario.Contrasena = Cryptography.Decrypt(usuario.Contrasena);
                    }

                    // Obtener el rol del usuario en modo edición
                    var rolesUsuarioResponse = await _rolService.ObtenerRolesPorUsuario(usuario.Id);
                    if (rolesUsuarioResponse.IsSuccess && rolesUsuarioResponse.Response != null)
                    {
                        var rolesUsuario = JsonConvert.DeserializeObject<List<Rol>>(rolesUsuarioResponse.Response.ToString());
                        if (rolesUsuario.Any())
                        {
                            rolSeleccionadoId = rolesUsuario.First().Id;
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = response.Message;
                }
            }

            // Asignar Sucursales
            if (id > 0 && usuario.Sucursal != null && usuario.Sucursal.Id > 0)
            {
                var selectListSucursales = new List<SelectListItem>();
                foreach (var s in sucursalesList)
                {
                    var item = new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Nombre,
                        Selected = (s.Id == usuario.Sucursal.Id)
                    };
                    selectListSucursales.Add(item);
                }
                ViewBag.Sucursales = selectListSucursales;
            }
            else
            {
                ViewBag.Sucursales = MappingPropertiToDropDownList(sucursalesList, "Id", "Nombre");
            }

            // Asignar Áreas
            if (id > 0 && usuario.Area != null && usuario.Area.Id > 0)
            {
                var selectListAreas = new List<SelectListItem>();
                foreach (var a in areasList)
                {
                    var item = new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre,
                        Selected = (a.Id == usuario.Area.Id)
                    };
                    selectListAreas.Add(item);
                }
                ViewBag.Areas = selectListAreas;
            }
            else
            {
                ViewBag.Areas = MappingPropertiToDropDownList(areasList, "Id", "Nombre");
            }

            // Asignar Roles con el valor seleccionado
            // Asignar Roles con el valor seleccionado (modo edición)
            var selectListRoles = new List<SelectListItem>();
            foreach (var r in rolesList)
            {
                var item = new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Nombre,
                    Selected = (id > 0 && r.Id == rolSeleccionadoId)
                };
                selectListRoles.Add(item);
            }
            ViewBag.Roles = selectListRoles;

            ViewBag.EmpresaId = tokenCookie.EmpresaID;

            return View(usuario);
        }

        public async Task<string> ConsultarTodosLosUsuarios()
        {
            var response = await httpClientConnection.ObtenerUsuarios();
            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            // Encriptar la contraseña antes de guardar
            if (!string.IsNullOrEmpty(usuario.Contrasena))
            {
                usuario.Contrasena = Cryptography.Encrypt(usuario.Contrasena);
            }

            // Asignar empresa
            usuario.Empresa = new Empresa { Id = tokenCookie.EmpresaID };

            // Guardar usuario
            var response = await httpClientConnection.GuardarOActualizarUsuarioAdmin(usuario);

            // Si el usuario se guardó correctamente y tiene un rol seleccionado
            if (response.IsSuccess && response.Response != null)
            {
                var usuarioGuardado = JsonConvert.DeserializeObject<Usuario>(response.Response.ToString());

                // Obtener el rol seleccionado del formulario (se envía como campo oculto o desde el DDL)
                var rolId = HttpContext.Request.Form["RolId"];
                if (!string.IsNullOrEmpty(rolId))
                {
                    // Eliminar roles existentes del usuario
                    var rolesUsuarioResponse = await _rolService.ObtenerRolesPorUsuario(usuarioGuardado.Id);
                    if (rolesUsuarioResponse.IsSuccess && rolesUsuarioResponse.Response != null)
                    {
                        var rolesUsuario = JsonConvert.DeserializeObject<List<Rol>>(rolesUsuarioResponse.Response.ToString());
                        foreach (var rol in rolesUsuario)
                        {
                            await _rolService.EliminarRolUsuario(rol.Id);
                        }
                    }

                    // Asignar el nuevo rol
                    await _rolService.AsignarRolUsuario(usuarioGuardado.Id, Convert.ToInt64(rolId));
                }
            }

            return JsonConvert.SerializeObject(response);
        }

        public async Task<string> EliminarUsuarioAdmin(Usuario usuario)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            usuario.ModificadoPor = tokenCookie?.UserName ?? "system";
            usuario.FechaModificacion = DateTime.Now;

            var response = await httpClientConnection.EliminarUsuario(usuario);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}