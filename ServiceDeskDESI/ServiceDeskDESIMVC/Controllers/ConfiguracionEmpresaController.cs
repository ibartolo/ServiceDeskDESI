using Newtonsoft.Json;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIMVC.Filters;
using ServiceDeskDESIMVC.Helpers;
using ServiceDeskDESIMVC.Models;
using ServiceDeskDESIMVC.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ServiceDeskDESIMVC.Controllers
{
    public class ConfiguracionEmpresaController : BaseController
    {
        private readonly EmpresaService _empresaService;
        private readonly HorarioLaboralService _horarioService;

        public ConfiguracionEmpresaController()
        {
            _empresaService = new EmpresaService(httpClientConnection);
            _horarioService = new HorarioLaboralService(httpClientConnection);
        }

        [Permiso("ConfiguracionEmpresa", "Leer")]
        public async Task<ActionResult> Index()
        {
            var tokenCookie = SessionHelper.GetSessionUser();
            if (tokenCookie == null || tokenCookie.UserID == 0)
            {
                return RedirectToAction("Autentication", "Home");
            }

            var empresa = await _empresaService.ObtenerEmpresaPorId(tokenCookie.EmpresaID);

            // Leer los permisos de la página para saber si el usuario puede editar.
            bool puedeEditar = false;
            var permisosResponse = await httpClientConnection.ObtenerPermisosPorUsuario();
            if (permisosResponse.IsSuccess && permisosResponse.Response != null)
            {
                var permiso = permisosResponse.Response.FirstOrDefault(p => p.PaginaNombre == "ConfiguracionEmpresa");
                puedeEditar = permiso != null && permiso.PuedeEditar;
            }
            ViewBag.PuedeEditar = puedeEditar;
            ViewBag.LogoEmpresaMaxTamanoKB = ConfigurationManager.AppSettings["LogoEmpresaMaxTamanoKB"] ?? "2048";
            ViewBag.LogoEmpresaTiposPermitidos = ConfigurationManager.AppSettings["LogoEmpresaTiposPermitidos"] ?? "svg,png";

            return View(empresa);
        }

        [Permiso("ConfiguracionEmpresa", "Leer")]
        public async Task<string> ObtenerHorario()
        {
            var response = new ModelResponse<List<HorarioViewModel>>();

            try
            {
                var horarioResponse = await _horarioService.ObtenerHorarioLaboral();
                if (horarioResponse.IsSuccess && horarioResponse.Response != null)
                {
                    response.IsSuccess = true;
                    response.Response = horarioResponse.Response
                        .OrderBy(h => h.DiaSemana)
                        .Select(h => new HorarioViewModel
                        {
                            DiaSemana = h.DiaSemana,
                            EsLaboral = h.Estatus,
                            HoraInicio = h.HoraInicio?.ToString("HH:mm"),
                            HoraFin = h.HoraFin?.ToString("HH:mm")
                        })
                        .ToList();
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = horarioResponse?.Message ?? "No se pudo obtener el horario laboral.";
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al obtener el horario laboral");
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al obtener el horario laboral.";
            }

            return JsonConvert.SerializeObject(response);
        }

        [Permiso("ConfiguracionEmpresa", "Editar")]
        public async Task<string> GuardarHorario(List<HorarioViewModel> horario)
        {
            var response = new ModelResponse();

            try
            {
                if (horario == null || horario.Count != 7)
                {
                    response.IsSuccess = false;
                    response.Message = "Debe enviar los 7 días de la semana.";
                    return JsonConvert.SerializeObject(response);
                }

                var lista = new List<HorarioLaboral>();
                foreach (var fila in horario)
                {
                    var item = new HorarioLaboral
                    {
                        DiaSemana = fila.DiaSemana,
                        Estatus = fila.EsLaboral
                    };

                    if (fila.EsLaboral)
                    {
                        DateTime horaInicio;
                        DateTime horaFin;

                        if (!TryParseHora(fila.HoraInicio, out horaInicio) || !TryParseHora(fila.HoraFin, out horaFin))
                        {
                            response.IsSuccess = false;
                            response.Message = $"El día {NombreDia(fila.DiaSemana)} está marcado como laboral y requiere hora de inicio y fin válidas.";
                            return JsonConvert.SerializeObject(response);
                        }

                        if (horaFin <= horaInicio)
                        {
                            response.IsSuccess = false;
                            response.Message = $"El día {NombreDia(fila.DiaSemana)} debe tener una hora de fin mayor a la hora de inicio.";
                            return JsonConvert.SerializeObject(response);
                        }

                        item.HoraInicio = horaInicio;
                        item.HoraFin = horaFin;
                    }
                    else
                    {
                        item.HoraInicio = null;
                        item.HoraFin = null;
                    }

                    lista.Add(item);
                }

                var guardarResponse = await _horarioService.GuardarHorarioLaboral(lista);
                return JsonConvert.SerializeObject(guardarResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al guardar el horario laboral");
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al guardar el horario laboral.";
                return JsonConvert.SerializeObject(response);
            }
        }

        [Permiso("ConfiguracionEmpresa", "Editar")]
        public async Task<string> SubirLogo(HttpPostedFileBase file)
        {
            var response = new ModelResponse();

            try
            {
                var tokenCookie = SessionHelper.GetSessionUser();
                if (tokenCookie == null || tokenCookie.EmpresaID == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "No se pudo determinar la empresa del usuario.";
                    return JsonConvert.SerializeObject(response);
                }

                // 1. Archivo vacío.
                if (file == null || file.ContentLength == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "El archivo está vacío.";
                    return JsonConvert.SerializeObject(response);
                }

                // 2. Extensión (guard principal, junto con accept del input).
                string ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
                string tiposPermitidos = ConfigurationManager.AppSettings["LogoEmpresaTiposPermitidos"] ?? "svg,png";
                var tipos = tiposPermitidos
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim().ToLowerInvariant())
                    .ToArray();

                if (tipos.Length == 0 || !tipos.Contains(ext))
                {
                    response.IsSuccess = false;
                    response.Message = "Formato de archivo no permitido. Solo se aceptan: " + string.Join(", ", tipos) + ".";
                    return JsonConvert.SerializeObject(response);
                }

                // 3. MIME (refuerzo; algunos navegadores envían octet-stream para SVG,
                //    por eso la extensión es el guard principal).
                bool mimeValido =
                    (ext == "svg" && string.Equals(file.ContentType, "image/svg+xml", StringComparison.OrdinalIgnoreCase))
                    || (ext == "png" && string.Equals(file.ContentType, "image/png", StringComparison.OrdinalIgnoreCase));

                if (!mimeValido)
                {
                    response.IsSuccess = false;
                    response.Message = "El tipo de archivo no coincide con el formato esperado.";
                    return JsonConvert.SerializeObject(response);
                }

                // 4. Tamaño.
                int maxTamanoKB;
                if (!int.TryParse(ConfigurationManager.AppSettings["LogoEmpresaMaxTamanoKB"], out maxTamanoKB))
                {
                    maxTamanoKB = 2048;
                }

                if (file.ContentLength > maxTamanoKB * 1024)
                {
                    response.IsSuccess = false;
                    response.Message = $"El archivo excede el tamaño máximo permitido de {maxTamanoKB} KB.";
                    return JsonConvert.SerializeObject(response);
                }

                // 5. Borrar logo previo (re-subida reemplaza; no hay acción de eliminar).
                var empresa = await _empresaService.ObtenerEmpresaPorId(tokenCookie.EmpresaID);
                if (empresa != null && !string.IsNullOrEmpty(empresa.LogoUrl))
                {
                    var rutaPrevia = Server.MapPath("~" + empresa.LogoUrl);
                    if (System.IO.File.Exists(rutaPrevia))
                    {
                        System.IO.File.Delete(rutaPrevia);
                    }
                }

                // 6. Guardar archivo en Uploads/Logos/{empresaId}/{guid}.{ext}.
                string guid = Guid.NewGuid().ToString();
                string carpetaRelativa = $"Uploads/Logos/{tokenCookie.EmpresaID}/";
                string carpetaAbsoluta = Server.MapPath("~/" + carpetaRelativa);

                if (!Directory.Exists(carpetaAbsoluta))
                {
                    Directory.CreateDirectory(carpetaAbsoluta);
                }

                string nombreArchivo = $"{guid}.{ext}";
                string rutaCompleta = Path.Combine(carpetaAbsoluta, nombreArchivo);
                file.SaveAs(rutaCompleta);

                string logoUrl = $"/{carpetaRelativa}{nombreArchivo}";

                // 7. Persistir únicamente la URL parcial vía WebApi.
                var guardarResponse = await _empresaService.GuardarLogoEmpresa(logoUrl);
                return JsonConvert.SerializeObject(guardarResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al subir el logotipo de la empresa");
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al subir el logotipo.";
                return JsonConvert.SerializeObject(response);
            }
        }

        [Permiso("ConfiguracionEmpresa", "Editar")]
        public async Task<string> QuitarLogo()
        {
            var response = new ModelResponse();

            try
            {
                var tokenCookie = SessionHelper.GetSessionUser();
                if (tokenCookie == null || tokenCookie.EmpresaID == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "No se pudo determinar la empresa del usuario.";
                    return JsonConvert.SerializeObject(response);
                }

                // Resolver la URL actual del logotipo para borrar el archivo físico.
                var empresa = await _empresaService.ObtenerEmpresaPorId(tokenCookie.EmpresaID);

                // Borrar el archivo físico si existe (no fallar si ya no está).
                if (empresa != null && !string.IsNullOrEmpty(empresa.LogoUrl))
                {
                    var rutaPrevia = Server.MapPath("~" + empresa.LogoUrl);
                    if (System.IO.File.Exists(rutaPrevia))
                    {
                        System.IO.File.Delete(rutaPrevia);
                    }
                }

                // Persistir NULL para quitar el logotipo (fallback a DESi en el sidebar).
                var guardarResponse = await _empresaService.GuardarLogoEmpresa(null);
                return JsonConvert.SerializeObject(guardarResponse);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al quitar el logotipo de la empresa");
                response.IsSuccess = false;
                response.Message = "Ocurrió un error al quitar el logotipo.";
                return JsonConvert.SerializeObject(response);
            }
        }

        #region Helpers

        /// <summary>
        /// Convierte "HH:mm" a un DateTime con ancla fija 1900-01-01
        /// (solo la componente de hora es significativa).
        /// </summary>
        private static bool TryParseHora(string hora, out DateTime resultado)
        {
            resultado = default(DateTime);
            if (string.IsNullOrWhiteSpace(hora))
            {
                return false;
            }

            DateTime parsed;
            if (!DateTime.TryParseExact(hora.Trim(), "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                return false;
            }

            resultado = new DateTime(1900, 1, 1, parsed.Hour, parsed.Minute, 0);
            return true;
        }

        private static string NombreDia(int diaSemana)
        {
            switch (diaSemana)
            {
                case 1: return "lunes";
                case 2: return "martes";
                case 3: return "miércoles";
                case 4: return "jueves";
                case 5: return "viernes";
                case 6: return "sábado";
                case 7: return "domingo";
                default: return diaSemana.ToString();
            }
        }

        #endregion
    }
}
