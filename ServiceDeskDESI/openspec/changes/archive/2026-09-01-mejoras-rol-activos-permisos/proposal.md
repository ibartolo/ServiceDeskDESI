# Proposal: Mejoras de Roles, Activos y Permisos

- **Change**: `mejoras-rol-activos-permisos`
- **Fase**: propose
- **Fecha**: 2026-08-31
- **Origen**: `explore.md` del change + decisiones de usuario (D1/D2 autoritativas).

## Resumen ejecutivo

Siete mejoras sobre tres módulos ya desplegados: (1) restricción de edición de Usuarios/Personas, (2) unicidad de No. de Serie en Activos por empresa, (3) nuevo campo `SerieLocal`, (4) gestor de mantenimientos de Activos, (5) convertir `Notas` a textarea de 250, (6) bug del contador de páginas asignadas en Permisos y (7) bug de tema oscuro en el chooser de permisos.

**Decisión central (ítem 1):** NO se agrega flag a Rol. Se reutiliza la acción "Editar" del sistema de Permisos existente. La app ya aplica `[Permiso("Personas", "Editar")]` y `[Permiso("Usuarios")]` (auto-resuelve "Crear"/"Editar" por el `Id` del modelo). El único hueco real es de UI: `Users.cshtml` no recibe `ViewBag.Permisos` ni bloquea inputs. Un flag en Rol sería un segundo mecanismo paralelo e inconsistente con `RolPaginaAccion` + `[Permiso]` + SP `ValidarPermisoUsuario`.

**Decisión central (ítem 4):** modal = captura + histórico. Fecha = auto (hoy, `GETDATE()`), no capturable por el usuario, **pero visible en el modal en un input deshabilitado (solo lectura)** para que el usuario sepa la fecha que quedará registrada.

## Alcance

### In Scope
1. Bloqueo de inputs en modo edición de Usuarios/Personas vía acción "Editar" del sistema de permisos (sin flag en Rol).
2. Unicidad de `Serial` **por empresa**, solo activos vigentes (`Estatus = 1`) y seriales no nulos.
3. Campo `SerieLocal` (texto libre, no único) en Activos.
4. Gestor de mantenimientos de Activos (tabla + SPs + modal captura/histórico).
5. Convertir `Notas` existente a `<textarea>` con `maxlength = 250`.
6. Fix del contador de páginas asignadas en `Permisos.cshtml`.
7. Fix de tema oscuro del chooser de permisos.

### Out of Scope
- **NO** se agrega flag `PuedeModificarUsuariosPersonas` en Rol (ítem 1 resuelto con Permisos).
- **NO** se crea campo `Comentarios` (ítem 5 resuelto reutilizando `Notas`).
- **NO** se reimplementa el CRUD núcleo de Activos/Usuarios/Personas ya desplegado.
- **NO** se hace `Serial` obligatorio (los seriales nulos quedan permitidos y no únicos).
- Ítem 7 cubre **solo** el chooser de Permisos; otros `<style>` inline (p. ej. `.is-invalid-dropdown` de `Active.cshtml`) quedan fuera.

## Decisiones resueltas (checkpoint usuario — autoritativas)

1. **Ítem 1 = enfoque Permisos (b), no flag (a).** El gate lo gobierna el rol del **usuario logueado** (sesión `tokenCookie.UserID`) a través de la acción "Editar" de la página "Usuarios"/"Personas". En modo edición (`Id > 0`) los inputs se deshabilitan si `!permisos.PuedeEditar`; los registros nuevos (`Id == 0`) se permiten sujetos a `permisos.PuedeCrear` (patrón idéntico al guardado de todos los catálogos).
2. **Ítem 2 = unicidad por empresa + soft-delete libera serial.** Índice único filtrado `(EmpresaId, Serial) WHERE Serial IS NOT NULL AND Estatus = 1`; el SP valida duplicado (activos vigentes, misma empresa, excluyendo `Id`) y devuelve `-2` para mensaje amigable.
3. **Ítem 4 = Fecha auto pero visible.** `Fecha = GETDATE()` al registrar; el modal muestra el campo Fecha en un input `disabled`/read-only con la fecha actual (del sistema), para que el usuario vea qué fecha quedará registrada. Sin campo de fecha editable (evita validación de fechas, consistente con bitácora "qué se hizo hoy"). Columna queda `DATETIME NOT NULL`; si luego se pide fecha editable es cambio menor.
4. **Ítem 5 = no hay `Comentarios`.** `Notas` ya es `NVARCHAR(250)` en BD → solo cambio de vista a textarea.

