# Guía funcional y flujo operativo de HelpDesk

## 1. Propósito y audiencia

HelpDesk concentra la atención de solicitudes internas, la administración de inventario tecnológico y la configuración operativa por empresa. Su finalidad es que cada empresa registre sus recursos, atienda incidencias con trazabilidad y consulte el desempeño de su operación sin mezclar información con otras empresas.

Esta guía está dirigida a patrocinadores de producto, administradores, solicitantes, agentes de atención, responsables de área, supervisores y analistas de QA. Describe el comportamiento evidenciado estáticamente y las capacidades definidas en OpenSpec; no sustituye la aceptación en un ambiente operativo.

### Mapa simple de punta a punta

| Momento | Actor principal | Resultado esperado |
|---|---|---|
| Alta de empresa | Representante o administrador inicial | Empresa registrada, administrador inicial y datos base provisionados. |
| Preparación | Administrador | Catálogos, personal, usuarios, roles, permisos, horarios y activos preparados. |
| Solicitud | Solicitante | Ticket con folio, clasificación, urgencia y, opcionalmente, evidencias. |
| Atención | Agente o responsable de área | Ticket tomado, resuelto, retomado o reasignado con historial. |
| Validación | Solicitante | Ticket cerrado o rechazado para que el agente lo retome. |
| Control | Administrador o supervisor | Consulta de indicadores, asignaciones de activos y configuración de la empresa. |

## 2. Cómo leer el estado

| Marca | Significado |
|---|---|
| **Confirmado por evidencia estática** | Código, migración y/o reporte de verificación muestran que el flujo fue implementado. Puede tener una migración aplicada documentada. |
| **Definido o planeado por OpenSpec** | Existe una propuesta o especificación, pero no hay evidencia suficiente para afirmarlo como disponible en operación. |
| **Verificación operativa pendiente** | El flujo requiere prueba manual en un ambiente con datos, permisos, correo y almacenamiento reales. |

La evidencia estática confirma intención e implementación, no aceptación de producción. En particular, no confirma credenciales, datos sembrados, permisos efectivamente asignados, entrega de correo, archivos escribibles ni que una migración se aplicó al ambiente que se liberará.

## 3. Actores y reglas transversales

| Actor | Alcance funcional |
|---|---|
| Administrador | Da de alta y organiza datos, configura permisos, administra activos, inicia desvinculaciones y puede configurar horario y logotipo cuando tiene autorización. |
| Solicitante o usuario básico | Crea tickets, consulta su detalle, cierra o rechaza los tickets propios resueltos y acepta/desvincula sus activos. |
| Agente | Toma tickets nuevos, resuelve los asignados y retoma tickets rechazados de su área. |
| Responsable de área | Mantiene responsables por categoría y puede reasignar tickets en progreso o rechazados cuando cuenta con permiso. |
| Supervisor | Consulta indicadores de su empresa; en la primera fase definida puede ver todas las áreas de su empresa. |

Reglas comunes:

1. La empresa de trabajo debe derivarse de la identidad autenticada, no de un valor elegido en pantalla. La separación estructural de datos está parcialmente documentada como completada y debe probarse con empresas A y B antes de liberar.
2. Los permisos se conceden por rol, página y acción: leer, crear, editar, eliminar y exportar. La fuente funcional prevista es la matriz de permisos por rol; que un botón no aparezca no sustituye el control al ejecutar la acción.
3. Los registros conservan trazabilidad de creación, modificación y estatus lógico donde aplica. Un registro dado de baja lógicamente deja de aparecer en consultas normales, pero puede conservar historial.
4. El acceso puede ser denegado aunque un usuario conozca una dirección directa. Existe un riesgo conocido: en tickets, algunos botones de acción pueden mostrarse a un agente o responsable que no posee permiso de edición; la operación debe ser denegada por el control de permisos. QA debe probar ambos casos.
5. Las listas históricas y los catálogos deben verificarse con volumen real; OpenSpec registra como pendiente la paginación e índices de rendimiento.

