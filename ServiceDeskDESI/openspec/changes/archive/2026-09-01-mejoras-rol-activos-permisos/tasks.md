# Tasks: Mejoras de Roles, Activos y Permisos

Orden: **BD → Entidades → WebApi → MVC DAL/Services → MVC Controllers → Vistas → CSS → Verificación**.

> **Decisiones autoritativas (usuario) ya codificadas:**
> - **D1 — Redirect `!PuedeLeer` en `Users()`**: `UserController.Users()` DEBE redirigir a `AccesoDenegado` cuando el usuario logueado no tenga `PuedeLeer` en "Usuarios" (consistente con los demás catálogos). `Persona()` YA tiene este redirect (CatalogsController líneas 144-148) → **sin cambio**.
> - **D2 — Modal Mantenimientos reutiliza el permiso "Activos"**: NO se crea página/permiso nuevo. El botón y el modal se gobiernan con la página "Activos" existente. Nivel elegido: **crear mantenimiento = acción "Editar" de "Activos"** (misma acción que ya aplica el catálogo en `[Permiso("Activos")]`; el modal *modifica* el historial del activo, no solo lo lee); **leer historial = acción "Leer"**. Justificación: el mantenimiento es una operación de escritura sobre el activo, por lo que reutiliza "Editar" en vez de "Crear" (no se crea una entidad-página nueva) — espejo del patrón `[Permiso("Activos","Editar")]` del guardado de activos.
> - **D3 — Serial vacío → NULL**: `GuardarOActualizarActivo` normaliza `@Serial` con `NULLIF(LTRIM(RTRIM(@Serial)),'')` ANTES del chequeo de duplicado y del INSERT/UPDATE.
>
> `UsuarioService.ObtenerPermisosParaUsuario()` YA existe (filtra `PaginaNombre=="Usuarios"`) → solo se cablea en `Users()`, no se crea método nuevo. `PermisosViewModel` vive en `ServiceDeskDESIEntities.Seguridad` (ya `using` en UserController).

---

## G1 — Base de datos (`migration.sql` + `rollback.sql`)

- [x] **T1** (S) — `openspec/changes/mejoras-rol-activos-permisos/migration.sql`: `ALTER TABLE dbo.Activo ADD SerieLocal NVARCHAR(100) NULL` idempotente con guard `IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Activo') AND name=N'SerieLocal')`. **(CAM-001, CAM-002)**
- [x] **T2** (S) — `migration.sql`: índice único filtrado `UX_Activo_EmpresaSerial` sobre `(EmpresaId, Serial)` con `WHERE Serial IS NOT NULL AND Estatus = 1`, guard `IF NOT EXISTS (sys.indexes ... name=N'UX_Activo_EmpresaSerial')`. **(SUA-001, SUA-002, SUA-003)**
- [x] **T3** (M) — `migration.sql`: `CREATE TABLE dbo.Mantenimiento` (Id BIGINT IDENTITY PK, ActivoId BIGINT NOT NULL, Comentario NVARCHAR(500) NOT NULL, Fecha DATETIME NOT NULL, CreadoPor NVARCHAR(25), FechaCreacion DATETIME, ModificadoPor NVARCHAR(25) NULL, FechaModificacion DATETIME NULL, Estatus BIT DEFAULT 1, EmpresaId BIGINT NOT NULL, `FK_Mantenimiento_Activo` → Activo(Id)), guard `IF OBJECT_ID(N'dbo.Mantenimiento',N'U') IS NULL`. **(MTA-001, MTA-004, MTA-005)**
- [x] **T4** (L) — `migration.sql`: reescritura DROP/CREATE de `GuardarOActualizarActivo`: añadir `@SerieLocal NVARCHAR(100) = NULL` a la firma y al UPDATE/INSERT; al inicio del `BEGIN`, **antes** de validaciones tenant: `SET @Serial = NULLIF(LTRIM(RTRIM(@Serial)), '');` seguido del chequeo de duplicado (`EXISTS ... WHERE Serial=@Serial AND Estatus=1 AND Id<>@Id AND EmpresaId=(SELECT EmpresaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1)`) → `SELECT -2; RETURN;`. Orden de validación: (1) duplicado `-2`, (2) tenant `0`, (3) UPDATE/INSERT. **(SUA-004, CAM-001 — D3)**
- [x] **T5** (M) — `migration.sql`: `CREATE PROCEDURE dbo.GuardarMantenimiento (@ActivoId BIGINT, @Comentario NVARCHAR(500), @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME, @Usuario NVARCHAR(25))` → valida tenant del activo (si no, `SELECT 0; RETURN`); `INSERT ... VALUES(@ActivoId, @Comentario, GETDATE(), @CreadoPor, @FechaCreacion, 1, (EmpresaId derivada))`; `SELECT SCOPE_IDENTITY();`. **(MTA-001, MTA-004)**
- [x] **T6** (S) — `migration.sql`: `CREATE PROCEDURE dbo.ObtenerMantenimientosPorActivo (@ActivoId BIGINT, @Usuario NVARCHAR(25))` → `SELECT m.* FROM Mantenimiento m INNER JOIN Activo a ... WHERE m.ActivoId=@ActivoId AND m.Estatus=1 AND m.Fecha IS NOT NULL AND a.EmpresaId=(derivada) ORDER BY m.Fecha DESC`. **(MTA-003, MTA-004, MTA-005)**
- [x] **T7** (S) — `migration.sql`: `CREATE PROCEDURE dbo.ObtenerConteoPaginasPorRol AS ... SELECT RolId, COUNT(*) AS TotalPaginas FROM RolPaginaAccion WHERE Estatus=1 GROUP BY RolId;` (una sola query, sin N+1). **(Ítem 6)**
- [x] **T8** (S) — `openspec/changes/mejoras-rol-activos-permisos/rollback.sql` en orden inverso: `DROP PROCEDURE` de los 3 SPs nuevos → `DROP TABLE Mantenimiento` → `DROP INDEX UX_Activo_EmpresaSerial` → `DROP COLUMN Activo.SerieLocal` → restaurar definición previa de `GuardarOActualizarActivo` (cada DROP con guard `IF EXISTS`).