## Enfoque técnico por punto

1. **Bloqueo edición Usuarios/Personas.** MVC: `UserController.Users()` puebla `ViewBag.Permisos` (nuevo `ObtenerPermisosParaUsuarios()` en `UsuarioService`, espejo de `ObtenerPermisosParaPersona`); `Users.cshtml` adopta el patrón estándar (`var permisos = ViewBag.Permisos as PermisosViewModel` + gate del botón + `disabled` en modo edición sin `PuedeEditar`). `Persona.cshtml`: extender condición de `disabled` de `estaVinculada` a `estaVinculada || (Model.Id > 0 && !permisos.PuedeEditar)`. **Server-side ya cubierto** por `[Permiso("Usuarios")]` / `[Permiso("Personas", "Editar")]` — no se toca. Sin cambios en entidad `Rol`, SP `GuardarOActualizarRol` ni BD.
2. **Serial único por empresa.** BD: índice único filtrado. SP `GuardarOActualizarActivo`: chequeo de duplicado → retorno `-2`. WebApi `DbWrapper.Activo.cs` mapea `-2` → mensaje "Ya existe un activo con ese No. de Serie". Vista `Active.cshtml` muestra el mensaje (ya lo hace vía Swal).
3. **SerieLocal.** `Activo.cs` (+ DTO hereda), `Active.cshtml` (textbox + `SerieLocal: $("#SerieLocal").val()`), `DbWrapper.Activo.cs` (`a.SerieLocal`), SP `GuardarOActualizarActivo` (`@SerieLocal NVARCHAR(100)`), `ALTER TABLE Activo ADD SerieLocal NVARCHAR(100) NULL`. Lectura auto vía `a.*`.
4. **Mantenimientos.** Nueva entidad `Mantenimiento` (patrón `PersonaActivo`): tabla + SPs `ObtenerMantenimientosPorActivo` / `GuardarMantenimiento`; partial `_MantenimientoActivo.cshtml` (modal `modalMantenimientoActivo`) + botón por fila en `Active.cshtml`; cadena completa MVC→HttpClient→WebApi→DbWrapper→SP. En el modal: campo Fecha visible en input deshabilitado (fecha actual del sistema) + textarea de comentario + historial cargado del SP.
5. **Notas.** En `Active.cshtml`: reemplazar `@Html.TextBoxFor(x => x.Notas)` por `@Html.TextAreaFor(x => x.Notas, ...)` + `maxlength: 250` en `jquery.validate`. Sin cambio de BD (ya 250).
6. **Contador páginas.** Nuevo SP `ObtenerConteoPaginasPorRol` (una query agrupada `SELECT RolId, COUNT(*) FROM RolPaginaAccion WHERE Estatus = 1 GROUP BY RolId` — sin N+1). MVC `SecurityController` expone endpoint; `Permisos.cshtml` lo consume en `CargarRoles()`, guarda mapa `conteoByRol` y `ActualizarBadges()` lee del mapa (para el rol seleccionado usa `paginasByRol.asignadas.length` en vivo).
7. **Tema oscuro chooser.** Añadir en `TemplatePage.css` reglas `body.dark-theme` para `.chooser-column`, `.chooser-item`, `.item-nombre`, `.item-direccion`, `.empty-message`, `.text-muted-small`, `.badge-paginas`, `.chooser-item.disponible/.asignada` (bordes) e inputs `.permisos-checkboxes`. Se conserva el `<style>` inline (light) y se sobreescribe en dark.

