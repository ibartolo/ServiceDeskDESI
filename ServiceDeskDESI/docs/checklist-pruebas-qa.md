# Checklist de Pruebas — ServiceDeskDESI

> Documento de trabajo para el equipo de pruebas (QA).
> Cubre **toda la funcionalidad documentada** en los specs del proyecto.
>
> **Cómo usarlo:** marca con una ✗ la casilla `[ ]` al ejecutar cada caso. Anota el
> resultado (OK / FALLA) y el número de incidente si aplica. Los casos marcados con
> la etiqueta **`[En curso]`** corresponden a funcionalidad cuya especificación está
> documentada pero cuya implementación puede estar todavía en despliegue; confirmar
> con el equipo de desarrollo antes de probar.

> **Cambios recientes (2026-09-01):** los módulos 22–26 cubren el cambio
> `mejoras-rol-activos-permisos` (bloqueo de edición de Usuarios/Personas por
> permisos, No. de Serie único por empresa en Activos, campos `SerieLocal` y `Notas`
> como textarea de 250, gestor de mantenimientos de Activos y correcciones en la
> ventana de Permisos). **Importante:** la migración de este cambio
> (`openspec/changes/archive/2026-09-01-mejoras-rol-activos-permisos/migration.sql`)
> debe aplicarse a la base de datos ANTES de probar estos módulos; sin ella, los
> casos de los módulos 22–25 fallarán por ausencia de columna/índice/tabla/SP.

---

## Resumen de cobertura

| # | Módulo | Casos |
|---|--------|-------|
| 1 | Autenticación (login) | 11 |
| 2 | Autorización y permisos | 11 |
| 3 | Aislamiento entre empresas (multi-empresa) | 7 |
| 4 | Sesión y expiración | 2 |
| 5 | Contraseñas seguras | 4 |
| 6 | Período de prueba (trial) | 3 |
| 7 | Tickets — captura | 8 |
| 8 | Tickets — catálogo de estatus | 1 |
| 9 | Tickets — transiciones del agente | 6 |
| 10 | Tickets — transiciones del solicitante | 4 |
| 11 | Tickets — reasignación | 4 |
| 12 | Tickets — historial | 2 |
| 13 | Tickets — bloqueo de "Tomar" | 1 |
| 14 | Tickets — detalle y acciones | 4 |
| 15 | Foliador de tickets | 6 |
| 16 | Menú y etiquetas (Personal / Administración) | 5 |
| 17 | Vinculación Persona ↔ Usuario | 6 |
| 18 | Notificación de asignación de activo | 6 |
| 19 | Mis Activos | 4 |
| 20 | Confirmación de recepción de activo | 8 |
| 21 | Correcciones de base de datos | 5 |
| 22 | Bloqueo de edición de Usuarios y Personas por permisos | 6 |
| 23 | No. de Serie único por empresa en Activos | 7 |
| 24 | Campos nuevos del Activo: SerieLocal y Notas (textarea 250) | 5 |
| 25 | Gestor de mantenimientos de Activos (modal) | 8 |
| 26 | Correcciones en ventana de Permisos: contador de páginas y tema oscuro | 5 |
| | **Total** | **~149** |

---

## 1. Autenticación (login)

**Objetivo:** validar que el acceso al sistema exige credenciales reales y que el
"pase de acceso" se entrega solo a clientes legítimos, por canal seguro y con la
identidad correcta del usuario.

- [ ] **AUT-01 — Login exitoso con roles reales**
  - *Pre:* existe un usuario válido con roles "Admin" y "Operador".
  - *Pasos:* iniciar sesión con credenciales correctas.
  - *Esperado:* el acceso se otorga; el usuario es reconocido con su nombre y sus roles reales (no un rol genérico).

- [ ] **AUT-02 — Login de usuario sin roles**
  - *Pre:* existe un usuario válido sin roles asignados.
  - *Pasos:* iniciar sesión.
  - *Esperado:* el acceso se otorga sin roles (no debe fallar la autenticación).

- [ ] **AUT-03 — Credenciales incorrectas**
  - *Pre:* usuario existente.
  - *Pasos:* iniciar sesión con contraseña incorrecta.
  - *Esperado:* se rechaza el acceso y se muestra error de autenticación.

- [ ] **AUT-04 — Cliente de aplicación válido**
  - *Pre:* la aplicación web usa credenciales de cliente correctas.
  - *Pasos:* iniciar sesión.
  - *Esperado:* la autenticación continúa con normalidad.

- [ ] **AUT-05 — Cliente de aplicación desconocido / secreto inválido**
  - *Pre:* credenciales de cliente incorrectas.
  - *Pasos:* solicitar acceso.
  - *Esperado:* la solicitud es rechazada.

- [ ] **AUT-06 — Acceso por canal seguro (HTTPS)**
  - *Pasos:* iniciar sesión sobre HTTPS.
  - *Esperado:* el acceso se otorga.

- [ ] **AUT-07 — Acceso por HTTP en producción**
  - *Pasos:* intentar iniciar sesión sobre HTTP (en producción).
  - *Esperado:* la solicitud es rechazada (no se permite conexión insegura).

- [ ] **AUT-08 — Origen (dominio) autorizado**
  - *Pasos:* llamar al servicio desde un dominio permitido.
  - *Esperado:* la respuesta permite el acceso desde ese dominio.

- [ ] **AUT-09 — Origen (dominio) no autorizado**
  - *Pasos:* llamar al servicio desde un dominio no permitido.
  - *Esperado:* el acceso desde ese dominio es denegado.

- [ ] **AUT-10 — Acciones de login/recuperación sin sesión**
  - *Pasos:* sin iniciar sesión, usar login, "olvidé mi contraseña" (validar token y restablecer).
  - *Esperado:* estas acciones funcionan sin sesión (son las únicas públicas).

- [ ] **AUT-11 — Lectura/escritura de usuarios sin sesión**
  - *Pasos:* sin iniciar sesión, intentar listar o modificar usuarios.
  - *Esperado:* se rechaza por falta de autenticación.

---

## 2. Autorización y permisos

**Objetivo:** verificar que cada acción de escritura (crear, editar, eliminar) solo la
ejecuta un usuario cuyo rol tenga el permiso, validado en el servidor (no solo
ocultando botones).

- [ ] **AUTZ-01 — Acción permitida por la matriz rol-página-acción**
  - *Pre:* rol con la acción habilitada.
  - *Pasos:* ejecutar la acción.
  - *Esperado:* se ejecuta correctamente.

- [ ] **AUTZ-02 — Página en el menú NO da permiso de operar**
  - *Pre:* usuario con la página en su menú, pero rol sin la acción.
  - *Pasos:* ejecutar una escritura.
  - *Esperado:* se deniega (tener la página visible no habilita la operación).

- [ ] **AUTZ-03 — Servicio: escritura con permiso**
  - *Pre:* usuario autenticado con permiso.
  - *Pasos:* invocar una escritura.
  - *Esperado:* se ejecuta.

- [ ] **AUTZ-04 — Servicio: escritura sin permiso**
  - *Pre:* usuario autenticado sin permiso.
  - *Pasos:* invocar una escritura.
  - *Esperado:* respuesta "prohibido" (403) y la acción no se ejecuta.

- [ ] **AUTZ-05 — Servicio: escritura sin autenticación**
  - *Pasos:* invocar una escritura sin iniciar sesión.
  - *Esperado:* respuesta "no autorizado" (401).

