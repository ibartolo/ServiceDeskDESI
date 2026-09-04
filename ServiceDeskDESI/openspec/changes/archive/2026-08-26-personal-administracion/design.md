# Design: Renombrar menús "Personas" → "Personal" y "Usuarios" → "Administración"

- **Change**: `personal-administracion`
- **Fase**: design
- **Fecha**: 2026-08-26

## Technical Approach

Separar la **etiqueta visible** del menú de la **llave de permisos**. Hoy ambas viven en `Pagina.Nombre`; se añade `Pagina.NombreVisible` (nullable) que solo se usa para renderizar. `Nombre` queda intacto como llave estable de permisos. El menú usa `SELECT p.*` + `LlenarEntidad<T>` (mapeo case-insensitive por nombre de columna↔propiedad, `DbWrapper.cs:28-61`), por lo que la columna nueva fluye al POCO **sin tocar DAL ni SPs**. Único cambio de código visible: render con fallback en `MenusUser.cshtml`.

## Architecture Decisions

| # | Decisión | Opciones descartadas | Rationale |
|---|---|---|---|
| **D1** | Columna nueva `NombreVisible NVARCHAR(250) NULL` + fallback `?? Nombre` | Renombrar `Nombre` (Opción B); mapeo hardcodeado en vista (Opción C) | `Nombre` es la llave: SP `ObtenerPaginaPorNombre` (`WHERE Nombre=@Nombre`), `[Permiso(...)]` y ~30 comparaciones `PaginaNombre ==`. Renombrarla rompe permisos en silencio (403). C es frágil. Nullable + fallback cubre filas nuevas/desconocidas. |
| **D2** | Nullable + backfill `= Nombre` en vez de `NOT NULL` | Columna `NOT NULL` | `NOT NULL` exige backfill en la misma transacción y rompe cualquier INSERT futuro a `Pagina` que no conozca la columna (no hay INSERT en repo, pero el seed vive fuera). `NULL` + fallback en vista es tolerante. |
| **D3** | Tipo/tamaño `nvarchar(250) NULL` idéntico a `Nombre`; **sin índice** | Índice sobre `NombreVisible` | Misma capacidad que `Nombre` (labels de menú). No se filtra ni ordena por `NombreVisible`; es solo display → índice innecesario. |
| **D4** | Propiedad `NombreVisible` (string) en POCO `Pagina`. **No existe `PaginaDTO`** (verificado); el menú viaja como `List<Pagina>` | Crear DTO nuevo | `Pagina.cs` ya está en el `.csproj`; añadir una propiedad **no crea archivo nuevo** → **sin cambio en `.csproj`**. |
| **D5** | Cambiar solo `MenusUser.cshtml` (líneas 23/34/46) | Cambiar `Permisos.cshtml`, títulos hardcodeados | `Permisos.cshtml` es el chooser de permisos y debe seguir mostrando `pagina.Nombre` (la llave). Títulos (`Users.cshtml`, `Persona.cshtml`) fuera de alcance. |
| **D6** | Migración idempotente + rollback en orden inverso en `openspec/changes/personal-administracion/` | — | Convención del repo (ver `tickets-ciclo-vida/migration.sql`+`rollback.sql`). Seed NO está en version control → aplicación manual a BD hosted. |

## Data Flow

```
Pagina (BD) ──SELECT p.*──▶ DbWrapper.ObtenerPaginasPorUsuario
   │  (incluye NombreVisible)         │ LlenarEntidad<Pagina> (reflexión)
   │                                  ▼
   │                          Pagina POCO (Nombre + NombreVisible)
   │                                  │ JSON List<Pagina>
   │                                  ▼
   │                     MenusUser.cshtml → @(x.NombreVisible ?? x.Nombre)
   │
   └─ Permisos: SP ObtenerPaginaPorNombre → WHERE Nombre = @Nombre  (SIN CAMBIO)
```

## File Changes

