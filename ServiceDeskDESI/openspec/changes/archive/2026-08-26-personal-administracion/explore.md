# Exploration: Renombrar menús "Personas" → "Personal" y "Usuarios" → "Administración" (sin romper permisos)

## Current State (verificado contra código real y BD en vivo)

### 1. Tabla `Pagina` — estructura y datos seed

**Estructura** (confirmada en dump `openspec/basededatosservicedesk.txt:273-291` y en vivo vía `INFORMATION_SCHEMA.COLUMNS`):

| Columna | Tipo | Nulos |
|---|---|---|
| Id | bigint IDENTITY | NO |
| Nombre | nvarchar(250) | YES |
| Descripcion | nvarchar(500) | YES |
| Tipo | nvarchar(50) | YES |
| Direccion | nvarchar(250) | YES |
| PermisosPadreId | bigint | YES |
| Logo | nvarchar(25) | YES |
| OrdenB | int | YES |
| Estatus | bit | YES |
| CreadoPor / FechaCreacion / ModificadoPor / FechaModificacion | (auditoría) | YES |

POCO: `ServiceDeskDESIEntities/Seguridad/Pagina.cs:5-14` → `Pagina : BaseObject` con `Nombre, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB`. **No existe** propiedad `NombreVisible`/`Etiqueta`.

**Seed (datos reales en la BD `db_9c7990_servicedeskdesi`)**: NO hay `INSERT INTO Pagina` en ningún `.sql` ni en el dump (`basededatosservicedesk.txt` solo trae schema + SPs, sin datos). El seed vive solo en la BD. Filas actuales (SELECT en vivo, ordenadas por `OrdenB`):

| Id | Nombre | Tipo | Direccion | PermisosPadreId | OrdenB | Estatus |
|---|---|---|---|---|---|---|
| 1 | Dashboard | Menu | /Home/Index | NULL | 1 | 1 |
| 2 | Tickets | Menu | /Ticket/Index | NULL | 2 | 1 |
| 3 | Catálogos | Menu | NULL | NULL | 3 | 1 |
| 7 | Áreas | SubMenu | /Catalogs/WorkArea | 3 | 1 | 1 |
| 8 | Sucursales | SubMenu | /Catalogs/Branch | 3 | 2 | 1 |
| 9 | Compañías | SubMenu | /Catalogs/Company | 3 | 3 | 1 |
| 10 | Categorías | SubMenu | /Catalogs/Category | 3 | 4 | 1 |
| 11 | Tipo Activo | SubMenu | /Catalogs/TypeActive | 3 | 5 | 1 |
| 12 | Marcas | SubMenu | /Catalogs/Mark | 3 | 6 | 1 |
| 13 | Modelos | SubMenu | /Catalogs/Model | 3 | 7 | 1 |
| 18 | Responsables por Categoría | SubMenu | /Catalogs/CategoriaResponsable | 3 | 8 | **0** |
| 19 | Puestos | SubMenu | /Catalogs/Puesto | 3 | 9 | 1 |
| **4** | **Usuarios** | Menu | /User/Users | NULL | **4** | 1 |
| 5 | Seguridad | Menu | NULL | NULL | 5 | 1 |
| 14 | Roles | SubMenu | /Security/Role | 5 | 1 | 1 |
| 15 | Permisos | SubMenu | /Security/Permisos | 5 | 2 | 1 |
| **20** | **Personas** | Menu | /Catalogs/Persona | NULL | **6** | 1 |
| 16 | Activos | Menu | /Catalogs/Active | NULL | 7 | 1 |
| 17 | Mi Perfil | Menu | /User/MyProfile | NULL | 8 | 1 |

- **"Personas" = Id 20** (Menu, `/Catalogs/Persona`). **"Usuarios" = Id 4** (Menu, `/User/Users`).
- **No existen typos** tipo `Tipped` ni `People` en la BD actual: todos los `Nombre` están limpios y con acentos correctos (`Áreas`, `Catálogos`, `Compañías`, `Categorías`, `Responsables por Categoría`). La única rareza es `Tipo Activo` (nombre con espacio) y la `Direccion` en inglés-ish (`TypeActive`, `Mark`, `Branch`), irrelevante para este cambio.
- Menús padre sin `Direccion` (`Catálogos` Id 3, `Seguridad` Id 5) NO tienen acciones `[Permiso]` asociadas.

### 2. Uso de `Pagina.Nombre` — dos buckets

#### Bucket DISPLAY (etiqueta visible)
- **`ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml`** — único render del menú de navegación:
  - `:23` `<span class="menu-text">@menu.Nombre</span>` (menú con hijos)
  - `:34` `<span class="menu-text">@sub.Nombre</span>` (submenú)
  - `:46` `<span class="menu-text">@menu.Nombre</span>` (menú hoja)
