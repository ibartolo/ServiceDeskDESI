# vinculacion-persona-usuario Specification

## Purpose

Relación 1:1 Persona↔Usuario ("usuario básico" deducible por `Usuarios.PersonaId`, sin flag) y sincronización desde el catálogo de Personas, sin cambios en token/OAuth/claims.

## Requirements

### Requirement: VPU-001 — Relación 1:1 Persona↔Usuario

El sistema MUST persistir la relación mediante `Usuarios.PersonaId BIGINT NULL` con FK a `Persona(Id)` y un índice único filtrado (`WHERE PersonaId IS NOT NULL`). "Usuario básico" MUST ser un `Usuario` con `PersonaId` no nulo; el sistema MUST NOT usar un flag dedicado.

#### Scenario: Un usuario básico por persona

- GIVEN una Persona ya vinculada a un Usuario
- WHEN se intenta vincularla a otro Usuario
- THEN se rechaza por el índice único filtrado

#### Scenario: Persona sin usuario

- GIVEN una Persona sin vínculo
- WHEN se consulta su `Usuarios.PersonaId`
- THEN no se considera usuario básico (no hay fila `Usuarios` con su `PersonaId`)

### Requirement: VPU-002 — Sincronización desde Persona.cshtml

El sistema MUST exponer un botón SVG con tooltip "Sincronizar con usuario" en la vista de Persona que abra un modal con la tabla de usuarios (nombre de usuario, nombre, apellido, correo) y un botón Sincronizar.

#### Scenario: Apertura del modal

- GIVEN el catálogo de Personas
- WHEN se presiona el botón SVG "Sincronizar con usuario"
- THEN se abre un modal con la tabla de usuarios (nombre de usuario, nombre, apellido, correo) y el botón Sincronizar

### Requirement: VPU-003 — Advertencia de sobrescritura

El sistema MUST advertir que "los datos se sobreescribirán" tanto en el modal como antes de guardar, antes de aplicar la sincronización.

#### Scenario: Advertencia en el modal

- GIVEN el modal de usuarios abierto
- WHEN se presiona Sincronizar
- THEN se muestra la advertencia de sobrescritura en el modal

#### Scenario: Advertencia antes de guardar

- GIVEN la advertencia del modal aceptada
- WHEN se procede a guardar
- THEN se muestra de nuevo la advertencia de sobrescritura antes de guardar

### Requirement: VPU-004 — Bloqueo de campos tras sincronizar

Tras aceptar, el sistema MUST mostrar el campo de username bloqueado y MUST deshabilitar Nombre, Apellido, Correo y Telefono de la Persona, tomando esos datos del Usuario vinculado. `PuestoId` MUST permanecer intacto.

#### Scenario: Campos deshabilitados

- GIVEN una sincronización aceptada
- WHEN se muestra la vista de Persona
- THEN el username queda bloqueado y Nombre/Apellido/Correo/Telefono quedan deshabilitados con los datos del Usuario

#### Scenario: PuestoId intacto

- GIVEN una sincronización aceptada
- WHEN se guarda la Persona
- THEN `PuestoId` no cambia

### Requirement: VPU-005 — Usuario pre-existente

El sistema MUST requerir que el Usuario a vincular ya exista; MUST NOT crear el usuario desde el catálogo de Personas.

#### Scenario: Vínculo a usuario existente

- GIVEN el modal de usuarios
- WHEN se selecciona un usuario para sincronizar
- THEN se vincula un Usuario pre-existente (no se crea uno nuevo)

### Requirement: VPU-006 — Sin cambios en token/auth/claims

El sistema MUST NOT modificar el token OAuth ni los claims; la relación Persona↔Usuario MUST resolverse sin añadir claims al token.

#### Scenario: Sesión sin claim personaId

- GIVEN un usuario autenticado
- WHEN inicia sesión
- THEN el token/claims no incluyen `personaId` y la relación se deduce por `Usuarios.PersonaId`
