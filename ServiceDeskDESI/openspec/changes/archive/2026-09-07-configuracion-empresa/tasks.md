# Tasks: Módulo "Configuración de Empresa" (horario laboral + logotipo)

Orden: **BD → Entidades → WebApi → MVC DAL/Services → MVC Controller/Vista → Verificación**. Lista plana; los grupos indican orden de dependencia.

> ⚠️ **Constraints para sdd-apply**: NO ejecutar `migration.sql`/`rollback.sql` contra ninguna BD (la migración la aplica el usuario manualmente, post-apply); NO correr la app; textos UI en español con acentos; todo `.cshtml` nuevo en **UTF-8 CON BOM**; no hay test project (`strict_tdd:false`); todo `.cs` nuevo DEBE registrarse en su `.csproj` old-style (`<Compile Include>`) o no compila (precedente `ThemeHelper`).

## G1 — BD / migración

- [x] **T1** — `openspec/changes/configuracion-empresa/migration.sql`: `CREATE TABLE dbo.EmpresaHorarioLaboral` (Id BIGINT IDENTITY PK, EmpresaId BIGINT NOT NULL, DiaSemana TINYINT, HoraInicio DATETIME NULL, HoraFin DATETIME NULL, Estatus BIT DEFAULT 1, auditoría `BaseObject`, `UQ_EmpresaHorarioLaboral_EmpresaDia` UNIQUE(EmpresaId,DiaSemana), FK→Empresa). **(CE-004)**
- [x] **T2** — `migration.sql`: `ALTER TABLE dbo.Empresa ADD LogoUrl NVARCHAR(500) NULL`. **(CE-006)**
- [x] **T3** — `migration.sql`: `CREATE PROCEDURE dbo.ObtenerHorarioLaboral @Usuario NVARCHAR(25)` (tenant desde `(SELECT EmpresaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1)`, `ORDER BY DiaSemana`). **(CE-004, CE-008)**
- [x] **T4** — `migration.sql`: `CREATE PROCEDURE dbo.GuardarHorarioLaboralDia @Usuario,@DiaSemana,@HoraInicio,@HoraFin,@EsLaboral` (upsert idempotente; `EsLaboral=0`→horas NULL; tenant desde `@Usuario`; devuelve 1/0). **(CE-004, CE-008)**
- [x] **T5** — `migration.sql`: `CREATE PROCEDURE dbo.GuardarLogoEmpresa @Usuario,@LogoUrl` (UPDATE Empresa SET LogoUrl, tenant desde `@Usuario`, devuelve 1/0). **(CE-006, CE-008)**
- [x] **T6** — `migration.sql`: backfill idempotente (`INSERT…SELECT…CROSS JOIN (VALUES 1..7)` con `WHERE NOT EXISTS`) — Lun–Vie 09:00–17:00 `Estatus=1`, Sáb/Dom NULL `Estatus=0`. **(CE-005)**
- [x] **T7** — `migration.sql`: seed `Pagina` (`Nombre='ConfiguracionEmpresa'`, `NombreVisible='Configuración de Empresa'`, `Tipo='Menu'`, `Direccion='/ConfiguracionEmpresa'`, `Logo='fa-cog'`, `OrdenB=9`, `PermisosPadreId=NULL`) + `RolPaginaAccion` SOLO rol `Administrador` (`PuedeLeer=1,PuedeEditar=1`, resto 0). ⚠️ verificar columnas reales de `Pagina`/`RolPaginaAccion` en BD hosted (drift conocido). **(CE-001, CE-002)**
- [x] **T8** — `openspec/changes/configuracion-empresa/rollback.sql` (orden inverso): DROP SPs → DELETE página/permiso → DROP tabla → DROP columna `LogoUrl` (cada uno con guard `IF EXISTS`).

## G2 — Entidades (+ csproj)

