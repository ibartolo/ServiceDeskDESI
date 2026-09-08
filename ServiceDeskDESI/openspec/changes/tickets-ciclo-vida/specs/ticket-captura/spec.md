# ticket-captura Specification

## Purpose

Captura de tickets desde un modal Bootstrap (solo alta) con validación de campos y catálogos en cascada. Un ticket, una vez creado, es inmutable.

## Requirements

### Requirement: Captura solo alta en modal

El sistema MUST presentar el formulario de captura en un modal Bootstrap (`_CapturarTicket.cshtml`) abierto por el botón "Nuevo Ticket". El formulario MUST permitir únicamente crear tickets; MUST NOT soportar modo edición.

#### Scenario: Crear ticket desde modal

- GIVEN un usuario autenticado con permiso de captura
- WHEN abre "Nuevo Ticket" y envía el formulario con datos válidos
- THEN el ticket se crea

#### Scenario: Sin modo edición

- GIVEN un ticket ya existente
- WHEN se intenta abrir el formulario en modo edición
- THEN no existe modo edición; solo es posible crear un ticket nuevo

### Requirement: Validación de campos

El sistema MUST validar al crear: `Título` (requerido, máx. 250), `Descripción` (requerida), y Área, Categoría, Subcategoría, Urgencia y Estatus (requeridos). La carga de catálogos MUST preservar la cascada Área → Categoría → Subcategoría.

#### Scenario: Título vacío

- GIVEN el formulario de captura abierto
- WHEN se envía sin `Título`
- THEN se muestra error de validación y el ticket no se crea

#### Scenario: Descripción vacía

- GIVEN el formulario de captura abierto
- WHEN se envía sin `Descripción`
- THEN se muestra error de validación y el ticket no se crea

#### Scenario: Cascada de catálogos

- GIVEN un Área seleccionada
- WHEN se cargan las opciones dependientes
- THEN solo se muestran las Categorías del área; y las Subcategorías de la categoría elegida

### Requirement: Inmutabilidad del ticket

Un ticket creado MUST NOT ser modificable tras su alta. No MUST existir acción de edición sobre el ticket.

#### Scenario: Ticket sin edición posterior

- GIVEN un ticket creado
- WHEN el usuario revisa las acciones disponibles
- THEN no existe acción de edición; el ticket es inmutable

### Requirement: Refresco y cierre del modal

Al crear con éxito, el sistema MUST refrescar la tabla de tickets y MUST cerrar y resetear el modal.

#### Scenario: Alta exitosa

- GIVEN un ticket creado con éxito
- WHEN la operación termina
- THEN la tabla se refresca y el modal se cierra y queda reseteado para el siguiente alta