## Cambios de BD

```sql
-- 3. SerieLocal
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Activo') AND name = N'SerieLocal')
    ALTER TABLE dbo.Activo ADD SerieLocal NVARCHAR(100) NULL;

-- 2. Índice único filtrado por empresa (seriales no nulos, activos vigentes)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_Activo_EmpresaSerial' AND object_id = OBJECT_ID(N'dbo.Activo'))
    CREATE UNIQUE INDEX UX_Activo_EmpresaSerial ON dbo.Activo (EmpresaId, Serial)
        WHERE Serial IS NOT NULL AND Estatus = 1;

-- 4. Tabla Mantenimiento (patrón PersonaActivo + EmpresaId tenant)
IF OBJECT_ID(N'dbo.Mantenimiento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Mantenimiento (
        Id               BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        ActivoId         BIGINT NOT NULL,
        Comentario       NVARCHAR(500) NOT NULL,
        Fecha            DATETIME NOT NULL,        -- auto GETDATE()
        CreadoPor        NVARCHAR(25) NOT NULL,
        FechaCreacion    DATETIME NOT NULL,
        ModificadoPor    NVARCHAR(25) NULL,
        FechaModificacion DATETIME NULL,
        Estatus          BIT NOT NULL CONSTRAINT DF_Mantenimiento_Estatus DEFAULT (1),
        EmpresaId        BIGINT NOT NULL,
        CONSTRAINT FK_Mantenimiento_Activo FOREIGN KEY (ActivoId) REFERENCES dbo.Activo (Id)
    );
END
```

SPs nuevos: `GuardarMantenimiento` (`Fecha = GETDATE()`, `Estatus = 1`), `ObtenerMantenimientosPorActivo` (`WHERE ActivoId = @ActivoId AND Estatus = 1 AND Fecha IS NOT NULL ORDER BY Fecha DESC`), `ObtenerConteoPaginasPorRol`. `GuardarOActualizarActivo` se reescribe para validar serial duplicado (`-2`). Sin cambios en `Rol`.

## Migración / Rollback

- `migration.sql` (idempotente, patrón `asignacion-activos`): ALTER/CREATE INDEX/CREATE TABLE/SPs con guards `IF NOT EXISTS`. Escrito contra el **esquema real hosted** (el dump `basededatosservicedesk.txt` está desactualizado; `Activo`/`Rol` ya tienen `EmpresaId`).
- `rollback.sql`: `DROP INDEX UX_Activo_EmpresaSerial`; `DROP COLUMN SerieLocal`; `DROP TABLE Mantenimiento`; `DROP PROCEDURE` de los 3 SPs nuevos; restaurar definición previa de `GuardarOActualizarActivo`.
- Código: revertir entidades/vistas/services; el resto de módulos queda intacto (sin cambios destructivos sobre datos).

## Riesgos y supuestos

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Dump `basededatosservicedesk.txt` desactualizado (falta `EmpresaId`) | Alta | Migración contra esquema real hosted; guard `IF NOT EXISTS` en `sys.columns`/`sys.indexes` |
| Índice único filtrado falla por seriales duplicados existentes (datos legacy) | Media | Pre-paso de dedup/revisión en migración; SP devuelve `-2` antes de fallar por índice |
| `-2` colisiona con otros códigos de `GuardarOActualizarActivo` | Media | Rama explícita `-2` antes de los demás; mapeo en `DbWrapper.Activo.cs` |
| `Users.cshtml` sin `ViewBag.Permisos` exige cambio mayor | Media | Replicar patrón exacto del resto de catálogos (gate + disabled) |
| Ítem 7 deja otros `<style>` inline sin dark-theme | Baja | Fuera de alcance explícito; documentado |
| N+1 en contador de páginas | Media | SP agrupado único (una sola query), no consultas por rol |
| Multi-tenant: nueva tabla/columna deben respetar `EmpresaId` | Baja | `Mantenimiento.EmpresaId` + filtro por empresa en SPs |