## 4. Identidad, sesión y acceso

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente. Algunas mejoras de robustez siguen planeadas.

### Inicio de sesión y vigencia

**Inicia:** cualquier usuario registrado.

**Precondiciones:** cuenta activa, credenciales válidas, empresa vigente y cliente autorizado.

1. El usuario captura sus credenciales en la pantalla de inicio.
2. El sistema valida la identidad y crea la sesión.
3. La sesión identifica al usuario, sus roles y su empresa de trabajo.
4. Si la empresa está en período de prueba vencido, se bloquea el inicio y se muestra un mensaje de expiración.
5. La navegación muestra solamente las páginas concedidas al rol.

**Resultado y evidencia:** sesión activa; identidad, roles y empresa asociados a la sesión.

**Excepciones:** credenciales inválidas no crean sesión; una sesión vencida debe redirigir al inicio antes de ejecutar una acción protegida. La respuesta de peticiones asíncronas ante expiración debe validarse manualmente.

### Recuperación y cambio de contraseña

**Estado:** Recuperación y restablecimiento están evidenciados en código; el endurecimiento uniforme de todos los caminos de cambio sigue parcialmente pendiente.

**Inicia:** usuario que olvidó su contraseña.

1. Solicita recuperación con sus datos de cuenta.
2. El sistema valida la solicitud y emite un enlace o token de recuperación de uso controlado.
3. El usuario abre la pantalla de recuperación, conoce las reglas visibles de contraseña y captura una nueva contraseña dos veces.
4. Si ambas capturas cumplen las reglas y el token es válido, la contraseña se restablece.
5. El usuario vuelve a iniciar sesión con la nueva contraseña.

**Resultado y evidencia:** token de recuperación, marca de uso y actualización de credencial.

**Excepciones y pendientes:** token inválido, expirado o ya usado debe impedir el cambio. La propuesta `contrasenas` documenta que el inicio y alta de empresa ya usan credenciales protegidas y que no se exponen en listados; sin embargo, identifica pendientes en actualización administrativa, restablecimiento y unificación de los caminos de cambio. No se debe considerar ese endurecimiento totalmente aceptado sin prueba de todos los caminos.

### Perfil propio

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

El usuario puede consultar su perfil y actualizar nombre, apellidos, celular, RFC, sucursal, área e imagen de perfil. El nombre de usuario y correo se muestran bloqueados. La imagen admite JPG o PNG y el límite visible es 2 MB. Al guardar se conserva el dato de empresa de la sesión.

## 5. Alta de empresa y preparación inicial

### Registro de empresa

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

**Inicia:** representante que aún no tiene cuenta.

**Precondiciones:** nombre comercial, razón social, RFC, responsable, dirección y correo de contacto. Ciudad, estado, código postal y teléfono son opcionales en el formulario.

1. El representante abre el registro público y captura los datos de negocio, ubicación y contacto.
2. El sistema valida campos obligatorios, formato de correo y RFC en mayúsculas.
3. El sistema valida que RFC, correo de contacto, nombre comercial y razón social no estén duplicados.
4. Si el registro procede, crea la empresa y sus datos iniciales; el administrador inicial recibe una credencial aleatoria por correo según la propuesta de contraseñas.
5. El representante inicia sesión y continúa con la preparación administrativa.

**Resultado y evidencia:** empresa, administrador inicial, roles base, área y sucursal iniciales, permisos y período de vigencia/prueba cuando corresponda.

**Excepciones:** datos duplicados o incompletos deben rechazar el registro. Si falla la preparación de datos iniciales, la empresa no debe quedar parcialmente configurada; el aprovisionamiento con transacción y plantilla de roles se reporta como realizado, pero requiere prueba de alta completa, correo y rollback ante falla.

### Período de prueba y suscripción

**Estado:** La validación de expiración de prueba está confirmada estáticamente. Un esquema de planes, renovaciones o facturación no existe: está fuera de alcance.

La empresa conserva fechas de vigencia y la marca de período de prueba. Si la prueba venció, el inicio de sesión debe bloquearse. No debe comunicarse como una suscripción comercial completa ni asumirse renovación automática.

