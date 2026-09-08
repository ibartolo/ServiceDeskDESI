# Archive Report — mejoras-rol-activos-permisos

- **Change**: `mejoras-rol-activos-permisos`
- **Archived on**: 2026-09-01
- **Archive location**: `openspec/changes/archive/2026-09-01-mejoras-rol-activos-permisos/`
- **Artifact store**: openspec (file-based)
- **Veredicto**: **PASS WITH WARNINGS** (2 warnings no bloqueantes + 3 sugerencias)

---

## Resumen del cambio

Siete mejoras sobre tres módulos ya desplegados (Usuarios/Personas, Activos y Permisos), respetando la arquitectura N-capas existente (MVC → HttpClient → WebApi → DbWrapper por reflexión → SPs) y sin tocar el núcleo CRUD ya desplegado. Las dos decisiones centrales: **(1)** la restricción de edición de Usuarios/Personas se resuelve reutilizando la acción "Editar" del sistema de Permisos (SIN flag en `Rol`); **(4)** el gestor de mantenimientos replica el patrón vertical de `PersonaActivo` con un modal de captura + histórico y fecha auto (`GETDATE()`) visible en input deshabilitado.

## Qué se implementó (7 ítems)

1. **Bloqueo de edición de Usuarios/Personas vía Permisos** (sin flag en `Rol`): `UserController.Users()` puebla `ViewBag.Permisos` (reutiliza `ObtenerPermisosParaUsuario()`); `Users.cshtml` adopta el patrón estándar (`disabled` en edición sin `PuedeEditar`, guardado gateado `PuedeCrear/PuedeEditar`); `Persona.cshtml` extiende la condición `estaVinculada || (Model.Id>0 && !permisos.PuedeEditar)`. Server-side intacto (`[Permiso("Usuarios")]` / `[Permiso("Personas","Editar")]`). **(PEU-001..005)**
2. **Unicidad de `Serial` por empresa**: índice único filtrado `UX_Activo_EmpresaSerial (EmpresaId, Serial) WHERE Serial IS NOT NULL AND Estatus = 1`; SP `GuardarOActualizarActivo` normaliza `NULLIF` y devuelve `-2` ante duplicado (antes de validaciones tenant); `DbWrapper.Activo.cs` mapea `-2` → "Ya existe un activo con ese No. de Serie". **(SUA-001..005)**
3. **Campo `SerieLocal`** (`NVARCHAR(100) NULL`, no único) end-to-end: entidad → vista → `DbWrapper.Activo.cs` → SP. **(CAM-001, CAM-002)**
4. **Gestor de mantenimientos**: tabla `Mantenimiento` (patrón `PersonaActivo`, multi-tenant `EmpresaId`, soft-delete `Estatus`), SPs `GuardarMantenimiento`/`ObtenerMantenimientosPorActivo`, entidad + `DbWrapper.Mantenimiento` + `MantenimientoService` + `MantenimientoController` (WebApi) + `HttpClientConnection.Mantenimiento` + `MantenimientoService` (MVC) + modal `_MantenimientoActivo.cshtml` con histórico `Fecha DESC`. Reutiliza permiso "Activos" (Leer/Editar). **(MTA-001..006)**
5. **`Notas` → `<textarea>`** con `maxlength = 250` (sin cambio de BD; ya `NVARCHAR(250)`). **(CAM-003, CAM-004)**
6. **Fix contador de páginas asignadas** en `Permisos.cshtml`: SP agrupado `ObtenerConteoPaginasPorRol` (una sola query `GROUP BY RolId`, sin N+1) + DTO `RolConteoPaginasDTO` + mapa `conteoByRol` en JS.
7. **Fix tema oscuro del chooser de permisos**: overrides `body.dark-theme` en `TemplatePage.css` para `.chooser-*`, `.badge-paginas`, `.permisos-checkboxes` (conserva el `<style>` inline light).

## Sync de specs (deltas → main specs)

Los 4 dominios eran **capabilities nuevas** (no existía main spec previo en `openspec/specs/`), y sus deltas son specs completos (sin anotaciones "(Previously: …)"). Acción: **copia íntegra**.

| Delta (origen) | Main spec (destino) | Acción |
|---|---|---|
| `specs/permisos-edicion-usuarios-personas/spec.md` | `openspec/specs/permisos-edicion-usuarios-personas/spec.md` | Copiado íntegro (5 reqs PEU-001..005) |
| `specs/serial-unico-activo/spec.md` | `openspec/specs/serial-unico-activo/spec.md` | Copiado íntegro (5 reqs SUA-001..005) |
| `specs/campos-activo/spec.md` | `openspec/specs/campos-activo/spec.md` | Copiado íntegro (4 reqs CAM-001..004) |
| `specs/mantenimiento-activo/spec.md` | `openspec/specs/mantenimiento-activo/spec.md` | Copiado íntegro (6 reqs MTA-001..006) |

## Estado de la migración

**NO aplicada — pendiente de aplicación manual por el usuario.** `migration.sql` es idempotente (guards `sys.columns`/`sys.indexes`/`OBJECT_ID(...) IS NULL`), escrito contra el esquema real hosted `db_9c7990_servicedeskdesi`. Contiene: `ALTER TABLE Activo ADD SerieLocal`, índice filtrado `UX_Activo_EmpresaSerial`, tabla `Mantenimiento` (+ `FK_Mantenimiento_Activo` + `DF_Mantenimiento_Estatus`), y 4 SPs (`GuardarOActualizarActivo` rewrite, `GuardarMantenimiento`, `ObtenerMantenimientosPorActivo`, `ObtenerConteoPaginasPorRol`). `rollback.sql` en orden inverso con guards.