- [x] **T9** — Crear `ServiceDeskDESIEntities/Catalogos/HorarioLaboral.cs`: POCO `HorarioLaboral : BaseObject` (`EmpresaId long`, `DiaSemana int`, `HoraInicio DateTime?`, `HoraFin DateTime?`). **(CE-004)**
- [x] **T10** — `ServiceDeskDESIEntities/Catalogos/Empresa.cs`: añadir `public string LogoUrl { get; set; }`. **(CE-006)**
- [x] **T11** — `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj`: registrar `<Compile Include="Catalogos\HorarioLaboral.cs" />`.

## G3 — WebApi (DAL + Services + Controllers)

- [x] **T12** — Crear `ServiceDeskDESIWebApi/DAL/DbWrapper.HorarioLaboral.cs` (partial): `ObtenerHorarioLaboral(string usuario)` (GetObjects + `LlenarEntidad<HorarioLaboral>`); `GuardarHorarioLaboral(string usuario, List<HorarioLaboral>)` (BeginTransaction → foreach `GuardarHorarioLaboralDia` → Commit; catch Rollback); `GuardarHorarioLaboralDia(string usuario, int diaSemana, DateTime? horaInicio, DateTime? horaFin, bool esLaboral)` **public** (ExecuteScalar, `(object)hora ?? DBNull.Value`). **(CE-004, CE-008)**
- [x] **T13** — `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs`: añadir `GuardarLogoEmpresa(string usuario, string logoUrl)` (ExecuteScalar `"GuardarLogoEmpresa"`, valida retorno 1). **(CE-006)**
- [x] **T14** — Crear `ServiceDeskDESIWebApi/Services/HorarioLaboralService.cs`: `ObtenerHorarioLaboral(string usuario)` y `GuardarHorarioLaboral(string usuario, List<HorarioLaboral>)` con validación (`Count==7`, `DiaSemana∈[1..7]`, laborable→`HoraInicio`/`HoraFin` no nulos y `HoraFin>HoraInicio`; no laborable→forzar null) + Serilog. **(CE-004)**
- [x] **T15** — `ServiceDeskDESIWebApi/Services/EmpresaService.cs`: añadir `GuardarLogoEmpresa(string usuario, string logoUrl)` (validar no vacío + Serilog); insertar **hook PASO 5.2** en `GuardarNuevaEmpresaConDatosIniciales` (entre PASO 5.1 ~línea 534 y PASO 6 ~537, dentro de la transacción ya abierta): loop `dia 1..7`, laborable `dia∈[1..5]`, `new DateTime(1900,1,1,9,0,0)`/`(17,0,0)` o null, `_dbWrapper.GuardarHorarioLaboralDia(usernameAdmin, dia, ini, fin, laborable)`, throw si `!IsSuccess`. **(CE-005, CE-006)**
- [x] **T16** — Crear `ServiceDeskDESIWebApi/Controllers/HorarioLaboralController.cs` (`[Authorize]`, `[RoutePrefix("api/HorarioLaboral")]`): `Obtener()` `[HttpGet, Route("Obtener")] [Permiso("ConfiguracionEmpresa","Leer")]`; `Guardar(List<HorarioLaboral>)` `[HttpPost, Route("Guardar")] [Permiso("ConfiguracionEmpresa","Editar")]` → `User.Identity.Name`. **(CE-002, CE-008)**
- [x] **T17** — `ServiceDeskDESIWebApi/Controllers/EmpresaController.cs`: añadir `GuardarLogo([FromBody] GuardarLogoRequest request)` `[HttpPost, Route("GuardarLogo")] [Permiso("ConfiguracionEmpresa","Editar")]` + `GuardarLogoRequest { public string LogoUrl { get; set; } }` inline (espeja `AsignarRolRequest`). **(CE-002, CE-006)**
- [x] **T18** — `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj`: registrar `<Compile Include>` de `DAL\DbWrapper.HorarioLaboral.cs`, `Services\HorarioLaboralService.cs`, `Controllers\HorarioLaboralController.cs`.

