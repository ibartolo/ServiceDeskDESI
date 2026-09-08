# metricas-desempeno Specification

## Purpose

Panel de solo lectura "Estadísticas" para Administradores y jefes de área, con KPIs accionables sobre el desempeño del equipo de soporte: volumen de tickets, eficiencia, tiempo promedio de resolución en horas hábiles, distribución de estatus, evolución diaria y rankings de áreas y de reasignaciones. No altera el flujo de tickets existente, no exporta datos y no tiene operaciones de escritura. La resolución de datos es multi-tenant por usuario autenticado (`@Usuario`).

## Requirements

### Requirement: MD-001 — Página de menú independiente

El sistema MUST crear la página global `Pagina.Nombre='Estadisticas'` (llave sin tilde, por seguridad de collation) con `NombreVisible='Estadísticas'` (con tilde), `Tipo='Menu'` y `PermisosPadreId=NULL` (ítem independiente, NO dentro de Administración).

#### Scenario: Ítem independiente en el menú

- GIVEN la fila `Pagina` sembrada
- THEN `Nombre='Estadisticas'` y `NombreVisible='Estadísticas'`, `Tipo='Menu'` y `PermisosPadreId=NULL`

#### Scenario: No anidado en Administración

- GIVEN el menú renderizado
- THEN "Estadísticas" aparece como ítem de primer nivel, no como submenú de "Administración"

### Requirement: MD-002 — Seed de permisos y visibilidad del menú

El sistema MUST sembrar `RolPaginaAccion` con `PuedeLeer=1` y el resto de acciones de escritura/exportación en 0 SOLO para los roles `Administrador` y `Supervisor` (patrón `MisActivos`, idempotente). El menú MUST mostrar "Estadísticas" únicamente a roles con `PuedeLeer=1`. Los roles `Agente` y `Usuario` MUST NOT ver el ítem.

#### Scenario: Administrador y Supervisor ven el ítem

- GIVEN un usuario con rol Administrador o Supervisor
- WHEN se renderiza el menú
- THEN ve "Estadísticas" en el menú principal

#### Scenario: Agente o Usuario no ven el ítem

- GIVEN un usuario con rol Agente o Usuario
- WHEN se renderiza el menú
- THEN no ve "Estadísticas" en el menú

#### Scenario: Sin acciones de escritura

- GIVEN la fila `RolPaginaAccion` sembrada
- THEN `PuedeCrear=PuedeEditar=PuedeEliminar=PuedeExportar=0`

### Requirement: MD-003 — Control de acceso en runtime

El sistema MUST autorizar la vista si el rol del usuario tiene la página (`PuedeLeer=1`) Y (`rol Administrador` O `rol Supervisor` O es jefe de departamento). "Jefe de departamento" se modela como `Area.UsuarioResponsableId == UserID` (no existe rol de jefe). Si no cumple, MUST redirigir a `Home/AccesoDenegado`. El rol `Supervisor` MUST ver todas las áreas en la fase 1.

#### Scenario: Administrador accede

- GIVEN un usuario rol Administrador con `PuedeLeer=1`
- WHEN abre `/Estadisticas`
- THEN accede y ve los KPIs de su empresa

#### Scenario: Jefe de área accede

- GIVEN un usuario (rol Supervisor o Agente) con `PuedeLeer=1` y `Area.UsuarioResponsableId == UserID` en al menos un área
- WHEN abre `/Estadisticas`
- THEN accede a la vista

#### Scenario: Sin rol con página, sin Administrador/Supervisor ni jefatura → denegado

- GIVEN un usuario con `PuedeLeer=0` sobre la página, o con `PuedeLeer=1` pero sin rol Administrador, sin rol Supervisor y sin ser `Area.UsuarioResponsableId`
- WHEN navega a `/Estadisticas`
- THEN es redirigido a `Home/AccesoDenegado`

#### Scenario: Supervisor ve todas las áreas

- GIVEN un Supervisor autorizado (fase 1)
- WHEN ve el ranking de áreas y KPIs
- THEN ve datos de TODAS las áreas de su empresa (sin filtro por su área)

### Requirement: MD-004 — Filtros globales de fecha

El sistema MUST presentar exactamente 2 inputs `datetime` nativos: fecha inicio y fecha fin. MUST default: inicio = 1 de enero del año actual, fin = hoy. MUST NOT permitir fechas futuras (máximo hoy) y MUST validar que inicio ≤ fin.

#### Scenario: Defaults

