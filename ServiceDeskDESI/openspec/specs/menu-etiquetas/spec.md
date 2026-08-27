# menu-etiquetas Specification

## Purpose

Separar la **etiqueta visible** del menú de la **llave de permisos** (hoy ambas en `Pagina.Nombre`). Añade `Pagina.NombreVisible` (nullable) con fallback a `Nombre`, y renombra en pantalla "Personas"→"Personal" y "Usuarios"→"Administración" sin tocar la resolución de permisos.

## Requirements

### Requirement: MEN-001 — Separación etiqueta vs llave

El sistema MUST agregar `Pagina.NombreVisible` (`nvarchar(250)`, nullable) como etiqueta visible independiente de `Pagina.Nombre`. Cuando `NombreVisible` es `NULL`, la etiqueta efectiva MUST resolverse al valor de `Nombre`. `Nombre` MUST permanecer inmutable como llave de permisos.

#### Scenario: Etiqueta explícita

- GIVEN una fila `Pagina` con `Nombre='Personas'` y `NombreVisible='Personal'`
- WHEN se resuelve la etiqueta visible
- THEN se devuelve `Personal`

#### Scenario: Fallback por NULL

- GIVEN una fila con `Nombre='Áreas'` y `NombreVisible=NULL`
- WHEN se resuelve la etiqueta visible
- THEN se devuelve `Áreas`

### Requirement: MEN-002 — Render del menú con fallback

El menú de navegación (`Views/Home/MenusUser.cshtml`, único render) MUST mostrar `NombreVisible ?? Nombre` para cada ítem y subítem, sin alterar la resolución de permisos.

#### Scenario: Menú muestra la etiqueta visible

- GIVEN un usuario autenticado con páginas que incluyen etiquetas visibles
- WHEN se renderiza el menú
- THEN cada ítem muestra `NombreVisible ?? Nombre`

#### Scenario: Render no afecta permisos

- GIVEN un ítem con `NombreVisible` distinto de `Nombre`
- WHEN se valida el permiso de esa página
- THEN la validación usa `Nombre`, no `NombreVisible`

### Requirement: MEN-003 — Renombre de 2 ítems

El sistema MUST mostrar "Personas" (Id 20) como "Personal" y "Usuarios" (Id 4) como "Administración" vía `NombreVisible`, y SHOULD dejar el resto de ítems sin cambios.

#### Scenario: Renombre en pantalla

- GIVEN las filas Id 20 e Id 4 con `NombreVisible='Personal'` y `'Administración'`
- WHEN se renderiza el menú
- THEN se muestran "Personal" y "Administración"; `Nombre` sigue siendo "Personas"/"Usuarios"

#### Scenario: Resto de ítems sin cambios

- GIVEN una fila distinta de Id 4/20 (p.ej. "Áreas")
- WHEN se renderiza el menú
- THEN su etiqueta visible es igual a `Nombre`

### Requirement: MEN-004 — Migración idempotente y rollback

La migración MUST (a) añadir `NombreVisible` solo si no existe, (b) hacer backfill `NombreVisible = Nombre`, y (c) actualizar Id 4→'Administración' e Id 20→'Personal'. MUST ser re-ejecutable sin error y MUST incluir rollback en orden inverso.

#### Scenario: Aplicación inicial

- GIVEN `Pagina` sin la columna `NombreVisible`
- WHEN se aplica la migración
- THEN existe la columna, el backfill está aplicado y las 2 filas están actualizadas

#### Scenario: Re-ejecución idempotente

- GIVEN la migración ya aplicada
- WHEN se re-ejecuta
- THEN no falla ni duplica la columna; el resultado es idéntico

#### Scenario: Rollback

- GIVEN la migración aplicada
- WHEN se ejecuta el rollback
- THEN "Personal"/"Administración" revierten a "Personas"/"Usuarios" (o se elimina la columna)

### Requirement: MEN-005 — No-regresión de permisos

Los permisos existentes (`RolPaginaAccion`, `[Permiso("...")]`, `ObtenerPaginaPorNombre`, comparaciones `PaginaNombre ==`) MUST seguir resolviendo por `Nombre` sin cambios de comportamiento.

#### Scenario: Acceso tras el renombre

- GIVEN un usuario con permisos sobre "Personas"/"Usuarios"
- WHEN accede después del renombre
- THEN `ObtenerPaginaPorNombre` y `ValidarPermisoUsuario` resuelven por `Nombre` y el acceso no cambia

#### Scenario: Chooser de permisos intacto

- GIVEN la vista `Permisos.cshtml`
- WHEN se muestra el chooser
- THEN sigue mostrando `pagina.Nombre` (la llave), sin usar `NombreVisible`