## G2 — Entidades (+ csproj)

- [x] **T9** (S) — `ServiceDeskDESIEntities/Catalogos/Activo.cs`: añadir `public string SerieLocal { get; set; }`. `ActivoDTO` hereda de `Activo` → flujo automático (sin tocar `ActivoDTO.cs`; lectura vía `a.*` en `ObtenerActivos`/`ObtenerActivoPorId`). **(CAM-001)**
- [x] **T10** (S) — Crear `ServiceDeskDESIEntities/Catalogos/Mantenimiento.cs` (hereda `BaseObject`): `public long ActivoId`, `public string Comentario`, `public DateTime Fecha`, `public long EmpresaId`. Sin DTO (historial mapea `m.*`). **(MTA-001)**
- [x] **T11** (S) — Crear `ServiceDeskDESIEntities/Seguridad/RolConteoPaginasDTO.cs`: `public long RolId; public int TotalPaginas;` (clase simple, sin herencia). **(Ítem 6)**
- [x] **T12** (S) — `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` (legacy, `<Compile Include>` manual): registrar `Catalogos\Mantenimiento.cs` y `Seguridad\RolConteoPaginasDTO.cs`.

## G3 — WebApi (DAL + Services + Controllers)

- [x] **T13** (M) — `ServiceDeskDESIWebApi/DAL/DbWrapper.Activo.cs` `GuardarOActualizarActivo`: añadir `a.SerieLocal` a `parametrosObj` (la reflexión `ObtenerParametrosSQL` genera `@SerieLocal`); tras `ExecuteScalar`, insertar rama **antes** del chequeo de `0`: `if (Convert.ToInt64(activoId) == -2) { modelResponse.IsSuccess=false; modelResponse.Message="Ya existe un activo con ese No. de Serie"; return modelResponse; }`. **(SUA-004, SUA-005, CAM-001)**
- [x] **T14** (S) — `ServiceDeskDESIWebApi/Services/ActivoServices.cs`: validación opcional `if (activo.SerieLocal != null && activo.SerieLocal.Length > 100)` → `IsSuccess=false` con mensaje. **(CAM-001)**
- [x] **T15** (M) — Crear `ServiceDeskDESIWebApi/DAL/DbWrapper.Mantenimiento.cs` (partial): `ModelResponse GuardarMantenimiento(Mantenimiento m, string usuario)` → `ExecuteScalar("GuardarMantenimiento", ...)`; `ModelResponse<List<Mantenimiento>> ObtenerMantenimientosPorActivo(long activoId, string usuario)` → `GetObjects(...)` + `LlenarEntidad<Mantenimiento>`. **(MTA-001, MTA-003)**
- [x] **T16** (S) — Crear `ServiceDeskDESIWebApi/Services/MantenimientoService.cs`: validar `ActivoId > 0`, `Comentario` requerido/`<=500`, `CreadoPor`/`usuario` requeridos; passthrough al DAL (el SP setea `Fecha`/`EmpresaId`). **(MTA-001, MTA-004)**
- [x] **T17** (M) — Crear `ServiceDeskDESIWebApi/Controllers/MantenimientoController.cs` (`[Authorize]`, `[RoutePrefix("api/Mantenimiento")]`): `[HttpGet, Route("PorActivo/{activoId:long}")] [Permiso("Activos","Leer")] ObtenerMantenimientosPorActivo(long activoId)` y `[HttpPost, Route("Guardar")] [Permiso("Activos","Editar")] GuardarMantenimiento(Mantenimiento m)` (NO setea `m.EmpresaId`/`m.Fecha`). **Reutiliza "Activos" (D2)**: leer=Leer, guardar=Editar; sin página nueva. **(MTA-006)**
- [x] **T18** (S) — `ServiceDeskDESIWebApi/DAL/DbWrapper.Permisos.cs`: `ObtenerConteoPaginasPorRol()` → `GetObjects("ObtenerConteoPaginasPorRol", ...)` + `LlenarEntidad<RolConteoPaginasDTO>`; `Services/PermisosService.cs` wrapper; `Controllers/PermisosController.cs`: `[HttpGet, Route("ConteoPaginasPorRol")]` → `ModelResponse<List<RolConteoPaginasDTO>>`. **(Ítem 6)**
- [x] **T19** (S) — `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` (legacy): registrar `<Compile Include>` de `DAL\DbWrapper.Mantenimiento.cs`, `Services\MantenimientoService.cs`, `Controllers\MantenimientoController.cs`.