- GIVEN un usuario abre la página por primera vez
- THEN inicio = 01-ene del año actual y fin = hoy

#### Scenario: Fecha futura rechazada

- GIVEN el usuario intenta fijar una fecha posterior a hoy
- WHEN en el input fecha fin
- THEN no se permite (máximo hoy)

#### Scenario: Inicio mayor que fin

- GIVEN inicio > fin
- WHEN aplica el filtro
- THEN se rechaza con mensaje de validación y no se consulta

#### Scenario: Rango personalizado

- GIVEN el usuario selecciona un rango válido (p. ej. 2025-12-01 a 2025-12-31)
- WHEN aplica
- THEN los KPIs y gráficas se recalculan para ese rango

### Requirement: MD-005 — Tarjetas de KPI (resumen)

El sistema MUST mostrar tarjetas: Total, Nuevos, En Progreso, Resueltos, Cerrados, Rechazados, Eficiencia (%) y Tiempo promedio de resolución (horas hábiles, 1 decimal). Semántica: Total = tickets creados en rango (`Ticket.FechaCreacion`) con `Estatus=1` de la empresa; las demás = ese mismo conjunto agrupado por `TicketEstatusId` actual (1=Nuevo, 2=En Progreso, 3=Resuelto, 4=Rechazado, 5=Cerrado). Eficiencia = `Cerrados / Total * 100`. Si no hay tickets en un estado, la tarjeta MUST mostrar "0".

#### Scenario: Tarjetas con datos

- GIVEN tickets creados en el rango
- WHEN se consulta
- THEN cada tarjeta muestra el conteo correcto según el estatus actual

#### Scenario: Tarjeta vacía muestra 0

- GIVEN ningún ticket en un estado (p. ej. Rechazados)
- WHEN se consulta
- THEN la tarjeta de ese estado muestra "0"

#### Scenario: Eficiencia

- GIVEN Total > 0
- WHEN se consulta
- THEN Eficiencia = `(Cerrados / Total) * 100`, mostrado como porcentaje (p. ej. "78%")

### Requirement: MD-006 — Tiempo promedio de resolución en horas hábiles

El sistema MUST calcular el tiempo de resolución en horas hábiles por empresa usando el horario laboral de `EmpresaHorarioLaboral`: días laborables = filas con `Estatus=1`; ventana por día = `HoraInicio..HoraFin` (solo la componente de hora; la fecha se ancla y es irrelevante). MUST sumar la intersección del intervalo [`Ticket.FechaCreacion`, última fila `TicketAsignacion` con `TipoMovimiento='Resolver'`] con cada día laborable, saltando días no laborables. El promedio MUST mostrarse con 1 decimal (p. ej. "4.5 h"). Si la empresa no tiene filas de horario (no debería ocurrir), el sistema SHOULD usar Lun–Vie 09:00–17:00 como respaldo (documentado).

#### Scenario: Resolución dentro de la jornada

- GIVEN un ticket creado y resuelto el mismo día dentro de la ventana laboral
- WHEN se calcula
- THEN las horas hábiles = diferencia entre creación y resolución dentro de la ventana

#### Scenario: Cruza fin de semana

- GIVEN un ticket creado viernes 16:00 y resuelto lunes 10:00
- WHEN se calcula
- THEN solo se suman las horas hábiles de viernes (16:00–17:00) y lunes (09:00–10:00), omitiendo sábado y domingo

#### Scenario: Ticket resuelto fuera de ventana

- GIVEN un movimiento `Resolver` ocurre fuera de la ventana del día
- WHEN se calcula
- THEN la intersección con la ventana se acota a los límites `HoraInicio`/`HoraFin` (sin horas negativas)

#### Scenario: Promedio con un decimal

- GIVEN varios tickets resueltos
- WHEN se muestra el KPI
- THEN el promedio se redondea a 1 decimal (p. ej. "4.5 h")

#### Scenario: Sin tickets resueltos

- GIVEN no hay tickets resueltos en el rango
- WHEN se consulta
- THEN la tarjeta muestra "0" (o un estado vacío equivalente)

### Requirement: MD-007 — Gráfica de pastel (distribución de estatus)

El sistema MUST mostrar una gráfica de pastel con la distribución por estatus actual (Nuevo, En Progreso, Resuelto, Cerrado, Rechazado) de los tickets creados en el rango, usando Chart.js. Al pasar el cursor sobre cada porción MUST mostrar nombre del estatus y conteo.

#### Scenario: Distribución con datos