- [ ] **AUTZ-06 — Web: escritura con permiso**
  - *Pre:* usuario con sesión y permiso.
  - *Pasos:* ejecutar una escritura en la web.
  - *Esperado:* se ejecuta.

- [ ] **AUTZ-07 — Web: escritura sin permiso**
  - *Pre:* usuario con sesión sin permiso.
  - *Pasos:* ejecutar una escritura.
  - *Esperado:* se deniega (acceso denegado).

- [ ] **AUTZ-08 — Web: acción pública de la lista blanca**
  - *Pasos:* un usuario sin sesión invoca una acción declarada pública.
  - *Esperado:* se ejecuta sin autenticación.

- [ ] **AUTZ-09 — Web: acción no pública sin sesión**
  - *Pasos:* usuario sin sesión invoca una acción no listada.
  - *Esperado:* se redirige al login.

- [ ] **AUTZ-10 — Web: acción de escritura sin permiso con sesión**
  - *Pre:* usuario con sesión sin permiso.
  - *Pasos:* invocar escritura.
  - *Esperado:* se deniega por falta de permiso.

- [ ] **AUTZ-11 — Endpoint que antes era anónimo ya exige token**
  - *Pasos:* invocar sin autenticación un endpoint que antes era anónimo.
  - *Esperado:* respuesta 401 (ya no es accesible anónimamente).

---

## 3. Aislamiento entre empresas (multi-empresa)

**Objetivo:** garantizar que cada usuario solo vea y manipule la información de **su
propia empresa**, nunca la de otras.

- [ ] **MT-01 — Listados limitados a la propia empresa**
  - *Pre:* usuario de la empresa A.
  - *Pasos:* consultar un listado (p. ej. asignaciones usuario-página).
  - *Esperado:* solo se devuelven registros de la empresa A.

- [ ] **MT-02 — Consulta por identificador de otra empresa**
  - *Pre:* usuario de la empresa A; existe un registro de la empresa B.
  - *Pasos:* consultar ese registro por su identificador.
  - *Esperado:* no se devuelve el registro.

- [ ] **MT-03 — Buscar usuario de otra empresa por nombre**
  - *Pre:* usuario de la empresa A.
  - *Pasos:* buscar un nombre de usuario que pertenece a la empresa B.
  - *Esperado:* no se devuelve ese usuario.

- [ ] **MT-04 — Eliminar ticket de otra empresa**
  - *Pre:* usuario de la empresa A; existe un ticket de la empresa B.
  - *Pasos:* intentar eliminar el ticket ajeno.
  - *Esperado:* el ticket NO se modifica/elimina (resultado "sin cambios").

- [ ] **MT-05 — No existe listado del directorio de empresas**
  - *Pasos:* intentar acceder al listado completo de empresas.
  - *Esperado:* no existe (respuesta "no encontrado").

- [ ] **MT-06 — Alta de empresa con correo duplicado**
  - *Pre:* ya existe una empresa con un correo de contacto.
  - *Pasos:* registrar una nueva empresa con el mismo correo.
  - *Esperado:* se rechaza con mensaje de duplicidad.

- [ ] **MT-07 — El acceso identifica la empresa del usuario**
  - *Pre:* usuario con empresa asignada.
  - *Pasos:* iniciar sesión.
  - *Esperado:* el acceso queda asociado a la empresa correcta del usuario.

---

## 4. Sesión y expiración

- [ ] **SES-01 — Acceso con sesión expirada**
  - *Pre:* usuario cuya sesión ya expiró.
  - *Pasos:* intentar acceder a una acción protegida.
  - *Esperado:* se redirige al login y la acción NO se ejecuta.

- [ ] **SES-02 — Consulta de permisos sin error interno**
  - *Pasos:* invocar la consulta de permisos de un usuario.
  - *Esperado:* responde correctamente, sin error interno.

---

## 5. Contraseñas seguras

- [ ] **CON-01 — Login con credenciales correctas/incorrectas**
  - *Pasos:* probar contraseña correcta e incorrecta.
  - *Esperado:* funciona/deniega correctamente.

- [ ] **CON-02 — Registro de empresa: contraseña aleatoria**
  - *Pasos:* dar de alta una empresa nueva.
  - *Esperado:* el correo de bienvenida incluye una contraseña aleatoria que permite iniciar sesión.

- [ ] **CON-03 — La contraseña no se muestra en pantalla**
  - *Pasos:* abrir la edición/consulta de un usuario.
  - *Esperado:* la contraseña NO aparece en ningún campo.

- [ ] **CON-04 — Usuarios existentes siguen iniciando sesión**
  - *Pre:* usuarios creados antes de este cambio.
  - *Pasos:* iniciar sesión.
  - *Esperado:* siguen pudiendo entrar (compatibilidad).

---

## 6. Período de prueba (trial)

- [ ] **TRIAL-01 — Empresa con prueba vencida no puede entrar**
  - *Pre:* empresa con período de prueba vencido.
  - *Pasos:* iniciar sesión.
  - *Esperado:* se bloquea el acceso con mensaje "el período de prueba ha expirado".

- [ ] **TRIAL-02 — Empresa con prueba vigente opera con normalidad**
  - *Pre:* empresa con prueba vigente.
  - *Pasos:* iniciar sesión y operar.
  - *Esperado:* funciona con normalidad.

- [ ] **TRIAL-03 — Los errores no exponen detalles técnicos**
  - *Pasos:* provocar un error en producción.
  - *Esperado:* no se muestran rastros técnicos ni detalles de configuración.

---

## 7. Tickets — captura

**Objetivo:** verificar el registro de tickets nuevos y su inmutabilidad.

- [ ] **CAP-01 — Crear ticket desde el modal**
  - *Pre:* usuario con permiso de captura.
  - *Pasos:* "Nuevo Ticket" → completar Área, Categoría, Subcategoría, Urgencia, Título y Descripción → enviar.
  - *Esperado:* el ticket se crea y aparece en la tabla.

- [ ] **CAP-02 — No existe modo edición**
  - *Pasos:* buscar cualquier vía para editar un ticket existente.
  - *Esperado:* no existe ninguna opción de edición; solo se crean tickets nuevos.

- [ ] **CAP-03 — Título vacío**
  - *Pasos:* enviar el formulario sin Título.
  - *Esperado:* error de validación; no se crea.

- [ ] **CAP-04 — Descripción vacía**
  - *Pasos:* enviar sin Descripción.
  - *Esperado:* error de validación; no se crea.

- [ ] **CAP-05 — Título con más de 250 caracteres**
  - *Pasos:* escribir un Título de 251+ caracteres y enviar.
  - *Esperado:* se bloquea o muestra error; no se crea (máximo 250).

- [ ] **CAP-06 — Cascada Área → Categoría → Subcategoría**
  - *Pasos:* elegir un Área y observar Categorías; elegir una Categoría y observar Subcategorías.
  - *Esperado:* solo se muestran las categorías del área y las subcategorías de la categoría elegida.

- [ ] **CAP-07 — Ticket inmutable**
  - *Pasos:* revisar acciones disponibles sobre un ticket creado.
  - *Esperado:* no existe edición posterior.

- [ ] **CAP-08 — Alta exitosa: refresco, cierre y reseteo del modal**
  - *Pasos:* enviar con datos válidos.
  - *Esperado:* la tabla se refresca; el modal se cierra y queda limpio para el siguiente alta.

---

## 8. Tickets — catálogo de estatus

- [ ] **EST-01 — El estatus 4 se llama "Rechazado"**
  - *Pre:* un ticket con estatus 4.
  - *Pasos:* observar el estatus en la tabla y en el detalle/historial.
  - *Esperado:* se muestra "Rechazado" (nunca "Reabierto").

