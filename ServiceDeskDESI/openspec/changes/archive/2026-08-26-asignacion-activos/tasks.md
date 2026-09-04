# Tasks: Asignación de Activos — notificación, confirmación de recepción y bitácora

Orden: BD → Entities → WebApi DAL → WebApi Services → WebApi Controllers → Template → MVC → Build → BD hosted → Verificación/smoke.

> No re-DROP/CREATE los 5 SPs existentes (`AsignarActivoPersona`, `DesvincularActivoPersona`, `ObtenerActivosPorPersona`, `ObtenerActivosDisponibles`, `ObtenerActivos`). No renombrar `EnvioEmaiil`. (D7)

## Lote 1: BD / migración

- [x] T1 — Extender `openspec/changes/asignacion-activos/migration.sql` (aditivo, NO tocar los 5 SPs): `ALTER TABLE dbo.PersonaActivo ADD FechaConfirmacion DATETIME NULL` y `ADD TokenConfirmacion UNIQUEIDENTIFIER NULL`, cada uno guardado con `IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.PersonaActivo') AND name=N'...')`. (D1.1)
- [x] T2 — `migration.sql`: índice NO único `IX_PersonaActivo_TokenConfirmacion` sobre `TokenConfirmacion`, guardado con `IF NOT EXISTS (sys.indexes)`. (D1.1)
- [x] T3 — `migration.sql`: `CREATE TABLE dbo.BitacoraCorreo` (Id BIGINT IDENTITY PK, TipoCorreo NVARCHAR(50) NOT NULL, Destinatario NVARCHAR(250) NOT NULL, Asunto NVARCHAR(250) NOT NULL, Estado NVARCHAR(20) NOT NULL, Error NVARCHAR(MAX) NULL, FechaEnvio DATETIME NOT NULL, ReferenciaId BIGINT NULL) con `IF OBJECT_ID(N'dbo.BitacoraCorreo',N'U') IS NULL`; **SIN FK** sobre ReferenciaId (soft ref a PersonaActivoId). (D1.2)
- [x] T4 — `migration.sql`: `CREATE PROCEDURE dbo.GenerarTokenConfirmacion` (@PersonaActivoId BIGINT, @TokenConfirmacion UNIQUEIDENTIFIER) → `UPDATE PersonaActivo SET TokenConfirmacion=@TokenConfirmacion WHERE Id=@PersonaActivoId AND FechaFin IS NULL; SELECT @@ROWCOUNT;` con guard DROP/CREATE `IF OBJECT_ID(...N'P') IS NOT NULL DROP`. (D1.3a)
- [x] T5 — `migration.sql`: `CREATE PROCEDURE dbo.ConfirmarRecepcionActivo` (@TokenConfirmacion UNIQUEIDENTIFIER) tri-estado: token desconocido→`SELECT 0`; ya confirmado (`FechaConfirmacion IS NOT NULL`)→`SELECT 2`; si no, `UPDATE ... SET FechaConfirmacion=GETDATE() WHERE TokenConfirmacion=@TokenConfirmacion AND FechaConfirmacion IS NULL; SELECT 1;` idempotente, SIN @Usuario. (D1.3b, CRA-001/003)
- [x] T6 — `migration.sql`: `CREATE PROCEDURE dbo.RegistrarBitacoraCorreo` (@TipoCorreo, @Destinatario, @Asunto, @Estado, @Error NVARCHAR(MAX)=NULL, @ReferenciaId BIGINT=NULL) → `INSERT ... VALUES(..., GETDATE(), @ReferenciaId); SELECT SCOPE_IDENTITY();`. (D1.3c, NAA-003)
- [x] T7 — Crear `openspec/changes/asignacion-activos/rollback.sql` en orden inverso: `DROP PROCEDURE` de los 3 SPs nuevos → `DROP INDEX IX_PersonaActivo_TokenConfirmacion` → `DROP TABLE BitacoraCorreo` → `DROP COLUMN TokenConfirmacion` → `DROP COLUMN FechaConfirmacion` (cada DROP COLUMN con guard `IF EXISTS` en `sys.columns`). (D1.4)

## Lote 2: Entities