- **`ServiceDeskDESIMVC/Views/Security/Permisos.cshtml`** — chooser de asignación de permisos muestra `pagina.Nombre`:
  - `:416`, `:427`, `:437`, `:458` (`item-nombre` = `pagina.Nombre`), `:480`, `:517`, `:541`.
- Títulos/encabezados **hardcodeados** en vistas (no vienen de `Pagina.Nombre`):
  - `ServiceDeskDESIMVC/Views/User/Users.cshtml:3` `ViewBag.Title = "Usuarios"`; `:21` `Gestión de Usuarios`.
  - `ServiceDeskDESIMVC/Views/Catalogs/Persona.cshtml:21` `Catálogo de Personas`.

#### Bucket PERMISSION KEY (llave de permisos)
Cadena completa de resolución:
1. Atributos `[Permiso("Personas")]` / `[Permiso("Usuarios")]` (MVC y WebApi).
2. MVC `PermisosService.TienePermiso` → `HttpClientConnection.ValidarPermisoUsuario(nombrePagina, accion)` (`ServiceDeskDESIMVC/DAL/HttpClientConnection.Permisos.cs:20-29`, POST `api/Permisos/Validar`).
3. WebApi `PermisosController` → `PermisosService.ValidarPermisoUsuario(usuario, nombrePagina, accion)` (`ServiceDeskDESIWebApi/Services/PermisosService.cs:45-87`).
4. `DbWrapper.ObtenerPaginaPorNombre(nombrePagina)` → SP `ObtenerPaginaPorNombre` (**`basededatosservicedesk.txt:4337-4346`**): `WHERE Nombre = @Nombre` → obtiene `pagina.Id`.
5. `DbWrapper.ValidarPermisoUsuario(usuarioId, pagina.Id, accion)` → SP `ValidarPermisoUsuario` (**`basededatosservicedesk.txt:5186-5217`**): valida por `PaginaId` contra `RolPaginaAccion`.

⇒ **`Pagina.Nombre` ES la llave**: se compara en el SP `ObtenerPaginaPorNombre` (`WHERE Nombre = @Nombre`) y en las proyecciones `p.Nombre AS PaginaNombre`.

Proyecciones que exponen `Nombre` como key:
- SP `ObtenerPermisosPorUsuario` (`basededatosservicedesk.txt:4448-4478`): `p.Nombre AS PaginaNombre`.
- SP `ObtenerPermisosPorRol` (`basededatosservicedesk.txt:4413-4441`): `p.Nombre AS PaginaNombre`.
- (ver también `openspec/changes/tenant-estructural/migration-d1-sp-rewrite.sql:984-986`).

Comparaciones por key en servicios MVC (gating de botones/lectura):
- `ServiceDeskDESIMVC/Services/PersonaService.cs:50` → `p.PaginaNombre == "Personas"`.
- `ServiceDeskDESIMVC/Services/UsuarioService.cs:65` → `p.PaginaNombre == "Usuarios"`.
- (patrón idéntico en los otros 14 servicios: `CompaniaService:50`, `AreaService:51`, `ActivoService:50`, `CategoriaService:50`, `EmpresaService:60`, `MarcaService:50`, `ModeloService:50`, `SucursalService:50`, `PuestoService:50`, `TipoActivoService:50`, `TicketService:193`, `RolService:69`, `PermisosService:75`).

Atributos `[Permiso("...")]` con la llave "Personas" y "Usuarios" (todos los archivos:línea):

**"Usuarios"** (llave):
- `ServiceDeskDESIMVC/Controllers/UserController.cs:239` `[Permiso("Usuarios")]`, `:278` `[Permiso("Usuarios","Eliminar")]`.
- `ServiceDeskDESIWebApi/Controllers/AutenticationController.cs:56`, `:69`, `:96` (`Eliminar`), `:163`.

**"Personas"** (llave):
- `ServiceDeskDESIMVC/Controllers/CatalogsController.cs:782`, `:789` (`Eliminar`), `:813` (`Editar`), `:821` (`Editar`).
- `ServiceDeskDESIWebApi/Controllers/PersonaController.cs:55`, `:69` (`Eliminar`).
- `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs:29` (`Leer`), `:41` (`Leer`), `:53` (`Editar`), `:65` (`Editar`).

(Claves de otras páginas, para referencia de completitud: `Tickets`, `Áreas`, `Roles`, `Permisos`, `Sucursales`, `Compañías`, `Categorías`, `Tipo Activo`, `Marcas`, `Modelos`, `Activos`, `Mi Perfil`, `Responsables por Categoría`, `Puestos`. No hay `[Permiso("Dashboard")]`.)