**Supuestos:** el rol del usuario logueado (no el del usuario editado) gobierna el bloqueo; "Usuarios" y "Personas" ya son páginas del sistema de permisos con acción "Editar" definida en el chooser.

## Capabilities

### New Capabilities
- `mantenimiento-activo`: registro y consulta (histórico) de mantenimientos por activo vía modal.
- `serial-unico-activo`: unicidad de `Serial` por empresa (índice filtrado + validación en SP).
- `campos-activo`: campo `SerieLocal` y `Notas` como textarea (250) en el catálogo de Activos.
- `permisos-edicion-usuarios-personas`: bloqueo de edición de Usuarios/Personas vía acción "Editar" del sistema de permisos.

### Modified Capabilities
- None (`openspec/specs/` no tiene specs de activos/roles/permisos).

## Affected Areas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `openspec/changes/mejoras-rol-activos-permisos/migration.sql` + `rollback.sql` | Nuevo | ALTER/INDEX/TABLE/SPs |
| `ServiceDeskDESIEntities/Catalogos/Activo.cs` | Mod | `SerieLocal` |
| `ServiceDeskDESIEntities/Catalogos/Mantenimiento.cs` | Nuevo | Entidad mantenimiento |
| `ServiceDeskDESIMVC/Views/Catalogs/Active.cshtml` | Mod | `SerieLocal`, `Notas` textarea, botón Mantenimientos |
| `ServiceDeskDESIMVC/Views/Catalogs/_MantenimientoActivo.cshtml` | Nuevo | Modal captura + histórico |
| `ServiceDeskDESIMVC/Views/User/Users.cshtml` + `Controllers/UserController.cs` | Mod | `ViewBag.Permisos` + bloqueo edición |
| `ServiceDeskDESIMVC/Views/Catalogs/Persona.cshtml` | Mod | Bloqueo por `!PuedeEditar` |
| `ServiceDeskDESIMVC/Views/Security/Permisos.cshtml` | Mod | Contador real + (conserva inline style) |
| `ServiceDeskDESIMVC/CSS/Comun/TemplatePage.css` | Mod | Overrides `body.dark-theme` chooser |
| `ServiceDeskDESIMVC/Controllers/SecurityController.cs` + `DAL/HttpClientConnection.Permisos.cs` | Mod | Endpoint conteo páginas |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Activo.cs` | Mod | `SerieLocal` + mapeo `-2` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Mantenimiento.cs` (nuevo) + `.Permisos.cs` (mod) | Nuevo/Mod | SPs mantenimiento + conteo |
| `ServiceDeskDESIWebApi/Services` + `Controllers` (Activo mod, Mantenimiento nuevo) | Mod/Nuevo | Validaciones + rutas |

## Success Criteria

- [ ] Sin flag en Rol; la edición de Usuarios/Personas se bloquea vía `PuedeEditar` (inputs deshabilitados en edición; creación permitida con `PuedeCrear`).
- [ ] `Serial` único por empresa entre activos vigentes; serial nulo permitido; soft-delete libera el serial; mensaje amigable al duplicar.
- [ ] `SerieLocal` capturable, almacenado y visible en el formulario/listado de Activos.
- [ ] Modal de mantenimientos permite agregar comentario (fecha auto **visible en input deshabilitado**) y ver historial ordenado.
- [ ] `Notas` se muestra como textarea con `maxlength = 250`.
- [ ] Columna "Páginas Asignadas" de Permisos muestra el conteo real por rol al cargar (sin N+1).
- [ ] Chooser de permisos respeta el tema oscuro (sin fondos claros hardcodeados visibles).
- [ ] `ServiceDeskDESI.sln` compila sin errores (verificación estática + MSBuild).