- [x] T8 — Modificar `ServiceDeskDESIEntities/Catalogos/PersonaActivo.cs`: añadir `public DateTime? FechaConfirmacion { get; set; }` y `public Guid? TokenConfirmacion { get; set; }` (el DTO hereda de PersonaActivo; NO tocar `PersonaActivoDTO.cs`). (D2.1)
- [x] T9 — Crear `ServiceDeskDESIEntities/Catalogos/BitacoraCorreo.cs` (clase `BitacoraCorreo`, NO hereda `BaseObject`; Id long, TipoCorreo/Destinatario/Asunto/Estado/Error string, FechaEnvio DateTime, ReferenciaId long?). Sin DTO. (D2.2)
- [x] T10 — Registrar `<Compile Include="Catalogos\BitacoraCorreo.cs" />` en `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` (junto a las demás entidades de Catalogos). (D2.3)

## Lote 3: WebApi DAL

- [x] T11 — Modificar `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs`: añadir `ModelResponse GenerarTokenConfirmacion(long personaActivoId, Guid token)` → `ExecuteScalar("GenerarTokenConfirmacion", ...)`; `IsSuccess = (Convert.ToInt64(resultado) > 0)`. (D3.4)
- [x] T12 — Mismo archivo: añadir `ModelResponse ConfirmarRecepcionActivo(Guid token)` → `ExecuteScalar("ConfirmarRecepcionActivo", ...)`; `Response = (long)estado` (0/1/2) y `IsSuccess = (estado != 0)`. (D3.4)
- [x] T13 — Mismo archivo: añadir `ModelResponse RegistrarBitacoraCorreo(string tipo, string destinatario, string asunto, string estado, string error, long? referenciaId)` → `ExecuteScalar("RegistrarBitacoraCorreo", ...)`; `IsSuccess = (Convert.ToInt64(resultado) > 0)`. (D3.4)

## Lote 4: WebApi Services

- [x] T14 — Modificar `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` `AsignarActivoPersona`: tras éxito del SP (`newId=(long)result.Response`), leer `persona=_dbWrapper.ObtenerPersonaPorId(...)`, `activo=_dbWrapper.ObtenerActivoPorId(...)`, `asignador=_dbWrapper.ObtenerUsuarioPorNombreUsuario(...)`; si alguna lectura falla → compensar (desvincular) + bitácora Fallido + `IsSuccess=false`. (D3.1 pasos 1-2, D3.3)
- [x] T15 — Mismo método: `Guid token=Guid.NewGuid();` → `_dbWrapper.GenerarTokenConfirmacion(newId, token)`; si `@@ROWCOUNT==0` → compensar + error. Construir `urlConfirmacion=$"{baseUri}Home/ConfirmarRecepcion/{token}"` con `baseUri=ConfigurationManager.AppSettings["BaseUri"]`. (D3.1 pasos 3-4, NAA-002)
- [x] T16 — Mismo método: resolver los 11 placeholders null-safe (`?? string.Empty`): `{{NombreUsuario}}`, `{{AsignadoPor}}`, `{{NombreActivo}}`, `{{Serial}}`, `{{TipoActivo}}`, `{{Marca}}`, `{{Modelo}}`, `{{FechaAsignacion}}` (`DateTime.Now.ToString("dd/MM/yyyy HH:mm")`), `{{PuestoUsuario}}`, `{{CorreoUsuario}}`, `{{UrlConfirmacion}}`. Leer `HostingEnvironment.MapPath("~/Template/Template_AsignacionActivo.html")` → `File.ReadAllText` → `Replace(...)` encadenado. (D3.1 pasos 5-6, NAA-001)
- [x] T17 — Mismo método: enviar en try/catch `EmailHelper.EnvioEmaiil(new List<string>{persona.Correo}, "Asignación de activo - Service Desk DESI", templateHtml, false)`; éxito → `RegistrarBitacoraCorreo(..., "Enviado", null, newId)`. Fallo (o `persona.Correo` null/blank): `DesvincularActivoPersona(newId, usuario)` (try/catch + Log.Error) + `RegistrarBitacoraCorreo(..., "Fallido", ex.Message, newId)` + `return IsSuccess=false` con mensaje exacto *"No se pudo enviar el correo de confirmación de asignación. La asignación fue revertida. Verifique la configuración de correo (SMTP) e intente nuevamente."* Añadir usings (`System.Configuration`, `System.Web.Hosting`, `System.IO`, `Helpers`). (D3.1 pasos 7-8, D3.3, D3.5, NAA-003/004/005)
- [x] T18 — Mismo archivo: añadir `ConfirmarRecepcion(Guid token)` → valida `token==Guid.Empty`; llama `_dbWrapper.ConfirmarRecepcionActivo(token)`; mapea tri-estado: `0`→`IsSuccess=false` "El enlace de confirmación no es válido o ha sido alterado.", `1`→`IsSuccess=true` "Recepción confirmada correctamente.", `2`→`IsSuccess=true` "La recepción de este activo ya fue confirmada anteriormente." (D4.1, CRA-004)

