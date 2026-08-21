using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIEntities.Tickets;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace ServiceDeskDESIWebApi.Services
{
    public class EvidenciaService
    {
        private readonly DbWrapper _dbWrapper;

        public EvidenciaService()
        {
            _dbWrapper = new DbWrapper();
        }

        /// <summary>
        /// Lee la configuración de evidencias desde Web.config (fuente de verdad única).
        /// </summary>
        public EvidenciaConfigDTO ObtenerConfiguracion()
        {
            var config = new EvidenciaConfigDTO();

            int maxArchivos;
            if (!int.TryParse(ConfigurationManager.AppSettings["EvidenciasMaxArchivos"], out maxArchivos))
                maxArchivos = 3;
            config.MaxArchivos = maxArchivos;

            int maxTamanoMB;
            if (!int.TryParse(ConfigurationManager.AppSettings["EvidenciasMaxTamanoMB"], out maxTamanoMB))
                maxTamanoMB = 3;
            config.MaxTamanoMB = maxTamanoMB;

            var extensiones = ConfigurationManager.AppSettings["EvidenciasExtensionesPermitidas"];
            if (string.IsNullOrWhiteSpace(extensiones))
                extensiones = "pdf,jpg,png";

            config.ExtensionesPermitidas = extensiones
                .Split(',')
                .Select(e => e.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();

            return config;
        }

        /// <summary>
        /// Orquestación autoritativa de subida: valida empresa, tope de archivos,
        /// peso y extensión; luego escribe a disco y guarda el registro.
        /// </summary>
        public ModelResponse<List<TicketEvidencia>> GuardarEvidencias(long ticketId, HttpFileCollection files, string usuario, long empresaId)
        {
            try
            {
                Log.Information("EvidenciaService.GuardarEvidencias para TicketId {TicketId} usuario {Usuario}, EmpresaId {EmpresaId}", ticketId, usuario, empresaId);

                if (ticketId <= 0)
                    return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "TicketId requerido." };

                if (string.IsNullOrWhiteSpace(usuario))
                    return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "El nombre de usuario es requerido." };

                if (files == null || files.Count == 0)
                    return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "Debe seleccionar al menos un archivo." };

                var config = ObtenerConfiguracion();

                // 1. El EmpresaId viene del claim del usuario autenticado.
                if (empresaId <= 0)
                    return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "No se pudo determinar la empresa del usuario." };

                // 2. El número total (existentes + nuevos) no debe exceder MaxArchivos.
                var existentes = _dbWrapper.ObtenerEvidenciasPorTicket(ticketId, usuario);
                if (existentes.Count + files.Count > config.MaxArchivos)
                    return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = $"No puede adjuntar más de {config.MaxArchivos} archivos a este ticket." };

                long maxTamanoBytes = (long)config.MaxTamanoMB * 1024 * 1024;

                // 3 y 4. Validar peso y extensión de cada archivo ANTES de escribir nada.
                foreach (string key in files.AllKeys)
                {
                    var file = files[key];
                    if (file == null) continue;

                    var extensionLower = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();

                    if (!config.ExtensionesPermitidas.Contains(extensionLower))
                        return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = "Extensión no permitida. Solo se aceptan: " + string.Join(", ", config.ExtensionesPermitidas) + "." };

                    if (file.ContentLength > maxTamanoBytes)
                        return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = $"El archivo '{file.FileName}' supera el tamaño máximo de {config.MaxTamanoMB} MB." };
                }

                // FASE A — Escribir TODOS los archivos a disco primero (sin tocar BD).
                // FASE B — Insertar TODOS los registros dentro de una transacción.
                // Garantiza "todo o nada": si algo falla se limpian los archivos y se
                // revierte la BD (ni archivo ni registro quedan a medias).
                var guardadas = new List<TicketEvidencia>();

                try
                {
                    foreach (string key in files.AllKeys)
                    {
                        var file = files[key];
                        if (file == null) continue;

                        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        var nombreFisico = Guid.NewGuid().ToString("N") + extension;
                        var rutaRelativa = $"Evidencias/{empresaId}/{ticketId}/{nombreFisico}";

                        var rutaAbsoluta = HostingEnvironment.MapPath("~/" + rutaRelativa);
                        if (string.IsNullOrEmpty(rutaAbsoluta))
                            throw new InvalidOperationException("No se pudo resolver la ruta de almacenamiento de evidencias.");

                        var directorio = Path.GetDirectoryName(rutaAbsoluta);
                        if (!Directory.Exists(directorio))
                            Directory.CreateDirectory(directorio);

                        file.SaveAs(rutaAbsoluta);

                        guardadas.Add(new TicketEvidencia
                        {
                            Id = 0,
                            TicketId = ticketId,
                            EmpresaId = empresaId,
                            NombreArchivo = file.FileName,
                            RutaArchivo = rutaRelativa,
                            FechaSubida = DateTime.Now
                        });
                    }
                }
                catch (Exception ex)
                {
                    LimpiarArchivosEnDisco(guardadas);
                    Log.Error(ex, "Error al escribir evidencias a disco para ticket {TicketId} usuario {Usuario}", ticketId, usuario);
                    return new ModelResponse<List<TicketEvidencia>>
                    {
                        IsSuccess = false,
                        Message = "No se pudo guardar la evidencia. No se registró ningún archivo."
                    };
                }

                try
                {
                    _dbWrapper.BeginTransaction();

                    foreach (var evidencia in guardadas)
                    {
                        var nuevoId = _dbWrapper.GuardarEvidencia(ticketId, evidencia.NombreArchivo, evidencia.RutaArchivo, usuario);
                        if (nuevoId == 0)
                            throw new InvalidOperationException("No se pudo registrar la evidencia en la base de datos.");

                        evidencia.Id = nuevoId;
                    }

                    _dbWrapper.CommitTransaction();
                }
                catch (Exception ex)
                {
                    _dbWrapper.RollbackTransaction();
                    LimpiarArchivosEnDisco(guardadas);
                    Log.Error(ex, "Error al registrar evidencias en BD para ticket {TicketId} usuario {Usuario}", ticketId, usuario);
                    return new ModelResponse<List<TicketEvidencia>>
                    {
                        IsSuccess = false,
                        Message = "No se pudo guardar la evidencia. No se registró ningún archivo."
                    };
                }

                var result = new ModelResponse<List<TicketEvidencia>>
                {
                    IsSuccess = true,
                    Response = guardadas,
                    Message = "Evidencias guardadas correctamente."
                };
                Log.Information("EvidenciaService.GuardarEvidencias RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EvidenciaService.GuardarEvidencias para ticket {TicketId} usuario {Usuario}", ticketId, usuario);
                return new ModelResponse<List<TicketEvidencia>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar las evidencias."
                };
            }
        }

        public ModelResponse<List<TicketEvidencia>> ObtenerEvidenciasPorTicket(long ticketId, string usuario)
        {
            try
            {
                Log.Information("EvidenciaService.ObtenerEvidenciasPorTicket para TicketId {TicketId} usuario {Usuario}", ticketId, usuario);

                if (ticketId <= 0) throw new ArgumentException("El ID del ticket es requerido.");
                if (string.IsNullOrWhiteSpace(usuario)) throw new ArgumentException("El nombre de usuario es requerido.");

                var evidencias = _dbWrapper.ObtenerEvidenciasPorTicket(ticketId, usuario);

                var result = new ModelResponse<List<TicketEvidencia>>
                {
                    IsSuccess = true,
                    Response = evidencias,
                    Message = "Evidencias obtenidas correctamente."
                };
                Log.Information("EvidenciaService.ObtenerEvidenciasPorTicket RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEvidenciasPorTicket para usuario {Usuario}", usuario);
                return new ModelResponse<List<TicketEvidencia>> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EvidenciaService.ObtenerEvidenciasPorTicket para ticket {TicketId} usuario {Usuario}", ticketId, usuario);
                return new ModelResponse<List<TicketEvidencia>>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las evidencias."
                };
            }
        }

        public ModelResponse<EvidenciaDescargaDTO> ObtenerEvidenciaParaDescarga(long id, string usuario)
        {
            try
            {
                Log.Information("EvidenciaService.ObtenerEvidenciaParaDescarga para Id {Id} usuario {Usuario}", id, usuario);

                if (id <= 0) throw new ArgumentException("El ID de la evidencia es requerido.");
                if (string.IsNullOrWhiteSpace(usuario)) throw new ArgumentException("El nombre de usuario es requerido.");

                var evidencia = _dbWrapper.ObtenerEvidencia(id, usuario);
                if (evidencia == null)
                    return new ModelResponse<EvidenciaDescargaDTO> { IsSuccess = false, Message = "Evidencia no encontrada." };

                var rutaAbsoluta = HostingEnvironment.MapPath("~/" + evidencia.RutaArchivo);
                if (string.IsNullOrEmpty(rutaAbsoluta) || !File.Exists(rutaAbsoluta))
                    return new ModelResponse<EvidenciaDescargaDTO> { IsSuccess = false, Message = "Archivo no encontrado." };

                var dto = new EvidenciaDescargaDTO
                {
                    NombreArchivo = evidencia.NombreArchivo,
                    ContentType = ObtenerContentType(evidencia.RutaArchivo),
                    Contenido = File.ReadAllBytes(rutaAbsoluta)
                };

                var result = new ModelResponse<EvidenciaDescargaDTO>
                {
                    IsSuccess = true,
                    Response = dto,
                    Message = "Evidencia obtenida correctamente."
                };
                Log.Information("EvidenciaService.ObtenerEvidenciaParaDescarga RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEvidenciaParaDescarga para usuario {Usuario}", usuario);
                return new ModelResponse<EvidenciaDescargaDTO> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EvidenciaService.ObtenerEvidenciaParaDescarga para evidencia {Id} usuario {Usuario}", id, usuario);
                return new ModelResponse<EvidenciaDescargaDTO>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la evidencia."
                };
            }
        }

        /// <summary>
        /// Elimina del disco los archivos físicos correspondientes a las evidencias dadas.
        /// Se usa como compensación cuando falla la escritura o el registro en BD, para
        /// garantizar que no queden archivos huérfanos (comportamiento "todo o nada").
        /// </summary>
        private void LimpiarArchivosEnDisco(IEnumerable<TicketEvidencia> evidencias)
        {
            if (evidencias == null) return;

            foreach (var evidencia in evidencias)
            {
                try
                {
                    if (evidencia == null || string.IsNullOrWhiteSpace(evidencia.RutaArchivo)) continue;

                    var rutaAbsoluta = HostingEnvironment.MapPath("~/" + evidencia.RutaArchivo);
                    if (!string.IsNullOrEmpty(rutaAbsoluta) && File.Exists(rutaAbsoluta))
                        File.Delete(rutaAbsoluta);
                }
                catch
                {
                    // No bloquear el flujo por un fallo de limpieza; el error principal ya se registra.
                }
            }
        }

        private string ObtenerContentType(string rutaArchivo)
        {
            var extension = Path.GetExtension(rutaArchivo).TrimStart('.').ToLowerInvariant();

            switch (extension)
            {
                case "pdf": return "application/pdf";
                case "jpg":
                case "jpeg": return "image/jpeg";
                case "png": return "image/png";
                default: return "application/octet-stream";
            }
        }
    }
}