### Datos base y catálogos organizacionales

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

El administrador prepara los catálogos que alimentan usuarios, inventario y tickets:

| Catálogo | Uso operativo |
|---|---|
| Áreas o departamentos | Clasifican responsables, usuarios y tickets. Un área puede tener un usuario responsable. |
| Sucursales | Ubicación organizacional para usuarios. |
| Puestos | Relación laboral de una persona. |
| Categorías y subcategorías | Clasifican tickets; una categoría pertenece a un área y puede contener subcategorías. |
| Responsables por categoría | Relacionan agentes capaces de atender una categoría; puede distinguirse un principal. |
| Tipos de activo, marcas y modelos | Clasifican el inventario; el modelo depende de la marca elegida. |
| Compañías | Catálogo simple separado de la empresa operativa; no se debe confundir con la empresa que delimita los datos. |

**Flujo general:** crear o editar un catálogo con permiso de crear/editar, usarlo en los formularios dependientes y aplicar baja lógica cuando se autorice eliminar. Si un catálogo falta, la captura dependiente debe detenerse con una selección pendiente o vacía.

## 6. Usuarios, personal, roles y permisos

### Usuarios y personal

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

El administrador administra dos conceptos distintos:

| Concepto | Propósito |
|---|---|
| Usuario | Cuenta que inicia sesión y recibe roles, permisos, sucursal y área. |
| Persona | Registro del colaborador, con puesto y datos de contacto; puede vincularse a un usuario. |

**Crear o editar usuario/persona:**

1. El administrador confirma que tiene permiso de crear para registros nuevos o editar para registros existentes.
2. Captura los datos requeridos y selecciona catálogos aplicables.
3. Guarda el registro y asigna roles al usuario según su función.
4. Si no tiene permiso de edición, los formularios existentes se muestran bloqueados; la operación del servidor también debe rechazar cambios directos.

**Resultado y evidencia:** usuario o persona activa, con sus datos organizacionales y permisos aplicables.

### Vínculo Persona-Usuario

**Estado:** Confirmado por evidencia estática y migración documentada; verificación operativa pendiente.

**Inicia:** administrador desde el catálogo de Personal.

**Precondiciones:** la persona y el usuario ya existen; una persona no está vinculada a otro usuario.

1. El administrador abre la persona y elige “Sincronizar con usuario”.
2. Selecciona un usuario existente en el listado mostrado.
3. El sistema advierte dos veces que los datos se sobrescribirán.
4. Al confirmar, vincula una sola persona con un solo usuario y sincroniza nombre, apellido, correo y teléfono desde la cuenta.
5. El nombre de usuario queda visible pero bloqueado; los datos sincronizados de la persona quedan sin edición. El puesto se conserva intacto.

**Resultado y evidencia:** relación uno a uno entre persona y usuario. Esta relación habilita “Mis Activos” y la aceptación de activos.

**Excepciones:** no se crea un usuario desde este flujo; seleccionar una persona ya vinculada debe rechazarse. OpenSpec advierte un caso a probar: el control de base impide dos usuarios para una persona, pero debe comprobarse que tampoco se permita reutilizar un mismo usuario en otra persona.

### Roles, páginas y acciones

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

**Inicia:** administrador con permiso de administración.

1. Crea o actualiza roles operativos, incluyendo la capacidad de atender tickets cuando aplique.
2. Abre la asignación de permisos por rol.
3. Selecciona páginas disponibles y marca acciones de leer, crear, editar, eliminar o exportar según corresponda.
4. Guarda los cambios y valida el menú con una cuenta del rol afectado.

**Resultado y evidencia:** matriz rol-página-acción. Las etiquetas de navegación pueden diferir de la clave interna de permiso; por ejemplo, las etiquetas visibles “Administración” y “Personal” no cambian el permiso subyacente.

**Excepciones:** un rol sin lectura no debe ver la página ni acceder directamente. Un usuario que solo tenga una asignación heredada de menú, sin el permiso por rol requerido, no debe obtener autorización para escribir.

