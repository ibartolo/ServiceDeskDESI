# Proposal: Provisioning con template de roles (D4)

- **Change**: `provisioning-template`
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 2 — "Registro de empresa + provisioning" (ref **D4**)

## Intent

Eliminar el hardcodeo de los roles por defecto del registro de empresa usando una plantilla (`PlantillaRol`) clonable, de modo que agregar/editar roles no requiera tocar código. La transacción ya estaba resuelta a nivel de capa de aplicación (una sola `SqlConnection`+`SqlTransaction`).

## Hecho
- Tabla `PlantillaRol` (Nombre, Descripcion, PuedeAtenderTickets, Orden) + seed de 4 roles.
- SP `ObtenerPlantillaRoles`.
- `EmpresaService.GuardarNuevaEmpresaConDatosIniciales` clona los roles desde la plantilla (antes hardcodeados).
- `DbWrapper.ObtenerPlantillaRoles()` (mapeo a `Rol`).

## Archivos
- `openspec/basededatosservicedesk.txt`
- `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs`
- `ServiceDeskDESIWebApi/Services/EmpresaService.cs`
- `openspec/changes/provisioning-template/migration.sql`
