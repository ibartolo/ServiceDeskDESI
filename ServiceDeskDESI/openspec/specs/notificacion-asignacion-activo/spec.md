# notificacion-asignacion-activo Specification

## Purpose

Notificar por correo la asignación de un activo: correo DUAL (admin informativo sin liga + usuario asociado con liga de aceptación), correo de desvinculación al usuario, registro de cada intento en `BitacoraCorreo` y compensación (desvinculación) si el envío falla, devolviendo siempre error (nunca éxito).

## Requirements

### Requirement: NAA-001 — Correo dual de asignación

Tras el éxito de la asignación, el sistema MUST enviar DOS correos: (a) al ADMIN, informativo (sin liga); (b) al USUARIO asociado, con la liga `UrlConfirmacion` apuntando a la nueva página anónima de aceptación. El correo al usuario MUST resolver el template `Template_AsignacionActivo.html` con sus placeholders; el correo al admin MUST NOT incluir liga.

#### Scenario: Envío dual exitoso

- GIVEN una asignación exitosa
- WHEN se envía la notificación
- THEN salen 2 correos: admin (informativo, sin liga) y usuario asociado (con liga a la página anónima)

#### Scenario: Placeholder sin dato

- GIVEN un activo sin `Marca`/`Modelo` (nullable)
- WHEN se resuelve el template del correo al usuario
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

### Requirement: NAA-006 — Validación de usuario vinculado (-2)

El sistema MUST validar en `AsignarActivoPersona` que la persona tenga un usuario asociado (`Usuarios.PersonaId`); si no lo tiene, el SP MUST devolver `-2` ("persona sin usuario vinculado") y no asignar. El cliente MUST interpretar `-2` con mensaje específico, distinto de `-1` ("ya asignado").

#### Scenario: Persona sin usuario

- GIVEN una persona sin usuario vinculado
- WHEN se intenta asignar un activo
- THEN el SP devuelve `-2`, no se asigna, y la UI muestra "persona sin usuario vinculado"

#### Scenario: Persona con usuario

- GIVEN una persona con usuario vinculado
- WHEN se asigna el activo
- THEN la asignación procede normalmente

### Requirement: NAA-007 — Correo de desvinculación

Cuando el admin inicia la desvinculación, el sistema MUST enviar al usuario asociado un correo con la liga a la misma página anónima que discierne "desvincular", y MUST registrar el envío en `BitacoraCorreo`.

#### Scenario: Envío de correo de desvinculación

- GIVEN un admin inicia la desvinculación
- WHEN se procesa
- THEN el usuario recibe correo con liga de desvinculación y se registra en `BitacoraCorreo`
