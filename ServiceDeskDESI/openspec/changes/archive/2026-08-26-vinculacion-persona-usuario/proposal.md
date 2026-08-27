# Proposal: Vinculación Persona ↔ Usuario, aceptación autenticada de activos y "Mis Activos"

- **Change**: `vinculacion-persona-usuario`
- **Fase**: propose
- **Fecha**: 2026-08-26
- **Origen**: `explore.md` del change + `asignacion-activos` (explore/design/proposal/migration) + decisiones D1-D5 autoritativas.

## Intent

Establecer la relación 1:1 Persona↔Usuario ("usuario básico" deducible por `Usuarios.PersonaId`, sin flag), rediseñar la confirmación de asignación de activos en un flujo de 2 estados con **aceptación autenticada** (reemplaza el confirm anónimo/auto-confirm) y desvinculación autenticada, y exponer la vista "Mis Activos" al usuario básico.

## Scope

### In Scope
- `Usuarios.PersonaId BIGINT NULL` + FK a `Persona` + índice único filtrado (`WHERE PersonaId IS NOT NULL`). Sin flag.
- Sincronización Persona↔Usuario desde `Persona.cshtml` (botón SVG + modal + warning "los datos se sobreescribirán…" en modal **y** antes de guardar). Sincronizado → Nombre/Apellido/Correo/Telefono **deshabilitados** y toman datos del Usuario (PuestoId intacto). El usuario **DEBE pre-existir**.
- Asignación → Status 1 "Asociado/Pendiente": botón "vincular activo" solo visible si la persona tiene usuario; correo al ADMIN (informativo, sin liga) + al USUARIO (con liga); validación `-2` ("persona sin usuario vinculado") en `AsignarActivoPersona`.
- Aceptación Status 2 autenticada: ventana anónima standalone (sin masterpage) → "Acepto la asignación" → modal login → `/token` → sesión (FormsAuth + TokenCookie) → Status 2 → redirect "Mis Activos". Re-clic ya aceptado → "este activo ya fue asignado" + redirect login. Aceptable también desde "Mis Activos".
- Desvincular autenticado (admin desvincula → correo usuario → misma página anónima discierne "desvincular").
- Vista + endpoint "Mis Activos" (`GET api/PersonaActivo/MisActivos`, sin `[Permiso("Personas")]`), menú vía `RolPaginaAccion` (configurable).
- **REESCRIBE** el flujo de confirmación del change `asignacion-activos` (NO archivado/desplegado): reemplaza `ConfirmarRecepcion/{token}` anónimo por aceptación autenticada.

### Out of Scope
- Cambios en token/OAuth/claims (se resuelve `PersonaId` vía `ObtenerUsuarioPorId` + `SELECT u.*`).
- Crear usuario desde el catálogo de Personas (el usuario se crea en el catálogo de Usuarios).
- Gestión de `PuestoId` en la sincronización.
- Reintentos/cola de correo (se conservan `BitacoraCorreo` y compensación existentes).
- Asignar rol/perfil distinto del rol "Usuario" (se deja configurable).

## Approach

1. **DB**: `ALTER Usuarios ADD PersonaId BIGINT NULL` + FK + índice único filtrado; reescribir `GuardarOActualizarUsuarioAdmin` (aceptar `@PersonaId`) y `AsignarActivoPersona` (retorno `-2`); INSERT `Pagina` "Mis Activos" + `RolPaginaAccion` al rol "Usuario"; exponer `PersonaId` en `ObtenerUsuarioPorId`.
2. **Relación**: `Usuario.PersonaId` en entidad + DTO; sync desde `Persona.cshtml`.
3. **Asignación (Status 1)**: validación `-2` en SP + rama en `DbWrapper`; correo dual reutilizando `Template_AsignacionActivo.html` + `BitacoraCorreo`/compensación existentes.
4. **Aceptación (Status 2)**: página anónima standalone (como login/NewCompany) + modal login replicando `AutenticacionService.AutenticarUsuario` → `/token` → sesión → `ConfirmarRecepcionActivo` (autenticado) → redirect "Mis Activos". Idempotente.
5. **Desvincular**: confirm modal → correo → misma página discierne "desvincular" → credenciales → `DesvincularActivoPersona`.
6. **Mis Activos**: endpoint que deriva `PersonaId` desde `Usuarios.PersonaId` del autenticado (`tokenCookie.UserID`); menú vía `RolPaginaAccion`.

**Estados**: Pendiente = `FechaFin IS NULL AND FechaConfirmacion IS NULL` · Aceptado = `FechaConfirmacion IS NOT NULL` · Desvinculado = `FechaFin IS NOT NULL`.

## Capabilities

### New Capabilities
- `vinculacion-persona-usuario`: relación 1:1 Persona↔Usuario (columna + FK + índice) y sincronización desde el catálogo de Personas.
- `mis-activos`: vista + endpoint "Mis Activos" (derivación autenticada de PersonaId, menú vía `RolPaginaAccion`).