| Archivo | Acción | Descripción |
|---|---|---|
| `openspec/changes/personal-administracion/migration.sql` | Create | Idempotente: ADD columna + backfill + 2 UPDATE |
| `openspec/changes/personal-administracion/rollback.sql` | Create | Orden inverso: 2 UPDATE revertidos + DROP columna |
| `ServiceDeskDESIEntities/Seguridad/Pagina.cs` | Modify | `+ public string NombreVisible { get; set; }` |
| `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` | Modify | Líneas 23/34/46: `@(x.NombreVisible ?? x.Nombre)` |
| `openspec/basededatosservicedesk.txt` | Modify (opcional) | Reflejar columna (evitar drift) |

## Interfaces / Contracts

**SQL — migration.sql (idempotente):**
```sql
IF COL_LENGTH('Pagina','NombreVisible') IS NULL
  ALTER TABLE Pagina ADD NombreVisible nvarchar(250) NULL;
UPDATE Pagina SET NombreVisible = Nombre WHERE NombreVisible IS NULL;
UPDATE Pagina SET NombreVisible = 'Administración' WHERE Id = 4;
UPDATE Pagina SET NombreVisible = 'Personal'       WHERE Id = 20;
```

**SQL — rollback.sql (orden inverso):**
```sql
UPDATE Pagina SET NombreVisible = 'Usuarios' WHERE Id = 4;
UPDATE Pagina SET NombreVisible = 'Personas' WHERE Id = 20;
-- Opcional (si se decide eliminar la columna por completo):
-- IF COL_LENGTH('Pagina','NombreVisible') IS NOT NULL
--   ALTER TABLE Pagina DROP COLUMN NombreVisible;
```

**Razor — MenusUser.cshtml (3 sitios):** `@(menu.NombreVisible ?? menu.Nombre)` y `@(sub.NombreVisible ?? sub.Nombre)`. El `??` de Razor/C# resuelve `null` → `Nombre`; debe ir entre paréntesis dentro del `<span>`.

## Explicitamente NO cambia

- SPs `ObtenerPaginaPorNombre`, `ObtenerPermisosPorUsuario`, `ObtenerPermisosPorRol`, `ObtenerPaginasPorUsuario` (`SELECT p.*` ya propaga la columna).
- Atributos `[Permiso("Personas"/"Usuarios")]`.
- Comparaciones MVC `PersonaService.cs:50`, `UsuarioService.cs:65` (`PaginaNombre == "Personas"/"Usuarios"`).
- `Permisos.cshtml` (chooser) y títulos hardcodeados.

## Testing Strategy

| Capa | Qué probar | Enfoque |
|---|---|---|
| Manual E2E | Menú muestra "Personal"/"Administración"; resto sin cambios | Login → sidebar; verificar Ids 4/20 |
| Manual regresión | Permisos siguen funcionando (403 ausente) | Acceder `/User/Users` y `/Catalogs/Persona` |
| Manual fallback | Fila con `NombreVisible=NULL` muestra `Nombre` | Insertar/editar fila test con NULL |
| Migración | Re-ejecución sin error; rollback restaura | Correr `migration.sql` 2×; luego `rollback.sql` |

## Migration / Rollout

1. Aplicar `migration.sql` a la BD hosted `db_9c7990_servicedeskdesi` @ `SQL5105.site4now.net` (key `cCon`):
   ```
   & "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" -S SQL5105.site4now.net -U <user> -P <pass> -d db_9c7990_servicedeskdesi -C -i openspec\changes\personal-administracion\migration.sql
   ```
2. **⚠ Manual**: el seed NO está en el repo; la migración debe aplicarse a la BD hosted por un humano (no hay paso automatizado). Si no se aplica, el fallback `?? Nombre` mantiene el comportamiento actual (las etiquetas simplemente no cambian).
3. Rollback: ejecutar `rollback.sql` en el mismo entorno.

## Open Questions

- [ ] ¿Eliminar la columna (`DROP COLUMN`) en el rollback, o solo revertir los 2 valores? (se proveen ambas; `DROP` comentado).

## Decision Records (referencias para tasks)

- **D1** separar etiqueta de llave (columna nueva, no renombrar). **D2** nullable + backfill. **D3** `nvarchar(250) NULL`, sin índice. **D4** propiedad POCO, sin DTO, sin cambio csproj. **D5** solo `MenusUser.cshtml`. **D6** migración idempotente + rollback, aplicación manual a hosted.