## 7. Inventario y ciclo de activos

### Alta y control del activo

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

**Inicia:** administrador o personal autorizado de inventario.

1. Captura nombre, descripción, número de serie, serie interna, fecha de compra, tipo, marca, modelo y notas.
2. Al elegir marca, el sistema carga sus modelos disponibles.
3. Guarda el activo con permiso de creación o edición.

**Resultado y evidencia:** activo disponible o administrable dentro de la empresa.

**Reglas:** el número de serie debe ser único entre activos vigentes de la misma empresa; puede repetirse en otra empresa. Puede quedar vacío y un activo dado de baja lógicamente libera su número de serie. La serie interna es libre y puede repetirse. Las notas admiten varias líneas hasta 250 caracteres.

**Excepciones:** un número de serie duplicado debe rechazar el guardado con un mensaje claro. Si no hay modelos para una marca, el usuario no debe asumir que existe un modelo válido.

### Asignación y recepción

**Estado:** Confirmado por evidencia estática y migración documentada; verificación operativa pendiente, especialmente correo y autenticación.

**Inicia:** administrador desde los activos de una persona.

**Precondiciones:** activo disponible, persona activa y persona vinculada a un usuario.

1. El administrador selecciona una persona y abre sus activos.
2. Elige un activo disponible; la fecha inicial se establece automáticamente.
3. El sistema valida que el activo no esté asignado y que la persona tenga usuario vinculado.
4. Se genera una asignación pendiente y un enlace de confirmación.
5. Se envían dos correos: uno informativo al administrador, sin enlace; otro al usuario vinculado, con enlace de aceptación.
6. Cada intento de correo queda registrado con destinatario, asunto, fecha, resultado y error si existe.
7. El usuario abre el enlace, revisa quién le asignó qué activo y se autentica con sus propias credenciales.
8. Tras autenticarse correctamente, acepta la asignación y es dirigido a “Mis Activos”.

**Resultado y evidencia:** asignación con estado pendiente o aceptado, token de confirmación, fecha de inicio, fecha de confirmación al aceptar y bitácora de correo.

**Estados:** pendiente cuando aún no hay confirmación; aceptado cuando el titular confirma; desvinculado cuando se registra fecha de fin.

**Excepciones:** una persona sin usuario no puede recibir el activo. Un token inválido o mal formado no cambia nada. Credenciales erróneas no aceptan. Reabrir una aceptación ya atendida no cambia el estado. Si el correo falla, el sistema debe compensar desvinculando la asignación y devolver error, nunca éxito; QA debe comprobar también la bitácora y la compensación con SMTP inválido.

### Mis Activos y desvinculación

**Estado:** Confirmado por evidencia estática y migración documentada; verificación operativa pendiente.

**Inicia:** usuario básico o administrador, según la acción.

1. El usuario autenticado abre “Mis Activos”.
2. Ve únicamente sus asignaciones vigentes, separadas entre pendientes de aceptar y aceptadas.
3. Puede aceptar una pendiente directamente sin volver a introducir credenciales porque ya tiene sesión.
4. Para retirar un activo, el administrador inicia la desvinculación y confirma la acción.
5. El usuario recibe correo con enlace, abre la misma página de asignación en modo desvinculación, se autentica y confirma.

**Resultado y evidencia:** la aceptación fija la fecha de confirmación; la desvinculación fija la fecha de fin y conserva el historial.

**Excepciones:** un usuario sin persona vinculada ve una lista vacía, no un error de permiso. El administrador no puede aceptar o desvincular en nombre del titular. El token no expira según la especificación actual, por lo que debe evaluarse operativamente el riesgo de enlaces antiguos.

### Mantenimiento

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

**Inicia:** usuario autorizado desde el listado de activos.

1. Abre “Mantenimientos” sobre un activo.
2. Consulta la fecha actual en modo solo lectura y captura un comentario.
3. Guarda el mantenimiento.
4. Revisa el historial del activo, ordenado del más reciente al más antiguo.

