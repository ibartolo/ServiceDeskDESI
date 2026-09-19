# Proposal: Username único POR EMPRESA (no global)

- **Change**: `username-por-empresa`
- **Fecha**: 2026-09-09
- **Estado**: ✅ **APLICADO en dev** — pendiente aplicar a prod (script listo).
- **Origen**: Bug de pruebas localhost (caso 6) — alta de usuario fallaba con excepción SQL 2601.

## Intent

Corregir el alta/edición de usuarios cuando el `NombreUsuario` ya existe en **otra** empresa. Decisión de negocio: el `NombreUsuario` es único **por empresa** (pueden existir N "Juan Perez" en empresas distintas) y se permite **reusar** el username de un usuario **inactivo**.

## Causa raíz

- El índice `UX_Usuarios_NombreUsuario` era **único GLOBAL** (solo `NombreUsuario`).
- Los guards del SP `GuardarOActualizarUsuarioAdmin` validan **por empresa** (`NombreUsuario` + `EmpresaId`).
- Al crear `a.mariano` en la empresa 26 existiendo en la empresa 1, el guard no lo detectó → el índice global rechazó el INSERT → excepción cruda en vez del mensaje amigable.

## Cambios

### BD (dev aplicado — script para prod en `migration.sql`)
1. `DROP INDEX UX_Usuarios_NombreUsuario` (global).
2. `CREATE UNIQUE INDEX UX_Usuarios_NombreUsuario_Empresa ON Usuarios (NombreUsuario, EmpresaId) WHERE Estatus = 1` (por empresa, filtrado a activos).
3. `ALTER PROCEDURE GuardarOActualizarUsuarioAdmin`: guard del INSERT alineado con `AND Estatus = 1` (username y correo).

### Código
4. `EmpresaService.NormalizarNombreResponsable`: formato `nombre.apellido` (antes `nombreapellido`).
5. `EmpresaService.GenerarUsernameAdminUnico(responsable, empresaId)`: unicidad por empresa.
6. `DbWrapper.ExisteNombreUsuario(nombreUsuario, empresaId)`: consulta acotada por empresa y activos.

## Archivos

- `ServiceDeskDESIWebApi/Services/EmpresaService.cs`
- `ServiceDeskDESIWebApi/DAL/DbWrapper.cs`
- `openspec/changes/username-por-empresa/migration.sql`

## Verificación

- ✅ Migración aplicada en dev (`SQL5105.site4now.net` / `db_9c7990_servicedeskdesi`); índice filtrado y guard confirmados.
- ✅ Build del WebApi: 0 errores.
- ⏳ Pendiente: reprobar el alta de un usuario con username repetido en **otra** empresa (debe permitir) y en la **misma** empresa (debe dar mensaje amigable).

## Despliegue a prod

Aplicar `migration.sql` a `db_9c7990_helpdeskdesi` @ `sql8005.site4now.net` con sqlcmd (`-C`), **antes** de publicar el WebApi con los cambios de código.

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" `
  -S sql8005.site4now.net -U db_9c7990_helpdeskdesi_admin -P "<pass>" `
  -d db_9c7990_helpdeskdesi -C -i openspec\changes\username-por-empresa\migration.sql -b
```

## Fuera de alcance

- Guards de duplicado en el SP `GuardarOActualizarUsuario` (no admin) — **nota**: no los tiene; revisar si aplica.
- Logging estandarizado (`logging-estandarizado`) y la obsolescencia de `MappingColumSecurity`.
