# Archive Report — personal-administracion

**Change**: `personal-administracion`
**Archived on**: 2026-08-26
**Archive location**: `openspec/changes/archive/2026-08-26-personal-administracion/`
**Artifact store**: openspec (file-based)
**Veredicto**: **PASS WITH WARNINGS** (sin issues bloqueantes)

---

## What was delivered

Separación de la **etiqueta visible** del menú de la **llave de permisos** (hoy ambas en `Pagina.Nombre`). Se añade `Pagina.NombreVisible` (nullable) con fallback `?? Nombre`, y se renombran en pantalla "Personas"→"Personal" (Id 20) y "Usuarios"→"Administración" (Id 4) **sin tocar** la resolución de permisos.

### Archivos entregados

- **`migration.sql`** — Migración idempotente: `IF COL_LENGTH('dbo.Pagina','NombreVisible') IS NULL` → `ALTER TABLE ... ADD [NombreVisible] NVARCHAR(250) NULL`; backfill `UPDATE ... SET NombreVisible = Nombre WHERE NombreVisible IS NULL`; y 2 `UPDATE` por llave `Nombre` (`'Personas'`→`'Personal'`, `'Usuarios'`→`'Administración'`).
- **`rollback.sql`** — Orden inverso conservador: revierte los 2 valores (`'Personal'`→`'Personas'`, `'Administración'`→`'Usuarios'`); `DROP COLUMN` dejado comentado (opcional).
- **`ServiceDeskDESIEntities/Seguridad/Pagina.cs`** — `+ public string NombreVisible { get; set; }` (después de `Nombre`). Sin DTO nuevo y sin cambio de `.csproj` (archivo ya incluido).
- **`ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml`** — líneas 23/34/46: `@(menu.NombreVisible ?? menu.Nombre)` / `@(sub.NombreVisible ?? sub.Nombre)` dentro del `<span class="menu-text">`.

### Cambios en BD

- **Columna** `Pagina.NombreVisible NVARCHAR(250) NULL` creada en la BD hosted `db_9c7990_servicedeskdesi` @ `SQL5105.site4now.net`.
- **Backfill** `= Nombre` aplicado a **19 filas**; `Usuarios` (Id 4) → `'Administración'`, `Personas` (Id 20) → `'Personal'`.
- **Sin cambios** en SPs (`ObtenerPaginaPorNombre`, `ObtenerPaginasPorUsuario`, `ValidarPermisoUsuario`), atributos `[Permiso(...)]`, comparaciones `PaginaNombre ==`, ni `Permisos.cshtml` (chooser). `Nombre` queda inmutable como llave de permisos.

### Desviación documentada (segura)

Los `UPDATE` usan `WHERE Nombre = 'Usuarios'/'Personas'` en lugar de `WHERE Id = 4/20` como describían `design.md`/`tasks.md`. Es **funcionalmente equivalente** y más robusto a re-seeds (explore.md confirma Id 4 = 'Usuarios', Id 20 = 'Personas'). Registrada en verify-report.md (WARNING #2).

## Spec sync location

- Delta spec `openspec/changes/personal-administracion/specs/menu-etiquetas/spec.md` → **main spec** `openspec/specs/menu-etiquetas/spec.md` (creado; no existía main spec previo para este dominio, por lo que el delta se copió íntegro, preservando formato — misma convención que `foliador-tickets`).
- 5 requirements (MEN-001 … MEN-005), 11 escenarios, sin merge sobre spec existente.

## DB migration status

**APLICADA (hosted DB `db_9c7990_servicedeskdesi`) — ya ejecutada.** `migration.sql` se aplicó manualmente vía `sqlcmd -C` (no hay runner de migraciones). Verificada: 19 filas backfilleadas, Id 4 → `'Administración'`, Id 20 → `'Personal'`. Idempotente (`COL_LENGTH` guard + backfill `WHERE NombreVisible IS NULL`). Rollback disponible en `rollback.sql` (orden inverso, conservador).

## Follow-ups (pendientes — NO bloqueantes)

- **T8** — Smoke E2E: login → sidebar verifica que Id 20 muestra "Personal" e Id 4 "Administración"; ítems con `NombreVisible = NULL` muestran `Nombre` (fallback). ⏳ *Diferido a validación manual del usuario.*
- **T9** — Regresión de permisos: acceder a `/User/Users` y `/Catalogs/Persona` sin 403 (`ObtenerPaginaPorNombre`/`ValidarPermisoUsuario` resuelven por `Nombre`). ⏳ *Diferido a validación manual.*
- **T10** — Verificar que `Permisos.cshtml` (chooser) sigue mostrando `pagina.Nombre` (la llave). Verificado estático ✅; runtime pendiente. ⏳
- **T11** (opcional) — Actualizar `openspec/basededatosservicedesk.txt`: reflejar `NombreVisible` en `CREATE TABLE [dbo].[Pagina]` (línea ~273) para evitar drift del dump.

### Sugerencias menores (no bloqueantes)

- **Bookkeeping**: `tasks.md` muestra T7 como `[ ]` aunque la migración ya fue aplicada y verificada en BD; debería marcarse `[x]`.
- **Alinear docs**: `design.md` y `tasks.md` aún describen `WHERE Id = 4/20`; la implementación real usa `WHERE Nombre` (desviación documentada y segura).
- **Rollback total**: el `DROP COLUMN` queda comentado en `rollback.sql` (pregunta abierta del design); el path conservador actual es válido.

## Verdict

**ARCHIVADO — PASS WITH WARNINGS (sin issues bloqueantes).**

Código y SQL completos y correctos (T1–T6), migración idempotente aplicada a BD, build limpio (0 errores en 3 proyectos), y verificación estática que confirma MEN-001..005 y D1..D6 sin regresión de permisos. Los pendientes son documentación (T7 sin marcar, dump T11, alinear desviación Nombre-vs-Id) y smoke E2E manual (T8–T10), todos diferidos al usuario; ninguno es defecto de código ni bloquea el cierre. Cambio **#2 de 3** del roadmap SDD.
