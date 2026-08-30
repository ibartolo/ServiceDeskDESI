# Checklist de Pruebas — ServiceDeskDESI

> Documento de trabajo para el equipo de pruebas (QA).
> Cubre **toda la funcionalidad documentada** en los specs del proyecto.
>
> **Cómo usarlo:** marca con una ✗ la casilla `[ ]` al ejecutar cada caso. Anota el
> resultado (OK / FALLA) y el número de incidente si aplica. Los casos marcados con
> la etiqueta **`[En curso]`** corresponden a funcionalidad cuya especificación está
> documentada pero cuya implementación puede estar todavía en despliegue; confirmar
> con el equipo de desarrollo antes de probar.

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
| | **Total** | **~118** |

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

## Criterios globales de cierre

- [ ] No hay incidentes **críticos** abiertos.
- [ ] El cruce de empresas se probó con al menos **dos empresas distintas** (A y B).
- [ ] Todos los casos de **seguridad** (autenticación, autorización, aislamiento, sesión) pasaron.
- [ ] Los flujos principales (reportar ticket, asignar activo, aceptar activo) funcionan de extremo a extremo.
