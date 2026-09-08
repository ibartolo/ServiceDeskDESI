# ServiceDeskDESI — Propuesta funcional

> Documento de promoción y difusión. Describe **qué hace** la aplicación y **cómo se
> usa**, en lenguaje de negocio, sin ningún detalle técnico.

---

## ¿Qué es ServiceDeskDESI?

ServiceDeskDESI es una **mesa de ayuda (help desk) multiusuario y multi-empresa** que
centraliza la gestión de solicitudes y de activos dentro de una organización.

Con una sola plataforma, una organización puede:

1. **Registrar y dar seguimiento** a las solicitudes de soporte de sus usuarios (tickets).
2. **Asignar y entregar** equipos y activos a las personas, con confirmación de recepción.
3. **Controlar quién accede a qué**, mediante roles y permisos.
4. **Aislar por completo** la información de cada empresa que usa el sistema.

---

## Módulos y funcionalidades

### 1. Mesa de ayuda (tickets)

Es el corazón de la aplicación. Cualquier usuario puede reportar un problema o
solicitud, y el sistema lo guía por un ciclo de vida controlado hasta su cierre.

**Captura de solicitudes**
- El usuario abre "Nuevo Ticket" en una ventana emergente.
- Selecciona **Área → Categoría → Subcategoría** (listas que se van filtrando entre sí).
- Indica la **urgencia** (niveles 1 a 4), un **título** y una **descripción**.
- Al guardar, el sistema asigna automáticamente un **folio único** (por ejemplo `T-00001`),
  que identifica a la solicitud de forma inequívoca dentro de su empresa.
- Una vez creada, la solicitud **no se puede editar** (queda registrada tal cual).

**Ciclo de vida de una solicitud**

Una solicitud pasa por los siguientes estados:

```
Nuevo  →  En Progreso  →  Resuelto  →  Cerrado
                     ↘  Rechazado  →  En Progreso (se retoma)
```

- **Nuevo:** la solicitud se crea y espera a ser atendida.
- **En Progreso:** un agente la "toma" y comienza a trabajar en ella.
- **Resuelto:** el agente la marca como atendida, explicando (con un comentario) qué se hizo.
- **Cerrado:** el solicitante confirma que quedó conforme y la cierra.
- **Rechazado:** el solicitante considera que no quedó resuelta y la devuelve, explicando
  por qué. El agente puede **retomarla** y volver a trabajarla.

**Historial completo**
- Todo movimiento queda registrado: quién tomó, reasignó, resolvió, rechazó o cerró la
  solicitud, con fecha y comentario.
- El usuario puede consultar en cualquier momento el **detalle** de una solicitud y su
  historial completo (vista de solo lectura).

**Roles dentro de los tickets**

| Rol | Qué puede hacer |
|-----|-----------------|
| **Solicitante** (quien reporta) | Crear tickets, ver los propios, **cerrar** o **rechazar** sus tickets resueltos. |
| **Agente** (quien atiende) | Ver los tickets de su área, **tomar**, **resolver** y **retomar** tickets. |
| **Responsable de área** | Ver los tickets de su área y **reasignarlos** a otro agente del área. |

### 2. Gestión de activos

Permite controlar el inventario asignado a las personas y cerrar el ciclo de entrega.

**Asignación**
- El administrador asigna un activo (equipo, licencia, etc.) a una persona.
- El sistema **notifica por correo**: al administrador (de forma informativa) y a la
  persona (con una liga para confirmar la recepción).
- Si el correo no puede enviarse, la asignación **se revierte automáticamente** para no
  dejar registros "a medias".

**Confirmación de recepción**
- La persona recibe un correo con una liga segura.
- Al abrirla, ve **quién le asignó qué** y confirma con el botón "Acepto la asignación".
- Para aceptar, la persona **se identifica con sus propias credenciales** (nadie puede
  aceptar en su nombre, ni siquiera el administrador).
- La liga **no caduca**: puede confirmar en cualquier momento.

**Mis Activos**
- Cada usuario cuenta con una vista "Mis Activos" donde ve:
  - Los activos **vigentes** (ya aceptados).
  - Los activos **por aceptar** (pendientes de su confirmación).
- Desde ahí puede **aceptar** directamente, sin volver a escribir sus credenciales.

**Desvinculación**
- El administrador puede iniciar la devolución de un activo.
- El usuario recibe un correo y, tras identificarse, confirma la desvinculación.

### 3. Gestión de personal

- **Catálogo de Personal** (personas) y **Administración de Usuarios** (cuentas de acceso).
- Cada persona puede **vincularse a una cuenta de usuario** (relación 1:1).
- Al vincular, los datos de la persona (nombre, apellido, correo, teléfono) se sincronizan
  desde la cuenta del usuario y quedan **bloqueados** para evitar inconsistencias.
