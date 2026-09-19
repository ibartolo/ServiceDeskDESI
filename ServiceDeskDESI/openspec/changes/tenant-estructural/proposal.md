# Proposal: Tenant estructural (D1)

- **Change**: `tenant-estructural`
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 2 — "Tenant estructural" (ref **D1**)

## Intent

Hacer del tenant un ciudadano de primera clase: columna `EmpresaId` en las tablas de dominio, `NombreUsuario` único global, y eliminar la inferencia de tenant por `CreadoPor`.

## Estado (parcial)

Hecho:
- `EmpresaId` (nullable) + FK en 12 tablas de dominio (Activo, Area, Categoria, CategoriaResponsable, Marca, Modelo, Persona, Puesto, Rol, Sucursal, Ticket, TipoActivo).
- Backfill de `EmpresaId` desde `CreadoPor → Usuarios.EmpresaId`.
- Índice único global `UX_Usuarios_NombreUsuario` (cierra la fuga por colisión).
- SPs de registro (`GuardarNuevaAreaParaEmpresa`, `GuardarNuevaSucursalParaEmpresa`, `GuardarRolParaNuevaEmpresa`) con `@EmpresaId`.
- `GuardarOActualizarActivo/Area/Categoria` reescritos para filtrar/insertar por `EmpresaId`.

Pendiente (siguiente cambio):
- Reescritura del resto de `GuardarOActualizar*`, `Eliminar*` y `Obtener*` para filtrar por `EmpresaId` (hoy siguen por `CreadoPor`).
- Endurecer `EmpresaId` a `NOT NULL` una vez todos los SPs lo asignen.

## Archivos
- `openspec/basededatosservicedesk.txt` (esquema + SPs)
- `ServiceDeskDESIWebApi/DAL/DbWrapper.{Area,Sucursal,Empresa}.cs`
- `ServiceDeskDESIWebApi/Services/EmpresaService.cs`
- `openspec/changes/tenant-estructural/migration.sql`