---

## 9. Tickets — transiciones del agente (Tomar / Resolver / Retomar)

- [ ] **AGE-01 — Tomar ticket nuevo**
  - *Pre:* ticket "Nuevo" sin agente, en el área del agente.
  - *Pasos:* pulsar "Tomar".
  - *Esperado:* pasa a "En Progreso" y el agente queda asignado.

- [ ] **AGE-02 — Resolver con comentario válido**
  - *Pre:* ticket "En Progreso" asignado al agente.
  - *Pasos:* "Resolver" → comentario de 1–300 caracteres → confirmar.
  - *Esperado:* pasa a "Resuelto" y el comentario queda en el historial.

- [ ] **AGE-03 — Resolver sin comentario**
  - *Pasos:* "Resolver" y confirmar sin comentario.
  - *Esperado:* la transición se rechaza; sigue "En Progreso".

- [ ] **AGE-04 — Resolver con comentario de más de 300 caracteres**
  - *Pasos:* "Resolver" con comentario de 300+ caracteres.
  - *Esperado:* se rechaza; no cambia de estatus.

- [ ] **AGE-05 — Resolver un ticket ajeno**
  - *Pre:* ticket "En Progreso" asignado a otro agente.
  - *Pasos:* intentar resolverlo.
  - *Esperado:* la acción "Resolver" no está disponible.

- [ ] **AGE-06 — Retomar ticket rechazado**
  - *Pre:* ticket "Rechazado" en el área del agente.
  - *Pasos:* pulsar "Retomar".
  - *Esperado:* pasa a "En Progreso".

---

## 10. Tickets — transiciones del solicitante (Cerrar / Rechazar)

- [ ] **SOL-01 — Cerrar ticket resuelto propio**
  - *Pre:* ticket "Resuelto" cuyo creador es el usuario actual.
  - *Pasos:* "Cerrar" y confirmar.
  - *Esperado:* pasa a "Cerrado".

- [ ] **SOL-02 — Rechazar ticket resuelto propio con comentario**
  - *Pasos:* "Rechazar" → comentario de 1–300 caracteres → confirmar.
  - *Esperado:* pasa a "Rechazado" y el comentario queda registrado.

- [ ] **SOL-03 — Rechazar sin comentario o con más de 300**
  - *Pasos:* "Rechazar" sin comentario; luego con 300+ caracteres.
  - *Esperado:* se rechaza en ambos casos; sigue "Resuelto".

- [ ] **SOL-04 — Cerrar/Rechazar un ticket ajeno**
  - *Pre:* ticket "Resuelto" cuyo creador NO es el usuario actual.
  - *Pasos:* intentar Cerrar o Rechazar.
  - *Esperado:* las acciones no están disponibles.

> **Nota de verificación:** confirmar con desarrollo si "Cerrar" exige o no comentario
> (la especificación lo define sin comentario; la implementación final puede exigirlo).
> Ajustar SOL-01 en consecuencia.

---

## 11. Tickets — reasignación (responsable de área)

- [ ] **REA-01 — Reasignar ticket**
  - *Pre:* responsable del área; ticket "En Progreso" o "Rechazado" de su área.
  - *Pasos:* "Reasignar" → elegir usuario del área → confirmar.
  - *Esperado:* pasa a "En Progreso" con nueva asignación activa; la anterior queda cerrada.

- [ ] **REA-02 — Lista de destinos limitada a agentes del área**
  - *Pasos:* abrir el modal de reasignación.
  - *Esperado:* solo se listan agentes del área (misma empresa); no usuarios de otras áreas.

- [ ] **REA-03 — Reasignar con y sin comentario**
  - *Pasos:* reasignar con comentario (≤ 300) y, en otra prueba, sin comentario.
  - *Esperado:* ambas se completan; el comentario (si existe) queda en el historial.

- [ ] **REA-04 — Reasignar no disponible fuera de estatus 2/4 o sin ser responsable**
  - *Pre:* ticket en estatus 1, 3 o 5; o usuario no responsable.
  - *Pasos:* revisar acciones.
  - *Esperado:* "Reasignar" no se muestra.

---

## 12. Tickets — historial

- [ ] **HIS-01 — Movimiento registrado**
  - *Pre:* se ejecuta una transición (p. ej. Tomar o Resolver).
  - *Pasos:* abrir el detalle/historial.
  - *Esperado:* aparece el movimiento con su tipo y estatus resultante.

- [ ] **HIS-02 — Única asignación activa**
  - *Pre:* ticket con varias transiciones.
  - *Pasos:* revisar historial.
  - *Esperado:* solo una fila queda activa en cada momento; tras Cerrar/Rechazar no queda agente activo.

---

## 13. Tickets — bloqueo de "Tomar"

- [ ] **BLO-01 — Ticket ya tomado no se puede volver a tomar**
  - *Pre:* ticket con asignación activa.
  - *Pasos:* otro agente intenta tomarlo.
  - *Esperado:* "Tomar" no está disponible para ese agente.

---

## 14. Tickets — detalle y acciones

- [ ] **DET-01 — Ver detalle e historial**
  - *Pasos:* pulsar "Ver" sobre un ticket.
  - *Esperado:* se muestra el detalle y el historial completo en solo lectura.

- [ ] **RES-01 — Ticket "mío" resaltado**
  - *Pre:* ticket cuyo agente asignado es el usuario actual.
  - *Pasos:* abrir la tabla.
  - *Esperado:* el ticket se resalta como "mío" conservando su badge de estatus.

- [ ] **RES-02 — Ticket de otro agente sin resaltado**
  - *Pre:* ticket de otro agente.
  - *Pasos:* abrir la tabla.
  - *Esperado:* se muestra sin el resaltado "mío".

- [ ] **ACC-01 — Acciones disponibles (sin Editar ni Eliminar)**
  - *Pasos:* observar las acciones de un ticket listado.
  - *Esperado:* "Editar" no existe, "Eliminar" no se muestra; sí está "Ver".

---

## 15. Foliador de tickets

**Objetivo:** verificar el folio único y secuencial (`T-00001`) por empresa.

- [ ] **FOL-01 — Primer folio**
  - *Pre:* empresa sin tickets.
  - *Pasos:* guardar el primer ticket.
  - *Esperado:* queda con folio `T-00001`.

- [ ] **FOL-02 — Folios secuenciales**
  - *Pasos:* guardar varios tickets.
  - *Esperado:* folios consecutivos (`T-00001`, `T-00002`, …).

- [ ] **FOL-03 — Aislamiento del folio entre empresas**
  - *Pasos:* generar tickets en la empresa A y en la empresa B.
  - *Esperado:* el consecutivo de cada empresa es independiente (B no se ve afectado por A).

- [ ] **FOL-04 — Vista previa del folio en captura**
  - *Pasos:* abrir la captura de un ticket nuevo.
  - *Esperado:* se muestra el siguiente folio en un campo de solo lectura (deshabilitado).

- [ ] **FOL-05 — Tickets históricos sin folio**
  - *Pre:* tickets creados antes de esta funcionalidad.
  - *Pasos:* ver su detalle.
  - *Esperado:* no muestran folio (vacío) sin error.

- [ ] **FOL-06 — Formato del folio**
  - *Pasos:* generar folios con más de 4 dígitos (p. ej. 1000).
  - *Esperado:* el formato es `T-` + 5 dígitos (`T-01000`).

---

## 16. Menú y etiquetas (Personal / Administración)

