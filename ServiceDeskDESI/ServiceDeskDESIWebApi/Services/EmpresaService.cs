using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServiceDeskDESIWebApi.Services
{
    public class EmpresaService
    {
        private readonly DbWrapper _dbWrapper;

        public EmpresaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse ObtenerTodasLasEmpresas()
        {
            try
            {
                return _dbWrapper.ObtenerTodasLasEmpresas();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.ObtenerTodasLasEmpresas");
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener las empresas."
                };
            }
        }

        public ModelResponse ObtenerEmpresaPorId(long id, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.ObtenerEmpresaPorId(id, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEmpresaPorId para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.ObtenerEmpresaPorId para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la empresa."
                };
            }
        }

        public ModelResponse ObtenerEmpresaPorRFC(string rfc)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rfc)) { throw new ArgumentException("El RFC es requerido."); }

                return _dbWrapper.ObtenerEmpresaPorRFC(rfc);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEmpresaPorRFC para RFC {RFC}", rfc);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.ObtenerEmpresaPorRFC para RFC {RFC}", rfc);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la empresa."
                };
            }
        }

        public ModelResponse GuardarOActualizarEmpresa(Empresa empresa, string usuario)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empresa.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (empresa.NombreComercial.Length > 250) { throw new ArgumentException("El nombre comercial no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (empresa.RazonSocial.Length > 250) { throw new ArgumentException("La razón social no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (empresa.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (empresa.Responsable.Length > 250) { throw new ArgumentException("El responsable no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (empresa.Direccion.Length > 500) { throw new ArgumentException("La dirección no puede exceder los 500 caracteres."); }
                if (empresa.Ciudad != null && empresa.Ciudad.Length > 100) { throw new ArgumentException("La ciudad no puede exceder los 100 caracteres."); }
                if (empresa.Estado != null && empresa.Estado.Length > 100) { throw new ArgumentException("El estado no puede exceder los 100 caracteres."); }
                if (empresa.CodigoPostal != null && empresa.CodigoPostal.Length > 10) { throw new ArgumentException("El código postal no puede exceder los 10 caracteres."); }
                if (empresa.Telefono != null && empresa.Telefono.Length > 50) { throw new ArgumentException("El teléfono no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }
                if (empresa.CorreoContacto.Length > 250) { throw new ArgumentException("El correo de contacto no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.GuardarOActualizarEmpresa(empresa, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.GuardarOActualizarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la empresa."
                };
            }
        }

        public ModelResponse GuardarNuevaEmpresa(Empresa empresa)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(empresa.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (empresa.NombreComercial.Length > 250) { throw new ArgumentException("El nombre comercial no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (empresa.RazonSocial.Length > 250) { throw new ArgumentException("La razón social no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (empresa.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (empresa.Responsable.Length > 250) { throw new ArgumentException("El responsable no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (empresa.Direccion.Length > 500) { throw new ArgumentException("La dirección no puede exceder los 500 caracteres."); }
                if (empresa.Ciudad != null && empresa.Ciudad.Length > 100) { throw new ArgumentException("La ciudad no puede exceder los 100 caracteres."); }
                if (empresa.Estado != null && empresa.Estado.Length > 100) { throw new ArgumentException("El estado no puede exceder los 100 caracteres."); }
                if (empresa.CodigoPostal != null && empresa.CodigoPostal.Length > 10) { throw new ArgumentException("El código postal no puede exceder los 10 caracteres."); }
                if (empresa.Telefono != null && empresa.Telefono.Length > 50) { throw new ArgumentException("El teléfono no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }
                if (empresa.CorreoContacto.Length > 250) { throw new ArgumentException("El correo de contacto no puede exceder los 250 caracteres."); }
                if (empresa.FechaVigenciaInicio == DateTime.MinValue) { throw new ArgumentException("La fecha de vigencia inicio es requerida."); }
                if (empresa.FechaVigenciaFin == DateTime.MinValue) { throw new ArgumentException("La fecha de vigencia fin es requerida."); }
                if (string.IsNullOrWhiteSpace(empresa.CreadoPor)) { throw new ArgumentException("El usuario creador es requerido."); }

                return _dbWrapper.GuardarNuevaEmpresa(empresa);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarNuevaEmpresa");
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.GuardarNuevaEmpresa");
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al registrar la empresa."
                };
            }
        }

        public ModelResponse EliminarEmpresa(long id, string modificadoPor, DateTime fechaModificacion, string usuario)
        {
            try
            {
                if (id <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                return _dbWrapper.EliminarEmpresa(id, modificadoPor, fechaModificacion, usuario);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en EliminarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.EliminarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al eliminar la empresa."
                };
            }
        }

        public ModelResponse GuardarNuevaEmpresaConDatosIniciales(Empresa empresa)
        {
            var modelResponse = new ModelResponse();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Empresa empresaGuardada = null;
            Sucursal sucursalGuardada = null;
            Area areaGuardada = null;
            Usuario usuarioAdmin = null;
            long rolAdminId = 0;
            long usuarioAdminId = 0;

            try
            {
                Log.Information("=== INICIO REGISTRO DE NUEVA EMPRESA ===");
                Log.Information("Datos recibidos - NombreComercial: {NombreComercial}, RazonSocial: {RazonSocial}, RFC: {RFC}, Correo: {CorreoContacto}, Responsable: {Responsable}",
                    empresa?.NombreComercial, empresa?.RazonSocial, empresa?.RFC, empresa?.CorreoContacto, empresa?.Responsable);

                // =========================================
                // VALIDACIONES DE EMPRESA (antes de la transacción)
                // =========================================
                Log.Debug("Iniciando validaciones de campos requeridos...");

                if (string.IsNullOrWhiteSpace(empresa.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (empresa.NombreComercial.Length > 250) { throw new ArgumentException("El nombre comercial no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (empresa.RazonSocial.Length > 250) { throw new ArgumentException("La razón social no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (empresa.RFC.Length > 50) { throw new ArgumentException("El RFC no puede exceder los 50 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (empresa.Responsable.Length > 250) { throw new ArgumentException("El responsable no puede exceder los 250 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (empresa.Direccion.Length > 500) { throw new ArgumentException("La dirección no puede exceder los 500 caracteres."); }
                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }
                if (empresa.CorreoContacto.Length > 250) { throw new ArgumentException("El correo de contacto no puede exceder los 250 caracteres."); }

                Log.Information("Validaciones completadas exitosamente para RFC: {RFC}", empresa.RFC);

                // =========================================
                // INICIO DE TRANSACCIÓN
                // =========================================
                using (var scope = new System.Transactions.TransactionScope(
                    System.Transactions.TransactionScopeOption.Required,
                    new System.Transactions.TransactionOptions
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted,
                        Timeout = TimeSpan.FromMinutes(5)
                    }))
                {
                    Log.Information("Iniciando transacción para registro de empresa...");

                    // =========================================
                    // PASO 1: GUARDAR EMPRESA
                    // =========================================
                    Log.Information("PASO 1/7 - Iniciando guardado de empresa en BD...");

                    empresa.FechaVigenciaInicio = DateTime.Now;
                    empresa.FechaVigenciaFin = DateTime.Now.AddDays(30);
                    empresa.EsPeriodoPrueba = true;
                    empresa.CreadoPor = "system.register";
                    empresa.FechaCreacion = DateTime.Now;
                    empresa.Estatus = true;

                    var empresaResponse = _dbWrapper.GuardarNuevaEmpresa(empresa);

                    if (!empresaResponse.IsSuccess || empresaResponse.Response == null)
                    {
                        Log.Error("❌ PASO 1/7 - FALLÓ el guardado de empresa. RFC: {RFC}, Error: {Error}", empresa.RFC, empresaResponse.Message);
                        throw new Exception(empresaResponse.Message ?? "Error al guardar la empresa");
                    }

                    empresaGuardada = (Empresa)empresaResponse.Response;
                    Log.Information("✅ PASO 1/7 - Empresa guardada exitosamente. Id: {EmpresaId}, Nombre: {NombreEmpresa}",
                        empresaGuardada.Id, empresaGuardada.NombreComercial);

                    var usernameAdmin = $"admin_{empresaGuardada.Id}";
                    Log.Debug("Username administrador generado: {Username}", usernameAdmin);

                    // =========================================
                    // PASO 2: GUARDAR SUCURSAL
                    // =========================================
                    Log.Information("PASO 2/7 - Creando sucursal para la empresa...");

                    var sucursal = new Sucursal()
                    {
                        Nombre = empresaGuardada.NombreComercial,
                        Descripcion = $"Sucursal principal de {empresaGuardada.NombreComercial}",
                        Calle = empresaGuardada.Direccion,
                        Ciudad = empresaGuardada.Ciudad,
                        Colonia = null,
                        CodigoPostal = empresaGuardada.CodigoPostal,
                        CreadoPor = usernameAdmin,
                        FechaCreacion = DateTime.Now,
                        Estatus = true
                    };

                    var sucursalResponse = _dbWrapper.GuardarNuevaSucursalParaEmpresa(sucursal);

                    if (!sucursalResponse.IsSuccess || sucursalResponse.Response == null)
                    {
                        Log.Error("❌ PASO 2/7 - FALLÓ la creación de sucursal. EmpresaId: {EmpresaId}, Error: {Error}",
                            empresaGuardada.Id, sucursalResponse.Message);
                        throw new Exception(sucursalResponse.Message ?? "Error al guardar la sucursal");
                    }

                    sucursalGuardada = (Sucursal)sucursalResponse.Response;
                    Log.Information("✅ PASO 2/7 - Sucursal creada exitosamente. Id: {SucursalId}, Nombre: {SucursalNombre}",
                        sucursalGuardada.Id, sucursalGuardada.Nombre);

                    // =========================================
                    // PASO 3: GUARDAR ÁREA (TI)
                    // =========================================
                    Log.Information("PASO 3/7 - Creando área 'TI' para la empresa...");

                    var area = new Area()
                    {
                        Nombre = "TI",
                        Descripcion = "Área de Tecnologías de la Información",
                        Correo = empresaGuardada.CorreoContacto,
                        CreadoPor = usernameAdmin,
                        FechaCreacion = DateTime.Now,
                        Estatus = true
                    };

                    var areaResponse = _dbWrapper.GuardarNuevaAreaParaEmpresa(area);

                    if (!areaResponse.IsSuccess || areaResponse.Response == null)
                    {
                        Log.Error("❌ PASO 3/7 - FALLÓ la creación del área. EmpresaId: {EmpresaId}, Error: {Error}",
                            empresaGuardada.Id, areaResponse.Message);
                        throw new Exception(areaResponse.Message ?? "Error al guardar el área");
                    }

                    areaGuardada = (Area)areaResponse.Response;
                    Log.Information("✅ PASO 3/7 - Área 'TI' creada exitosamente. Id: {AreaId}, Nombre: {AreaNombre}",
                        areaGuardada.Id, areaGuardada.Nombre);

                    // =========================================
                    // PASO 4: GUARDAR USUARIO ADMINISTRADOR
                    // =========================================
                    Log.Information("PASO 4/7 - Creando usuario administrador para la empresa...");

                    usuarioAdmin = new Usuario()
                    {
                        NombreUsuario = usernameAdmin,
                        Contrasena = Cryptography.Encrypt("Admin123!"),
                        ImagenPerfil = null,
                        Correo = empresaGuardada.CorreoContacto,
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        Celular = empresaGuardada.Telefono,
                        Sucursal = sucursalGuardada,
                        Firma = null,
                        RFC = empresaGuardada.RFC,
                        Area = areaGuardada,
                        Empresa = empresaGuardada,
                        CreadoPor = usernameAdmin,
                        FechaCreacion = DateTime.Now,
                        Estatus = true
                    };

                    var usuarioResponse = _dbWrapper.GuardarOActualizarUsuario(usuarioAdmin);

                    if (!usuarioResponse.IsSuccess)
                    {
                        Log.Error("❌ PASO 4/7 - FALLÓ la creación del usuario administrador. EmpresaId: {EmpresaId}, Username: {Username}, Error: {Error}",
                            empresaGuardada.Id, usernameAdmin, usuarioResponse.Message);
                        throw new Exception(usuarioResponse.Message ?? "Error al guardar el usuario administrador");
                    }

                    usuarioAdmin = (Usuario)usuarioResponse.Response;
                    usuarioAdminId = usuarioAdmin.Id;
                    Log.Information("✅ PASO 4/7 - Usuario administrador creado exitosamente. Id: {UsuarioId}, Username: {Username}, Correo: {Correo}",
                        usuarioAdminId, usernameAdmin, empresaGuardada.CorreoContacto);

                    // =========================================
                    // PASO 5: CREAR ROLES BASE
                    // =========================================
                    Log.Information("PASO 5/7 - Creando roles base para la empresa...");

                    var rolesBase = new List<Rol>
                    {
                        new Rol { Nombre = "Administrador", Descripcion = "Control total del sistema", CreadoPor = usernameAdmin, FechaCreacion = DateTime.Now, Estatus = true },
                        new Rol { Nombre = "Supervisor", Descripcion = "Gestión de tickets y usuarios", CreadoPor = usernameAdmin, FechaCreacion = DateTime.Now, Estatus = true },
                        new Rol { Nombre = "Agente", Descripcion = "Atención de tickets", CreadoPor = usernameAdmin, FechaCreacion = DateTime.Now, Estatus = true },
                        new Rol { Nombre = "Usuario", Descripcion = "Creación de tickets", CreadoPor = usernameAdmin, FechaCreacion = DateTime.Now, Estatus = true }
                    };

                    foreach (var rol in rolesBase)
                    {
                        var rolResponse = _dbWrapper.GuardarRolParaNuevaEmpresa(rol);
                        if (!rolResponse.IsSuccess)
                        {
                            Log.Error("❌ PASO 5/7 - FALLÓ la creación del rol {NombreRol}. Error: {Error}", rol.Nombre, rolResponse.Message);
                            throw new Exception($"Error al crear el rol '{rol.Nombre}': {rolResponse.Message}");
                        }

                        if (rol.Nombre == "Administrador")
                        {
                            rolAdminId = ((Rol)rolResponse.Response).Id;
                        }
                    }

                    Log.Information("✅ PASO 5/7 - Roles base creados exitosamente para empresa {EmpresaId}", empresaGuardada.Id);

                    // =========================================
                    // PASO 6: ASIGNAR ROL "ADMINISTRADOR" AL USUARIO
                    // =========================================
                    Log.Information("PASO 6/7 - Asignando rol 'Administrador' al usuario...");

                    var asignarRolResponse = _dbWrapper.AsignarRolUsuarioParaNuevaEmpresa(usuarioAdminId, rolAdminId, usernameAdmin);

                    if (!asignarRolResponse.IsSuccess)
                    {
                        Log.Error("❌ PASO 6/7 - FALLÓ la asignación del rol 'Administrador' al usuario {Username}", usernameAdmin);
                        throw new Exception($"Error al asignar el rol 'Administrador' al usuario: {asignarRolResponse.Message}");
                    }

                    Log.Information("✅ PASO 6/7 - Rol 'Administrador' asignado al usuario {Username}", usernameAdmin);

                    // =========================================
                    // PASO 7: ASIGNAR PÁGINAS AL USUARIO ADMINISTRADOR
                    // =========================================
                    Log.Information("PASO 7/7 - Asignando páginas al usuario administrador...");

                    var paginasResponse = _dbWrapper.ObtenerPaginas();
                    if (paginasResponse.IsSuccess && paginasResponse.Response != null)
                    {
                        var paginas = (List<Pagina>)paginasResponse.Response;
                        int paginasAsignadas = 0;

                        foreach (var pagina in paginas)
                        {
                            var insertResponse = _dbWrapper.InsertarUsuarioPaginaParaNuevaEmpresa(usuarioAdminId, pagina.Id, usernameAdmin);
                            if (insertResponse.IsSuccess)
                            {
                                paginasAsignadas++;
                            }
                            else
                            {
                                Log.Warning("⚠️ No se pudo asignar la página {PaginaId} al usuario {UsuarioId}: {Error}",
                                    pagina.Id, usuarioAdminId, insertResponse.Message);
                            }
                        }

                        Log.Information("✅ PASO 7/7 - {Count} páginas asignadas al usuario administrador", paginasAsignadas);
                    }
                    else
                    {
                        Log.Warning("⚠️ No se encontraron páginas para asignar al usuario administrador");
                    }

                    // =========================================
                    // COMPLETAR TRANSACCIÓN
                    // =========================================
                    scope.Complete();
                    Log.Information("✅ Transacción completada exitosamente.");
                }

                // =========================================
                // FIN DE LA TRANSACCIÓN - ENVIAR CORREO
                // =========================================
                Log.Information("Enviando correo de bienvenida a: {Correo}...", empresaGuardada.CorreoContacto);

                var emailSent = EnviarCorreoBienvenida(empresaGuardada, usuarioAdmin.NombreUsuario, "Admin123!");

                if (emailSent)
                {
                    Log.Information("✅ Correo de bienvenida enviado exitosamente a: {Correo}", empresaGuardada.CorreoContacto);
                }
                else
                {
                    Log.Warning("⚠️ El correo de bienvenida NO pudo ser enviado a: {Correo}. La empresa y usuario fueron creados correctamente, pero el usuario no recibirá sus credenciales por correo.",
                        empresaGuardada.CorreoContacto);
                }

                stopwatch.Stop();
                Log.Information("=== REGISTRO DE EMPRESA COMPLETADO EXITOSAMENTE ===");
                Log.Information("Resumen final - EmpresaId: {EmpresaId}, SucursalId: {SucursalId}, AreaId: {AreaId}, Username: {Username}, Duración total: {Duration}ms",
                    empresaGuardada.Id, sucursalGuardada.Id, areaGuardada.Id, usuarioAdmin.NombreUsuario, stopwatch.ElapsedMilliseconds);

                modelResponse.IsSuccess = true;
                modelResponse.Response = empresaGuardada;
                modelResponse.Message = "Empresa registrada correctamente con sucursal, área, usuario administrador, roles base y permisos.";
            }
            catch (ArgumentException ex)
            {
                stopwatch.Stop();
                Log.Warning(ex, "⚠️ VALIDACIÓN FALLIDA - Error de validación al registrar empresa. Datos: {@Empresa}, Duración: {Duration}ms",
                    new { empresa?.NombreComercial, empresa?.RFC, empresa?.CorreoContacto }, stopwatch.ElapsedMilliseconds);

                modelResponse.IsSuccess = false;
                modelResponse.Message = ex.Message;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error(ex, "❌ ERROR CRÍTICO - Fallo en el registro de empresa. Datos: {@Empresa}, Duración: {Duration}ms",
                    new { empresa?.NombreComercial, empresa?.RFC, empresa?.CorreoContacto }, stopwatch.ElapsedMilliseconds);

                modelResponse.IsSuccess = false;
                modelResponse.Message = "Ocurrió un error al registrar la empresa. Por favor, intente nuevamente.";
            }

            return modelResponse;
        }

        private bool EnviarCorreoBienvenida(Empresa empresa, string usuario, string contrasenaTemporal)
        {
            try
            {
                Log.Debug("Preparando plantilla de correo para: {Email}", empresa.CorreoContacto);

                // Obtener URL base del Web.config
                string baseUri = System.Configuration.ConfigurationManager.AppSettings["BaseUri"];
                string urlLogin = $"{baseUri}Home/Autentication";

                // Leer template
                string templatePath = System.Web.Hosting.HostingEnvironment.MapPath("~/Template/Template_AltaEmpresa.html");

                if (!System.IO.File.Exists(templatePath))
                {
                    Log.Error("No se encontró la plantilla de correo en: {TemplatePath}", templatePath);
                    return false;
                }

                string templateHtml = System.IO.File.ReadAllText(templatePath);

                // Reemplazar variables en el template
                templateHtml = templateHtml.Replace("{{NombreCompleto}}", empresa.Responsable);
                templateHtml = templateHtml.Replace("{{NombreEmpresa}}", empresa.NombreComercial);
                templateHtml = templateHtml.Replace("{{RFC}}", empresa.RFC);
                templateHtml = templateHtml.Replace("{{CorreoContacto}}", empresa.CorreoContacto);
                templateHtml = templateHtml.Replace("{{Usuario}}", usuario);
                templateHtml = templateHtml.Replace("{{ContrasenaTemporal}}", contrasenaTemporal);
                templateHtml = templateHtml.Replace("{{UrlLogin}}", urlLogin);

                Log.Debug("Plantilla procesada, enviando correo a: {Email}", empresa.CorreoContacto);

                // Enviar correo
                var para = new List<string> { empresa.CorreoContacto };
                EmailHelper.EnvioEmaiil(para, "Bienvenido a Service Desk DESI - Tus credenciales de acceso", templateHtml, false);

                Log.Information("Correo enviado exitosamente a: {Email}", empresa.CorreoContacto);
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "FALLO al enviar correo de bienvenida a: {Email}. La empresa quedó registrada sin credenciales enviadas.",
                    empresa.CorreoContacto);
                return false;
            }
        }
    }
}