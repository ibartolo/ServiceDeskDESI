# Proposal: Renombrar menús "Personas" → "Personal" y "Usuarios" → "Administración"

- **Change**: `personal-administracion`
- **Fase**: propose
- **Fecha**: 2026-08-26
- **Propuesta padre**: SDD roadmap — cambio #2 de 3
- **Origen**: `openspec/changes/personal-administracion/explore.md`

## Intent

Separar la **etiqueta visible** del menú de la **llave de permisos**, hoy ambas en `Pagina.Nombre`. Renombrar en pantalla "Personas"→"Personal" (Id 20) y "Usuarios"→"Administración" (Id 4) SIN romper el sistema de permisos, que usa `Nombre` como llave en SPs (`ObtenerPaginaPorNombre`), `[Permiso(...)]` y comparaciones `PaginaNombre ==` en los servicios.

## Scope

### In Scope

- Añadir columna `NombreVisible` (`nvarchar(250) NULL`) a `Pagina`, con backfill `= Nombre`.
- Propiedad `NombreVisible` (nullable `string`) en el POCO `Pagina` (no existe `PaginaDTO`; el menú viaja como `List<Pagina>`).
- Render `@(x.NombreVisible ?? x.Nombre)` en `Views/Home/MenusUser.cshtml` (único render del menú, líneas 23/34/46).
- Migración SQL idempotente + rollback; `UPDATE` de 2 filas: Id 4 → `Administración`, Id 20 → `Personal`.

### Out of Scope

- Títulos hardcodeados ("Gestión de Usuarios" `Users.cshtml:21`, "Catálogo de Personas" `Persona.cshtml:21`).
- Chooser de permisos `Permisos.cshtml` (sigue mostrando `pagina.Nombre` = llave).
- Breadcrumbs/páginas; SPs de permisos, `[Permiso(...)]`, comparaciones `PaginaNombre ==` y `ObtenerPaginaPorNombre` (NO cambian).

## Capabilities

### New Capabilities

- `menu-etiquetas`: render de etiquetas visibles del menú independientes de la llave de permisos, con fallback `NombreVisible ?? Nombre`.

### Modified Capabilities

- None.

## Approach

**Opción A** (recomendada en explore): la columna `NombreVisible` separa etiqueta de llave. `Nombre` queda intacto como llave estable; no se tocan SPs ni `[Permiso]`. `ObtenerPaginasPorUsuario` usa `SELECT p.*` y `LlenarEntidad<T>` mapea por nombre de columna↔propiedad, por lo que la columna nueva fluye sin tocar el DAL.

Migración idempotente:

```sql
IF COL_LENGTH('Pagina','NombreVisible') IS NULL
  ALTER TABLE Pagina ADD NombreVisible nvarchar(250) NULL;
UPDATE Pagina SET NombreVisible = Nombre WHERE NombreVisible IS NULL;
UPDATE Pagina SET NombreVisible = 'Administración' WHERE Id = 4;
UPDATE Pagina SET NombreVisible = 'Personal'       WHERE Id = 20;
```

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| BD `Pagina` (hosted `db_9c7990_servicedeskdesi`) | Modificado | +columna `NombreVisible`, backfill, 2 `UPDATE` |
| `ServiceDeskDESIEntities/Seguridad/Pagina.cs` | Modificado | +propiedad `NombreVisible` |
| `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` | Modificado | `@(x.NombreVisible ?? x.Nombre)` |
| `openspec/basededatosservicedesk.txt` | Opcional | Reflejar columna (evitar drift) |

## Riesgos

| Riesgo | Probabilidad | Mitigación |
|---|---|---|
| Seed fuera de repo → migración no aplicada a la BD hosted | Media | Script idempotente entregado para SQL5105; fallback `?? Nombre` cubre `NULL` |
| Cambiar `Nombre` por error rompe permisos (403 silencioso) | Baja | Solo se toca `NombreVisible`; `Nombre` permanece intocable |
| Drift entre dump y BD | Baja | Actualizar `basededatosservicedesk.txt` |

## Rollback Plan

1. Datos: `UPDATE Pagina SET NombreVisible = 'Usuarios' WHERE Id = 4; UPDATE Pagina SET NombreVisible = 'Personas' WHERE Id = 20;` (o `ALTER TABLE Pagina DROP COLUMN NombreVisible`).
2. Código: revertir `MenusUser.cshtml` a `@x.Nombre` y quitar la propiedad `NombreVisible` del POCO.

## Success Criteria

- [ ] El menú muestra "Personal" y "Administración"; el resto de menús sin cambios.
- [ ] Los permisos de ambas páginas siguen funcionando (login, botones, `ObtenerPaginaPorNombre`).
- [ ] Filas con `NombreVisible = NULL` muestran `Nombre` (fallback).
- [ ] Migración re-ejecutable sin error (idempotente); rollback restaura las etiquetas previas.

## Preguntas abiertas

- Ninguna. (Mostrar también `NombreVisible` en `Permisos.cshtml` se descartó: queda fuera de alcance.)