## G4 — MVC DAL/Services

- [x] **T19** — Crear `ServiceDeskDESIMVC/DAL/HttpClientConnection.HorarioLaboral.cs` (partial): `ObtenerHorarioLaboral()` (GET `api/HorarioLaboral/Obtener` → `ModelResponse<List<HorarioLaboral>>`); `GuardarHorarioLaboral(List<HorarioLaboral>)` (POST `api/HorarioLaboral/Guardar` → `ModelResponse`). **(CE-004)**
- [x] **T20** — `ServiceDeskDESIMVC/DAL/HttpClientConnection.Empresa.cs`: añadir `GuardarLogoEmpresa(string logoUrl)` (POST `api/Empresas/GuardarLogo`, body `new { LogoUrl = logoUrl }` → `ModelResponse`). **(CE-006)**
- [x] **T21** — Crear `ServiceDeskDESIMVC/Services/HorarioLaboralService.cs` (wrappers MVC→HttpClientConnection: `ObtenerHorarioLaboral()`, `GuardarHorarioLaboral(List<HorarioLaboral>)`); `ServiceDeskDESIMVC/Services/EmpresaService.cs`: añadir `GuardarLogoEmpresa(string logoUrl)`. **(CE-004, CE-006)**
- [x] **T22** — Crear `ServiceDeskDESIMVC/Models/HorarioViewModel.cs`: `{ int DiaSemana; bool EsLaboral; string HoraInicio; string HoraFin }` (contrato UI "HH:mm"). **(CE-004, CE-009)**

## G5 — MVC Controller/Vista/Logo/Sidebar/Footer

- [x] **T23** — Crear `ServiceDeskDESIMVC/Controllers/ConfiguracionEmpresaController.cs` (hereda `BaseController`, cablea `_empresaService`/`_horarioService`): `Index()` `[Permiso("ConfiguracionEmpresa","Leer")]` (TokenCookie null→redirect `Home/Autentication`; `_empresaService.ObtenerEmpresaPorId(EmpresaID)`; `ViewBag.PuedeEditar` desde `ObtenerPermisosPorUsuario`); `ObtenerHorario()` `[Permiso(...,"Leer")]` (mapea a `HorarioViewModel`, `HoraInicio?.ToString("HH:mm")`); `GuardarHorario(List<HorarioViewModel>)` `[Permiso(...,"Editar")]` (normaliza ancla `1900-01-01`, `EsLaboral=false`→null, valida `HoraFin>HoraInicio`); `SubirLogo(HttpPostedFileBase file)` `[Permiso(...,"Editar")]` (pipeline: vacío→error; extensión∈`LogoEmpresaTiposPermitidos`; MIME; peso `LogoEmpresaMaxTamanoKB`; borrar previo; `Uploads/Logos/{empresaId}/{guid}.{ext}`; `_empresaService.GuardarLogoEmpresa(logoUrl)`; JSON → JS recarga `$("#sidebar").load("/Home/MenusUser")`). **(CE-002, CE-003, CE-004, CE-006, CE-008)**
- [x] **T24** — `ServiceDeskDESIMVC/Controllers/HomeController.cs` `MenusUser()`: tras obtener `paginas`, leer `TokenCookie.EmpresaID` → `_empresaService.ObtenerEmpresaPorId(...)` → `ViewBag.LogoUrl`/`ViewBag.NombreComercial` (null si no hay). No tocar login. **(CE-006)**
- [x] **T25** — Crear `ServiceDeskDESIMVC/Views/ConfiguracionEmpresa/Index.cshtml` (**UTF-8 CON BOM**): card datos generales solo lectura (`NombreComercial…EsPeriodoPrueba`), editor 7 filas (checkbox "labora" + `datetime-local`, botón único "Guardar" con spinner/"Guardando…" + Swal), subida logo (`accept=".svg,.png"`, preview `FileReader.readAsDataURL`); ocultar edición si `!ViewBag.PuedeEditar`. **(CE-003, CE-004, CE-006, CE-009)**
- [x] **T26** — `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml`: en `<div class="sidebar-header">`, si `!string.IsNullOrEmpty(ViewBag.LogoUrl)` → `<img src="@ViewBag.LogoUrl" alt="@ViewBag.NombreComercial">`; si no, fallback `fa-headset` + texto actual. **(CE-006)**
- [x] **T27** — `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml`: footer estático inmediatamente tras `@RenderBody()` (línea 223), antes del cierre de `.main-content`: "© `@DateTime.Now.Year` Service Desk by **DESi**" + enlaces Ayuda/Términos/Privacidad (placeholder `#`). **(CE-007)**
- [x] **T28** — `ServiceDeskDESIMVC/Web.config`: añadir appSettings `LogoEmpresaMaxTamanoKB=2048` y `LogoEmpresaTiposPermitidos=svg,png`. **(CE-006)**
- [x] **T29** — `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj`: registrar `<Compile Include>` de `DAL\HttpClientConnection.HorarioLaboral.cs`, `Services\HorarioLaboralService.cs`, `Models\HorarioViewModel.cs`, `Controllers\ConfiguracionEmpresaController.cs`. ⚠️ sin esto no compila. **(CE-001..009)**