- El sistema **advierte** antes de sobrescribir los datos.

### 4. Seguridad y control de acceso

- **Acceso por credenciales:** solo usuarios válidos pueden entrar.
- **Permisos por rol:** cada rol define qué módulos puede ver y qué acciones puede
  ejecutar (crear, editar, eliminar). Los permisos se validan siempre en el servidor:
  ocultar un botón no es suficiente para operar.
- **Contraseñas protegidas:** nunca se muestran ni se guardan en texto plano.
- **Sesión con expiración:** al vencer la sesión, el usuario es llevado de nuevo al login.
- **Período de prueba:** las empresas en período de prueba vencido no pueden operar hasta
  renovar su acceso.

### 5. Multi-empresa

ServiceDeskDESI está pensado para operar **varias empresas a la vez**:

- Cada empresa ve y administra **únicamente su propia información** (sus tickets, sus
  usuarios, sus activos, su foliador de tickets).
- No existe forma de consultar ni modificar datos de otra empresa.
- El registro de nuevas empresas valida que sus datos sean únicos (RFC, correo, nombre).

### 6. Configuración de Empresa

Cada empresa puede personalizar el sistema y definir sus reglas de operación desde una
página independiente del menú (ícono de engranaje): **Configuración de Empresa**. Solo
la ven los roles con el permiso de la página asignado; por defecto el rol
**Administrador** la tiene con permisos de **lectura** y **edición** (otros roles pueden
recibirla después desde la administración de permisos). Ver la página exige permiso de
lectura y guardar cambios exige permiso de edición; el módulo **no tiene operación de
eliminar**.

**Datos de la empresa (solo lectura)**
- La página abre con una tarjeta que muestra los **datos generales de la empresa** en
  modo solo lectura; ahí no se edita nada.

**Horario laboral**
- Un editor con una fila por día de la semana (**lunes a domingo**). Cada día tiene una
  casilla **"Labora"** y las horas de **inicio** y **fin** en formato de 12 horas con
  **AM/PM** (hora de 1 a 12 y minutos de 00 a 55 en pasos de 5).
- Un solo botón **Guardar** persiste la semana completa en una sola transacción. Si un
  día queda desmarcado, sus horas se limpian. El sistema valida que la **hora de fin sea
  mayor que la de inicio**.
- Cada empresa guarda su propio horario (una fila de registro por día). Las empresas de
  nuevo registro reciben por defecto **lunes a viernes de 09:00 a 17:00**, con sábado y
  domingo no laborables.
- **Uso futuro:** este horario alimentará al módulo de **Estadísticas** para calcular
  tiempos de resolución dentro de las horas hábiles de la empresa.

**Logotipo de la empresa**
- La empresa puede subir su **logotipo** (formato **SVG o PNG**, máximo **2048 KB**).
  El archivo se guarda en la carpeta de la empresa (`Uploads/Logos/{empresaId}/`) y la
  base de datos conserva solo su ruta relativa (`LogoUrl`).
- Cuando hay logotipo, este aparece en la parte superior del menú lateral en lugar del
  logo de DESi. Si no hay logotipo, se muestra el **logo DESi por defecto** (ícono y
  texto).
- La opción **"Quitar logo"** (con confirmación) elimina el logotipo y regresa al logo
  DESi; volver a subir una imagen reemplaza al archivo anterior, sin duplicados.

**Pie de página institucional**
- El sitio muestra un pie de página fijo **"Service Desk by DESi"** con los enlaces
  Ayuda · Términos · Privacidad. Es informativo y no se edita desde la página.

### 7. Estadísticas (Métricas y Desempeño)

Panel independiente de **solo lectura** con los indicadores (KPIs) del service desk de la
empresa: cuántos tickets se atienden, en qué estatus están, qué tan rápido se resuelven y
cómo se reparte la carga entre áreas y agentes. Su propósito es dar a la empresa métricas
accionables **sin tener que exportar datos a hojas de cálculo**.

**Acceso y alcance**
- Es un ítem **independiente del menú** ("Estadísticas"). Por defecto lo ven los roles
  **Administrador** y **Supervisor**; en tiempo de ejecución también entra un **jefe de
  área** (usuario responsable de al menos un área). Quien no tenga el permiso no ve el
  ítem y, si entra por URL directa, es llevado a "Acceso denegado".
- El módulo es **solo lectura**: no hay crear, editar, eliminar ni exportar.
- En esta primera versión el **Supervisor** ve la información de **todas las áreas** de
  su empresa.

