using Serilog;
using ServiceDeskDESIEntities.Autenticacion;
using ServiceDeskDESIEntities.Catalogos;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using ServiceDeskDESIWebApi.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ServiceDeskDESIWebApi.Services
{
    public class EmpresaService
    {
        private readonly DbWrapper _dbWrapper;

        public EmpresaService()
        {
            _dbWrapper = new DbWrapper();
        }

        public ModelResponse<Empresa> ObtenerEmpresaPorId(long id, string usuario)
        {
            try
            {
                Log.Information("EmpresaService.ObtenerEmpresaPorId para id {Id} y usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.ObtenerEmpresaPorId(id, usuario);
                Log.Information("EmpresaService.ObtenerEmpresaPorId RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEmpresaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Empresa> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.ObtenerEmpresaPorId para usuario {Usuario}", usuario);
                return new ModelResponse<Empresa>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la empresa."
                };
            }
        }

        public ModelResponse<Empresa> ObtenerEmpresaPorRFC(string rfc)
        {
            try
            {
                Log.Information("EmpresaService.ObtenerEmpresaPorRFC para RFC {RFC}", rfc);

                if (string.IsNullOrWhiteSpace(rfc)) { throw new ArgumentException("El RFC es requerido."); }

                var result = _dbWrapper.ObtenerEmpresaPorRFC(rfc);
                Log.Information("EmpresaService.ObtenerEmpresaPorRFC RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en ObtenerEmpresaPorRFC para RFC {RFC}", rfc);
                return new ModelResponse<Empresa> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.ObtenerEmpresaPorRFC para RFC {RFC}", rfc);
                return new ModelResponse<Empresa>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al obtener la empresa."
                };
            }
        }

        public ModelResponse<Empresa> GuardarOActualizarEmpresa(Empresa empresa, string usuario)
        {
            try
            {
                Log.Information("EmpresaService.GuardarOActualizarEmpresa para usuario {Usuario} y RFC {RFC}", usuario, empresa?.RFC);

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

                var result = _dbWrapper.GuardarOActualizarEmpresa(empresa, usuario);
                Log.Information("EmpresaService.GuardarOActualizarEmpresa RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarOActualizarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse<Empresa> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.GuardarOActualizarEmpresa para usuario {Usuario}", usuario);
                return new ModelResponse<Empresa>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al guardar la empresa."
                };
            }
        }

        public ModelResponse<Empresa> GuardarNuevaEmpresa(Empresa empresa)
        {
            try
            {
                Log.Information("EmpresaService.GuardarNuevaEmpresa para RFC {RFC} y nombre comercial {NombreComercial}", empresa?.RFC, empresa?.NombreComercial);

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

                var result = _dbWrapper.GuardarNuevaEmpresa(empresa);
                Log.Information("EmpresaService.GuardarNuevaEmpresa RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en GuardarNuevaEmpresa");
                return new ModelResponse<Empresa> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.GuardarNuevaEmpresa");
                return new ModelResponse<Empresa>
                {
                    IsSuccess = false,
                    Message = "Ocurrió un error al registrar la empresa."
                };
            }
        }

        public ModelResponse<Empresa> RegistrarEmpresa(Empresa empresa)
        {
            try
            {
                // 1. Validar campos requeridos (espejo del flujo MVC pre-login)
                if (empresa == null) { throw new ArgumentException("Los datos de la empresa son requeridos."); }
                if (string.IsNullOrWhiteSpace(empresa.NombreComercial)) { throw new ArgumentException("El nombre comercial es requerido."); }
                if (string.IsNullOrWhiteSpace(empresa.RazonSocial)) { throw new ArgumentException("La razón social es requerida."); }
                if (string.IsNullOrWhiteSpace(empresa.RFC)) { throw new ArgumentException("El RFC es requerido."); }
                if (string.IsNullOrWhiteSpace(empresa.Responsable)) { throw new ArgumentException("El responsable es requerido."); }
                if (string.IsNullOrWhiteSpace(empresa.Direccion)) { throw new ArgumentException("La dirección es requerida."); }
                if (string.IsNullOrWhiteSpace(empresa.CorreoContacto)) { throw new ArgumentException("El correo de contacto es requerido."); }

                // 2. Unicidad server-side
                var rfcResponse = ObtenerEmpresaPorRFC(empresa.RFC);
                if (rfcResponse.IsSuccess && rfcResponse.Response != null)
                {
                    throw new ArgumentException("Ya existe una empresa registrada con este RFC.");
                }

                var correoResponse = _dbWrapper.ObtenerEmpresaPorCorreoContacto(empresa.CorreoContacto);
                if (correoResponse.IsSuccess && correoResponse.Response != null)
                {
                    throw new ArgumentException("Ya existe una empresa registrada con este correo de contacto.");
                }

                var nombreResponse = _dbWrapper.ObtenerEmpresaPorNombreComercial(empresa.NombreComercial);
                if (nombreResponse.IsSuccess && nombreResponse.Response != null)
                {
                    throw new ArgumentException("Ya existe una empresa registrada con este nombre comercial.");
                }

                var razonResponse = _dbWrapper.ObtenerEmpresaPorRazonSocial(empresa.RazonSocial);
                if (razonResponse.IsSuccess && razonResponse.Response != null)
                {
                    throw new ArgumentException("Ya existe una empresa registrada con esta razón social.");
                }

                // 3. Campos de vigencia / periodo de prueba (espejo del flujo MVC pre-login)
                empresa.FechaVigenciaInicio = DateTime.Now;
                empresa.FechaVigenciaFin = DateTime.Now.AddDays(30);
                empresa.EsPeriodoPrueba = true;
                empresa.CreadoPor = "system.register";
                empresa.FechaCreacion = DateTime.Now;
                empresa.Estatus = true;

                // 4. Registro completo (empresa + datos iniciales vía SPs)
                return GuardarNuevaEmpresaConDatosIniciales(empresa);
            }
            catch (ArgumentException ex)
            {
                Log.Warning(ex, "Error de validación en RegistrarEmpresa");
                return new ModelResponse<Empresa> { IsSuccess = false, Message = ex.Message };
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error en EmpresaService.RegistrarEmpresa");
                return new ModelResponse<Empresa>
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
                Log.Information("EmpresaService.EliminarEmpresa para id {Id} por usuario {Usuario}", id, usuario);

                if (id <= 0) { throw new ArgumentException("El ID de la empresa es requerido."); }
                if (string.IsNullOrWhiteSpace(modificadoPor)) { throw new ArgumentException("El usuario modificador es requerido."); }
                if (string.IsNullOrWhiteSpace(usuario)) { throw new ArgumentException("El nombre de usuario es requerido."); }

                var result = _dbWrapper.EliminarEmpresa(id, modificadoPor, fechaModificacion, usuario);
                Log.Information("EmpresaService.EliminarEmpresa RESULTADO: IsSuccess={IsSuccess}, Message={Message}", result?.IsSuccess, result?.Message);
                return result;
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

        public ModelResponse<Empresa> GuardarNuevaEmpresaConDatosIniciales(Empresa empresa)
        {
            var modelResponse = new ModelResponse<Empresa>();
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Empresa empresaGuardada = null;
            Sucursal sucursalGuardada = null;
            Area areaGuardada = null;
            Usuario usuarioAdmin = null;
            long rolAdminId = 0;
            long rolUsuarioId = 0;
            long usuarioAdminId = 0;
            string contrasenaTemporal = null;

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
                // Se usa una única SqlConnection + SqlTransaction (NO TransactionScope)
                // para evitar la escalación a MSDTC al abrir múltiples conexiones.
                _dbWrapper.BeginTransaction();
                try
                {
                    Log.Information("Iniciando transacción para registro de empresa...");

                    // =========================================
                    // PASO 1: GUARDAR EMPRESA
                    // =========================================
                    Log.Information("PASO 1/8 - Iniciando guardado de empresa en BD...");

                    empresa.FechaVigenciaInicio = DateTime.Now;
                    empresa.FechaVigenciaFin = DateTime.Now.AddDays(30);
                    empresa.EsPeriodoPrueba = true;
                    empresa.CreadoPor = "system.register";
                    empresa.FechaCreacion = DateTime.Now;
                    empresa.Estatus = true;

                    var empresaResponse = _dbWrapper.GuardarNuevaEmpresa(empresa);

                    if (!empresaResponse.IsSuccess || empresaResponse.Response == null)
                    {
                        Log.Error("❌ PASO 1/8 - FALLÓ el guardado de empresa. RFC: {RFC}, Error: {Error}", empresa.RFC, empresaResponse.Message);
                        throw new Exception(empresaResponse.Message ?? "Error al guardar la empresa");
                    }

                    empresaGuardada = empresaResponse.Response;
                    Log.Information("✅ PASO 1/8 - Empresa guardada exitosamente. Id: {EmpresaId}, Nombre: {NombreEmpresa}",
                        empresaGuardada.Id, empresaGuardada.NombreComercial);

                    var usernameAdmin = GenerarUsernameAdminUnico(empresa.Responsable);
                    Log.Debug("Username administrador generado a partir del responsable: {Username}", usernameAdmin);

                    // =========================================
                    // PASO 2: GUARDAR SUCURSAL
                    // =========================================
                    Log.Information("PASO 2/8 - Creando sucursal para la empresa...");

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

                    var sucursalResponse = _dbWrapper.GuardarNuevaSucursalParaEmpresa(sucursal, empresaGuardada.Id);

                    if (!sucursalResponse.IsSuccess || sucursalResponse.Response == null)
                    {
                        Log.Error("❌ PASO 2/8 - FALLÓ la creación de sucursal. EmpresaId: {EmpresaId}, Error: {Error}",
                            empresaGuardada.Id, sucursalResponse.Message);
                        throw new Exception(sucursalResponse.Message ?? "Error al guardar la sucursal");
                    }

                    sucursalGuardada = (Sucursal)sucursalResponse.Response;
                    Log.Information("✅ PASO 2/8 - Sucursal creada exitosamente. Id: {SucursalId}, Nombre: {SucursalNombre}",
                        sucursalGuardada.Id, sucursalGuardada.Nombre);

                    // =========================================
                    // PASO 3: GUARDAR ÁREA (TI)
                    // =========================================
                    Log.Information("PASO 3/8 - Creando área 'TI' para la empresa...");

                    var area = new Area()
                    {
                        Nombre = "TI",
                        Descripcion = "Área de Tecnologías de la Información",
                        Correo = empresaGuardada.CorreoContacto,
                        CreadoPor = usernameAdmin,
                        FechaCreacion = DateTime.Now,
                        Estatus = true
                    };

                    var areaResponse = _dbWrapper.GuardarNuevaAreaParaEmpresa(area, empresaGuardada.Id);

                    if (!areaResponse.IsSuccess || areaResponse.Response == null)
                    {
                        Log.Error("❌ PASO 3/8 - FALLÓ la creación del área. EmpresaId: {EmpresaId}, Error: {Error}",
                            empresaGuardada.Id, areaResponse.Message);
                        throw new Exception(areaResponse.Message ?? "Error al guardar el área");
                    }

                    areaGuardada = areaResponse.Response;
                    Log.Information("✅ PASO 3/8 - Área 'TI' creada exitosamente. Id: {AreaId}, Nombre: {AreaNombre}",
                        areaGuardada.Id, areaGuardada.Nombre);

                    // =========================================
                    // PASO 4: GUARDAR USUARIO ADMINISTRADOR
                    // =========================================
                    Log.Information("PASO 4/8 - Creando usuario administrador para la empresa...");

                    contrasenaTemporal = Cryptography.GeneratePassword();
                    Log.Information("Contraseña temporal generada para el administrador (se envía por correo).");

                    usuarioAdmin = new Usuario()
                    {
                        NombreUsuario = usernameAdmin,
                        Contrasena = Cryptography.HashPassword(contrasenaTemporal),
                        ImagenPerfil = null,
                        Correo = empresaGuardada.CorreoContacto,
                        Nombre = "Administrador",
                        Apellido = "Sistema",
                        Celular = empresaGuardada.Telefono,
                        SucursalId = sucursalGuardada.Id,
                        Firma = null,
                        RFC = empresaGuardada.RFC,
                        AreaId = areaGuardada.Id,
                        EmpresaId = empresaGuardada.Id,
                        CreadoPor = usernameAdmin,
                        FechaCreacion = DateTime.Now,
                        Estatus = true
                    };

                    var usuarioResponse = _dbWrapper.GuardarOActualizarUsuario(usuarioAdmin);

                    if (!usuarioResponse.IsSuccess)
                    {
                        Log.Error("❌ PASO 4/8 - FALLÓ la creación del usuario administrador. EmpresaId: {EmpresaId}, Username: {Username}, Error: {Error}",
                            empresaGuardada.Id, usernameAdmin, usuarioResponse.Message);
                        throw new Exception(usuarioResponse.Message ?? "Error al guardar el usuario administrador");
                    }

                    usuarioAdmin = usuarioResponse.Response;
                    usuarioAdminId = usuarioAdmin.Id;
                    Log.Information("✅ PASO 4/8 - Usuario administrador creado exitosamente. Id: {UsuarioId}, Username: {Username}, Correo: {Correo}",
                        usuarioAdminId, usernameAdmin, empresaGuardada.CorreoContacto);

                    // =========================================
                    // PASO 5: CREAR ROLES BASE
                    // =========================================
                    Log.Information("PASO 5/8 - Creando roles base para la empresa...");

                    var plantillaResponse = _dbWrapper.ObtenerPlantillaRoles();
                    if (!plantillaResponse.IsSuccess || plantillaResponse.Response == null)
                    {
                        throw new Exception("No se pudo obtener la plantilla de roles para el registro de la empresa.");
                    }

                    var rolesBase = plantillaResponse.Response
                        .Select(t => new Rol
                        {
                            Nombre = t.Nombre,
                            Descripcion = t.Descripcion,
                            PuedeAtenderTickets = t.PuedeAtenderTickets,
                            CreadoPor = usernameAdmin,
                            FechaCreacion = DateTime.Now,
                            Estatus = true
                        })
                        .ToList();

                    foreach (var rol in rolesBase)
                    {
                        var rolResponse = _dbWrapper.GuardarRolParaNuevaEmpresa(rol, empresaGuardada.Id);
                        if (!rolResponse.IsSuccess)
                        {
                            Log.Error("❌ PASO 5/8 - FALLÓ la creación del rol {NombreRol}. Error: {Error}", rol.Nombre, rolResponse.Message);
                            throw new Exception($"Error al crear el rol '{rol.Nombre}': {rolResponse.Message}");
                        }

                        if (rol.Nombre == "Administrador")
                        {
                            rolAdminId = ((Rol)rolResponse.Response).Id;
                        }

                        if (rol.Nombre == "Usuario")
                        {
                            rolUsuarioId = ((Rol)rolResponse.Response).Id;
                        }
                    }

                    Log.Information("✅ PASO 5/8 - Roles base creados exitosamente para empresa {EmpresaId}", empresaGuardada.Id);

                    // =========================================
                    // PASO 5.1: ASIGNAR PÁGINA "MIS ACTIVOS" AL ROL "USUARIO"
                    // (provisioning de la página Mis Activos para el rol "Usuario" recién creado)
                    // =========================================
                    Log.Information("PASO 5.1/8 - Asignando página 'Mis Activos' al rol 'Usuario'...");

                    if (rolUsuarioId > 0)
                    {
                        var paginasResponseMisActivos = _dbWrapper.ObtenerPaginas();
                        if (paginasResponseMisActivos.IsSuccess && paginasResponseMisActivos.Response != null)
                        {
                            var paginaMisActivos = ((List<Pagina>)paginasResponseMisActivos.Response)
                                .FirstOrDefault(p => p.Nombre == "MisActivos");

                            if (paginaMisActivos != null)
                            {
                                var insertMisActivosResponse = _dbWrapper.InsertarRolPaginaAccion(
                                    rolUsuarioId,
                                    paginaMisActivos.Id,
                                    true,  // PuedeLeer
                                    false, // PuedeCrear
                                    false, // PuedeEditar
                                    false, // PuedeEliminar
                                    false, // PuedeExportar
                                    usernameAdmin,
                                    usernameAdmin
                                );

                                if (insertMisActivosResponse.IsSuccess)
                                {
                                    Log.Information("✅ PASO 5.1/8 - Página 'Mis Activos' asignada al rol 'Usuario' ({RolId})", rolUsuarioId);
                                }
                                else
                                {
                                    Log.Warning("⚠️ No se pudo asignar la página 'Mis Activos' al rol 'Usuario': {Error}", insertMisActivosResponse.Message);
                                }
                            }
                            else
                            {
                                Log.Warning("⚠️ No se encontró la página 'Mis Activos' (aplicar migration.sql del change vinculacion-persona-usuario).");
                            }
                        }
                    }
                    else
                    {
                        Log.Warning("⚠️ No se encontró el rol 'Usuario' creado; no se asignó la página 'Mis Activos'.");
                    }

                    // =========================================
                    // PASO 6: ASIGNAR ROL "ADMINISTRADOR" AL USUARIO
                    // =========================================
                    Log.Information("PASO 6/8 - Asignando rol 'Administrador' al usuario...");

                    var asignarRolResponse = _dbWrapper.AsignarRolUsuarioParaNuevaEmpresa(usuarioAdminId, rolAdminId, usernameAdmin);

                    if (!asignarRolResponse.IsSuccess)
                    {
                        Log.Error("❌ PASO 6/8 - FALLÓ la asignación del rol 'Administrador' al usuario {Username}", usernameAdmin);
                        throw new Exception($"Error al asignar el rol 'Administrador' al usuario: {asignarRolResponse.Message}");
                    }

                    Log.Information("✅ PASO 6/8 - Rol 'Administrador' asignado al usuario {Username}", usernameAdmin);

                    // =========================================
                    // PASO 7: ASIGNAR PÁGINAS AL ROL ADMINISTRADOR EN RolPaginaAccion
                    // =========================================
                    Log.Information("PASO 7/8 - Asignando todas las páginas al rol Administrador...");

                    var paginasResponse = _dbWrapper.ObtenerPaginas();
                    if (paginasResponse.IsSuccess && paginasResponse.Response != null)
                    {
                        var paginas = (List<Pagina>)paginasResponse.Response;
                        int paginasAsignadas = 0;

                        foreach (var pagina in paginas)
                        {
                            // Insertar en RolPaginaAccion para el rol Administrador
                            // Todos los permisos en true para el administrador
                            var insertRolPaginaResponse = _dbWrapper.InsertarRolPaginaAccion(
                                rolAdminId,
                                pagina.Id,
                                true,  // PuedeLeer
                                true,  // PuedeCrear
                                true,  // PuedeEditar
                                true,  // PuedeEliminar
                                true,  // PuedeExportar
                                usernameAdmin,
                                usernameAdmin
                            );

                            if (insertRolPaginaResponse.IsSuccess)
                            {
                                paginasAsignadas++;
                            }
                            else
                            {
                                Log.Warning("⚠️ No se pudo asignar la página {PaginaId} al rol {RolId}: {Error}",
                                    pagina.Id, rolAdminId, insertRolPaginaResponse.Message);
                            }
                        }

                        Log.Information("✅ PASO 7/8 - {Count} páginas asignadas al rol Administrador", paginasAsignadas);
                    }
                    else
                    {
                        Log.Warning("⚠️ No se encontraron páginas para asignar al rol Administrador");
                    }

                    // =========================================
                    // PASO 8: ASIGNAR PÁGINAS AL USUARIO ADMINISTRADOR (UsuarioPagina)
                    // =========================================
                    Log.Information("PASO 8/8 - Asignando páginas al usuario administrador...");

                    var paginasResponseUser = _dbWrapper.ObtenerPaginas();
                    if (paginasResponseUser.IsSuccess && paginasResponseUser.Response != null)
                    {
                        var paginas = (List<Pagina>)paginasResponseUser.Response;
                        int paginasAsignadasUser = 0;

                        foreach (var pagina in paginas)
                        {
                            var insertResponse = _dbWrapper.InsertarUsuarioPaginaParaNuevaEmpresa(usuarioAdminId, pagina.Id, usernameAdmin);
                            if (insertResponse.IsSuccess)
                            {
                                paginasAsignadasUser++;
                            }
                            else
                            {
                                Log.Warning("⚠️ No se pudo asignar la página {PaginaId} al usuario {UsuarioId}: {Error}",
                                    pagina.Id, usuarioAdminId, insertResponse.Message);
                            }
                        }

                        Log.Information("✅ PASO 8/8 - {Count} páginas asignadas al usuario administrador", paginasAsignadasUser);
                    }
                    else
                    {
                        Log.Warning("⚠️ No se encontraron páginas para asignar al usuario administrador");
                    }

                    // =========================================
                    // COMPLETAR TRANSACCIÓN
                    // =========================================
                    _dbWrapper.CommitTransaction();
                    Log.Information("✅ Transacción completada exitosamente.");
                }
                catch
                {
                    _dbWrapper.RollbackTransaction();
                    throw;
                }

                // =========================================
                // FIN DE LA TRANSACCIÓN - ENVIAR CORREO
                // =========================================
                Log.Information("Enviando correo de bienvenida a: {Correo}...", empresaGuardada.CorreoContacto);

                var emailSent = EnviarCorreoBienvenida(empresaGuardada, usuarioAdmin.NombreUsuario, contrasenaTemporal);

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

        /// <summary>
        /// Genera el nombre de usuario del administrador a partir del "Responsable" registrado:
        /// normaliza (minúsculas, sin acentos, sin caracteres especiales) y arma
        /// primer nombre + apellido (paterno), omitiendo segundos nombres y apellido materno.
        /// Garantiza unicidad global agregando un sufijo numérico si ya existe.
        /// </summary>
        private string GenerarUsernameAdminUnico(string responsable)
        {
            string baseUsuario = NormalizarNombreResponsable(responsable);
            if (string.IsNullOrWhiteSpace(baseUsuario))
                return $"admin{DateTime.Now:yyyyMMddHHmmss}";

            string candidato = baseUsuario;
            int sufijo = 1;
            while (_dbWrapper.ExisteNombreUsuario(candidato))
            {
                sufijo++;
                candidato = AjustarLargoConSufijo(baseUsuario, sufijo);
            }
            return candidato;
        }

        private string NormalizarNombreResponsable(string responsable)
        {
            if (string.IsNullOrWhiteSpace(responsable)) return null;

            // 1) Quitar acentos/marcas diacríticas (á->a, é->e, ñ->n ...)
            var formaD = responsable.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in formaD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(char.IsLetterOrDigit(c) ? c : ' ');
            }
            var sinAcentos = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();

            // 2) Separar en tokens (por espacios)
            var tokens = sinAcentos.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) return null;

            // 3) Regla: nombre + apellido (paterno). Omite segundos nombres y apellido materno.
            //    "Ivan Francisco Bartolo Castro" -> ivanbartolo | "Juan Pérez López" -> juanperez
            string baseNombre;
            if (tokens.Length == 1)
                baseNombre = tokens[0];
            else
            {
                int idxApellido = tokens.Length == 2 ? 1 : tokens.Length - 2;
                baseNombre = tokens[0] + tokens[idxApellido];
            }

            // 4) Dejar margen (<=20) para que el sufijo numérico quepa en nvarchar(25)
            return baseNombre.Length > 20 ? baseNombre.Substring(0, 20) : baseNombre;
        }

        private string AjustarLargoConSufijo(string baseUsuario, int sufijo)
        {
            string sufijoStr = sufijo.ToString();
            int maxBase = 25 - sufijoStr.Length;
            string b = baseUsuario.Length > maxBase ? baseUsuario.Substring(0, maxBase) : baseUsuario;
            return b + sufijoStr;
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