### 3. Renderizado del menú (end-to-end)

1. `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml:52` → `$("#sidebar").empty().load("/Home/MenusUser", ...)`.
2. `ServiceDeskDESIMVC/Controllers/HomeController.cs:82-91` → `MenusUser()` llama `httpClientConnection.ObtenerPaginasPorUsuario()` y devuelve `PartialView(List<Pagina>)`.
3. `ServiceDeskDESIMVC/DAL/HttpClientConnection.Pagina.cs:15-18` → GET `api/Pagina/List`.
4. `ServiceDeskDESIWebApi/Controllers/PaginaController.cs:27-33` → `PaginaService.ObtenerPaginasPorUsuario(User.Identity.Name)`.
5. `ServiceDeskDESIWebApi/DAL/DbWrapper.Paginas.cs:13-39` → SP `ObtenerPaginasPorUsuario`.
6. SP `ObtenerPaginasPorUsuario` (`basededatosservicedesk.txt:4372-4402`): junta `Pagina` → `RolPaginaAccion` → `Rol` → `UsuarioRol` → `Usuarios` (`rpa.PuedeLeer=1`), devuelve `SELECT DISTINCT p.*` (todas las columnas, incl. `Nombre`).

**Respuesta**: SÍ, `MenusUser.cshtml` lee `Pagina.Nombre` directamente para el texto visible. **`UsuarioPagina` NO participa en el menú** (grep de `UsuarioPagina` en MVC = 0 resultados; solo se usa en WebApi: `DbWrapper.UsuarioPagina.cs`, `UsuarioPaginaController.cs`, y `EmpresaService.cs:553` para provisioning — pero no para renderizar menú).

### 4. Strings hardcodeados

- **"Personas"**: display `Views/Catalogs/Persona.cshtml:21`; llave `[Permiso(...)]` + `PersonaService.cs:50`.
- **"Usuarios"**: display `Views/User/Users.cshtml:3,21`; llave `[Permiso(...)]` + `UsuarioService.cs:65`.
- **"Personal"** y **"Administración"**: grep en todo el repo = **0 coincidencias**. Son etiquetas nuevas; no colisionan con nada.

### 5. Entities/DTOs que transportan el nombre al frontend

- `Pagina` (POCO): `Entities/Seguridad/Pagina.cs`. **No existe `PaginaDTO`** — el menú viaja como `List<Pagina>` serializada (JSON en `MenusUser.cshtml` y `Permisos.cshtml`).
- `PermisosViewModel`: `Entities/Seguridad/PermisosViewModel.cs:9-19` → `PaginaId, PaginaNombre, Direccion, PuedeLeer/Crear/Editar/Eliminar/Exportar`. `PaginaNombre` = key.
- `RolPaginaAccionDTO`: `Entities/Seguridad/RolPaginaAccionDTO.cs:5-9` → hereda `RolPaginaAccion` + `PaginaNombre` + `Direccion`.
- `UsuarioPagina`: `Entities/Catalogos/UsuarioPagina.cs:11-15` → `UsuarioId, PaginaId`. **No hay `UsuarioPaginaDTO`**. No transporta nombre.

### 6. Esquema existente — `NombreVisible`/`Etiqueta`

**No existe** en ninguna parte: grep `NombreVisible|Etiqueta` en todo el repo = 0 coincidencias; `INFORMATION_SCHEMA.COLUMNS` de `Pagina` confirma solo las 13 columnas listadas arriba. El campo deberá **crearse**.

## Key Findings (hallazgos clave)

1. `Pagina.Nombre` es a la vez **etiqueta** (solo `MenusUser.cshtml`) y **llave de permiso** (SP `ObtenerPaginaPorNombre` + `p.Nombre AS PaginaNombre` + strings `[Permiso]` + comparaciones `PaginaNombre ==`).
2. El seed del menú **NO está en control de versiones**; vive solo en la BD hosted. Cualquier cambio de datos requiere migración aplicada a la BD (como se hizo en cambios previos) y/o actualizar `openspec/basededatosservicedesk.txt`.
3. El menú usa `SELECT p.*` → añadir una columna a `Pagina` + una propiedad homónima al POCO fluye automáticamente por `LlenarEntidad<T>` (`DbWrapper.cs:28-61`, mapeo case-insensitive por nombre de columna↔propiedad).
4. `UsuarioPagina` es legacy para el menú (no se usa en MVC).
5. Los títulos de página ("Gestión de Usuarios", "Catálogo de Personas") están hardcodeados y NO cambiarán con solo renombrar el menú; si se quiere consistencia, son puntos adicionales (fuera del alcance mínimo).
6. `EmpresaService` (provisioning, líneas 500 y 545) itera páginas por **Id** (no por Nombre) → inmune al renombre.