**Resultado y evidencia:** comentario, fecha automática, autor y registro de mantenimiento asociado al activo y empresa.

**Excepciones:** la fecha no es capturable. Registros dados de baja lógica o sin fecha no aparecen en el historial. Un activo de otra empresa no debe exponer sus mantenimientos.

## 8. Tickets: captura, atención y cierre

### Creación, folio y evidencias

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

**Inicia:** solicitante con permiso para crear tickets.

**Precondiciones:** áreas, categorías y subcategorías disponibles; permisos de creación; almacenamiento de archivos operativo si se adjuntan evidencias.

1. El solicitante presiona “Nuevo Ticket”.
2. Consulta una vista previa del siguiente folio. La vista previa no reserva el número.
3. Selecciona área, categoría padre, subcategoría y urgencia: baja, media, alta o crítica.
4. Captura título de hasta 250 caracteres y descripción obligatoria.
5. Puede adjuntar evidencias. La interfaz documenta hasta tres archivos PDF, JPG o PNG de hasta 3 MB cada uno; los límites efectivos deben confirmarse contra configuración del ambiente.
6. Guarda el ticket y sus evidencias en una sola operación.

**Resultado y evidencia:** ticket nuevo, folio de formato `T-00001` secuencial por empresa, clasificación, urgencia, solicitante, fecha y metadatos de cada evidencia.

**Excepciones:** no se permite guardar sin clasificar o describir la solicitud. Si otro usuario guarda antes, el folio persistido puede diferir de la vista previa. Un fallo al guardar debe evitar un folio huérfano y no dejar evidencias sin ticket. Los tickets históricos pueden no tener folio y deben mostrarse sin error.

### Ciclo de vida

**Estado:** Confirmado por evidencia estática; verificación operativa pendiente.

| Estado o acción | Quién la inicia | Precondición | Resultado y evidencia |
|---|---|---|---|
| Nuevo | Solicitante | Ticket creado | Queda disponible sin agente. |
| Tomar | Agente | Ticket nuevo sin agente y permiso aplicable | Pasa a En Progreso y registra movimiento. |
| Resolver | Agente asignado | Ticket En Progreso | Pasa a Resuelto con comentario obligatorio de hasta 300 caracteres. |
| Cerrar | Solicitante creador | Ticket Resuelto | Pasa a Cerrado y queda constancia del actor. |
| Rechazar | Solicitante creador | Ticket Resuelto | Pasa a Rechazado con comentario obligatorio de hasta 300 caracteres. |
| Retomar | Agente | Ticket Rechazado dentro de su área | Vuelve a En Progreso y registra el movimiento. |
| Reasignar | Responsable de área | Ticket En Progreso o Rechazado | Selecciona un agente de la misma área; el ticket queda En Progreso. |

Reglas de operación:

1. El ticket se captura solo para alta. Después se consulta en modo detalle; no se presenta edición funcional del contenido ni eliminación en la interfaz.
2. Tomar, resolver, retomar, reasignar, cerrar y rechazar generan un historial unificado con actor, fecha, acción, estado resultante y comentario cuando aplica.
3. Solo la asignación vigente identifica al agente actual. La tabla resalta visualmente los tickets asignados al usuario actual.
4. “Rechazado” es el nombre operativo del estado que antes pudo identificarse como reabierto en datos históricos.
5. El detalle permite revisar clasificación, solicitante, agente, estatus, evidencias e historial, pero no modificar el ticket.

**Excepciones:** un agente no puede tomar un ticket que ya tiene asignación activa; quien no es solicitante no puede cerrar ni rechazar; un comentario vacío o mayor a 300 caracteres bloquea resolver o rechazar. La reasignación o las acciones de agente pueden verse en pantalla aun sin permiso de edición por una discrepancia documentada; el servidor debe denegarlas y la matriz real de permisos debe corregirse o probarse antes de operación.

### Notificaciones de tickets

**Estado:** Existe plantilla para cambios de estado y evidencia de archivos adjuntos; no hay confirmación suficiente de una notificación funcional completa a responsables de área.