## Lote 5: WebApi Controllers

- [x] T19 — Modificar `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs`: añadir `[AllowAnonymous] [HttpGet, Route("confirmarRecepcion/{token:guid}")] public ModelResponse ConfirmarRecepcion(Guid token)` → `_personaActivoService.ConfirmarRecepcion(token)`. Sin `[Permiso(...)]`. (D4.1, CRA-001/005)

## Lote 6: Template

- [x] T20 — Renombrar `ServiceDeskDESIWebApi/Template/Templat_AsignacionActivo.html` → `Template_AsignacionActivo.html` y actualizar `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` línea 206 a `<Content Include="Template\Template_AsignacionActivo.html" />`. NO cambiar contenido; conservar botón "Confirmar Recepción" apuntando a `{{UrlConfirmacion}}`. (D5)

## Lote 7: MVC

- [x] T21 — Modificar `ServiceDeskDESIMVC/Controllers/HomeController.cs`: añadir acción pública `public async Task<ActionResult> ConfirmarRecepcion(string token)` → `_personaActivoService.ConfirmarRecepcion(token)`, setear `ViewBag.Resultado` y `ViewBag.Token`, `return View()`. (D4.2)
- [x] T22 — Modificar `ServiceDeskDESIMVC/App_Start/FilterConfig.cs`: añadir `"Home.ConfirmarRecepcion"` a la allowlist `PublicActions`. (D4.2)
- [x] T23 — Modificar `ServiceDeskDESIMVC/DAL/HttpClientConnection.PersonaActivo.cs`: añadir método anónimo `public async Task<ModelResponse> ConfirmarRecepcion(string token)` → `GET api/PersonaActivo/confirmarRecepcion/{token}` SIN bearer (replicar `ValidarTokenRecuperacion`), deserializar `ModelResponse`. (D4.2)
- [x] T24 — Modificar `ServiceDeskDESIMVC/Services/PersonaActivoService.cs`: añadir wrapper `ConfirmarRecepcion(string token)` → `_httpClientConnection.ConfirmarRecepcion(token)`. (D4.2)
- [x] T25 — Crear `ServiceDeskDESIMVC/Views/Home/ConfirmarRecepcion.cshtml`: layout standalone (sin barra autenticada, como `RecoverPassword.cshtml`); renderizar según `ViewBag.Resultado`: tarjeta verde "Recepción confirmada", informativa "ya confirmada" (idempotencia) o error "Enlace inválido". (D4.2, CRA-001/004)

## Lote 8: Build

- [x] T26 — Compilar `ServiceDeskDESI.sln` con `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe` (Debug) → 0 errores en los 3 proyectos. (proposal success criteria)

## Lote 9: BD hosted (⚠️ manual — requiere confirmación del usuario)

- [x] T27 — ⚠️ **NO ejecutar sin confirmación explícita del usuario.** Aplicar `openspec/changes/asignacion-activos/migration.sql` a `db_9c7990_servicedeskdesi` vía `SQLCMD.EXE` (flag `-C`), credenciales en `Web.config` → `connectionStrings/cCon`. Verificar post-migración: `sys.columns` de `PersonaActivo` incluye `FechaConfirmacion`/`TokenConfirmacion`; `SELECT OBJECT_ID('BitacoraCorreo')` y `SELECT OBJECT_ID('ConfirmarRecepcionActivo')` no nulos. (D6) — **APLICADA y verificada (los 6 objetos existen en BD hosted).**

## Lote 10: Verificación / smoke

- [ ] T28 — Smoke manual: asignar → correo Enviado + fila `BitacoraCorreo` estado `Enviado` con `ReferenciaId`; forzar fallo SMTP → compensación (desvincula) + `Fallido` + `IsSuccess=false`; abrir enlace → `FechaConfirmacion` seteada; reabrir enlace → "ya confirmado" (idempotente, sin cambio); build 0 errores. (NAA-001…005, CRA-001…005)