## G4 — MVC DAL/Services

- [x] **T20** (M) — Crear `ServiceDeskDESIMVC/DAL/HttpClientConnection.Mantenimiento.cs` (partial): `ObtenerMantenimientosPorActivo(long)` → GET `api/Mantenimiento/PorActivo/{id}`; `GuardarMantenimiento(Mantenimiento)` → POST `api/Mantenimiento/Guardar` (con `MappingColumSecurity`). **(MTA)**
- [x] **T21** (S) — Crear `ServiceDeskDESIMVC/Services/MantenimientoService.cs`: wrappers que proxean a `HttpClientConnection.Mantenimiento`. **(MTA)**
- [x] **T22** (S) — `ServiceDeskDESIMVC/DAL/HttpClientConnection.Permisos.cs` + `Services/PermisosService.cs`: `ObtenerConteoPaginasPorRol()` → GET `api/Permisos/ConteoPaginasPorRol`. **(Ítem 6)**
- [x] **T23** (S) — `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` (legacy): registrar `<Compile Include>` de `DAL\HttpClientConnection.Mantenimiento.cs` y `Services\MantenimientoService.cs`. (Nota: `SerieLocal` fluye por `ActivoDTO` sin cambio en MVC `ActivoService`.)

## G5 — MVC Controllers

- [x] **T24** (M) — `ServiceDeskDESIMVC/Controllers/UserController.cs` `Users(long id=0)`: añadir `var permisos = await _usuarioService.ObtenerPermisosParaUsuario();` + redirect `if (permisos == null || !((PermisosViewModel)permisos).PuedeLeer) return RedirectToAction("AccesoDenegado", "Home");` + `ViewBag.Permisos = permisos;` antes de `return View(usuario)`. `Persona()` ya tiene este redirect → sin cambio. **(PEU-001, PEU-002, PEU-005 — D1)**
- [x] **T25** (M) — `ServiceDeskDESIMVC/Controllers/CatalogsController.cs`: añadir región `#region Mantenimiento` con `[HttpGet] public async Task<string> ObtenerMantenimientosPorActivo(long activoId)` y `[HttpPost][Permiso("Activos","Editar")] public async Task<string> GuardarMantenimiento(Mantenimiento m)`; inyectar `_mantenimientoService` en el constructor. **(MTA-006 — D2)**
- [x] **T26** (S) — `ServiceDeskDESIMVC/Controllers/SecurityController.cs`: `public async Task<string> ConsultarConteoPaginasPorRol()` → `_permisosService.ObtenerConteoPaginasPorRol()` serializado (`JsonConvert`). **(Ítem 6)**

## G6 — Vistas

