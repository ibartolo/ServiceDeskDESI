# permisos-edicion-usuarios-personas Specification

## Purpose

Bloqueo de la edición de Usuarios y Personas mediante la acción "Editar" del sistema de Permisos existente, gobernado por el rol del usuario logueado (sin flag en Rol). En modo edición (`Id > 0`) los inputs se deshabilitan cuando el usuario no tiene el permiso "Editar"; los registros nuevos (`Id == 0`) se permiten sujetos al permiso "Crear". La aplicación server-side ya está cubierta por los atributos `[Permiso]` y no se toca.

## Requirements

### Requirement: PEU-001 — Gate por permisos, sin flag en Rol

El sistema MUST controlar la edición de Usuarios/Personas mediante la acción "Editar" de las páginas "Usuarios"/"Personas" del sistema de permisos, usando el rol del usuario logueado. El sistema MUST NOT agregar un flag de edición en la entidad `Rol` ni en el SP `GuardarOActualizarRol`.

#### Scenario: Sin flag en Rol

- GIVEN el modelo `Rol` y el SP `GuardarOActualizarRol`
- WHEN se implementa el bloqueo de edición
- THEN no existe ningún flag nuevo en `Rol` ni cambios en el SP; el gate usa únicamente permisos

### Requirement: PEU-002 — Usuarios: inputs deshabilitados en edición sin "Editar"

`UserController` MUST poblar `ViewBag.Permisos` (vía un nuevo `ObtenerPermisosParaUsuarios()`, espejo de `ObtenerPermisosParaPersona`) y `Users.cshtml` MUST deshabilitar los inputs en modo edición (`Id > 0`) cuando `!permisos.PuedeEditar`.

#### Scenario: Edición bloqueada

- GIVEN un usuario logueado cuyo rol NO tiene la acción "Editar" en "Usuarios"
- WHEN abre la edición de un Usuario (`Id = 10`)
- THEN los inputs del formulario quedan `disabled` y no puede modificar datos

#### Scenario: Edición permitida

- GIVEN un usuario logueado cuyo rol SÍ tiene "Editar" en "Usuarios"
- WHEN abre la edición de un Usuario (`Id = 10`)
- THEN los inputs están habilitados para editar

### Requirement: PEU-003 — Personas: extiende el bloqueo de estaVinculada

`Persona.cshtml` MUST deshabilitar los campos cuando `estaVinculada || (Model.Id > 0 && !permisos.PuedeEditar)`, extendiendo la condición existente de vínculo.

#### Scenario: Persona vinculada sigue bloqueada

- GIVEN una Persona vinculada a un Usuario (`estaVinculada = true`)
- WHEN se muestra su formulario
- THEN los campos Nombre/Apellido/Correo/Teléfono permanecen `disabled`

#### Scenario: Persona no vinculada y sin permiso de edición

- GIVEN una Persona no vinculada (`Id = 20`) y un usuario logueado sin "Editar" en "Personas"
- WHEN se muestra el formulario
- THEN los campos quedan `disabled` aunque `estaVinculada = false`

### Requirement: PEU-004 — Creación sujeta a "Crear"

El sistema MUST permitir la creación de nuevos Usuarios/Personas (`Id == 0`) sujeta al permiso "Crear"; en creación los inputs MUST permanecer editables (salvo el bloqueo por `estaVinculada`).

#### Scenario: Creación permitida con "Crear"

- GIVEN un usuario logueado con "Crear" en "Usuarios"
- WHEN abre un Usuario nuevo (`Id = 0`)
- THEN los inputs están habilitados para captura

#### Scenario: Creación sin "Crear"

- GIVEN un usuario logueado sin "Crear" en "Usuarios"
- WHEN intenta crear un Usuario
- THEN el guardado está bloqueado y no puede registrar

### Requirement: PEU-005 — Aplicación server-side sin cambios

La aplicación server-side MUST permanecer cubierta por los atributos `[Permiso("Usuarios")]` / `[Permiso("Personas", "Editar")]` existentes; el bloqueo de UI MUST NOT ser el único control.

#### Scenario: Persistencia protegida por servidor

- GIVEN una solicitud de guardado de Usuario sin el permiso correspondiente
- WHEN llega al servidor
- THEN el atributo `[Permiso]` rechaza la operación independientemente de la UI
