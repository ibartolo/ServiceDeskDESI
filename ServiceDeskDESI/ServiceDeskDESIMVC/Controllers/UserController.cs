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
using ServiceDeskDESIMVC.Filters;
using static ServiceDeskDESIMVC.Controllers.CatalogsController;

namespace ServiceDeskDESIMVC.Controllers
{
    public class UserController : BaseController
    {
        private readonly AreaService _areaService;
        private readonly RolService _rolService;
        private readonly UsuarioService _usuarioService;
        private readonly SucursalService _sucursalService;
        private readonly AutenticacionService _autenticacionService;

        public UserController()
        {
            _areaService = new AreaService(httpClientConnection);
            _rolService = new RolService(httpClientConnection);
            _usuarioService = new UsuarioService(httpClientConnection);
            _sucursalService = new SucursalService(httpClientConnection);
            _autenticacionService = new AutenticacionService(httpClientConnection);
        }

        public async Task<ActionResult> MyProfile()
        {
            // Obtener el ID del usuario desde la sesión
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            var usuario = await _usuarioService.ObtenerUsuarioPorId(tokenCookie.UserID);
            if (usuario == null)
            {
                usuario = new Usuario();
                ViewBag.ErrorMessage = "No se pudo obtener el usuario.";
            }
            else
            {
                // Sincronizar la sesión con los datos frescos (imagen de perfil) para que el navbar (_Layout user-avatar) la muestre
                if (tokenCookie.ProfileImage != usuario.ImagenPerfil)
                {
                    tokenCookie.ProfileImage = usuario.ImagenPerfil;
                    SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));
                }
            }

            // Cargar listas para los dropdowns
            var sucursalesResponse = await _sucursalService.ConsultarTodasSucursales();
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

        [Permiso("Mi Perfil", "Editar")]
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

                var response = await _autenticacionService.ActualizarPerfilUsuario(usuario);

                // Refrescar la sesión con la nueva imagen para que el navbar (user-avatar del _Layout) la muestre de inmediato
                if (response.IsSuccess)
                {
                    var tokenCookie = SessionHelper.GetSessionUser();
                    if (tokenCookie != null)
                    {
                        tokenCookie.ProfileImage = usuario.ImagenPerfil;
                        SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));
                    }
                }

                return JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al guardar el perfil";
                return JsonConvert.SerializeObject(modelResponse);
            }
        }

        #region Catelogo de usuarios
        public async Task<ActionResult> Users(long id = 0)
        {
            // 1. Obtener permisos para la página "Usuarios"
            var permisos = await _usuarioService.ObtenerPermisosParaUsuario();

            // 2. Validar permiso de lectura (D1)
            if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer)
            {
                return RedirectToAction("AccesoDenegado", "Home");
            }

            var usuario = new Usuario();

            // Cargar listas para los dropdowns
            var sucursalesResponse = await _sucursalService.ConsultarTodasSucursales();
            var sucursalesList = new List<Sucursal>();
            if (sucursalesResponse.IsSuccess && sucursalesResponse.Response != null)
            {
                sucursalesList = sucursalesResponse.Response;
            }

            var areasResponse = await _areaService.ConsultarTodasAreas();
            var areasList = new List<Area>();
            if (areasResponse.IsSuccess && areasResponse.Response != null)
            {
                areasList = areasResponse.Response;
            }

            // Cargar roles
            var rolesResponse = await _rolService.ObtenerTodosLosRoles();
            var rolesList = new List<Rol>();
            long rolSeleccionadoId = 0;

            if (rolesResponse.IsSuccess && rolesResponse.Response != null)
            {
                rolesList = rolesResponse.Response;
            }

            if (id > 0)
            {
                var usuarioResponse = await _usuarioService.ObtenerUsuarioPorId(id);

                if (usuarioResponse != null)
                {
                    usuario = usuarioResponse;

                    // La contraseña no se expone (viene vacía del API); el campo solo acepta una nueva contraseña.

                    // Obtener el rol del usuario en modo edición
                    var rolesUsuarioResponse = await _rolService.ObtenerRolesPorUsuario(usuario.Id);
                    if (rolesUsuarioResponse.IsSuccess && rolesUsuarioResponse.Response != null)
                    {
                        if (rolesUsuarioResponse.Response.Any())
                        {
                            rolSeleccionadoId = rolesUsuarioResponse.Response.First().Id;
                        }
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "No se pudo obtener el usuario.";
                }
            }

            // Asignar Sucursales
            if (id > 0 && usuario.SucursalId.HasValue && usuario.SucursalId.Value > 0)
            {
                var selectListSucursales = new List<SelectListItem>();
                foreach (var s in sucursalesList)
                {
                    var item = new SelectListItem
                    {
                        Value = s.Id.ToString(),
                        Text = s.Nombre,
                        Selected = (s.Id == usuario.SucursalId)
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
            if (id > 0 && usuario.AreaId.HasValue && usuario.AreaId.Value > 0)
            {
                var selectListAreas = new List<SelectListItem>();
                foreach (var a in areasList)
                {
                    var item = new SelectListItem
                    {
                        Value = a.Id.ToString(),
                        Text = a.Nombre,
                        Selected = (a.Id == usuario.AreaId)
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

            // 3. Pasar permisos a la vista
            ViewBag.Permisos = permisos;

            return View(usuario);
        }

        public async Task<string> ConsultarTodosLosUsuarios()
        {
            var response = await _usuarioService.ConsultarTodosLosUsuarios();
            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Usuarios")]
        public async Task<string> GuardarOActualizarUsuarioAdmin(Usuario usuario)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            // Asignar empresa
            usuario.EmpresaId = tokenCookie.EmpresaID;

            // Guardar usuario
            var response = await _usuarioService.GuardarOActualizarUsuarioAdmin(usuario);

            // Si el usuario se guardó correctamente y tiene un rol seleccionado
            if (response.IsSuccess && response.Response != null)
            {
                var usuarioGuardado = response.Response;

                // Obtener el rol seleccionado del formulario (se envía como campo oculto o desde el DDL)
                var rolId = HttpContext.Request.Form["RolId"];
                if (!string.IsNullOrEmpty(rolId))
                {
                    // Eliminar las asignaciones usuario-rol existentes del usuario.
                    // Usar UsuarioRol.Id (fila junction), NO Rol.Id: EliminarRolUsuario espera UsuarioRol.Id.
                    var usuarioRolesResponse = await _rolService.ObtenerUsuarioRolesPorUsuario(usuarioGuardado.Id);
                    if (usuarioRolesResponse.IsSuccess && usuarioRolesResponse.Response != null)
                    {
                        foreach (var usuarioRol in usuarioRolesResponse.Response)
                        {
                            await _rolService.EliminarRolUsuario(usuarioRol.Id);
                        }
                    }

                    // Asignar el nuevo rol
                    await _rolService.AsignarRolUsuario(usuarioGuardado.Id, Convert.ToInt64(rolId));
                }
            }

            return JsonConvert.SerializeObject(response);
        }

        [Permiso("Usuarios", "Eliminar")]
        public async Task<string> EliminarUsuarioAdmin(Usuario usuario)
        {
            var tokenCookie = SessionHelper.GetSessionUser();

            usuario.ModificadoPor = tokenCookie?.UserName ?? "system";
            usuario.FechaModificacion = DateTime.Now;

            var response = await _usuarioService.EliminarUsuario(usuario);
            return JsonConvert.SerializeObject(response);
        }
        #endregion
    }
}