## Approaches

| Opción | Descripción | Pros | Contras | Esfuerzo |
|---|---|---|---|---|
| **A. Columna `NombreVisible`/`Etiqueta` (RECOMENDADA)** | Añadir columna nullable a `Pagina`, backfill `= Nombre`; propiedad en POCO `Pagina`; render en `MenusUser.cshtml` usa `@(menu.NombreVisible ?? menu.Nombre)`; UPDATE de 2 filas: Usuarios→"Administración", Personas→"Personal". | No toca la llave de permisos; `Nombre` queda estable; cambio de datos aislado; reversible; `SELECT *` lo propaga solo. | Requiere migración SQL + 1 propiedad POCO + 1 ajuste de vista. | Baja |
| **B. Renombrar `Nombre` directo** | `UPDATE Pagina SET Nombre='Personal' WHERE Id=20` etc. | Mínimo cambio de datos. | **Rompe permisos**: hay que actualizar TODOS los `[Permiso("Personas"/"Usuarios")]`, comparaciones `PaginaNombre ==`, y el SP `ObtenerPaginaPorNombre`/proyecciones dejan de matchear; efecto dominó en ~30 puntos de código. | Alta |
| **C. Mapeo hardcodeado en vista/JS** | Mantener `Nombre` y mapear en el frontend (`if Nombre=='Personas' mostrar 'Personal'`). | Sin cambio de BD. | Frágil, duplica lógica, no escala a futuros renombres; ensucia la vista. | Media |

## Recommendation

**Opción A.** Añadir columna `NombreVisible` (o `Etiqueta`) `nvarchar(250) NULL` a `Pagina`, con backfill `NombreVisible = Nombre` para todas las filas, y luego `UPDATE` de las 2 filas objetivo. Añadir propiedad `NombreVisible` al POCO `Pagina` (nullable `string`). Modificar `MenusUser.cshtml` para renderizar `@(menu.NombreVisible ?? menu.Nombre)` (y lo mismo para submenús). No tocar: SPs de permisos, `[Permiso(...)]`, comparaciones `PaginaNombre ==`, ni `ObtenerPaginaPorNombre`.

Justificación: separa limpiamente la **etiqueta** de la **llave** (que es el objetivo acordado), es de bajo riesgo, mantiene `Nombre` como llave estable, y aprovecha que `ObtenerPaginasPorUsuario`/`ObtenerPaginas`/`ObtenerPaginaPorNombre` usan `SELECT p.*` + mapeo por reflexión (la columna nueva fluye sin tocar DAL).

Notas de implementación (para design/apply):
- El chooser de `Permisos.cshtml` seguirá mostrando `pagina.Nombre` (la llave "Personas"/"Usuarios"). Decidir si también se muestra `NombreVisible` ahí (cosmético; los ids no cambian, es seguro). Si se desea, usar la misma lógica de fallback.
- Los títulos hardcodeados (`Users.cshtml`, `Persona.cshtml`) son un cambio independiente opcional para coherencia visual.
- Nombre de columna ASCII (`NombreVisible`/`Etiqueta`) para evitar problemas de acentos en identificadores.
- Migración SQL debe ser idempotente (`IF COL_LENGTH(...) IS NULL ALTER TABLE ... ADD`).

## Risks

- **Seed fuera de repo**: la migración debe aplicarse a la BD hosted (SQL5105.site4now.net) y, opcionalmente, reflejarse en `openspec/basededatosservicedesk.txt` (dump) para no generar drift.
- **Doble fuente de verdad temporal**: hasta aplicar el backfill, si alguna fila queda con `NombreVisible = NULL`, el fallback `?? Nombre` en la vista lo cubre (mitigación).
- **No romper `ObtenerPaginaPorNombre`**: mantener `Nombre` intacto garantiza que `[Permiso]` y `ValidarPermisoUsuario` sigan funcionando. Si por error se cambia `Nombre`, los permisos de esas 2 páginas caen silenciosamente (403).
- **Alcance de "Administración"**: no existe un menú "Administración" hoy; "Usuarios" es un Menu de nivel superior. El renombre solo cambia el texto, no la estructura ni el orden (OrdenB=4).

## Ready for Proposal

Sí. Recomiendo pasar a **sdd-propose** con la Opción A, dejando explícito en el proposal: (1) qué columna se crea y su backfill, (2) las 2 filas a actualizar, (3) el único punto de vista a tocar (`MenusUser.cshtml`), (4) si se incluyen o no los títulos hardcodeados y el chooser de permisos como alcance adicional, y (5) el plan de migración/rollback a la BD hosted.