- [ ] **MEN-01 — El menú muestra "Personal"**
  - *Pasos:* abrir el menú lateral.
  - *Esperado:* se muestra "Personal" en lugar de "Personas".

- [ ] **MEN-02 — El menú muestra "Administración"**
  - *Pasos:* abrir el menú lateral.
  - *Esperado:* se muestra "Administración" en lugar de "Usuarios".

- [ ] **MEN-03 — Los permisos no se rompen tras el renombre**
  - *Pre:* usuario con permiso sobre "Personas"/"Usuarios".
  - *Pasos:* acceder a esas secciones después del renombre.
  - *Esperado:* el acceso sigue funcionando (la llave de permisos no cambió).

- [ ] **MEN-04 — El resto de ítems no cambia**
  - *Pasos:* revisar otros ítems del menú (p. ej. "Áreas").
  - *Esperado:* su etiqueta sigue igual.

- [ ] **MEN-05 — El selector de permisos sigue mostrando la llave**
  - *Pasos:* abrir la pantalla de permisos.
  - *Esperado:* muestra "Personas"/"Usuarios" (la llave), no las etiquetas visibles.

---

## 17. Vinculación Persona ↔ Usuario

- [ ] **VPU-01 — Botón "Sincronizar con usuario" abre el modal**
  - *Pasos:* en la vista de una Persona, pulsar el botón.
  - *Esperado:* se abre un modal con la tabla de usuarios (nombre de usuario, nombre, apellido, correo) y botón Sincronizar.

- [ ] **VPU-02 — Advertencia de sobrescritura**
  - *Pasos:* pulsar Sincronizar y luego guardar.
  - *Esperado:* se muestra la advertencia "los datos se sobreescribirán" en el modal y antes de guardar.

- [ ] **VPU-03 — Campos bloqueados tras sincronizar**
  - *Pre:* sincronización aceptada.
  - *Pasos:* ver la vista de la Persona.
  - *Esperado:* username bloqueado; Nombre, Apellido, Correo y Teléfono deshabilitados con los datos del Usuario.

- [ ] **VPU-04 — Puesto intacto**
  - *Pre:* sincronización aceptada.
  - *Pasos:* guardar la Persona.
  - *Esperado:* el Puesto no cambia.

- [ ] **VPU-05 — Solo se vincula usuario existente**
  - *Pasos:* seleccionar un usuario para sincronizar.
  - *Esperado:* se vincula un usuario ya existente (no se crea uno nuevo).

- [ ] **VPU-06 — Una persona no puede tener dos usuarios**
  - *Pre:* Persona ya vinculada a un Usuario.
  - *Pasos:* intentar vincularla a otro Usuario.
  - *Esperado:* se rechaza (relación 1:1).

---

## 18. Notificación de asignación de activo

- [ ] **NAA-01 — Asignación envía dos correos**
  - *Pasos:* asignar un activo a una persona con usuario.
  - *Esperado:* se envían 2 correos: uno informativo al administrador (sin liga) y otro al usuario (con liga de aceptación).

- [ ] **NAA-02 — La liga lleva a la página de aceptación**
  - *Pasos:* abrir la liga del correo al usuario.
  - *Esperado:* lleva a la página de aceptación del activo.

- [ ] **NAA-03 — Fallo de correo deshace la asignación**
  - *Pre:* correo configurado de forma incorrecta.
  - *Pasos:* asignar un activo.
  - *Esperado:* la asignación se deshace y se muestra error (no queda huérfana).

- [ ] **NAA-04 — Persona sin usuario vinculado**
  - *Pre:* persona sin usuario.
  - *Pasos:* intentar asignarle un activo.
  - *Esperado:* no se asigna y se muestra "persona sin usuario vinculado".

- [ ] **NAA-05 — Correo de desvinculación**
  - *Pasos:* el administrador inicia la desvinculación.
  - *Esperado:* el usuario recibe un correo con la liga de desvinculación.

- [ ] **NAA-06 — Bitácora de envíos**
  - *Pasos:* revisar la bitácora tras envíos exitosos y fallidos.
  - *Esperado:* cada intento queda registrado con destinatario, asunto y estado (Enviado/Fallido).

---

## 19. Mis Activos

- [ ] **MA-01 — El usuario ve "Mis Activos" en el menú**
  - *Pre:* rol con permiso de lectura.
  - *Pasos:* iniciar sesión.
  - *Esperado:* aparece el menú "Mis Activos".

- [ ] **MA-02 — Sin permiso no se ve el menú**
  - *Pre:* rol sin permiso.
  - *Pasos:* iniciar sesión.
  - *Esperado:* no aparece el menú.

- [ ] **MA-03 — Activos vigentes y por aceptar**
  - *Pasos:* abrir "Mis Activos".
  - *Esperado:* se listan los activos vigentes (aceptados) y los "por aceptar".

- [ ] **MA-04 — Aceptar sin volver a pedir credenciales**
  - *Pre:* asignación "por aceptar".
  - *Pasos:* pulsar "Aceptar".
  - *Esperado:* se marca como aceptado sin pedir credenciales de nuevo.

---

## 20. Confirmación de recepción de activo

- [ ] **CRA-01 — La liga muestra "quién le asignó qué"**
  - *Pasos:* abrir una liga válida de asignación.
  - *Esperado:* se muestra una página con el asignador y el activo, y el botón "Acepto la asignación".

- [ ] **CRA-02 — Aceptar con login correcto**
  - *Pasos:* pulsar "Acepto la asignación" y autenticarse correctamente.
  - *Esperado:* el activo queda aceptado y se redirige a "Mis Activos".

- [ ] **CRA-03 — Aceptar con credenciales incorrectas**
  - *Pasos:* pulsar "Acepto" y autenticarse mal.
  - *Esperado:* error de autenticación; el activo NO se marca como aceptado.

- [ ] **CRA-04 — Token inválido o desconocido**
  - *Pasos:* abrir una liga con token inválido.
  - *Esperado:* error claro; no cambia el estado.

- [ ] **CRA-05 — Re-clic tras aceptado (idempotencia)**
  - *Pre:* asignación ya aceptada.
  - *Pasos:* abrir de nuevo la liga.
  - *Esperado:* se muestra "este activo ya fue asignado" y se redirige al login; sin cambio de estado.

- [ ] **CRA-06 — Token sin caducidad**
  - *Pre:* token generado hace varios días.
  - *Pasos:* confirmar la recepción.
  - *Esperado:* se acepta sin importar el tiempo transcurrido.

- [ ] **CRA-07 — El administrador no puede aceptar, pero sí desvincular**
  - *Pasos:* como administrador, intentar aceptar (no debe haber flujo) e iniciar desvinculación.
  - *Esperado:* no hay aceptación administrativa; sí puede iniciar la desvinculación (envía correo).

- [ ] **CRA-08 — Desvinculación autenticada por el usuario**
  - *Pasos:* abrir la liga de desvinculación, autenticarse y confirmar.
  - *Esperado:* la asignación queda desvinculada.

---

## 21. Correcciones de base de datos

- [ ] **BD-01 — Rol Administrador al alta de empresa**
  - *Pasos:* registrar una empresa nueva.
  - *Esperado:* el rol Administrador se crea con capacidad de atender tickets y asignar responsables.

- [ ] **BD-02 — Asignar rol valida la pertenencia por empresa**
  - *Pasos:* asignar un rol a un usuario.
  - *Esperado:* la validación de pertenencia del rol a la empresa es correcta.

