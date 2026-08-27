# Delta for notificacion-asignacion-activo

## ADDED Requirements

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

## MODIFIED Requirements

### Requirement: NAA-001 — Correo dual de asignación

Tras el éxito de la asignación, el sistema MUST enviar DOS correos: (a) al ADMIN, informativo (sin liga); (b) al USUARIO asociado, con la liga `UrlConfirmacion` apuntando a la nueva página anónima de aceptación. El correo al usuario MUST resolver el template `Template_AsignacionActivo.html` con sus placeholders; el correo al admin MUST NOT incluir liga. (Previously: un único correo al `CorreoUsuario` de la persona, con liga a `ConfirmarRecepcion/{token}`.)

#### Scenario: Envío dual exitoso

- GIVEN una asignación exitosa
- WHEN se envía la notificación
- THEN salen 2 correos: admin (informativo, sin liga) y usuario asociado (con liga a la página anónima)

#### Scenario: Placeholder sin dato

- GIVEN un activo sin `Marca`/`Modelo` (nullable)
- WHEN se resuelve el template del correo al usuario
- THEN el placeholder se reemplaza por vacío sin error
