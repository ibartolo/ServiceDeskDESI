# notificacion-asignacion-activo Specification

## Purpose

Notificar por correo la asignación de un activo a una persona, registrar cada intento de envío en `BitacoraCorreo` y compensar (desvincular) la asignación si el envío falla, devolviendo siempre error (nunca éxito).

## Requirements

### Requirement: NAA-001 — Correo de asignación con placeholders resueltos

Tras el éxito de la asignación, el sistema MUST enviar un correo usando el template `Template_AsignacionActivo.html` con los 11 placeholders resueltos: `NombreUsuario`, `AsignadoPor`, `NombreActivo`, `Serial`, `TipoActivo`, `Marca`, `Modelo`, `FechaAsignacion`, `PuestoUsuario`, `CorreoUsuario` y `UrlConfirmacion`. El destinatario MUST ser `CorreoUsuario` (correo de la persona asignada).

#### Scenario: Envío exitoso

- GIVEN una asignación exitosa de activo a persona
- WHEN se resuelve el template con los datos de persona, activo y asignador
- THEN el correo sale al `CorreoUsuario` con los 11 placeholders resueltos

#### Scenario: Placeholder sin dato

- GIVEN un activo sin `Marca`/`Modelo` (nullable)
- WHEN se resuelve el template
- THEN el placeholder se reemplaza por vacío sin error

### Requirement: NAA-002 — Token de confirmación persistido

El sistema MUST generar un `TokenConfirmacion` (GUID) y persistirlo en la fila de asignación (`PersonaActivo`) para construir el enlace `{{UrlConfirmacion}}`.

#### Scenario: Generación en asignación

- GIVEN una asignación exitosa
- WHEN se genera el token
- THEN `PersonaActivo.TokenConfirmacion` contiene un GUID único y `UrlConfirmacion` apunta al endpoint de confirmación con ese token

### Requirement: NAA-003 — Bitácora de envíos

El sistema MUST registrar cada intento de envío en `BitacoraCorreo` con destinatario, asunto, estado (`Enviado`/`Fallido`), error (si lo hay), fecha y `ReferenciaId` (= `PersonaActivoId`).

#### Scenario: Registro exitoso

- GIVEN un envío exitoso
- WHEN se registra la bitácora
- THEN existe una fila con estado `Enviado` y `ReferenciaId` = `PersonaActivoId`

#### Scenario: Registro fallido

- GIVEN un envío que falla
- WHEN se registra la bitácora
- THEN existe una fila con estado `Fallido` y el mensaje de error

### Requirement: NAA-004 — Compensación ante fallo

Si el envío falla, el sistema MUST desvincular la asignación (`DesvincularActivoPersona`) y devolver `IsSuccess=false` con mensaje accionable. El sistema MUST NOT devolver éxito.

#### Scenario: Fallo de correo compensa

- GIVEN una asignación recién creada cuyo correo falla
- WHEN el envío lanza error
- THEN la asignación se desvincula (queda `FechaFin`, conservando histórico) y la API responde `IsSuccess=false` con mensaje para reintentar

#### Scenario: Compensación también falla

- GIVEN el envío falla y la desvinculación también falla
- WHEN se intenta compensar
- THEN se registra el error y la API devuelve `IsSuccess=false` (nunca éxito)

### Requirement: NAA-005 — Resiliencia ante infraestructura mal configurada

El sistema MUST capturar la excepción del envío y NO dejar la asignación huérfana. La excepción de `EmailHelper` MUST NOT propagarse como error no controlado.

#### Scenario: SMTP mal configurado

- GIVEN credenciales SMTP inválidas
- WHEN se asigna un activo
- THEN la API responde error controlado (`IsSuccess=false`) y la asignación queda compensada (desvinculada)