- [ ] **BD-03 — Listado de empresas sin duplicados**
  - *Pasos:* abrir el listado de empresas.
  - *Esperado:* no aparecen filas duplicadas.

- [ ] **BD-04 — Despliegue sin error de script**
  - *Pasos:* ejecutar el despliegue de la base de datos.
  - *Esperado:* compila y despliega sin error.

- [ ] **BD-05 — Usuarios borrados no aparecen en listados**
  - *Pre:* un usuario borrado (lógicamente).
  - *Pasos:* consultar el listado de usuarios.
  - *Esperado:* el usuario borrado no aparece.

---

## 22. Bloqueo de edición de Usuarios y Personas por permisos

**Objetivo:** validar que la edición de Usuarios y Personas se bloquea (inputs
deshabilitados) cuando el rol del usuario logueado no tiene la acción "Editar", que la
creación de registros nuevos queda sujeta al permiso "Crear" y que todo se gobierna por
el sistema de Permisos existente, SIN flag nuevo en la entidad `Rol`.

- [ ] **PEU-01 — Edición permitida con rol que sí tiene "Editar"**
  - *Objetivo:* validar que un usuario con la acción "Editar" en "Usuarios" puede modificar los inputs de un Usuario existente.
  - *Pre:* existe un usuario "OperadorAdmin" cuyo rol tiene la acción "Editar" habilitada en la página "Usuarios"; existe un Usuario "U-10" (Id > 0) de la misma empresa.
  - *Pasos:*
    1. Iniciar sesión como "OperadorAdmin".
    2. Ir al menú "Administración" → "Usuarios".
    3. Abrir el Usuario "U-10" en modo edición (pulsar la fila o el botón "Editar").
    4. Verificar que los campos (nombre de usuario, nombre, apellido, correo, teléfono, rol, etc.) están habilitados.
    5. Modificar un campo (p. ej. el correo) y pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - Los inputs del formulario están habilitados (sin atributo `disabled`).
    - El botón "Guardar" está activo y el cambio se persiste correctamente.
    - No aparece ningún bloqueo ni mensaje de permiso.

- [ ] **PEU-02 — Edición bloqueada sin "Editar" (Usuarios)**
  - *Objetivo:* validar que un usuario sin la acción "Editar" en "Usuarios" ve los inputs DESHABILITADOS al abrir la edición de un Usuario existente.
  - *Pre:* existe un usuario "Consultor" cuyo rol tiene "Leer" en "Usuarios" pero NO "Editar"; existe un Usuario "U-10" (Id > 0) de la misma empresa.
  - *Pasos:*
    1. Iniciar sesión como "Consultor".
    2. Ir a "Administración" → "Usuarios".
    3. Abrir el Usuario "U-10" en modo edición (Id > 0).
    4. Intentar hacer clic y escribir en cada input del formulario (nombre de usuario, nombre, apellido, correo, teléfono, contraseña, rol, etc.).
    5. Buscar y pulsar los botones de acción de guardado.
  - *Criterios de aceptación (Esperado):*
    - Todos los inputs del formulario aparecen `disabled` y no pueden modificarse.
    - El botón de guardar/editar no permite persistir cambios (deshabilitado o sin efecto).
    - No existe ninguna vía en la UI para alterar los datos del Usuario.

- [ ] **PEU-03 — Edición bloqueada sin "Editar" (Personas)**
  - *Objetivo:* validar que en el módulo Personas el bloqueo por permisos se aplica igual que en Usuarios (extiende el bloqueo previo por `estaVinculada`).
  - *Pre:* existe un usuario "Consultor" cuyo rol tiene "Leer" en "Personas" pero NO "Editar"; existe una Persona "P-20" NO vinculada a un Usuario (estaVinculada = false, Id = 20).
  - *Pasos:*
    1. Iniciar sesión como "Consultor".
    2. Ir al menú "Personal" → "Personas".
    3. Abrir la Persona "P-20" en modo edición (Id > 0, no vinculada).
    4. Intentar modificar los campos Nombre, Apellido, Correo y Teléfono.
    5. Intentar guardar.
  - *Criterios de aceptación (Esperado):*
    - Los campos Nombre/Apellido/Correo/Teléfono quedan `disabled` aunque la Persona NO esté vinculada a un Usuario.
    - No se puede persistir ningún cambio desde la UI.
    - El comportamiento del vínculo existente se conserva (una Persona vinculada sigue bloqueada, ver PEU-05).

- [ ] **PEU-04 — Creación permitida con "Crear" aunque falte "Editar"**
  - *Objetivo:* validar que un rol con "Crear" pero sin "Editar" puede CREAR un Usuario/Persona nuevo (Id == 0, inputs habilitados) pero no puede editar existentes.
  - *Pre:* existe un usuario "Capturista" cuyo rol tiene "Crear" en "Usuarios" y "Personas" pero NO "Editar".
  - *Pasos:*
    1. Iniciar sesión como "Capturista".
    2. Ir a "Administración" → "Usuarios" (repetir después en "Personal" → "Personas").
    3. Pulsar el botón "Nuevo"/"Crear" (abre un registro con Id == 0).
    4. Verificar que los inputs del formulario vacío están HABILITADOS para captura.
    5. Capturar los datos de un Usuario/Persona nuevo y pulsar "Guardar".
    6. Abrir en edición un registro EXISTENTE (Id > 0) y comprobar el estado de los inputs.
  - *Criterios de aceptación (Esperado):*
    - En un registro nuevo (Id == 0) los inputs están editables y el guardado procede.
    - El registro nuevo se crea correctamente y aparece en el listado.
    - En un registro existente (Id > 0) los inputs están `disabled` y no se puede editar.
    - Un usuario SIN "Crear" no puede registrar nuevos Usuarios/Personas (guardado bloqueado o denegado).

- [ ] **PEU-05 — Sin "Leer" en Usuarios: acceso denegado**
  - *Objetivo:* validar que un usuario sin el permiso "Leer" en "Usuarios" (o "Personas") no puede abrir la ventana (redirección/denegación de acceso).
  - *Pre:* existe un usuario "SinAcceso" cuyo rol NO tiene "Leer" en la página "Usuarios" ni en "Personas".
  - *Pasos:*
    1. Iniciar sesión como "SinAcceso".
    2. Intentar abrir "Administración" → "Usuarios".
    3. Repetir el intento con "Personal" → "Personas".
    4. Observar la respuesta de la aplicación.
  - *Criterios de aceptación (Esperado):*
    - La ventana NO se abre; el usuario es redirigido a una página de "Acceso Denegado" (o equivalente).
    - No se muestra el formulario de Usuarios/Personas ni sus datos.
    - El bloqueo proviene del servidor (no solo de ocultar el botón).

- [ ] **PEU-06 — No existe flag nuevo en la ventana de Roles**
  - *Objetivo:* validar que el bloqueo se gobierna por el sistema de Permisos y que NO se agregó ninguna casilla/flag de edición en la ventana de Roles.
  - *Pre:* existe un usuario (p. ej. "Admin") con acceso a "Administración" → "Roles".
  - *Pasos:*
    1. Iniciar sesión como usuario con permiso sobre la página "Roles".
    2. Abrir "Administración" → "Roles".
    3. Revisar el formulario de alta/edición de un Rol (campos, casillas y checkboxes).
    4. Buscar cualquier casilla tipo "Permitir modificar usuarios/personas" o similar.
  - *Criterios de aceptación (Esperado):*
    - La ventana de Roles NO muestra ninguna casilla nueva relacionada con la edición de Usuarios/Personas.
    - No existe ningún flag nuevo en la entidad `Rol` ni en su SP de guardado.
    - El control de la edición depende exclusivamente de las acciones "Leer"/"Crear"/"Editar" del sistema de Permisos.