La plantilla de correo de cambio de estatus está presente. La propuesta del ciclo de tickets dejó explícitamente fuera de alcance notificar por correo al responsable de área. Por ello, no debe asumirse que cada transición de ticket envía correo hasta que QA lo confirme por acción y destinatario.

## 9. Configuración de la empresa

**Estado:** Implementación estática confirmada. El reporte archivado registra la migración como pendiente de aplicación manual y contiene una nota posterior contradictoria sobre su aplicación; por seguridad funcional, la disponibilidad en el ambiente objetivo es **verificación operativa pendiente**.

**Inicia:** administrador con lectura y edición de “Configuración de Empresa”.

### Consulta de información general

La página muestra, sin permitir edición, nombre comercial, razón social, RFC, responsable, dirección, ciudad, estado, código postal, teléfono, correo de contacto, vigencia y condición de prueba de la empresa autenticada. Un usuario sin lectura no debe ver el menú ni acceder por una dirección directa.

### Horario laboral

1. El administrador abre la configuración y revisa siete filas, de lunes a domingo.
2. Para cada día marca si se labora e indica inicio y fin mediante hora, minutos en intervalos de cinco y AM/PM.
3. Guarda toda la semana como una sola operación.

**Resultado y evidencia:** horario semanal de la empresa. Por defecto se plantea lunes a viernes de 09:00 a 17:00 y fin de semana no laborable; este horario también alimenta el cálculo de horas hábiles en Estadísticas.

**Excepciones:** un día laboral con fin menor o igual al inicio se rechaza y no se guarda ningún día. No se permiten turnos que crucen medianoche. Un día no laborable debe guardar sus horas vacías.

### Logotipo

1. El administrador selecciona un archivo SVG o PNG.
2. Revisa una vista previa y guarda.
3. El sistema valida formato, tipo, contenido y tamaño máximo configurado; el valor de referencia documentado es 2 MB.
4. El menú lateral muestra el logotipo de la empresa si existe o la imagen institucional de respaldo si no existe.
5. El administrador puede quitarlo tras confirmar; el menú vuelve al respaldo.

**Resultado y evidencia:** referencia al archivo del logotipo de la empresa.

**Excepciones:** archivo vacío, con extensión o tipo no permitido, o excedido debe rechazarse. Algunos navegadores pueden reportar SVG con un tipo genérico; esta compatibilidad requiere prueba manual.

## 10. Tableros, estadísticas y seguimiento

### Inicio operativo

**Estado:** Confirmado por evidencia estática; el contenido de inicio fuera de tarjetas de agente contiene bloques de demostración deshabilitados y no debe presentarse como funcionalidad disponible.

Para un agente, el inicio muestra tarjetas semanales de tickets activos, resueltos, asignados al usuario y cerrados. Son indicadores de consulta; deben validarse contra datos reales de la empresa.

### Estadísticas de desempeño

**Estado:** Confirmado por evidencia estática y migración manual documentada; verificación operativa pendiente.

**Inicia:** administrador, supervisor o jefe de área autorizado. El jefe de área se determina por ser responsable de al menos un área, no por un rol independiente.

1. El usuario abre “Estadísticas” desde el menú de primer nivel si su rol tiene lectura.
2. Define fecha inicial y final. El valor inicial propuesto es 1 de enero del año en curso hasta hoy; no se aceptan fechas futuras ni inicio posterior al fin.
3. Consulta las tarjetas: total, nuevos, en progreso, resueltos, cerrados, rechazados, eficiencia y tiempo promedio de resolución en horas hábiles.
4. Revisa distribución por estatus, evolución diaria de creados y resueltos, ranking completo de áreas con actividad y rankings de reasignaciones recibidas o retiradas.
5. Ajusta el rango o recarga para actualizar la información. No hay edición, exportación ni actualización automática en tiempo real.

**Resultado y evidencia:** métricas solo de la empresa del usuario. La eficiencia es cerrados entre total; los conteos agrupan los tickets creados en el rango por su estatus actual. El tiempo de resolución toma la jornada configurada y excluye días no laborables; si no existe horario, se documenta respaldo de lunes a viernes, 09:00 a 17:00.