**Filtros por fecha**
- Dos filtros nativos de fecha (inicio y fin) con valores por defecto: **1 de enero del
  año actual → hoy**.
- No se permiten fechas futuras ni rangos donde el inicio sea mayor que el fin.
- El botón **"Aplicar"** recalcula todas las tarjetas y gráficas del panel.

**Indicadores (8 tarjetas)**
- **Total:** tickets creados en el rango.
- **Nuevos, En Progreso, Resueltos, Cerrados y Rechazados:** tickets agrupados por su
  **estatus actual**; si un estatus no tiene tickets, la tarjeta muestra "0".
- **Eficiencia:** porcentaje de cierre = (Cerrados ÷ Total) × 100.
- **Tiempo promedio de resolución:** promedio en horas con un decimal (ej. "4.5 h"),
  calculado en **horas hábiles** según el horario laboral que la empresa configuró en
  "Configuración de Empresa"; los días y horas no laborables no suman tiempo.

**Gráficas**
- **Pastel de distribución de estatus:** cada estatus con su color; al pasar el cursor se
  muestra el nombre del estatus y la cantidad de tickets.
- **Evolución diaria (líneas):** tickets creados vs. resueltos por día dentro del rango.
- Ambas se dibujan con Chart.js y muestran **"Sin datos"** cuando el rango no tiene tickets.

**Ranking de Áreas**
- Lista las áreas que tienen tickets, **ordenada por Total descendente**, con Área, Total,
  Promedio de urgencia (un decimal), Cerrados y Rechazados. Las áreas sin tickets no
  aparecen.

**Ranking de Reasignaciones**
- Dos listas lado a lado: **"Agentes que más reciben tickets reasignados"** y **"Agentes a
  quienes más les quitan tickets"**, con **TOP N** configurable (5 por defecto, parámetro
  `EstadisticasTopAgentes`).
- Solo cuentan las **reasignaciones reales** (el ticket cambió de agente); la toma inicial
  de un ticket no se considera. Sin reasignaciones, se muestra "No hay reasignaciones
  registradas.".

**Experiencia de uso y aislamiento**
- Cada sección muestra **"Cargando datos…"** mientras consulta sus datos y un mensaje
  amigable (**"Aún no hay tickets registrados…"**) cuando la empresa aún no tiene tickets.
- Los datos están **aislados por empresa**: un usuario de la empresa A nunca ve las
  métricas de la empresa B.

---

## Flujos de trabajo principales

### Flujo 1 — Reportar y resolver una solicitud

1. Un usuario abre "Nuevo Ticket" e indica el área, la categoría, la subcategoría, la
   urgencia y la descripción del problema.
2. El sistema genera el folio (p. ej. `T-00123`) y lo deja en estado **Nuevo**.
3. Un **agente** del área correspondiente **toma** el ticket (pasa a **En Progreso**).
4. El agente trabaja en el problema y lo marca como **Resuelto**, dejando un comentario.
5. El solicitante revisa la solución:
   - Si está conforme, **cierra** el ticket (queda **Cerrado**).
   - Si no está conforme, **lo rechaza** con un comentario; el ticket regresa a ser
     **retomado** por el agente.

### Flujo 2 — Asignar y entregar un activo

1. El administrador asigna un activo a una persona.
2. El sistema envía un correo a la persona con una liga de confirmación.
3. La persona abre la liga, se identifica y acepta la recepción.
4. El activo queda registrado como aceptado y aparece en "Mis Activos".

### Flujo 3 — Devolver un activo

1. El administrador inicia la desvinculación de un activo.
2. La persona recibe un correo con la liga de devolución.
3. La persona se identifica y confirma la desvinculación.
4. El activo deja de estar asignado a esa persona.

### Flujo 4 — Alta de una empresa y primer acceso

1. Una empresa nueva se registra (nombre, RFC, correo de contacto, razón social).
2. El sistema valida que esos datos sean únicos.
3. Se crean los roles por defecto (incluido el Administrador) y se envía un correo de
   bienvenida con las credenciales iniciales.
4. El administrador entra, configura permisos y comienza a operar.

---

## Valor para la organización

- **Trazabilidad total:** cada solicitud y cada activo dejan un historial completo de
  quién hizo qué y cuándo.
- **Responsabilidad clara:** los roles definen exactamente qué puede hacer cada persona.
- **Reducción de errores:** los tickets son inmutables y los activos requieren confirmación
  de recepción por parte del usuario final.
- **Seguridad y privacidad:** contraseñas protegidas, permisos por rol y aislamiento
  estricto entre empresas.
- **Una sola herramienta** para gestionar solicitudes, personal y activos.
