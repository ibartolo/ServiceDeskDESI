# Verificación — mejoras-rol-activos-permisos

- **Fase**: verify
- **Fecha**: 2026-09-01
- **Modo**: Standard (strict_tdd DISABLED — sin framework de tests; verificación = análisis estático + MSBuild + inspección de código dirigida).
- **Alcance**: 20 requisitos / 26 escenarios (4 specs) + ítems 6/7 del proposal + integridad de migración + build.

---

## Veredicto: **PASS WITH WARNINGS**

La implementación es completa (35/35 tareas), correcta frente a los 4 specs (20 requisitos / 26 escenarios) y compila con **0 errores** en los 3 proyectos. No se detectaron defectos CRÍTICOS. Las advertencias son: (W1) `MvcBuildViews` desactivado → la sintaxis Razor no se valida en build (solo revisión estática/manual); (W2) los endpoints de conteo de páginas (`ConteoPaginasPorRol`) no llevan `[Permiso]`, exponiendo el conteo de páginas por rol a cualquier usuario autenticado (riesgo aceptado explícitamente en `design.md`).

---

## Completitud (tasks.md)

| Métrica | Valor |
|---------|-------|
| Tareas totales | 35 |
| Tareas completadas `[x]` | 35 |
| Tareas incompletas `[ ]` | 0 |

Verificado: las 35 marcas `[x]` corresponden a código real presente (inspeccionado archivo por archivo). No hay tareas pendientes.

---

## Build (MSBuild — ejecución real)

| Ítem | Resultado |
|------|-----------|
| Solución | `ServiceDeskDESI.sln` (Debug, `/t:Rebuild`) |
| MSBuild | `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe` |
| **Errores** | **0** ✅ |
| Advertencias | 4 (todas PRE-EXISTENTES, no introducidas por este cambio) |
| Proyectos | ServiceDeskDESIEntities ✅ · ServiceDeskDESIMVC ✅ · ServiceDeskDESIWebApi ✅ |

**Advertencias confirmadas (pre-existentes, fuera de alcance):**
- `UserController.cs(128,30)` CS0168 — variable `ex` sin usar.
- `CatalogsController.cs(647,30)` CS0168 — variable `ex` sin usar.
- `Startup.cs(163,36)` CS1998 — método async sin `await`.
- `Startup.cs(186,36)` CS1998 — método async sin `await`.

El rebuild compila los `<Compile Include>` manuales nuevos (G2/G3/G4) y produce los 3 DLL sin error.

---

## Matriz de trazabilidad (REQ → estado → evidencia)

### PEU — permisos-edicion-usuarios-personas

| REQ | Escenarios | Estado | Evidencia |
|-----|-----------|--------|-----------|
| PEU-001 (sin flag en Rol) | 1 | ✅ COMPLIANT | `git status` no muestra `Rol.cs`/`GuardarOActualizarRol`; grep `PuedeModificarUsuariosPersonas` = 0 coincidencias en código; `migration.sql` no toca `Rol`. |
| PEU-002 (inputs disabled en edición sin "Editar") | 2 | ✅ COMPLIANT | `UserController.cs:140` (`ObtenerPermisosParaUsuario()`), `:142-146` (redirect D1), `:261` (`ViewBag.Permisos`); `Users.cshtml:6` (`soloLectura`), `:38-104` (10 inputs con `disabled` condicional). |
| PEU-003 (Persona extiende estaVinculada) | 2 | ✅ COMPLIANT | `Persona.cshtml:7` (`bloqueoCampos = estaVinculada \|\| (Model.Id>0 && !permisos.PuedeEditar)`), `:36/42/48/56` (4 campos), `:142` (`personaIdEdicion`), `:530` (`AplicarBloqueoSincronizado`). |
| PEU-004 (creación sujeta a "Crear") | 2 | ✅ COMPLIANT | `Users.cshtml:113` y `Persona.cshtml:75` gatean Guardar con `(Model.Id==0 && PuedeCrear) \|\| (Model.Id>0 && PuedeEditar)`. |
| PEU-005 (server-side intacto) | 1 | ✅ COMPLIANT | `UserController.cs:272` `[Permiso("Usuarios")]`; `CatalogsController.cs:799/807/831/839/847` `[Permiso("Personas","Editar")]` — sin cambios. |

### SUA — serial-unico-activo

