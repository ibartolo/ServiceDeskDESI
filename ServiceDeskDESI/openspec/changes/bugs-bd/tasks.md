# Tasks: bugs-bd

## 1. Base de datos (SPs)
- [x] 1.1 D6: `GuardarRolParaNuevaEmpresa` añade `@PuedeAtenderTickets`.
- [x] 1.2 D10: `AsignarRolUsuario` valida rol vía `Rol.CreadoPor`.
- [x] 1.3 D11: quitar JOIN muerto de `ObtenerEmpresas`.
- [x] 1.4 D12: corregir `nvarchaR` + `@@IDENTITY` en `GuardarOActualizarUsuarioPagina`.
- [x] 1.5 D13: restaurar `Estatus = 1` en `ObtenerUsuarioPorId` y `ObtenerUsuarios`.

## 2. Código
- [x] 2.1 `DbWrapper.Empresa.cs`: propagar `rol.PuedeAtenderTickets` en `GuardarRolParaNuevaEmpresa`.
- [x] 2.2 `EmpresaService.cs`: `PuedeAtenderTickets = true` en roles Administrador/Supervisor/Agente.

## 3. Scripts y verificación
- [x] 3.1 Generar `migration.sql` (delta con los 6 SPs).
- [x] 3.2 Compilar `ServiceDeskDESI.sln` (0 errores).