**Excepciones:** sin tickets, las tarjetas muestran cero y las gráficas muestran un estado sin datos. Sin reasignaciones aparece un mensaje específico. El ranking de reasignaciones excluye la toma inicial y limita cada lista al máximo configurable, con referencia de cinco. El supervisor ve todas las áreas de su empresa en la primera fase definida; el alcance de un jefe de área debe validarse manualmente.

## 11. Secuencia diaria recomendada

1. El administrador registra la empresa o confirma que su período de vigencia permite iniciar sesión.
2. El administrador valida que existan área, sucursal, puesto, categorías/subcategorías, responsables de categoría y catálogos de activos.
3. El administrador crea usuarios, personas y roles; vincula persona con usuario para quienes recibirán activos o usarán “Mis Activos”.
4. El administrador asigna permisos por rol y comprueba el menú y accesos directos con una cuenta de cada rol.
5. El administrador registra activos y, si corresponde, los asigna. El titular recibe el correo, se autentica y confirma la recepción.
6. El solicitante crea un ticket clasificado, adjunta evidencias cuando sean necesarias y conserva el folio realmente asignado.
7. El agente toma el ticket, trabaja en él y lo resuelve dejando un comentario útil. El responsable reasigna solo cuando corresponde a su área.
8. El solicitante consulta el detalle: cierra si el resultado es aceptable o rechaza con explicación para que el agente lo retome.
9. El supervisor o administrador revisa Estadísticas en un rango acordado, detecta volumen, urgencia, tiempos hábiles y reasignaciones.
10. El administrador revisa bitácora de correo ante fallas de asignación, mantiene el horario y conserva la identidad visual de su empresa.

## 12. Contexto técnico mínimo

La aplicación tiene una interfaz web que llama a servicios y estos a la base de datos. Esta separación importa operativamente porque permisos, empresa activa, correo, archivos y base de datos deben estar disponibles y coherentes para completar un flujo. No es necesario conocer su implementación para usar la guía; sí es necesario probar los recorridos completos, incluida la denegación de acceso y los fallos de infraestructura.

## 13. Dependencias, preguntas abiertas y salida a operación

| Tema | Situación y acción operativa recomendada |
|---|---|
| Separación entre empresas | Hay avances estructurales documentados, pero deben ejecutarse pruebas cruzadas de lectura, escritura, archivos, activos, tickets y métricas entre empresas A y B. |
| Migración de configuración | Confirmar en el ambiente objetivo tablas, permisos, horario, página de configuración y almacenamiento de logotipos; los artefactos contienen estado documental contradictorio. |
| Correo | Validar SMTP, destinatarios, plantillas, bitácora y compensación de asignación cuando el envío falla. |
| Archivos | Confirmar permisos de escritura, límites, respaldo, retención y protección de evidencias, fotos y logotipos. |
| Permisos | Aprobar una matriz por rol y probar menú, acceso directo y operación. Corregir o aceptar explícitamente la discrepancia de visibilidad de acciones de tickets. |
| Contraseñas | Completar y probar la protección uniforme de alta administrativa, cambio y restablecimiento; definir política de longitud y complejidad con negocio. |
| Rendimiento y errores | Acordar volumen, paginación, tiempos de respuesta y manejo consistente de errores antes de operación masiva. |
| Suscripción | Definir si se requiere facturación, renovación o planes. Hoy solo existe control del período de prueba. |

### Recomendación de despliegue operativo

1. Preparar un ambiente QA con dos empresas, datos representativos y cuentas para cada actor.
2. Aplicar y registrar la versión de cada migración requerida; comprobar catálogos, permisos y datos iniciales antes de probar la interfaz.
3. Ejecutar los flujos de esta guía en orden: acceso, alta de empresa, catálogos, usuarios/personas, activos, tickets, configuración y estadísticas.
4. Ejecutar casos negativos: sin permiso, sesión vencida, prueba vencida, duplicado de serial, persona sin usuario, correo fallido, token inválido, ticket ajeno y rango de fechas inválido.
5. Conservar evidencia de aceptación por caso: actor, empresa, datos de prueba, resultado esperado, resultado real, capturas y bitácora de correo cuando aplique.
6. Autorizar salida solo si no hay mezcla entre empresas, las denegaciones funcionan, los flujos críticos terminan y las dependencias de correo/archivos están confirmadas.