- GIVEN tickets creados en el rango
- WHEN se renderiza
- THEN el pie muestra cada estatus con su conteo

#### Scenario: Tooltip

- GIVEN el cursor sobre una porción
- THEN se muestra "nombre del estatus" + "número de tickets"

#### Scenario: Sin datos

- GIVEN no hay tickets en el rango
- THEN el pie muestra estado vacío "Sin datos"

### Requirement: MD-008 — Gráfica de evolución diaria (línea)

El sistema MUST mostrar una gráfica de líneas con dos series por día del rango: tickets creados/día (`Ticket.FechaCreacion`) vs tickets resueltos/día (filas `TicketAsignacion` con `TipoMovimiento='Resolver'`), usando Chart.js. Días sin datos MUST mostrarse como 0 u omitirse de forma consistente. Si todo el rango no tiene tickets, MUST mostrar "Sin datos".

#### Scenario: Dos series por día

- GIVEN tickets creados y resueltos en el rango
- WHEN se renderiza
- THEN el eje X son los días y hay dos series: creados y resueltos

#### Scenario: Día sin actividad

- GIVEN un día del rango sin tickets creados ni resueltos
- THEN ese día se representa como 0 (o se omite) sin romper la serie

#### Scenario: Rango sin tickets

- GIVEN ninguna actividad en todo el rango
- THEN la gráfica muestra "Sin datos" en el centro

### Requirement: MD-009 — Ranking de Áreas

El sistema MUST mostrar una tabla con TODAS las áreas que tengan tickets creados en el rango (sin TOP N), con columnas: Área, Total tickets, Promedio urgencia (`AVG(Ticket.Urgencia)` 1–4, 1 decimal), Cerrados, Rechazados. MUST ordenar por Total desc. Las áreas sin tickets MUST NOT aparecer.

#### Scenario: Lista completa ordenada

- GIVEN varias áreas con tickets en el rango
- WHEN se consulta
- THEN aparecen todas las áreas con tickets, ordenadas por Total desc

#### Scenario: Área sin tickets omitida

- GIVEN un área sin tickets en el rango
- THEN no aparece en la lista

#### Scenario: Promedio de urgencia con decimal

- GIVEN un área con tickets
- THEN el promedio de urgencia se muestra con 1 decimal (p. ej. "3.2")

### Requirement: MD-010 — Ranking de Reasignaciones

El sistema MUST contar SOLO filas `TicketAsignacion.TipoMovimiento='Reasignar'` dentro del rango (excluye la toma inicial `Tomar`). MUST mostrar dos listas: (a) agentes que más reciben tickets reasignados (agrupado por `UsuarioId` destino de la fila `Reasignar`); (b) agentes a quienes más les quitan tickets (agente de la fila anterior activa puesta en `EsActiva=0` inmediatamente antes de un `Reasignar`, vía LAG/autounión por `FechaCreacion`). MUST aplicar TOP N = 5 por defecto, configurable con appSettings `EstadisticasTopAgentes`. Los agentes sin reasignaciones MUST NOT aparecer.

#### Scenario: Reciben reasignados

- GIVEN filas `Reasignar` en el rango
- WHEN se consulta
- THEN la lista A agrupa por agente destino (`UsuarioId`) con su conteo

#### Scenario: Quitan reasignados

- GIVEN filas `Reasignar` y sus filas previas activas (`EsActiva=0`)
- WHEN se consulta
- THEN la lista B agrupa por el agente de la fila previa activa (origen) con su conteo

#### Scenario: Excluye toma inicial

- GIVEN movimientos `Tomar` en el rango
- THEN no se cuentan en ninguna lista de reasignaciones

#### Scenario: TOP N configurable

- GIVEN `EstadisticasTopAgentes` con valor (p. ej. 5)
- WHEN se consulta
- THEN cada lista muestra como máximo ese número de agentes; si falta la key, usa 5

#### Scenario: Sin reasignaciones

- GIVEN ninguna fila `Reasignar` en el rango
- THEN se muestra "No hay reasignaciones registradas."

### Requirement: MD-011 — Experiencia de usuario (UX)

El sistema MUST mostrar un spinner con "Cargando datos…" desde que se dispara cada petición AJAX y ocultarlo al responder. MUST mostrar mensajes vacíos amigables en español con acentos ("Aún no hay tickets registrados…", "No hay reasignaciones registradas."). Los `.cshtml` MUST estar en UTF-8 CON BOM. Las gráficas MUST usar Chart.js (ya referenciado en `_Layout.cshtml`).