---

## 23. No. de Serie único por empresa en Activos

**Objetivo:** validar que el No. de Serie de un Activo es único por empresa entre
activos VIGENTES (Estatus = 1) con serial no nulo; los seriales nulos se permiten y no
colisionan, el soft-delete libera el serial y el duplicado se rechaza con el mensaje
amigable "Ya existe un activo con ese No. de Serie".

- [ ] **SUA-01 — Creación de activo con serial en la empresa A (correcta)**
  - *Objetivo:* validar que un Activo nuevo se guarda correctamente con su No. de Serie.
  - *Pre:* usuario de la empresa A con permiso de captura en Activos; no existe aún un activo vigente con el serial a usar.
  - *Pasos:*
    1. Iniciar sesión como usuario de la empresa A.
    2. Ir a la ventana de Activos (catálogo).
    3. Pulsar "Nuevo" y capturar los datos del activo, incluido el campo "No. de Serie" = "SN-001".
    4. Pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El activo se guarda con serial "SN-001" y aparece en el listado.
    - No se muestra ningún error de duplicidad.
    - Al consultar el activo se muestra el serial correctamente.

- [ ] **SUA-02 — Duplicado en la misma empresa rechazado con mensaje amigable**
  - *Objetivo:* validar que no se pueden crear DOS activos vigentes con el mismo serial en la MISMA empresa y que se muestra el mensaje amigable.
  - *Pre:* la empresa A tiene un activo VIGENTE (Estatus = 1) con serial "SN-001".
  - *Pasos:*
    1. Iniciar sesión como usuario de la empresa A.
    2. Ir a Activos → "Nuevo".
    3. Capturar los datos del activo y poner "No. de Serie" = "SN-001" (el mismo del activo vigente existente).
    4. Pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El guardado se RECHAZA (no se crea la fila duplicada).
    - Se muestra el mensaje "Ya existe un activo con ese No. de Serie" en la vista (vía Swal), no un error genérico.
    - El listado sigue mostrando únicamente el activo original.

- [ ] **SUA-03 — Edición que reutiliza el serial de otro activo vigente rechazada**
  - *Objetivo:* validar que al EDITAR un activo y asignarle un serial ya usado por otro activo vigente de la misma empresa, el cambio se rechaza.
  - *Pre:* la empresa A tiene dos activos vigentes: activo 1 con serial "SN-001" y activo 2 con serial "SN-002".
  - *Pasos:*
    1. Abrir el activo 2 en modo edición.
    2. Cambiar su "No. de Serie" de "SN-002" a "SN-001".
    3. Pulsar "Guardar".
    4. Reabrir el activo 2 y revisar su serial.
  - *Criterios de aceptación (Esperado):*
    - El guardado se rechaza y el activo 2 conserva su serial original ("SN-002").
    - Se muestra el mensaje amigable "Ya existe un activo con ese No. de Serie".
    - No se persiste el cambio (al reabrir, el serial sigue siendo "SN-002").

- [ ] **SUA-04 — Mismo serial en empresas DIFERENTES permitido**
  - *Objetivo:* validar que la unicidad es POR EMPRESA (dos empresas pueden usar el mismo serial).
  - *Pre:* la empresa A tiene un activo vigente con serial "SN-001"; existe un usuario de la empresa B.
  - *Pasos:*
    1. Iniciar sesión como usuario de la empresa B.
    2. Ir a Activos → "Nuevo" y capturar "No. de Serie" = "SN-001".
    3. Pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El activo de la empresa B se guarda correctamente con serial "SN-001".
    - No se muestra error de duplicidad.
    - Al consultar los activos de la empresa A y de la empresa B, cada uno conserva su serial sin afectarse mutuamente.

- [ ] **SUA-05 — Serial nulo o vacío permitido (se guarda como NULL)**
  - *Objetivo:* validar que un activo SIN serial (nulo o cadena vacía) se guarda sin error y no colisiona con la regla de unicidad.
  - *Pre:* la empresa A tiene un activo vigente con serial "SN-001".
  - *Pasos:*
    1. Crear un activo dejando el campo "No. de Serie" VACÍO y pulsar "Guardar".
    2. Crear un SEGUNDO activo también sin serial y pulsar "Guardar".
    3. (Opcional) en un tercer intento escribir espacios en blanco en el serial y guardar.
  - *Criterios de aceptación (Esperado):*
    - Los activos se guardan sin error; el serial queda como NULL (la cadena vacía se normaliza a NULL).
    - No se muestra error de unicidad por falta de serial.
    - Varios activos sin serial pueden coexistir en la misma empresa.

- [ ] **SUA-06 — Soft-delete (Estatus = 0) libera el serial**
  - *Objetivo:* validar que al eliminar lógicamente un activo, su serial queda disponible para otro activo de la misma empresa.
  - *Pre:* la empresa A tiene un activo vigente con serial "SN-001".
  - *Pasos:*
    1. Eliminar (lógicamente, Estatus = 0) el activo de serial "SN-001".
    2. Crear un activo NUEVO con "No. de Serie" = "SN-001".
    3. Pulsar "Guardar".
    4. Consultar el listado de activos vigentes.
  - *Criterios de aceptación (Esperado):*
    - El activo eliminado lógicamente deja de contar para la unicidad.
    - El activo nuevo con serial "SN-001" se guarda correctamente.
    - El activo eliminado NO aparece en los listados de vigentes.

- [ ] **SUA-07 — Mensaje de duplicado visible en la vista (Swal)**
  - *Objetivo:* validar que el error de serial duplicado se presenta como mensaje amigable en la interfaz, no como error genérico o de servidor.
  - *Pre:* un intento de guardado que devuelve el código -2 (duplicado en la misma empresa).
  - *Pasos:*
    1. Intentar crear o editar un activo con un serial duplicado (como en SUA-02 o SUA-03).
    2. Observar el mensaje que aparece en pantalla.
  - *Criterios de aceptación (Esperado):*
    - Aparece el texto exacto "Ya existe un activo con ese No. de Serie" en la vista (ventana emergente Swal).
    - No se muestra un error técnico/genérico ni un código de error.
    - La ventana permanece abierta con los datos capturados para que el usuario corrija el serial.

---

## 24. Campos nuevos del Activo: SerieLocal y Notas (textarea 250)

**Objetivo:** validar el nuevo campo `SerieLocal` (texto libre, no obligatorio, no
único) y la conversión del campo `Notas` existente a textarea con máximo 250 caracteres.

- [ ] **CAM-01 — SerieLocal capturable y persistido**
  - *Objetivo:* validar que SerieLocal se captura como texto libre (p. ej. "LAP-PR-001"), se guarda y se muestra al editar.
  - *Pre:* usuario con permiso de captura/edición de Activos; formulario de Activo disponible.
  - *Pasos:*
    1. Pulsar "Nuevo" en la ventana de Activos.
    2. En el campo "Serie Local" capturar "LAP-PR-001".
    3. Completar los datos obligatorios del activo y pulsar "Guardar".
    4. Abrir el activo de nuevo en modo edición.
  - *Criterios de aceptación (Esperado):*
    - El activo se guarda con SerieLocal = "LAP-PR-001".
    - Al reabrir el activo, el campo "Serie Local" muestra "LAP-PR-001".
    - El valor persiste en la base de datos (se mantiene tras guardar y consultar).

