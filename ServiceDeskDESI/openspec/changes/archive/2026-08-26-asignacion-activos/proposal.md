# Proposal: Asignación de Activos — notificación, confirmación de recepción y bitácora

- **Change**: `asignacion-activos`
- **Fase**: propose
- **Fecha**: 2026-08-26
- **Origen**: exploración de `asignacion-activos` (commits `bd47e85` + `e88aea0`).

## Intent

Cerrar los vacíos del flujo de asignación de activos **ya implementado y desplegado**: (1) enviar el correo de notificación con el template existente, (2) crear el flujo "Confirmar Recepción" (columna + token + endpoint + página), (3) registrar cada intento de envío en una bitácora ligera, y (4) compensar la asignación si el correo falla, devolviendo siempre **error** (nunca éxito).

## Scope

### In Scope
- Envío del correo de asignación rellenando los 11 placeholders del template, usando datos de `ObtenerPersonaPorId`, `ObtenerActivoPorId`, `ObtenerUsuarioPorNombreUsuario`.
- Flujo "Confirmar Recepción": columnas `FechaConfirmacion`/`TokenConfirmacion` en `PersonaActivo`, endpoint anónimo (WebApi) + página MVC, siguiendo el precedente de reset de contraseña.
- Bitácora ligera de envíos `BitacoraCorreo` (destinatario, asunto, estado Enviado/Fallido, error, fecha). **No** es cola de reintentos.
- Compensación: si el correo falla, desvincular la asignación y devolver `IsSuccess=false`.
- Renombrar `Templat_AsignacionActivo.html` → `Template_AsignacionActivo.html` + actualizar csproj.

### Out of Scope
- El núcleo asignar/desvincular + UI (ya implementado y desplegado; migración ya aplicada).
- Cola de reintentos / reintentos automáticos de correo.
- Renombrar el método `EnvioEmaiil` (se conserva, usado en otros flujos).
- Envío asíncrono (incompatible con compensación síncrona del punto 4).

## Capabilities

### New Capabilities
- `notificacion-asignacion-activo`: envío del correo de asignación, compensación ante fallo y registro en bitácora.
- `confirmacion-recepcion-activo`: confirmación de recepción anónima con token GUID y marcado de `FechaConfirmacion`.

### Modified Capabilities
- None (`openspec/specs/` no tiene specs de activos/personas).

## Approach

1. **DB (migración aditiva, idempotente)**: `ALTER PersonaActivo ADD FechaConfirmacion DATETIME NULL, TokenConfirmacion UNIQUEIDENTIFIER NULL`; `CREATE TABLE BitacoraCorreo`; SPs nuevos `GenerarTokenConfirmacion`, `ConfirmarRecepcionActivo`, `RegistrarBitacoraCorreo`. No re-DROP/CREATE los 5 SPs existentes.
2. **WebApi `PersonaActivoService.AsignarActivoPersona`**: tras éxito del SP → obtener persona/activo/asignador → generar token (`NEWID()`, persistido en la fila) → construir `{{UrlConfirmacion}}` (`{BaseUri}Home/ConfirmarRecepcion/{token}`) → rellenar template → `EmailHelper.EnvioEmaiil` en try/catch → registrar `BitacoraCorreo`. **Si falla el envío**: `DesvincularActivoPersona(newId)` (compensación) + registrar `Fallido` + devolver `IsSuccess=false` con mensaje para reintentar.
3. **Confirmación**: WebApi `[AllowAnonymous] GET api/PersonaActivo/confirmarRecepcion/{token}` → valida token, setea `FechaConfirmacion=GETDATE()` (idempotente). MVC: acción pública `Home.ConfirmarRecepcion(id)` (añadida a `PublicActions` en `FilterConfig`) + vista `ConfirmarRecepcion.cshtml` que llama al WebApi.
4. **Template**: renombrar archivo + entrada csproj; dejar botón "Confirmar Recepción" apuntando a `{{UrlConfirmacion}}`.

## Affected Areas