#### Scenario: Spinner durante la carga

- GIVEN se dispara una petición AJAX
- WHEN está en curso
- THEN se muestra spinner + "Cargando datos…" y se oculta al responder

#### Scenario: Estado vacío de tickets

- GIVEN la empresa no tiene tickets en el rango
- THEN se muestra "Aún no hay tickets registrados…" (mensaje amigable)

#### Scenario: Codificación del view

- GIVEN `Views/Estadisticas/Index.cshtml`
- THEN está en UTF-8 CON BOM y usa acentos correctos

### Requirement: MD-012 — Aislamiento multi-tenant

El sistema MUST recibir `@Usuario` (NombreUsuario) en todos los SPs de métricas y resolver `EmpresaId` internamente vía `Usuarios.EmpresaId`. MUST NOT aceptar `EmpresaId` desde el cliente. Un usuario de la empresa A MUST NOT ver datos de la empresa B.

#### Scenario: Resolución interna por usuario

- GIVEN una petición de métricas
- WHEN se procesa
- THEN la empresa se resuelve desde `@Usuario`, nunca de un parámetro del cliente

#### Scenario: Sin fuga entre empresas

- GIVEN un usuario de la empresa A
- WHEN consulta métricas
- THEN solo se devuelven datos de la empresa A; ningún dato de B

### Requirement: MD-013 — Módulo de solo lectura

El sistema MUST NOT ofrecer acciones de crear/editar/eliminar ni exportación (Excel/PDF) en el módulo. MUST NOT tener actualización en tiempo real (push/SignalR); los datos se actualizan al recargar o cambiar filtros.

#### Scenario: Sin acciones de escritura

- GIVEN la vista de Estadísticas
- THEN no existen botones ni endpoints de crear/editar/eliminar/exportar

#### Scenario: Actualización por recarga

- GIVEN el usuario cambia el filtro o recarga
- THEN los datos se vuelven a consultar; no hay push automático en tiempo real

## Criterios de éxito (verificables)

- [ ] La página "Estadísticas" aparece en el menú solo para roles Administrador y Supervisor (`PuedeLeer=1`); Agente/Usuario no la ven.
- [ ] El controlador redirige a `Home/AccesoDenegado` solo si el usuario no es Administrador, no es Supervisor y no es `Area.UsuarioResponsableId`.
- [ ] Las tarjetas muestran "0" en estados sin datos y la Eficiencia se calcula como `Cerrados/Total*100`.
- [ ] El tiempo promedio de resolución usa `EmpresaHorarioLaboral` (días `Estatus=1` y ventana `HoraInicio..HoraFin`), con 1 decimal, y respaldo Lun–Vie 09:00–17:00 si no hay filas.
- [ ] Pie, evolución diaria y rankings se cargan con spinner "Cargando datos…" y muestran "Sin datos"/mensajes vacíos cuando corresponde.
- [ ] Los 2 filtros `datetime` cumplen: default 1-ene-año-actual → hoy, sin fechas futuras, inicio ≤ fin.
- [ ] Ranking de reasignaciones cuenta solo `TipoMovimiento='Reasignar'` con TOP N desde `EstadisticasTopAgentes` (default 5).
- [ ] Ranking de áreas lista todas las áreas con tickets, ordenado por Total desc, urgencia con 1 decimal.
- [ ] Ningún SP acepta `EmpresaId` del cliente; todo se resuelve por `@Usuario`.
- [ ] `ServiceDeskDESI.sln` compila con 0 errores.

## Supuestos y decisiones (documentados para diseño/tareas)

1. **Respaldo de horario**: si una empresa no tiene filas en `EmpresaHorarioLaboral`, se usa Lun–Vie 09:00–17:00 (no debería ocurrir por los defaults de `configuracion-empresa`).
2. **Selector 7/15/30/60/90 días CANCELADO**: solo 2 inputs `datetime` nativos.
3. **"Jefe de departamento"** no es rol; se modela como `Area.UsuarioResponsableId == UserID`.
4. **Supervisor ve todas las áreas en fase 1** (decisión del usuario).
5. **TOP N de reasignaciones**: `EstadisticasTopAgentes` en `ServiceDeskDESIWebApi/Web.config` (default 5, patrón `EvidenciasMax*`).
6. **Días sin actividad en evolución diaria**: se muestran como 0 u omitidos de forma consistente (detalle de implementación en diseño).
