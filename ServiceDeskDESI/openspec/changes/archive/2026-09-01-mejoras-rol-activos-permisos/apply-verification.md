# Verificación — mejoras-rol-activos-permisos

- **Fase**: apply (G8 — Verificación: T34 + T35)
- **Fecha**: 2026-09-01
- **Alcance**: compilación MSBuild + revisión estática contra los 4 specs (20 requisitos, 26 escenarios) + ítems 6/7 del proposal.

---

## T34 — Compilación MSBuild

| Ítem | Resultado |
|------|-----------|
| Solución | `ServiceDeskDESI.sln` (Debug) |
| Errores | **0** ✅ |
| Advertencias | 4 (todas PRE-EXISTENTES, no introducidas por este cambio) |
| Proyectos | ServiceDeskDESIEntities ✅ · ServiceDeskDESIMVC ✅ · ServiceDeskDESIWebApi ✅ |

**Advertencias pre-existentes (registradas, sin acción):**
- `UserController.cs(128)` CS0168 — variable `ex` sin usar.
- `CatalogsController.cs(647)` CS0168 — variable `ex` sin usar.
- `Startup.cs(163)` CS1998 — método async sin `await`.
- `Startup.cs(186)` CS1998 — método async sin `await`.

> Nota: `MvcBuildViews` **no** está activo en el csproj; las vistas Razor NO se compilan en build. La validación de sintaxis Razor de las vistas modificadas (`Active.cshtml`, `Users.cshtml`, `Persona.cshtml`, `Permisos.cshtml`, `_MantenimientoActivo.cshtml`) fue **manual/estática** (verificación de runtime pendiente en verify).

---

## T35 — Revisión estática por requisito

### PEU — permisos-edicion-usuarios-personas

| REQ | Resultado | Evidencia |
|-----|-----------|-----------|
| PEU-001 (sin flag en Rol) | ✅ PASS | `git status` no muestra `Rol.cs`; `migration.sql` no toca `Rol`/`GuardarOActualizarRol`; grep de `PuedeModificarUsuariosPersonas` solo en docs (explore/design/proposal/specs), no en código. |
| PEU-002 (inputs disabled en edición sin Editar) | ✅ PASS | `UserController.Users()` puebla `ViewBag.Permisos` (línea 261) vía `ObtenerPermisosParaUsuario()`; `Users.cshtml` calcula `soloLectura = Model.Id>0 && !permisos.PuedeEditar` y lo aplica a los 10 inputs (`txtNombre, txtApellido, txtNombreUsuario, Correo, Contrasena, Celular, RFC, ddlSucursal, ddlArea, ddlRol`). |
| PEU-003 (Persona extiende estaVinculada) | ✅ PASS | `Persona.cshtml` `bloqueoCampos = estaVinculada || (Model.Id>0 && !permisos.PuedeEditar)` (línea 7) aplicado a Nombre/Apellido/Correo/Teléfono; JS `AplicarBloqueoSincronizado()` añade `|| (personaIdEdicion>0 && !permisosGlobal.PuedeEditar)` con `personaIdEdicion = @Model.Id`. |
| PEU-004 (creación sujeta a Crear) | ✅ PASS | Botón Guardar gateado `permisos != null && ((Model.Id==0 && PuedeCrear) || (Model.Id>0 && PuedeEditar))` en `Users.cshtml` y `Persona.cshtml`; en creación los inputs no llevan `disabled`. |
| PEU-005 (server-side intacto) | ✅ PASS | `[Permiso("Usuarios")]` en `GuardarOActualizarUsuarioAdmin` y `[Permiso("Personas","Editar")]` en `GuardarOActualizarPersona` sin cambios. |

### SUA — serial-unico-activo

| REQ | Resultado | Evidencia |
|-----|-----------|-----------|
| SUA-001 (índice único filtrado por empresa) | ✅ PASS | `migration.sql` crea `UX_Activo_EmpresaSerial ON Activo(EmpresaId, Serial) WHERE Serial IS NOT NULL AND Estatus = 1` con guard `sys.indexes`. |
| SUA-002 (serial nulo permitido) | ✅ PASS | Filtro del índice `Serial IS NOT NULL`; el SP normaliza vacío→NULL (`NULLIF`) antes del chequeo. |
| SUA-003 (soft-delete libera serial) | ✅ PASS | Filtro `Estatus = 1`; chequeo de duplicado en SP también filtra `Estatus = 1`. |
| SUA-004 (validación SP retorno -2) | ✅ PASS | `GuardarOActualizarActivo`: `SET @Serial = NULLIF(LTRIM(RTRIM(@Serial)),'')` → chequeo `EXISTS` (excluyendo `Id` actual, misma empresa) → `SELECT -2; RETURN;` **antes** de validaciones tenant. |
| SUA-005 (mensaje amigable) | ✅ PASS | `DbWrapper.Activo.cs` rama `-2` **antes** del chequeo `0` → mensaje "Ya existe un activo con ese No. de Serie"; `Active.cshtml` ya muestra `response.Message` vía Swal. |

### CAM — campos-activo

| REQ | Resultado | Evidencia |
|-----|-----------|-----------|
| CAM-001 (SerieLocal end-to-end) | ✅ PASS | Entidad `Activo.SerieLocal` → `Active.cshtml` textbox + `SerieLocal:$("#SerieLocal").val()` → `DbWrapper.Activo.cs` `a.SerieLocal` en `parametrosObj` → SP `@SerieLocal` en UPDATE/INSERT → `migration.sql` ALTER (lectura vía `a.*`). |
| CAM-002 (SerieLocal no único) | ✅ PASS | Sin índice/constraint sobre `SerieLocal` (solo el índice filtrado es sobre `Serial`). |
| CAM-003 (Notas textarea maxlength 250) | ✅ PASS | `@Html.TextAreaFor(x => x.Notas, ...)` + regla `"Notas": { maxlength: 250 }` + mensaje en `jquery.validate`. |
| CAM-004 (sin campo Comentarios) | ✅ PASS | Grep de `Comentarios` solo en docs; `migration.sql` no crea `Comentarios`; `Notas` sigue `NVARCHAR(250)`. |