| Área | Impacto | Descripción |
|------|---------|-------------|
| `openspec/changes/asignacion-activos/migration.sql` | Mod | Columnas + `BitacoraCorreo` + 3 SPs |
| `ServiceDeskDESIEntities/Catalogos/PersonaActivo.cs` + DTO | Mod | `FechaConfirmacion`, `TokenConfirmacion` |
| `ServiceDeskDESIEntities/Catalogos/BitacoraCorreo.cs` + DTO | Nuevo | Entidad bitácora |
| `ServiceDeskDESIEntities` (.csproj) | Mod | Registrar entidades |
| `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` | Mod | Orquestar envío + compensación |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs` | Mod | Nuevos SPs |
| `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs` | Mod | Endpoint confirmación |
| `ServiceDeskDESIWebApi/Template/Templat_AsignacionActivo.html` | Mod | Renombrar a `Template_AsignacionActivo.html` |
| `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` | Mod | Actualizar path template |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` + `FilterConfig.cs` | Mod | Acción pública confirmación |
| `ServiceDeskDESIMVC/Views/Home/ConfirmarRecepcion.cshtml` | Nuevo | Vista de confirmación |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.PersonaActivo.cs` + Services | Mod | Cliente HTTP confirmación |

## Risks

| Riesgo | Prob. | Mitigación |
|--------|-------|------------|
| Re-implementar el núcleo ya desplegado | Baja | Explícitamente fuera de alcance |
| La compensación (`DesvincularActivoPersona`) falla tras fallo de correo | Baja | try/catch anidado + `Log.Error`; aun así devolver error |
| Endpoint anónimo expone `PersonaActivoId` | Media | Token GUID, sin ID en URL, confirmación idempotente |
| Latencia SMTP síncrona en POST Asignar | Media | Aceptada: la compensación exige envío síncrono |
| `EnvioEmaiil` re-lanza y fuerza `EnableSsl` | Media | Capturar excepción en el servicio; no alterar helper |

## Rollback Plan

- SQL: columnas nuevas son `NULL` (aditivas) → `DROP COLUMN`; `DROP TABLE BitacoraCorreo`; `DROP PROCEDURE` de los 3 SPs nuevos.
- Código: revertir cambios en `PersonaActivoService` → la asignación vuelve a funcionar sin correo ni compensación (estado previo).
- Template: renombrar de vuelta y restaurar csproj. Sin cambios destructivos sobre datos ni SPs existentes.

## Dependencies

- Ejecutar la migración SQL en la BD hosted antes de desplegar. Config `BaseUri` ya usada por reset de contraseña.

## Success Criteria

- [ ] Al asignar, se envía el correo con los 11 placeholders resueltos y `{{UrlConfirmacion}}` funcional.
- [ ] Si el correo falla, la asignación se desvincula, se registra `Fallido` y la API devuelve error.
- [ ] El enlace confirma recepción y setea `FechaConfirmacion` (idempotente, solo una vez).
- [ ] Cada intento queda en `BitacoraCorreo` con estado Enviado/Fallido + error.
- [ ] `ServiceDeskDESI.sln` compila sin errores.

## Decisiones resueltas (checkpoint usuario — autoritativas)

1. **Confirmación OBLIGATORIA.** La asignación queda en estado "pendiente" (`FechaConfirmacion IS NULL`) hasta que el usuario la confirme desde el enlace del correo. El administrador NO puede confirmar (el enlace llega solo al correo del usuario); así se evita que el admin asigne y auto-apruebe sin consentimiento. La asignación no se considera cerrada hasta la confirmación.
2. **Compensación = desvincular** (`DesvincularActivoPersona`, deja `FechaFin` y conserva histórico/auditoría). No borrar la fila.
3. **Bitácora = `BitacoraCorreo`.**
4. **Añadir `ReferenciaId`** a la bitácora (p.ej. `PersonaActivoId`) para trazabilidad del correo a la asignación.
5. **Sin caducidad del token** de confirmación (el usuario puede ser remoto y confirmar al recibir el equipo, N días después).