### Modified Capabilities
- `confirmacion-recepcion-activo`: de confirmación anónima auto-confirm → **aceptación autenticada** (Status 2) + desvinculación autenticada (redefine el flujo del change `asignacion-activos` no archivado).
- `notificacion-asignacion-activo`: correo de asignación ahora **dual** (admin informativo + usuario con liga) y correo de desvinculación.

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `openspec/changes/vinculacion-persona-usuario/migration.sql` | Nuevo | ALTER Usuarios + FK + índice; reescribir SPs; `Pagina` "Mis Activos" + `RolPaginaAccion` |
| `openspec/changes/asignacion-activos/migration.sql` | Mod | Reescribir `AsignarActivoPersona` (`-2`), `ConfirmarRecepcionActivo` (autenticado), correo dual |
| `ServiceDeskDESIEntities/Autenticacion/Usuario.cs` (+ DTO) | Mod | `PersonaId long?` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs` | Mod | `GuardarOActualizarUsuarioAdmin` con `PersonaId`; `ObtenerUsuarioPorId` expone `PersonaId` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs` | Mod | rama `-2`; métodos MisActivos/aceptación autenticada |
| `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` | Mod | `ObtenerMisActivos`; correo dual; aceptación/desvinculación |
| `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs` | Mod | `GET MisActivos`; confirmación autenticada |
| `ServiceDeskDESIMVC/Controllers/CatalogsController.cs` | Mod | Sync Persona↔Usuario; "vincular activo" solo si tiene usuario |
| `ServiceDeskDESIMVC/Views/Catalogs/Persona.cshtml` (+ partial) | Mod | Botón sync + modal + inputs deshabilitados |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` | Mod | Acción anónima aceptación/desvinculación + `MisActivos` |
| `ServiceDeskDESIMVC/Views/Home/...` (nuevas) | Nuevo | Página anónima standalone + vista `MisActivos` |
| `ServiceDeskDESIMVC/FilterConfig.cs` | Mod | `PublicActions` aceptación/desvinculación |
| `ServiceDeskDESI*.csproj` | Mod | `<Compile Include>` / `<Content Include>` (legacy) |

## Risks

| Riesgo | Prob. | Mitigación |
|---|---|---|
| `asignacion-activos` NO archivado → solape/conflicto de flujo | Alta | Reescribir explícitamente su migration/design en este change; coordinar orden de archivo |
| `-2` mal interpretado (`<= -1` hoy = "ya asignado") | Alta | Rama explícita `-2` antes de `-1` |
| Exponer catálogo Personas al usuario básico | Media | Endpoint `MisActivos` dedicado, sin `[Permiso("Personas")]` |
| Roles "Usuario" por-empresa múltiples (3, 31…) | Media | `RolPaginaAccion` para TODOS los roles "Usuario"; provisioning de nuevas empresas (sin SP dedicado) |
| Sesión no creada antes de Status 2 | Media | Replicar patrón login exacto (FormsAuth + TokenCookie) antes de confirmar |
| FK/índice en `Usuarios` con datos existentes | Media | Migración aditiva `NULL`; validar sin `PersonaId` previo |
| csproj legacy sin `<Compile Include>` | Media | Registrar entidades/vistas manualmente |

## Rollback Plan

- SQL: `DROP COLUMN PersonaId` + `DROP INDEX`/`DROP CONSTRAINT` FK; restaurar definiciones previas de `AsignarActivoPersona`/`ConfirmarRecepcionActivo`; `DELETE` fila `Pagina` "Mis Activos" + sus `RolPaginaAccion`.
- Código: revertir sync/aceptación/mis-activos; el flujo vuelve a `ConfirmarRecepcion` anónimo (estado `asignacion-activos` sin archivar).
- Sin cambios destructivos sobre datos (columna `NULL` aditiva; la relación es opcional).

## Dependencies

- Migración SQL a `db_9c7990_servicedeskdesi` antes de desplegar.
- `asignacion-activos` aún activo (no archivado): este change lo reescribe.
- `ObtenerUsuarioPorId` debe exponer `PersonaId` (vía `SELECT u.*`).

## Success Criteria

- [ ] `Usuarios.PersonaId` + FK + índice único filtrado aplicados; "usuario básico" deducible sin flag.
- [ ] Sincronizar desde `Persona.cshtml` deshabilita Nombre/Apellido/Correo/Telefono y toma datos del Usuario (warning en modal y antes de guardar).
- [ ] Botón "vincular activo" solo visible si la persona tiene usuario; SP devuelve `-2` si no.
- [ ] Asignación envía correo admin (informativo) + usuario (con liga); Status 1 = `FechaFin IS NULL AND FechaConfirmacion IS NULL`.
- [ ] Aceptación autenticada: login → Status 2 (`FechaConfirmacion IS NOT NULL`) → redirect "Mis Activos"; re-clic → "ya fue asignado" + redirect login.
- [ ] Desvincular autenticado → `FechaFin IS NOT NULL`.
- [ ] "Mis Activos" visible solo vía `RolPaginaAccion` al rol "Usuario"; endpoint deriva `PersonaId` del `tokenCookie`.
- [ ] Sin cambios en claims/OAuth. `ServiceDeskDESI.sln` compila.

## Open Questions

1. **Correo de desvinculación**: ¿se reutiliza `Template_AsignacionActivo.html` o se crea un template nuevo ("Tu activo fue desvinculado")?
2. **Asignación del rol "Usuario"**: ¿la sincronización asigna automáticamente el rol "Usuario" al usuario vinculado, o se asume que el admin ya lo asignó al crear el usuario en el catálogo de Usuarios? (D1 dice "el usuario DEBE pre-existir"; el explore sugiere asignarlo en `GuardarOActualizarUsuarioAdmin`).
3. **Provisioning de empresas nuevas**: sin un SP "para nueva empresa" de `RolPaginaAccion`, ¿cómo se garantiza que futuras empresas vean "Mis Activos"?
4. **Credenciales del usuario básico**: ¿quién define la contraseña inicial (admin al crear el usuario en el catálogo de Usuarios)?
