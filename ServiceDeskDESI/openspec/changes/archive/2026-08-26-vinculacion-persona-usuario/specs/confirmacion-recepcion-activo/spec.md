# Delta for confirmacion-recepcion-activo

## ADDED Requirements

### Requirement: CRA-006 — Página de aceptación anónima (standalone)

El sistema MUST exponer una página pública sin masterpage (standalone, como login/NewCompany) que, a partir de la liga con token, muestre "quién le asignó qué" (asignador + activo) sin requerir sesión, con el botón "Acepto la asignación".

#### Scenario: Apertura de la liga

- GIVEN una liga válida de asignación
- WHEN el usuario la abre
- THEN se muestra la página anónima con asignador y activo, y el botón "Acepto la asignación"

### Requirement: CRA-007 — Modal de login y creación de sesión

Al presionar "Acepto la asignación", el sistema MUST mostrar un modal de login (usuario/contraseña) y validar las credenciales mediante `/token`, creando sesión (FormsAuthentication + TokenCookie) antes de marcar la aceptación y redirigir a "Mis Activos".

#### Scenario: Login y redirección

- GIVEN credenciales válidas en el modal
- WHEN se autentica vía `/token`
- THEN se crea la sesión, se marca Status 2 y se redirige a "Mis Activos"

### Requirement: CRA-008 — Credenciales incorrectas

El sistema MUST devolver error de autenticación ante credenciales incorrectas y MUST NOT marcar la aceptación.

#### Scenario: Credenciales inválidas

- GIVEN credenciales incorrectas en el modal
- WHEN se intenta autenticar
- THEN se muestra error de autenticación y no cambia el estado (sigue Status 1)

### Requirement: CRA-009 — Desvinculación autenticada

El sistema MUST soportar desvinculación autenticada: el admin (con permiso) inicia la desvinculación (botón + confirmación), se envía correo al usuario, el usuario abre la liga, la MISMA página anónima discierne "desvincular", pide credenciales y desvincula (`FechaFin`).

#### Scenario: Desvinculación por el usuario

- GIVEN un usuario recibe la liga de desvinculación y se autentica
- WHEN acepta desvincular
- THEN la asignación queda con `FechaFin` establecido ("Desvinculado")

## MODIFIED Requirements

### Requirement: CRA-001 — Aceptación autenticada (reemplaza confirmación anónima)

El sistema MUST reemplazar la confirmación anónima por una aceptación AUTENTICADA: para fijar `FechaConfirmacion`, el usuario MUST autenticarse con sus credenciales. La aceptación MUST ser idempotente. (Previously: endpoint anónimo que validaba el token GUID y fijaba `FechaConfirmacion` sin credenciales.)

#### Scenario: Aceptación exitosa

- GIVEN una asignación en Status 1 con liga válida
- WHEN el usuario acepta y se autentica correctamente
- THEN `FechaConfirmacion` queda establecida (Status 2) y se redirige a "Mis Activos"

#### Scenario: Re-clic tras aceptado (idempotencia)

- GIVEN una asignación ya aceptada (Status 2)
- WHEN se abre de nuevo la liga
- THEN se muestra "este activo ya fue asignado" y se redirige a login, sin cambio de estado

### Requirement: CRA-002 — Flujo de 2 estados

El sistema MUST modelar la asignación en 2 estados: Status 1 "Asociado/Pendiente" (`FechaFin IS NULL` AND `FechaConfirmacion IS NULL`) y Status 2 "Aceptado" (`FechaConfirmacion IS NOT NULL`). Una asignación desvinculada MUST tener `FechaFin` establecido. (Previously: pendiente vs cerrada, únicamente por `FechaConfirmacion`.)

#### Scenario: Asignación pendiente (Status 1)

- GIVEN una asignación recién creada
- WHEN no se ha aceptado
- THEN `FechaFin IS NULL` AND `FechaConfirmacion IS NULL` → Status 1 "Asociado/Pendiente"

#### Scenario: Asignación aceptada (Status 2)

- GIVEN una asignación aceptada
- WHEN se consulta
- THEN `FechaConfirmacion IS NOT NULL` → Status 2 "Aceptado"

#### Scenario: Desvinculada

- GIVEN una asignación desvinculada
- WHEN se consulta
- THEN `FechaFin IS NOT NULL` → "Desvinculado"

### Requirement: CRA-005 — El administrador no puede aceptar; sí desvincular

El sistema MUST NOT permitir que el administrador acepte en nombre del usuario; la aceptación MUST ser con credenciales del propio usuario. El administrador con permiso MAY iniciar la desvinculación. (Previously: el admin no podía confirmar; sin rol en la desvinculación.)

#### Scenario: Sin aceptación administrativa

- GIVEN un administrador autenticado
- WHEN intenta aceptar una asignación manualmente
- THEN no existe flujo de aceptación para admin; la aceptación solo ocurre vía la liga + credenciales del usuario

#### Scenario: Admin inicia desvinculación

- GIVEN un administrador con permiso
- WHEN inicia la desvinculación (botón + confirmación)
- THEN se envía correo al usuario para que éste desvincule (ver CRA-009)
