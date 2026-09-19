# Tasks: Renombrar menús "Personas" → "Personal" y "Usuarios" → "Administración"

Orden: SQL → Entities → MVC → Build → BD (manual) → Verificación.
Referencias: D1..D6 (design.md); MEN-001..005 (spec.md).

## Lote 1: SQL / migración

- [x] T1 — Crear `openspec/changes/personal-administracion/migration.sql` (idempotente): `IF COL_LENGTH('Pagina','NombreVisible') IS NULL ALTER TABLE Pagina ADD NombreVisible nvarchar(250) NULL;` + backfill `UPDATE Pagina SET NombreVisible = Nombre WHERE NombreVisible IS NULL;` + `UPDATE Pagina SET NombreVisible = 'Administración' WHERE Nombre = 'Usuarios';` + `UPDATE Pagina SET NombreVisible = 'Personal' WHERE Nombre = 'Personas';`. (D1, D2, D3, D6; MEN-004)
  > Desviación (documentada): se filtra por `Nombre` (no por `Id`) — más robusto a re-seeds; verificado que `Personas`=Id 20 y `Usuarios`=Id 4.
- [x] T2 — Crear `openspec/changes/personal-administracion/rollback.sql` (orden inverso, default conservador): `UPDATE Pagina SET NombreVisible = 'Usuarios' WHERE Nombre = 'Usuarios';` + `UPDATE Pagina SET NombreVisible = 'Personas' WHERE Nombre = 'Personas';`. NO eliminar columna (opción `DROP COLUMN` dejada comentada). (D6; MEN-004)

## Lote 2: Entities

- [x] T3 — Modificar `ServiceDeskDESIEntities/Seguridad/Pagina.cs`: añadir `public string NombreVisible { get; set; }` (después de `Nombre`). Sin DTO y sin cambio en `.csproj` (archivo ya incluido). (D4; MEN-001)

## Lote 3: MVC

- [x] T4 — Modificar `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` líneas 23 y 46: `@menu.Nombre` → `@(menu.NombreVisible ?? menu.Nombre)` dentro del `<span class="menu-text">`. (D1, D5; MEN-002)
- [x] T5 — Modificar `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` línea 34: `@sub.Nombre` → `@(sub.NombreVisible ?? sub.Nombre)`. (D1, D5; MEN-002)

## Lote 4: Build

- [x] T6 — Compilar la solución (3 proyectos) con `C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe` (no está en PATH) → 0 errores. Verifica que la propiedad fluye por `SELECT p.*` + `LlenarEntidad<T>` sin tocar DAL/SPs. (D4)

## Lote 5: BD (manual — requiere confirmación)

- [x] T7 — ⚠ Requiere confirmación del usuario antes de tocar la BD hosted. Aplicar `migration.sql` a `db_9c7990_servicedeskdesi` con sqlcmd `-C` (APLICADA y verificada: 19 filas backfill; Id 4→'Administración', Id 20→'Personal'):
  `& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" -S SQL5105.site4now.net -U <user> -P <pass> -d db_9c7990_servicedeskdesi -C -i openspec\changes\personal-administracion\migration.sql`
  Verificar re-ejecución sin error (idempotencia). Si no se aplica, el fallback `?? Nombre` mantiene el comportamiento actual. (D6; MEN-004)

## Lote 6: Verificación / smoke

- [ ] T8 — Login → sidebar: verificar que Id 20 muestra "Personal" e Id 4 muestra "Administración"; ítems con `NombreVisible = NULL` muestran `Nombre` (fallback). (MEN-001, MEN-002, MEN-003)
- [ ] T9 — Regresión de permisos: acceder a `/User/Users` y `/Catalogs/Persona` sin 403; `ObtenerPaginaPorNombre`/`ValidarPermisoUsuario` resuelven por `Nombre`, no `NombreVisible`. (MEN-005)
- [ ] T10 — Verificar que el chooser `Permisos.cshtml` sigue mostrando `pagina.Nombre` (la llave), no `NombreVisible`. (MEN-005)

## Follow-up (opcional)

- [ ] T11 — Actualizar `openspec/basededatosservicedesk.txt`: reflejar la columna `NombreVisible` en `CREATE TABLE [dbo].[Pagina]` (línea ~273) para evitar drift del dump. (D6)