### MTA — mantenimiento-activo

| REQ | Resultado | Evidencia |
|-----|-----------|-----------|
| MTA-001 (tabla + SP guardar) | ✅ PASS | `Mantenimiento` (patrón PersonaActivo, FK, DF_Estatus default 1, EmpresaId); SP `GuardarMantenimiento` inserta `Fecha = GETDATE()`, `Estatus = 1`, `EmpresaId` derivada. |
| MTA-002 (Fecha visible deshabilitada) | ✅ PASS | `_MantenimientoActivo.cshtml` input `#mantenimientoFecha` con `value="@DateTime.Now..." readonly disabled`. |
| MTA-003 (historial Fecha DESC) | ✅ PASS | SP `ObtenerMantenimientosPorActivo` → `WHERE ... Estatus=1 AND Fecha IS NOT NULL ... ORDER BY m.Fecha DESC`. |
| MTA-004 (multi-tenant EmpresaId) | ✅ PASS | `Mantenimiento.EmpresaId` + SPs derivan empresa de `@Usuario` (subconsulta `Usuarios`); MVC/WebApi no pasan EmpresaId explícito. |
| MTA-005 (soft-delete) | ✅ PASS | `Estatus BIT DEFAULT 1`; historial filtra `Estatus = 1`. |
| MTA-006 (modal + histórico) | ✅ PASS | Botón "Mantenimientos" por fila gateado `permisosGlobal.PuedeLeer` (reutiliza "Activos"); `@Html.Partial("_MantenimientoActivo")`; guardar `[Permiso("Activos","Editar")]` (MVC + WebApi). Cadena completa: modal → MVC `CatalogsController` → `HttpClientConnection.Mantenimiento` (MappingColumSecurity setea CreadoPor/FechaCreacion al ser `Id==0`) → WebApi `MantenimientoController` → `DbWrapper.Mantenimiento` → SP. |

### Ítems 6 y 7 (sin spec)

| Ítem | Resultado | Evidencia |
|------|-----------|-----------|
| 6 — Contador páginas sin N+1 | ✅ PASS | SP `ObtenerConteoPaginasPorRol` (una sola query agrupada). `Permisos.cshtml`: `var conteoByRol={}`; `CargarRoles()` llama `CargarConteoPaginas()` una vez tras `draw()`; `CargarConteoPaginas()` hace **un único** GET `/Security/ConsultarConteoPaginasPorRol` y llena el mapa; `ActualizarBadges()` usa `paginasByRol.asignadas.length` para el rol seleccionado y `conteoByRol[rolId] || 0` para el resto. Cadena WebApi `PermisosController/ConteoPaginasPorRol` → `PermisosService` → `DbWrapper.ObtenerConteoPaginasPorRol`. |
| 7 — Tema oscuro chooser | ✅ PASS | `TemplatePage.css` (líneas 898-942) bloques `body.dark-theme` para `.chooser-column`, `.chooser-item`(+`:hover`), `.item-nombre`, `.item-direccion/.empty-message/.text-muted-small`, `.badge-paginas`, `.disponible/.asignada`, `.permisos-checkboxes` (+`accent-color`). Se conserva el `<style>` inline light. |

### migration.sql / rollback.sql

| Ítem | Resultado |
|------|-----------|
| Objetos presentes | ✅ SerieLocal, índice `UX_Activo_EmpresaSerial`, tabla `Mantenimiento`, SPs `GuardarOActualizarActivo` (rewrite), `GuardarMantenimiento`, `ObtenerMantenimientosPorActivo`, `ObtenerConteoPaginasPorRol`. |
| Guards idempotentes | ✅ `sys.columns` / `sys.indexes` / `OBJECT_ID(... IS NULL)` / `IS NOT NULL ... DROP`. |
| rollback espejo | ✅ Orden inverso: DROP 3 SPs nuevos → DROP TABLE → DROP INDEX → DROP COLUMN → restaurar `GuardarOActualizarActivo` previo (cada uno con guard). |
| Firma SP vs DAL | ✅ `GuardarMantenimiento(@ActivoId,@Comentario,@CreadoPor,@FechaCreacion,@Usuario)` coincide con `DbWrapper.Mantenimiento`; `ObtenerMantenimientosPorActivo(@ActivoId,@Usuario)` coincide; `GuardarOActualizarActivo` incluye `@SerieLocal` en la posición del `parametrosObj` de `DbWrapper.Activo`. |

---

## Veredicto final

- **T34**: PASS (0 errores).
- **T35**: **PASS en los 20 requisitos + ítems 6/7** (sin gaps funcionales).

### Observaciones (no bloqueantes)
1. `MvcBuildViews` desactivado → la sintaxis Razor no se valida en build; revisión de vistas fue manual. Verificar en runtime.
2. La `Fecha` del modal se renderiza con `@DateTime.Now` del servidor MVC, mientras la fecha persistida la asigna `GETDATE()` del SP. Es coherente con el diseño (visible/auto), con posible diferencia de segundos.
3. Advertencias CS0168/CS1998 pre-existentes no fueron tocadas (fuera de alcance).

### Archivos de 4 sesiones previas NO tocados (respetado)
`Views/Home/Configuration.cshtml`, `ServiceDeskDESIWebApi/DAL/DbWrapper.cs`, `Web.Debug.config`, `Web.Release.config`.