| REQ | Escenarios | Estado | Evidencia |
|-----|-----------|--------|-----------|
| SUA-001 (índice único filtrado por empresa) | 2 | ✅ COMPLIANT | `migration.sql:42-46` `UX_Activo_EmpresaSerial ON Activo(EmpresaId, Serial) WHERE Serial IS NOT NULL AND Estatus = 1`. |
| SUA-002 (serial nulo permitido) | 1 | ✅ COMPLIANT | Filtro `Serial IS NOT NULL`; `migration.sql:99` `SET @Serial = NULLIF(LTRIM(RTRIM(@Serial)),'')` normaliza vacío→NULL. |
| SUA-003 (soft-delete libera serial) | 1 | ✅ COMPLIANT | Filtro `Estatus = 1` en índice y en chequeo de duplicado (`migration.sql:105`). |
| SUA-004 (SP retorno -2) | 1 | ✅ COMPLIANT | `migration.sql:102-112` `EXISTS` (misma empresa, `Id <> @Id`, `Estatus=1`) → `SELECT -2; RETURN;` **antes** de validaciones tenant. |
| SUA-005 (mensaje amigable) | 1 | ✅ COMPLIANT | `DbWrapper.Activo.cs:109-114` rama `-2` **antes** del chequeo `0` → `"Ya existe un activo con ese No. de Serie"`; `Active.cshtml:434-442` muestra `response.Message` vía Swal. |

### CAM — campos-activo

| REQ | Escenarios | Estado | Evidencia |
|-----|-----------|--------|-----------|
| CAM-001 (SerieLocal end-to-end) | 2 | ✅ COMPLIANT | `Activo.cs:15` → `Active.cshtml:68` (textbox) + `:405` (`SerieLocal:$("#SerieLocal").val()`) → `DbWrapper.Activo.cs:93` (`a.SerieLocal`) → `migration.sql:82,165,225,228` (`@SerieLocal`). |
| CAM-002 (SerieLocal no único) | 1 | ✅ COMPLIANT | Sin índice/constraint sobre `SerieLocal` (solo `UX_Activo_EmpresaSerial` sobre `Serial`). |
| CAM-003 (Notas textarea maxlength 250) | 2 | ✅ COMPLIANT | `Active.cshtml:74` `@Html.TextAreaFor` (rows=3) + `:258` regla `"Notas": { maxlength: 250 }`. |
| CAM-004 (sin campo Comentarios) | 1 | ✅ COMPLIANT | grep `Comentarios` = 0 en Entities/migración; `Notas` sigue `NVARCHAR(250)`. |

### MTA — mantenimiento-activo

| REQ | Escenarios | Estado | Evidencia |
|-----|-----------|--------|-----------|
| MTA-001 (tabla + SP guardar) | 1 | ✅ COMPLIANT | `migration.sql:50-65` (tabla `Mantenimiento` + `FK_Mantenimiento_Activo` + `DF_Mantenimiento_Estatus`) y `:240-262` (`GuardarMantenimiento` → `GETDATE()`, `Estatus=1`, `EmpresaId` derivada). `Mantenimiento.cs` hereda `BaseObject` (Id/CreadoPor/FechaCreacion/…/Estatus) + ActivoId/Comentario/Fecha/EmpresaId → mapea `m.*`. |
| MTA-002 (Fecha visible disabled) | 1 | ✅ COMPLIANT | `_MantenimientoActivo.cshtml:14` input `#mantenimientoFecha` `value="@DateTime.Now" readonly disabled`. |
| MTA-003 (historial DESC + fecha NOT NULL) | 2 | ✅ COMPLIANT | `migration.sql:279-281` `WHERE ... Estatus=1 AND Fecha IS NOT NULL ... ORDER BY m.Fecha DESC`. |
| MTA-004 (multi-tenant EmpresaId) | 1 | ✅ COMPLIANT | `Mantenimiento.EmpresaId`; SPs derivan empresa de `@Usuario` (subconsulta `Usuarios`); MVC/WebApi no pasan `EmpresaId`. |
| MTA-005 (soft-delete) | 1 | ✅ COMPLIANT | `Estatus BIT DEFAULT 1`; historial filtra `Estatus = 1`. |
| MTA-006 (modal + histórico) | 1 | ✅ COMPLIANT | `Active.cshtml:305-308` botón gateado `permisosGlobal.PuedeLeer`, `:156` `@Html.Partial("_MantenimientoActivo")`; `CatalogsController.cs:968-983`; cadena completa MVC→HttpClient→WebApi→DbWrapper→SP (validada por código). |

### Ítems 6/7 (sin spec — contra proposal/design)