- [ ] **CAM-02 — SerieLocal NO obligatorio (puede quedar vacío)**
  - *Objetivo:* validar que SerieLocal no es un campo obligatorio.
  - *Pre:* formulario de Activo disponible.
  - *Pasos:*
    1. Crear un activo dejando el campo "Serie Local" VACÍO.
    2. Completar el resto de datos y pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El activo se guarda sin error.
    - SerieLocal queda NULL (vacío) en el registro.
    - No existe validación que exija capturar SerieLocal.

- [ ] **CAM-03 — SerieLocal no único**
  - *Objetivo:* validar que dos activos de la misma empresa pueden tener el mismo SerieLocal.
  - *Pre:* existe un activo A con SerieLocal = "LAP-PR-001".
  - *Pasos:*
    1. Crear un activo B con SerieLocal = "LAP-PR-001" (igual que el activo A).
    2. Pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El activo B se guarda correctamente.
    - No se muestra ningún error de unicidad por SerieLocal.
    - Ambos activos coexisten con el mismo SerieLocal en el listado.

- [ ] **CAM-04 — Notas como textarea con máximo 250 caracteres**
  - *Objetivo:* validar que Notas se muestra como textarea y que el máximo es 250 caracteres (maxlength + guardado respeta el límite).
  - *Pre:* formulario de Activo con el campo "Notas".
  - *Pasos:*
    1. Abrir un Activo en modo edición.
    2. Verificar que el campo "Notas" es un área de texto (textarea), no una caja de una línea.
    3. Intentar escribir o pegar más de 250 caracteres en "Notas" (p. ej. 251+).
    4. Guardar con un texto de 250 caracteres.
  - *Criterios de aceptación (Esperado):*
    - El campo "Notas" se renderiza como textarea (área multilínea).
    - El navegador impide exceder 250 caracteres (maxlength) y la validación de jquery.validate la refuerza.
    - Si se guarda con 250 caracteres, el valor se persiste completo.

- [ ] **CAM-05 — Notas acepta texto largo multilínea**
  - *Objetivo:* validar que Notas acepta texto largo de varias líneas (hasta 250 caracteres).
  - *Pre:* formulario de Activo.
  - *Pasos:*
    1. En el campo "Notas" escribir un texto de varias líneas (con saltos de línea) de hasta 250 caracteres.
    2. Pulsar "Guardar".
    3. Reabrir el activo en modo edición.
  - *Criterios de aceptación (Esperado):*
    - El texto completo (con saltos de línea) se guarda sin recortarse.
    - Al reabrir el activo, las Notas se muestran completas en el textarea con las líneas conservadas.
    - No hay error al persistir el texto multilínea.

---

## 25. Gestor de mantenimientos de Activos (modal)

**Objetivo:** validar el registro y consulta (histórico) de mantenimientos por activo
vía modal: la fecha es automática (visible en un input deshabilitado), el comentario es
obligatorio, el historial se ordena de más reciente a más antiguo y respeta el
multi-tenant y los permisos.

- [ ] **MTA-01 — Botón "Mantenimientos" por fila (visible según permiso de lectura)**
  - *Objetivo:* validar que el listado de Activos expone el botón "Mantenimientos" por fila cuando el usuario tiene permiso de lectura en Activos.
  - *Pre:* usuario con rol que tiene "Leer" en "Activos".
  - *Pasos:*
    1. Iniciar sesión como usuario con "Leer" en Activos.
    2. Ir a la ventana de Activos.
    3. Revisar las filas del listado y localizar el botón "Mantenimientos".
  - *Criterios de aceptación (Esperado):*
    - Cada fila muestra el botón "Mantenimientos".
    - El botón está visible cuando el usuario puede leer Activos; con un rol sin "Leer", la opción no aparece.
    - Pulsar el botón abre el modal de mantenimientos.

- [ ] **MTA-02 — Apertura del modal: Fecha deshabilitada con la fecha actual y Comentario editable**
  - *Objetivo:* validar que el modal muestra el campo Fecha en un input DESHABILITADO (solo lectura) con la fecha actual del sistema y el campo Comentario editable.
  - *Pre:* existe un activo en el listado; usuario con "Leer" en Activos.
  - *Pasos:*
    1. Pulsar el botón "Mantenimientos" de una fila.
    2. Observar el modal que se abre.
    3. Revisar el campo de fecha y el campo de comentario.
  - *Criterios de aceptación (Esperado):*
    - El modal se abre con un campo "Fecha" en un input `disabled` (solo lectura) que muestra la fecha actual del sistema.
    - El usuario NO puede modificar el campo Fecha.
    - El campo "Comentario" está habilitado y es editable.
    - El modal carga el historial existente del activo (si lo hay).

- [ ] **MTA-03 — Guardar mantenimiento con comentario: aparece en el historial con fecha automática**
  - *Objetivo:* validar que al guardar un comentario, el mantenimiento aparece en el historial del modal con la fecha automática que se mostraba deshabilitada.
  - *Pre:* modal de mantenimiento abierto para un activo.
  - *Pasos:*
    1. Escribir el comentario "Cambio de disco SSD" en el campo Comentario.
    2. Anotar la fecha mostrada en el campo Fecha (deshabilitado).
    3. Pulsar "Guardar".
    4. Revisar el historial del modal.
  - *Criterios de aceptación (Esperado):*
    - El mantenimiento se registra y aparece en el historial.
    - La fecha del registro coincide con la fecha que se mostraba en el campo deshabilitado (fecha/hora actual del sistema).
    - El comentario "Cambio de disco SSD" se muestra en la fila del historial.

- [ ] **MTA-04 — Historial ordenado de más reciente a más antiguo (Fecha DESC)**
  - *Objetivo:* validar que el historial de mantenimientos se ordena por fecha descendente.
  - *Pre:* un activo con al menos 3 mantenimientos registrados en fechas distintas (p. ej. día 1, día 3 y día 2).
  - *Pasos:*
    1. Abrir el modal "Mantenimientos" del activo.
    2. Revisar el orden de las filas del historial.
  - *Criterios de aceptación (Esperado):*
    - El historial se lista de más reciente a más antiguo (día 3, día 2, día 1).
    - La fila más reciente aparece primero.
    - No se listan registros sin fecha (los que tienen Fecha NULL quedan excluidos).

- [ ] **MTA-05 — Comentario persistido y visible al volver a abrir el modal**
  - *Objetivo:* validar que los mantenimientos guardados persisten y siguen visibles al cerrar y reabrir el modal.
  - *Pre:* un activo con al menos un mantenimiento guardado.
  - *Pasos:*
    1. Abrir el modal "Mantenimientos" del activo.
    2. Guardar un mantenimiento con comentario.
    3. Cerrar el modal.
    4. Volver a abrir el modal del mismo activo.
  - *Criterios de aceptación (Esperado):*
    - El comentario guardado sigue visible en el historial tras reabrir el modal.
    - El mantenimiento no se pierde (persistido en la base de datos).
    - El historial muestra todos los registros previos.

- [ ] **MTA-06 — Guardar con comentario vacío: validación (campo obligatorio)**
  - *Objetivo:* validar que el comentario es obligatorio y no se puede guardar un mantenimiento sin comentario.
  - *Pre:* modal de mantenimiento abierto.
  - *Pasos:*
    1. Dejar el campo "Comentario" VACÍO.
    2. Pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El guardado se rechaza por validación (el comentario es obligatorio).
    - Se muestra una indicación de campo obligatorio (mensaje/estilo de validación).
    - No se inserta ningún mantenimiento sin comentario en el historial.