## G6 — Verificación

- [x] **T30** — Compilar `ServiceDeskDESI.sln` con MSBuild VS2022 Debug → **0 errores** en los 3 proyectos (confirma los `<Compile Include>` manuales de G2/G3/G5): `"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ServiceDeskDESI.sln /t:Build /p:Configuration=Debug`. **(success criteria)**
- [x] **T31** — Revisión estática contra CE-001..CE-009 (28 escenarios): página/permiso sembrados, `[Permiso]` MVC+WebApi, card solo lectura, 7 filas transaccionales, `HoraFin>HoraInicio`, defaults/backfill/hook PASO 5.2, logo validado + fallback, footer estático, multi-tenant por usuario (nunca `EmpresaId` del cliente), `Index.cshtml` UTF-8 CON BOM. **(todas)**

## G7 — Enmienda (post revisión visual G5)

> Lote de enmienda tras la revisión visual del usuario sobre G5. Dos cambios de UX + ajustes documentales. **Sin objetos nuevos de BD** (la migración ya fue aplicada por el usuario; el pipeline existente ya soporta NULL).

- [x] **T32** — **Quitar logotipo** (revierte "no limpiar logo"): WebApi `Services/EmpresaService.cs` relaja la validación de `GuardarLogoEmpresa` (permite `logoUrl` NULL/vacío; el SP `GuardarLogoEmpresa` persiste NULL). MVC `ConfiguracionEmpresaController.QuitarLogo()` `[Permiso("ConfiguracionEmpresa","Editar")]` (resuelve `LogoUrl`, borra archivo físico sin fallar si falta, persiste NULL reutilizando `GuardarLogoEmpresa`). Vista `Index.cshtml`: botón "Quitar logo" (visible solo con logo + confirmación Swal) que recarga el sidebar para el fallback DESi. Doc: CE-006 + Supuesto #1. **(CE-006)**
- [x] **T33** — **Inputs AM/PM 12h**: `Index.cshtml` reemplaza `datetime-local` por 3 `<select>` (Hora 1–12, Minuto 00–55 paso 5, AM/PM) vía `@helper HoraSelect`; JS convierte "HH:mm"↔12h y mantiene el contrato "HH:mm" con el backend (sin cambios backend/WebApi). Selects se deshabilitan si el día no es laborable. Doc: CE-004 + Supuesto #5. **(CE-004)**
- [x] **T34** — **Rebuild + re-check estático** CE-004/CE-006: MSBuild Rebuild solución (0 errores), revisión estática de la enmienda y `Index.cshtml` UTF-8 CON BOM. **(CE-004, CE-006)**