| Ítem | Estado | Evidencia |
|------|--------|-----------|
| 6 — Contador sin N+1 | ✅ COMPLIANT | `Permisos.cshtml:259` `conteoByRol={}`, `:390` `CargarConteoPaginas()` (una sola llamada tras `draw()`), `:395-406` (1 GET, llena mapa, `ActualizarBadges()`), `:567-587` (`asignadas.length` para rol seleccionado, `conteoByRol[rolId]\|\|0` para el resto), `:360` `<span class="badge-paginas">0</span>`. SP `ObtenerConteoPaginasPorRol` = una query `GROUP BY RolId` (`migration.sql:288-296`). Sin bucle por rol. |
| 7 — Tema oscuro chooser | ✅ COMPLIANT | `TemplatePage.css:899-942` overrides `body.dark-theme` para `.chooser-column`, `.chooser-item`(+`:hover`), `.item-nombre`, `.item-direccion/.empty-message/.text-muted-small`, `.badge-paginas`, `.disponible/.asignada`, `.permisos-checkboxes` (+`accent-color`). Variables `--primary:#6d8df0` definidas en `:445`. |

---

## Coherencia (Design)

| Decisión | Seguida | Notas |
|----------|---------|-------|
| 1 — Ítem 1 vía PermisosViewModel (sin flag en Rol) | ✅ | `UserController.Users()` + `ViewBag.Permisos`; sin flag en `Rol`. |
| 2 — Reutilizar `ObtenerPermisosParaUsuario()` (no crear `ObtenerPermisosParaUsuarios()`) | ✅ | `UsuarioService.cs:60-68` reutilizado (el spec PEU-002 menciona `ObtenerPermisosParaUsuarios()`; la decisión D2/tasks lo resuelve autoritativamente a favor del método existente — desviación documentada, correcta). |
| 3 — Normalizar serial vacío→NULL (`NULLIF`) | ✅ | `migration.sql:99`. |
| 4 — Duplicado vs `EmpresaId` derivado de `@Usuario` | ✅ | `migration.sql:107`. |
| 5 — Modal: Fecha auto, visible en input disabled | ✅ | `_MantenimientoActivo.cshtml:14`. |
| 6 — 1 SP agrupado (sin N+1) | ✅ | `ObtenerConteoPaginasPorRol`. |
| 7 — Overrides `body.dark-theme` en CSS global (conservar inline light) | ✅ | `TemplatePage.css:899-942`; `<style>` inline light conservado en `Permisos.cshtml:16-160`. |

**D1 (redirect `!PuedeLeer` en `Users()`)** aplicado en `UserController.cs:142-146` (resuelve el open question del design; coherencia con `Persona()`/`Role()`).

**D2 (modal reutiliza "Activos")** aplicado: botón gateado `PuedeLeer` (`Active.cshtml:306`), guardar `[Permiso("Activos","Editar")]` (`CatalogsController.cs:977` + `MantenimientoController.cs:42`), leer `[Permiso("Activos","Leer")]` (`MantenimientoController.cs:29`).

**D3 (serial vacío→NULL antes del chequeo)** aplicado en `migration.sql:99`.

---

## Integridad de migración (migration.sql / rollback.sql)

| Ítem | Resultado |
|------|-----------|
| Objetos presentes | ✅ `SerieLocal` (ALTER), `UX_Activo_EmpresaSerial` (índice filtrado), `Mantenimiento` (tabla + FK + DF), 4 SPs (`GuardarOActualizarActivo` rewrite, `GuardarMantenimiento`, `ObtenerMantenimientosPorActivo`, `ObtenerConteoPaginasPorRol`). |
| Guards idempotentes | ✅ `sys.columns` / `sys.indexes` / `OBJECT_ID(...) IS NULL` / `OBJECT_ID(...) IS NOT NULL DROP`. |
| rollback espejo (orden inverso) | ✅ `rollback.sql:14-42` → DROP 3 SPs nuevos → DROP TABLE → DROP INDEX → DROP COLUMN → restaurar `GuardarOActualizarActivo` previo (cada DROP con guard). |
| Firma SP vs DAL | ✅ `GuardarMantenimiento(@ActivoId,@Comentario,@CreadoPor,@FechaCreacion,@Usuario)` = `DbWrapper.Mantenimiento.cs:20-27`; `ObtenerMantenimientosPorActivo(@ActivoId,@Usuario)` = `:60-64`; `GuardarOActualizarActivo` incluye `@SerieLocal` en la posición exacta del `parametrosObj` de `DbWrapper.Activo.cs:86-104`. |

**Firma `GuardarOActualizarActivo` (orden) verificada campo a campo:** `@Id, @Nombre, @Descripcion, @TipoActivoId, @Serial, @SerieLocal, @MarcaId, @ModeloId, @Notas, @FechaCompra, @CreadoPor, @FechaCreacion, @ModificadoPor, @FechaModificacion, @Estatus, @Usuario` ↔ `parametrosObj` (reflexión `ObtenerParametrosSQL`). Coincide.

