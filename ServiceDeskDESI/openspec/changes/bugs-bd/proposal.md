# Proposal: Bugs de BD que rompen flujos (D6, D10, D11, D12, D13)

- **Change**: `bugs-bd`
- **Fase**: propose
- **Fecha**: 2026-08-18
- **Origen**: `security-remediation` Fase 2 — hallazgo ALTO "Bugs de BD que rompen flujos" (refs **D6, D10, D11, D12, D13**)

## Intent

Corregir cinco bugs concretos en stored procedures que rompen flujos funcionales (catálogo de estatus, asignación de responsables, registro de empresa) y uno de ellos bloquea el despliegue (`nvarchaR`).

## Scope

| Ref | Bug | Fix |
|---|---|---|
| D6 | `GuardarRolParaNuevaEmpresa` inserta rol sin `PuedeAtenderTickets` → el admin nuevo no puede atender tickets | Añadir `@PuedeAtenderTickets` y propagarlo desde `EmpresaService` (Administrador/Supervisor/Agente = true) |
| D10 | `AsignarRolUsuario` valida pertenencia del rol vía `UsuarioRol` (asignación) en vez de `Rol.CreadoPor` | Validar vía `Rol.CreadoPor → Usuarios.EmpresaId` |
| D11 | `ObtenerEmpresas` tiene `INNER JOIN Usuarios` muerto (duplica filas) | Quitar el JOIN muerto |
| D12 | `GuardarOActualizarUsuarioPagina` tiene typo `@CreadoPor nvarchaR(25)` (no compila) y usa `@@IDENTITY` | `nvarchar(25)` + `SCOPE_IDENTITY()` |
| D13 | `ObtenerUsuarioPorId` tiene `--AND u.Estatus = 1` comentado y `ObtenerUsuarios` no filtra `Estatus` → devuelven usuarios borrados | Restaurar filtro `Estatus = 1` |

## Success Criteria
- [ ] Los 5 bugs corregidos en el script general (`basededatosservicedesk.txt`) y en un script de migración delta.
- [ ] `ServiceDeskDESI.sln` compila sin errores.
- [ ] Registro de empresa crea el rol Administrador con `PuedeAtenderTickets = 1`.