> Nota: si existen seriales duplicados legacy entre activos vigentes de la misma empresa, la creación del índice fallará; revisar/dedup antes de migrar (riesgo documentado en proposal/design).

## Veredicto de verificación

**PASS WITH WARNINGS** — 35/35 tareas `[x]`, build MSBuild con **0 errores** en los 3 proyectos, 20/20 requisitos y 26/26 escenarios cubiertos estáticamente, 7/7 decisiones de diseño seguidas. Sin issues CRÍTICOS.

### Warnings

- **W1 — `MvcBuildViews` desactivado.** La sintaxis Razor de las 5 vistas tocadas (`Active.cshtml`, `Users.cshtml`, `Persona.cshtml`, `Permisos.cshtml`, `_MantenimientoActivo.cshtml`) se validó solo de forma **estática/manual**; un error Razor solo aparecería en runtime. *Recomendación:* activar `MvcBuildViews` (Debug) o smoke-test de navegación tras aplicar la migración.
- **W2 — Endpoints de conteo sin `[Permiso]`.** Los 2 endpoints de `ConteoPaginasPorRol` no llevan `[Permiso]`, exponiendo `RolId/TotalPaginas` de todos los roles a cualquier usuario autenticado (la tabla `RolPaginaAccion` no tiene `EmpresaId`):
  1. `ServiceDeskDESIWebApi/Controllers/PermisosController.cs:114-119` — `GET api/Permisos/ConteoPaginasPorRol`
  2. `ServiceDeskDESIMVC/Controllers/SecurityController.cs:137-141` — `ConsultarConteoPaginasPorRol`
  
  *Recomendación:* añadir `[Permiso("Permisos","Leer")]` a ambos para cerrar la fuga de información cross-tenant (consistente con `ObtenerPermisosPorRol`, que hoy también carece de `[Permiso]`).

### Sugerencias (no bloqueantes)

- **S1 —** `CatalogsController.cs:970` `ObtenerMantenimientosPorActivo` (MVC) sin `[Permiso("Activos","Leer")]`; la defensa se delega al `[Permiso("Activos","Leer")]` del WebApi (`MantenimientoController.cs:29`). Añadir el atributo en MVC reforzaría defensa en profundidad (simetría con `GuardarMantenimiento`).
- **S2 —** Diferencia de segundos en `Fecha`: el modal muestra `@DateTime.Now` (servidor MVC) mientras la fecha persistida la asigna `GETDATE()` del SP. Coherente con el diseño (fecha auto visible); posible desfase de segundos entre lo mostrado y lo guardado.
- **S3 —** `PermisosController.ObtenerConteoPaginasPorRol` (WebApi) no deriva `usuario` (no lo necesita: el SP no es multi-tenant). Documentado; sin impacto.

## Archivos pre-existentes NO tocados (constraint respetado)

Los 4 archivos de una sesión previa **no fueron tocados por este change** (cambios ajenos de hosting/conexión, confirmado por `git diff`):

| Archivo | Cambio pre-existente (ajeno a este change) |
|---|---|
| `ServiceDeskDESIMVC/Views/Home/Configuration.cshtml` | Comenta `<small>` de vigencia |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.cs` | Cadena de conexión desde variable de entorno `sConSql` |
| `ServiceDeskDESIWebApi/Web.Debug.config` | Transform vacía `cCon` |
| `ServiceDeskDESIWebApi/Web.Release.config` | Transform vacía `cCon` |

## Next steps

1. **Aplicar `migration.sql`** contra la BD hosted (manual, `sqlcmd -C`), verificando `sys.columns`/`sys.indexes`/`sys.objects`.
2. **Resolver W2** (recomendado antes de cerrar): añadir `[Permiso("Permisos","Leer")]` a los 2 endpoints de `ConteoPaginasPorRol`.
3. **Smoke-test de vistas** (W1): navegar `Active.cshtml`, `Users.cshtml`, `Persona.cshtml`, `Permisos.cshtml`, abrir el modal de mantenimientos.
4. (Opcional) aplicar S1 (defensa en profundidad MVC) y evaluar S2/S3.

## Contenido del archivo

| Artefacto | Presente |
|-----------|----------|
| `proposal.md` | ✅ |
| `design.md` | ✅ |
| `explore.md` | ✅ |
| `tasks.md` (35/35 `[x]`) | ✅ |
| `verify-report.md` (PASS WITH WARNINGS) | ✅ |
| `apply-verification.md` (T34/T35) | ✅ |
| `migration.sql` | ✅ |
| `rollback.sql` | ✅ |
| `specs/permisos-edicion-usuarios-personas/spec.md` | ✅ |
| `specs/serial-unico-activo/spec.md` | ✅ |
| `specs/campos-activo/spec.md` | ✅ |
| `specs/mantenimiento-activo/spec.md` | ✅ |

## Notas de trazabilidad

- Origen: exploración del change (`explore.md`) + decisiones autoritativas de usuario (D1/D2 en proposal/design).
- Regla respetada: **sin flag en `Rol`** (PEU-001), **sin campo `Comentarios`** (CAM-004), sin cambios en `GuardarOActualizarRol`.
- Migración **no ejecutada** — este archive registra el estado pre-migración; la aplicación manual queda a cargo del usuario.

## Nota post-archivo (W2 resuelto)
El W2 (endpoints ConteoPaginasPorRol sin [Permiso]) fue resuelto el 2026-09-01 tras el archivo: se agreg� [Permiso("Permisos", "Leer")] a ServiceDeskDESIWebApi/Controllers/PermisosController.cs (ObtenerConteoPaginasPorRol) y a ServiceDeskDESIMVC/Controllers/SecurityController.cs (ConsultarConteoPaginasPorRol). Build verificado: 0 errores. Pendiente solo la migraci�n manual.