- [ ] **MTA-07 — Sin permiso de "Editar" en Activos no se puede guardar**
  - *Objetivo:* validar que un usuario sin la acción "Editar" en Activos no puede registrar mantenimientos (botón deshabilitado o rechazo del servidor).
  - *Pre:* usuario con "Leer" en Activos pero SIN "Editar".
  - *Pasos:*
    1. Iniciar sesión como el usuario sin "Editar" en Activos.
    2. Abrir el modal "Mantenimientos" de un activo.
    3. Escribir un comentario y pulsar "Guardar".
  - *Criterios de aceptación (Esperado):*
    - El botón de guardar está deshabilitado, o la operación es rechazada por el servidor (403 / Acceso Denegado).
    - No se inserta ningún mantenimiento.
    - El usuario solo puede consultar el historial.

- [ ] **MTA-08 — Aislamiento multi-empresa de los mantenimientos**
  - *Objetivo:* validar que los mantenimientos de un activo de la empresa A no aparecen para un usuario de la empresa B (si el tester puede probar multi-empresa).
  - *Pre:* dos empresas A y B; el activo X pertenece a la empresa A y tiene mantenimientos registrados.
  - *Pasos:*
    1. Iniciar sesión como usuario de la empresa B.
    2. Abrir el modal "Mantenimientos" del activo correspondiente a la empresa B.
    3. Revisar el historial y (si es accesible) intentar consultar el del activo de la empresa A.
  - *Criterios de aceptación (Esperado):*
    - El historial de los activos de la empresa B solo muestra mantenimientos de la empresa B.
    - Los mantenimientos del activo de la empresa A NO aparecen para el usuario de la empresa B.
    - Cada usuario solo ve y registra mantenimientos de activos de su propia empresa.

---

## 26. Correcciones en ventana de Permisos: contador de páginas y tema oscuro

**Objetivo:** validar las dos correcciones de la ventana de Permisos: (1) la columna
"Páginas Asignadas" muestra el conteo REAL por rol al cargar (sin N+1 y sin datos del
rol seleccionado) y (2) el chooser de asignación de páginas/acciones se ve
correctamente en tema oscuro.

- [ ] **PER-01 — Carga inicial: conteo real de páginas por rol**
  - *Objetivo:* validar que al abrir la ventana de Permisos por PRIMERA vez, la columna "Páginas Asignadas" muestra el conteo REAL de cada rol (no ceros ni los datos del rol seleccionado).
  - *Pre:* al menos 2 roles con distinto número de páginas asignadas (p. ej. Admin con 10 y Consultor con 3); usuario con permiso sobre la página "Permisos".
  - *Pasos:*
    1. Iniciar sesión como usuario con permiso sobre la página "Permisos".
    2. Abrir "Administración" → "Permisos".
    3. Observar la columna "Páginas Asignadas" de la tabla de roles ANTES de seleccionar ningún rol.
  - *Criterios de aceptación (Esperado):*
    - Cada fila de rol muestra su conteo REAL de páginas asignadas (Admin = 10, Consultor = 3), no ceros.
    - Ninguna fila muestra los datos del rol seleccionado (no se "contamina" un conteo con el de otro rol).
    - Los valores coinciden con el número de páginas realmente asignadas a cada rol en el chooser.

- [ ] **PER-02 — Cambiar de rol no altera los conteos de las demás filas**
  - *Objetivo:* validar que al seleccionar un rol diferente, los conteos de las demás filas NO cambian incorrectamente.
  - *Pre:* ventana de Permisos abierta con varios roles.
  - *Pasos:*
    1. Observar los conteos de todas las filas de roles.
    2. Seleccionar el rol "Admin".
    3. Seleccionar después el rol "Consultor".
    4. Volver a revisar la columna "Páginas Asignadas" de todas las filas.
  - *Criterios de aceptación (Esperado):*
    - Al cambiar el rol seleccionado, los conteos de las demás filas permanecen iguales.
    - No se propagan los datos del rol seleccionado a otras filas.
    - Cada fila conserva su propio conteo en todo momento.

- [ ] **PER-03 — Conteo del rol seleccionado refleja en vivo las páginas asignadas**
  - *Objetivo:* validar la consistencia entre la fila del rol seleccionado y el chooser (al asignar/quitar páginas, el conteo se actualiza en vivo).
  - *Pre:* ventana de Permisos abierta con un rol seleccionado.
  - *Pasos:*
    1. Seleccionar un rol y anotar su conteo en "Páginas Asignadas".
    2. En el chooser, asignar una página adicional (moverla de "Disponibles" a "Asignadas").
    3. Quitar una página (moverla de "Asignadas" a "Disponibles").
    4. Verificar el conteo de la fila del rol seleccionado tras cada cambio.
  - *Criterios de aceptación (Esperado):*
    - Al asignar una página, el conteo de la fila del rol seleccionado aumenta en 1.
    - Al quitar una página, el conteo disminuye en 1.
    - El conteo de la fila coincide siempre con el número de páginas en la columna "Asignadas" del chooser.
    - Las demás filas NO se ven afectadas por estos cambios.

- [ ] **PER-04 — Tema oscuro: el chooser se ve correctamente en oscuro**
  - *Objetivo:* validar que con el tema oscuro activado, el chooser de asignación de páginas/acciones (columnas disponibles/asignadas, badges y checkboxes) se ve correctamente, sin fondos claros ni texto ilegible.
  - *Pre:* usuario con permiso sobre "Permisos"; la aplicación permite activar el tema oscuro.
  - *Pasos:*
    1. Activar el tema oscuro de la aplicación.
    2. Abrir "Administración" → "Permisos".
    3. Seleccionar un rol para mostrar el chooser.
    4. Revisar visualmente: columnas "Disponibles"/"Asignadas", ítems del chooser, nombres, badges de "Páginas Asignadas" y checkboxes de acciones.
  - *Criterios de aceptación (Esperado):*
    - Las columnas e ítems del chooser usan fondos oscuros coherentes con el tema (sin fondos claros hardcodeados visibles).
    - El texto de los ítems, nombres y badges es legible (contraste suficiente) sobre el fondo oscuro.
    - Los checkboxes y badges se distinguen correctamente.
    - No hay zonas blancas/claras fuera de lugar ni texto ilegible.

- [ ] **PER-05 — Cambiar entre tema claro y oscuro: el chooser se ve bien en ambos**
  - *Objetivo:* validar que al alternar entre tema claro y oscuro, el chooser se renderiza correctamente en ambos estados.
  - *Pre:* ventana de Permisos abierta.
  - *Pasos:*
    1. Con el tema claro activo, abrir el chooser de un rol y verificar su apariencia.
    2. Cambiar al tema oscuro y volver a revisar el chooser.
    3. Cambiar de vuelta al tema claro y revisar de nuevo.
  - *Criterios de aceptación (Esperado):*
    - En tema claro el chooser se ve como antes (fondo claro, texto legible).
    - En tema oscuro el chooser aplica los estilos oscuros (sin fondos claros ni texto ilegible).
    - Alternar el tema no rompe el layout ni deja estilos mezclados.
    - El chooser se ve correctamente en ambos temas sin necesidad de recargar la página.

---

## Criterios globales de cierre

- [ ] No hay incidentes **críticos** abiertos.
- [ ] El cruce de empresas se probó con al menos **dos empresas distintas** (A y B).
- [ ] Todos los casos de **seguridad** (autenticación, autorización, aislamiento, sesión) pasaron.
- [ ] Los flujos principales (reportar ticket, asignar activo, aceptar activo) funcionan de extremo a extremo.