- [x] **T27** (M) — `Views/Catalogs/Active.cshtml`: añadir `@Html.TextBoxFor(x => x.SerieLocal, new { @class = "form-control", placeholder = "Serie local" })` + `SerieLocal: $("#SerieLocal").val()` en el objeto `activo`; reemplazar `@Html.TextBoxFor(x => x.Notas, ...)` por `@Html.TextAreaFor(x => x.Notas, new { @class = "form-control", placeholder = "Notas de Activo", rows = 3 })` + regla `"Notas": { maxlength: 250 }` en `jquery.validate`. **(CAM-001, CAM-003)**
- [x] **T28** (M) — Crear `Views/Catalogs/_MantenimientoActivo.cshtml` (modal `modalMantenimientoActivo`, espejo de `_AsignarActivoPersona.cshtml`): input `Fecha` **visible** con `value="@DateTime.Now.ToString("yyyy-MM-dd HH:mm")"` + `readonly disabled`; `textarea` comentario (`id="mantenimientoComentario"`); tabla historial (`tblMantenimientos`); JS `AbrirMantenimientos(id)`, `CargarMantenimientos(activoId)`, `GuardarMantenimiento()` (PostMVC + recarga). **(MTA-002, MTA-006)**
- [x] **T29** (S) — `Views/Catalogs/Active.cshtml`: `@Html.Partial("_MantenimientoActivo")` al final del formulario + botón "Mantenimientos" por fila en la columna `Acciones` del DataTable, gateado por `permisosGlobal.PuedeLeer` (reutiliza permiso "Activos"). **(MTA-006 — D2)**
- [x] **T30** (M) — `Views/User/Users.cshtml`: adoptar patrón estándar (espejo de `Active.cshtml`/`Persona.cshtml`): cabecera `var permisos = ViewBag.Permisos as PermisosViewModel;`, JS `var permisosGlobal = @Html.Raw(JsonConvert.SerializeObject(permisos));`, botón Guardar gateado (`Model.Id==0 && PuedeCrear` || `Model.Id>0 && PuedeEditar`), inputs `disabled` en edición sin `PuedeEditar` (`txtNombre, txtApellido, txtNombreUsuario, Correo, Contrasena, Celular, RFC, ddlSucursal, ddlArea, ddlRol`), gate `Acciones` Editar/Eliminar con `permisosGlobal.PuedeEditar/PuedeEliminar`. **(PEU-002, PEU-004)**
- [x] **T31** (S) — `Views/Catalogs/Persona.cshtml`: extender condición de los 4 campos (`Nombre/Apellido/Correo/Telefono`) de `estaVinculada` a `estaVinculada || (Model.Id > 0 && !permisos.PuedeEditar)`; en JS `AplicarBloqueoSincronizado()` añadir `|| (personaIdEdicion > 0 && !permisosGlobal.PuedeEditar)` con nueva variable `personaIdEdicion = @Model.Id`. **(PEU-003)**
- [x] **T32** (M) — `Views/Security/Permisos.cshtml` JS: `var conteoByRol = {};`; en `CargarRoles()` tras `tablaRoles.draw()` llamar `CargarConteoPaginas()`; `CargarConteoPaginas()` → GET `/Security/ConsultarConteoPaginasPorRol` y `conteoByRol[c.RolId]=c.TotalPaginas` + `ActualizarBadges()`; `ActualizarBadges()` usa `paginasByRol.asignadas.length` para el rol seleccionado y `conteoByRol[rolId] || 0` para el resto; conservar `<span class="badge-paginas">0</span>` en `InicializarTablaRoles`. **(Ítem 6)**

## G7 — CSS

- [x] **T33** (S) — `CSS/Comun/TemplatePage.css`: añadir bloque `body.dark-theme` (al final de la sección TEMA OSCURO) con overrides del chooser: `.chooser-column`/`.chooser-item` (fondo `#1e1e2e`/`#232334`, borde `#3a3a52`), `.chooser-item:hover` borde `var(--primary)`, `.item-nombre` `#e4e6ef`, `.item-direccion`/`.empty-message`/`.text-muted-small` `#9aa0b5`, `.badge-paginas` `background:var(--primary); color:#fff`, `.chooser-item.disponible`/`.asignada` border-left, `.permisos-checkboxes` `#d5d7e3` + `accent-color:var(--primary)` (clase `permiso-check`). Conservar el `<style>` inline (light). **(Ítem 7)**

## G8 — Verificación

- [x] **T34** (M) — Compilar `ServiceDeskDESI.sln` con MSBuild VS2022 (Debug) → 0 errores en los 3 proyectos (confirma los `<Compile Include>` manuales de G2/G3/G4). **(success criteria)**
- [x] **T35** (M) — Revisión estática contra los 20 REQ (CAM-001..004, MTA-001..006, PEU-001..005, SUA-001..005) + ítems 6/7: checklist de que cada escenario está cubierto y que no se introdujo flag en `Rol`/`GuardarOActualizarRol` (PEU-001) ni campo `Comentarios` (CAM-004). **(todas)**