## 14. Trazabilidad OpenSpec

| Artefacto exacto | Cobertura en esta guía | Estado utilizado |
|---|---|---|
| `specs/vinculacion-persona-usuario` | Vínculo uno a uno y sincronización de datos. | Confirmado estáticamente; prueba pendiente. |
| `specs/mis-activos` | Consulta y aceptación autenticada desde Mis Activos. | Confirmado estáticamente; prueba pendiente. |
| `specs/confirmacion-recepcion-activo` | Aceptación y desvinculación autenticadas mediante enlace. | Confirmado estáticamente; prueba pendiente. |
| `specs/notificacion-asignacion-activo` | Correo dual, bitácora y compensación por fallo. | Confirmado estáticamente; prueba de correo pendiente. |
| `specs/campos-activo` y `specs/serial-unico-activo` | Serie interna, notas y unicidad de serial. | Confirmado estáticamente; prueba pendiente. |
| `specs/mantenimiento-activo` | Registro e historial de mantenimiento. | Confirmado estáticamente; prueba pendiente. |
| `specs/foliador-tickets` | Folio secuencial por empresa. | Confirmado estáticamente; prueba de concurrencia pendiente. |
| `changes/tickets-ciclo-vida` y sus specs `ticket-captura`, `ticket-ciclo-vida`, `ticket-detalle` | Captura, evidencias, detalle, historial y transiciones del ticket. | Verificación estática PASS WITH WARNINGS; prueba por rol pendiente. |
| `changes/evidencias-tickets` | Persistencia y aislamiento de evidencias. | Evidencia de migración; prueba de archivos pendiente. |
| `specs/menu-etiquetas` y `specs/permisos-edicion-usuarios-personas` | Etiquetas de menú y bloqueo por acciones de permiso. | Confirmado estáticamente; prueba pendiente. |
| `changes/autorizacion-e2e`, `changes/sesion-expiracion` y sus specs | Identidad, sesión, autorización y páginas públicas acotadas. | Implementado según artefactos; prueba de acceso pendiente. |
| `changes/tenant-isolation`, `changes/tenant-estructural` y spec `multi-tenancy` | Separación por empresa. | Contención y avance estructural documentados; prueba A/B obligatoria. |
| `changes/provisioning-template` y `changes/bugs-bd` | Datos iniciales y correcciones que habilitan el alta y atención. | Reportados como realizados; prueba de alta completa pendiente. |
| `specs/configuracion-empresa` y archivo `archive/2026-09-07-configuracion-empresa` | Horario, logotipo y datos generales. | Implementación estática; migración/disponibilidad a confirmar. |
| `specs/metricas-desempeno` y archivo `archive/2026-09-08-metricas-desempeno` | Panel de Estadísticas. | Migración manual documentada; smoke operativo pendiente. |
| `changes/trial-info-disclosure` | Bloqueo de prueba vencida. | Reportado como realizado; prueba pendiente. |
| `changes/contrasenas` | Inicio, alta y recuperación de contraseña; pendientes de uniformidad. | Parcial, según propuesta. |
| `changes/entidades-faltantes`, `changes/fk-escalares`, `changes/modelresponse-tipado`, `changes/mapeo-reflection` | Soporte a permisos, recuperación y consistencia de datos. | Cambios técnicos que respaldan los flujos; smoke pendiente donde se indica. |
| `changes/security-remediation`, `changes/webapi-review`, `changes/mvc-review`, `changes/database-review`, `changes/entities-review` | Riesgos, restricciones y pendientes operativos transversales. | Fuente de riesgos y seguimiento, no funcionalidad adicional. |