---

## Archivos pre-existentes (constraint respetado)

Los 4 archivos de sesión previa **no fueron tocados por este change**. Su diff no contiene ningún token de este cambio (`SerieLocal`/`Mantenimiento`/`conteo`/`permisosGlobal`/dark-theme). Su contenido modificado es de una sesión anterior (configuración de hosting/conexión):

| Archivo | Cambio pre-existente (ajeno a este change) |
|---------|--------------------------------------------|
| `ServiceDeskDESIMVC/Views/Home/Configuration.cshtml` | Comenta `<small>` de vigencia (4 +/−) |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.cs` | Cadena de conexión desde variable de entorno `sConSql` (15 +/−) |
| `ServiceDeskDESIWebApi/Web.Debug.config` | Transform vacía `cCon` (5 +/−) |
| `ServiceDeskDESIWebApi/Web.Release.config` | Transform vacía `cCon` (5 +/−) |

Confirmado vía `git status` + `git diff` por archivo.

---

## Issues encontrados

### CRITICAL
Ninguno.

### WARNING
- **W1 — `MvcBuildViews` desactivado (Razor no compilado en build).** La sintaxis Razor de las 5 vistas modificadas/creadas (`Active.cshtml`, `Users.cshtml`, `Persona.cshtml`, `Permisos.cshtml`, `_MantenimientoActivo.cshtml`) se validó solo de forma **estática/manual**; un error de compilación Razor solo aparecería en runtime. *Recomendación:* activar `MvcBuildViews` en `ServiceDeskDESIMVC.csproj` (Debug) o realizar un smoke-test de navegación a las páginas afectadas tras aplicar la migración. No bloquea el archive (patrón existente del proyecto).
- **W2 — Endpoints de conteo sin `[Permiso]`.** `PermisosController.cs:114-119` (`api/Permisos/ConteoPaginasPorRol`) y `SecurityController.cs:137-141` (`ConsultarConteoPaginasPorRol`) no tienen `[Permiso("Permisos",...)`. Devuelven `RolId/TotalPaginas` de **todos** los roles (la tabla `RolPaginaAccion` no tiene `EmpresaId`). Riesgo aceptado explícitamente en `design.md` (sección Riesgos) porque `Permisos()` solo renderiza roles scoped por empresa, y es consistente con el endpoint existente `ObtenerPermisosPorRol` (también sin `[Permiso]`). *Recomendación:* añadir `[Permiso("Permisos","Leer")]` para cerrar la fuga de información cross-tenant.

### SUGGESTION
- **S1 — `ObtenerMantenimientosPorActivo` (MVC) sin `[Permiso]`.** `CatalogsController.cs:970` carece de `[Permiso("Activos","Leer")]`, delegando el control al `[Permiso("Activos","Leer")]` del WebApi (`MantenimientoController.cs:29`). Correcto funcionalmente (defensa en WebApi), pero añadir el atributo en MVC reforzaría la defensa en profundidad y sería simétrico con `GuardarMantenimiento`.
- **S2 — Diferencia de segundos en `Fecha`.** El modal muestra `@DateTime.Now` (servidor MVC, `_MantenimientoActivo.cshtml:14`) mientras la fecha persistida la asigna `GETDATE()` del SP. Coherente con el diseño (fecha auto visible), con posible desfase de segundos entre lo mostrado y lo guardado.
- **S3 — `PermisosController.ObtenerConteoPaginasPorRol` (WebApi) no deriva `usuario`.** A diferencia de otros endpoints, no pasa `User.Identity.Name` (no lo necesita: el SP no es multi-tenant). Documentado; sin impacto.

---

## Resumen de cumplimiento

| Métrica | Valor |
|---------|-------|
| Requisitos cubiertos | 20/20 ✅ |
| Escenarios cubiertos | 26/26 ✅ |
| Ítems 6/7 | ✅ / ✅ |
| Tareas completadas | 35/35 ✅ |
| Build | 0 errores ✅ |
| CRITICAL | 0 |
| WARNING | 2 (W1 Razor no compilado, W2 endpoints conteo sin `[Permiso]`) |
| SUGGESTION | 3 |

**Veredicto final: PASS WITH WARNINGS.** Sin bloqueos; la migración queda pendiente de aplicación manual por el usuario (NO ejecutada contra BD). Recomendado: archivar con las 2 advertencias documentadas (o resolver W2 antes de archivar si se quiere cerrar la fuga de información).
