# Exploración — Módulo "Estadísticas" (Métricas y Desempeño)

- **Change**: `metricas-desempeno`
- **Fase**: explore (solo lectura)
- **Fecha**: 2026-09-07
- **Fuentes**: `openspec/basededatosservicedesk.txt` (esquema + 107 SPs), migraciones en `openspec/changes/*` y `openspec/changes/archive/*`, código `ServiceDeskDESIEntities` / `ServiceDeskDESIWebApi` / `ServiceDeskDESIMVC`, `docs/propuesta-servicedesk.md`, `nuevas reglas.txt`.

---

## Resumen ejecutivo

El sistema **ya tiene el 100 % de la base de datos y de la arquitectura** necesarias para el módulo de métricas: una tabla de histórico de tickets (`TicketAsignacion`) con `TipoMovimiento` (Tomar/Resolver/Rechazar/Cerrar/Retomar/Reasignar) y `TicketEstatusId` resultante, un catálogo de estatus (`TicketEstatus` 1..5), `Ticket.Urgencia int` (1–4), `Area.UsuarioResponsableId` (concepto de "jefe de departamento"), y un precedente de dashboard (`DashboardController`/`ObtenerIndicadoresDashboard`) más la librería **Chart.js** ya referenciada (comentada) en `_Layout.cshtml`. No hay que inventar nada estructural: se replica el patrón completo MVC → HttpClient → WebApi → `DbWrapper` → SP, se añade **una página global "Estadisticas"** (sin acento como llave, con `NombreVisible='Estadísticas'`), se siembra el permiso de lectura a los roles **Administrador** (y opcional **Supervisor**), y se aplica en runtime el chequeo "jefe de departamento" (`Area.UsuarioResponsableId == UserID`) ya existente en `HomeController.Configuration`. Los KPIs se calculan con **1 SP nuevo** (o varios pequeños) parametrizados por `@Usuario` + `@FechaInicio`/`@FechaFin`, con tenant resuelto por `Usuarios.EmpresaId`.

---

## 1. Estado actual (cómo funciona hoy)

### Arquitectura (confirmada)
- **Front**: `ServiceDeskDESIMVC` (ASP.NET MVC 5, .NET Framework 4.8). Controlador `: BaseController` (que expone `httpClientConnection` y `tokenCookie`). Los controladores llaman `*Service` (MVC) → `HttpClientConnection.*` (DAL MVC, partial) → WebApi. Respuestas envueltas en `ModelResponse<T>` (`IsSuccess`, `Message`, `Response`).
- **Back**: `ServiceDeskDESIWebApi` (Web API 2 + OWIN/OAuth2). `[Authorize]` + `[RoutePrefix]` + `[Permiso("Pagina","Accion")]`. `*Service` (WebApi) → `DbWrapper.*` (partial, ADO.NET crudo) → stored procedures. `DbWrapper.LlenarEntidad<T>` mapea por nombre de columna (case-insensitive) con `Convert.ChangeType` (ya soporta int→long).
- **Shared**: `ServiceDeskDESIEntities` (POCOs). `ModelResponse` y `ModelResponse<T>` en `Seguridad/ModelResponse.cs`. `BaseObject` centraliza `Id/CreadoPor/FechaCreacion/ModificadoPor/FechaModificacion/Estatus`.
- **Auth**: OAuth2 password grant en `ServiceDeskDESIWebApi/App_Start/Startup.cs` emite claims `Name` (NombreUsuario), `usuarioId`, `empresaId` y `ClaimTypes.Role` por cada rol. El MVC guarda el token en cookie FormsAuthentication (`TokenCookie`: `Token`, `UserID`, `EmpresaID`, `UserName`, `ProfileImage`, `UserAvatar`).
- **Menú**: cargado por AJAX `$("#sidebar").load("/Home/MenusUser")` → `HomeController.MenusUser` → `ObtenerPaginasPorUsuario` → `Views/Home/MenusUser.cshtml` (único render, muestra `NombreVisible ?? Nombre`).

### Precedente de dashboard (Q7)
- `ServiceDeskDESIWebApi/Controllers/DashboardController.cs`: `[Authorize] [RoutePrefix("api/Dashboard")]`, acción `[HttpGet, Route("Indicadores")]` devuelve `ModelResponse<DashboardIndicadoresDTO>`, `var usuario = User.Identity.Name`.
- `ServiceDeskDESIWebApi/Services/DashboardService.cs` → `_dbWrapper.ObtenerIndicadoresDashboard(usuario)`.
- `ServiceDeskDESIWebApi/DAL/DbWrapper.Dashboard.cs`: `GetObject(...)` con `LlenarEntidad<DashboardIndicadoresDTO>`.
- `ServiceDeskDESIEntities/Tickets/DashboardIndicadoresDTO.cs` (4 `int`).
- MVC: `HomeController.Index` detecta `esAgente` (`ObtenerRolesPorUsuario` → `PuedeAtenderTickets`) y llama `DashboardService.ObtenerIndicadores()`; `Views/Home/Index.cshtml` muestra `stat-card`s y deja **comentadas** las secciones de gráficas (`<canvas id="ticketsChart">`, `<canvas id="priorityChart">`).
- **Chart.js ya está referenciado** en `Views/Shared/_Layout.cshtml:14-15 y 43` (comentado): `https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.css` y `chart.min.js`. Solo hay que descomentar/reincluir. **No hay ninguna otra librería de gráficas** (grep de apexcharts/highcharts/echarts = 0).
- **Spinner**: `Views/Catalogs/Active.cshtml:188` tiene un comentario "Mostrar spinner de carga en el DDL de modelos". No hay un spinner global estándar; el patrón AJAX existente (p. ej. `_DetalleTicket.cshtml` con DataTables) muestra datos vía `$.get`/`$.ajax`. El requisito de "Cargando datos…" se implementará con Bootstrap 4 `spinner-border` (ya disponible).

### Ciclo de vida de tickets (confirmado en `openspec/changes/tickets-ciclo-vida`)
- Estatus: **1=Nuevo, 2=En Progreso, 3=Resuelto, 4=Rechazado (renombrado de "Reabierto"), 5=Cerrado**.
- Transiciones vía `SP TransicionarTicket` (`@TipoMovimiento` ∈ Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar). **Cada transición inserta una fila en `TicketAsignacion`** con `EsActiva` (solo la última en 1), `TipoMovimiento`, `TicketEstatusId` (resultante), `FechaCreacion` (timestamp del movimiento), `CreadoPor`, `EmpresaId`.
- Cerrar: solo el **solicitante** (`CreadoPor == @Usuario`) sobre un ticket en estatus 3 (Resuelto) con comentario. Rechazar: solicitante sobre Resuelto. Retomar: agente del área sobre Rechazado → vuelve a En Progreso.

---

## 2. Respuestas a las preguntas concretas (Q1–Q10)

### Q1. Catálogo de estatus y columna de resolución
- **Columna de estatus actual**: `Ticket.TicketEstatusId` (int, FK a `TicketEstatus.Id`). `Ticket.Estatus` es el bit de soft-delete (NO confundir).
- **Catálogo `TicketEstatus`** (tabla con `Id int IDENTITY`, única tabla con Id int; columnas `Id, Nombre, Descripcion, Color, Orden, CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, Estatus`):
  - 1 = Nuevo, 2 = En Progreso, 3 = Resuelto, 4 = Rechazado, 5 = Cerrado.
  - Evidencia: `dashboard-indicadores/migration.sql:45` comenta "Nuevo=1 / En Progreso=2"; `tickets-ciclo-vida/migration.sql:100-101` mapea `Resolver→3, Cerrar→5, Rechazar→4, Tomar/Retomar/Reasignar→2`; `migration.sql:8` renombra id 4 a "Rechazado".
- **No existe columna `FechaResolucion` ni `FechaCerrado`** en `Ticket` (CREATE TABLE en `basededatosservicedesk.txt:411-429` solo tiene `FechaCreacion`, `FechaModificacion`; más `Folio` y `EmpresaId` añadidos por migraciones posteriores). Tampoco hay "fecha de resolución" en `Activo` ni en `TicketHistorico` (no existe `TicketHistorico`).
- **"Resuelto en" debe derivarse del histórico**: la fila de `TicketAsignacion` con `TipoMovimiento='Resolver'` (la **última** si fue rechazado/retomado varias veces) y su `FechaCreacion`. Idem "Cerrado en" = fila `TipoMovimiento='Cerrar'`. El SP `TransicionarTicket` no escribe ninguna fecha dedicada (solo actualiza `Ticket.TicketEstatusId`, `ModificadoPor`, `FechaModificacion`).

### Q2. Flujo de cierre y campo propuesto
- Resuelto **es** un paso distinto y previo a Cerrado. Cierre: solicitante marca `Cerrar` sobre estatus 3 (Resuelto). El estatus 5 se alcanza **únicamente** vía `SP TransicionarTicket` (`@TipoMovimiento='Cerrar'`), y también existe `ResolverTicket`/`CerrarTicket`/`RechazarTicket`/`RetomarTicket`/`TomarTicket`/`ReasignarTicket` en `DbWrapper.Ticket.cs` que **todos** llaman a `TransicionarTicket`.
- **El proceso externo que actualiza estatus es `SP TransicionarTicket`** (unificado; `tickets-ciclo-vida/migration.sql:34-122`).
- **Campo al marcar Cerrado**: factible (añadir `Ticket.FechaCerrado DATETIME NULL` y setearlo dentro de `TransicionarTicket` cuando `@TipoMovimiento='Cerrar'`). **Recomendación**: NO es necesario para las métricas — tanto "resuelto en" como "cerrado en" se derivan ya de `TicketAsignacion` por `TipoMovimiento`. Añadir `FechaCerrado`/`FechaResolucion` es opcional (utilidad futura: evitar join al histórico); si se quiere, es un `ALTER TABLE` aditivo de bajo riesgo.

### Q3. Cálculo de horas hábiles (tiempo promedio de resolución)
- **Campos disponibles**: `Ticket.FechaCreacion` (creación, `datetime NOT NULL`). Resolución = `TicketAsignacion.FechaCreacion` de la última fila `TipoMovimiento='Resolver'`. No hay `FechaActualizacion` (solo `FechaModificacion`, que no es confiable para esto).
- **Regla defendible propuesta** (día hábil de 8 h, Lun–Vie 09:00–17:00, excluye fines de semana):
  1. Resolver `inicio = Ticket.FechaCreacion` y `fin = FechaCreacion` del último movimiento `Resolver`.
  2. Si `fin` está en fin de semana o fuera de [09:00,17:00], no se mueve (los timestamps se generan `GETDATE()` en horario del servidor; asumir zona central).
  3. Recorrer cada día entre `inicio` y `fin`: sumar la intersección del día con [09:00,17:00], saltando sábado/domingo.
  4. Mismo día: `max(0, min(fin,17:00) − max(inicio,09:00))`.
  5. **KPI = promedio** de esas horas (decimal, ej. "4.5 h"). Mostrar con 1 decimal.
- Nota de implementación: puede calcularse en el SP con una función inline/CTE o en C# (el WebApi puede recibir filas brutas y calcular). Dado el patrón "toda lógica en SP", lo natural es un **SP** que devuelva horas por ticket o el promedio ya calculado; la alternativa C# es aceptable y más simple de probar.

### Q4. Área y Urgencia
- **`Ticket.AreaId`** (bigint NOT NULL, FK a `Area`) y **`Ticket.Urgencia`** (int NOT NULL, magic number, sin catálogo) existen desde el script original (`basededatosservicedesk.txt:413,416`). La captura (`_CapturarTicket.cshtml` / `GuardarTicketConEvidencias`) incluye área y urgencia. Urgencia **1–4** (confirmado en `docs/propuesta-servicedesk.md:32` "urgencia (niveles 1 a 4)"; `TicketController.cs:128` documenta `1=Baja,2=Media,3=Alta,4=Crítica`).
- **Áreas**: tabla `Area` (`Id, Nombre, Descripcion, Correo, CreadoPor, FechaCreacion, ..., UsuarioResponsableId` — `UsuarioResponsableId` se añadió en migración; `Area.cs` entidad lo incluye). **Scope por empresa**: `Area.EmpresaId` (añadido por `tenant-estructural`). El "promedio de urgencia por área" = `AVG(Ticket.Urgencia)` agrupado por `Ticket.AreaId` dentro del rango.

### Q5. Historial / reasignaciones
- **Tabla única de histórico = `TicketAsignacion`** (no existe `TicketHistorial` ni `Bitacora` de tickets; `BitacoraCorreo` es de correos de activos, no aplica). Columnas relevantes (entidad `ServiceDeskDESIEntities/Tickets/TicketAsignacion.cs` + `TicketAsignacionDTO.cs`): `Id, TicketId, UsuarioId (agente), Comentario, EsActiva, TipoMovimiento, TicketEstatusId` + `BaseObject` (`CreadoPor`, `FechaCreacion`, `ModificadoPor`, `FechaModificacion`, `Estatus`) + `EmpresaId` (en la fila, insertada por `TransicionarTicket`).
- **Reasignaciones distinguibles de asignación inicial**: SÍ. El movimiento `TipoMovimiento='Reasignar'` es explícito; `'Tomar'` es la toma inicial. Para el ranking se filtra **`TipoMovimiento='Reasignar'`** únicamente (cumple decisión #7: solo cambios de agente, no la asignación inicial).
- **Timestamps para el filtro de rango**: `TicketAsignacion.FechaCreacion` (fecha del movimiento) para "reaasignado en el rango". La columna `EmpresaId` de la fila permite filtrar por empresa directamente (o vía join a `Usuarios`).
- **SP de lectura**: `ObtenerTicketAsignaciones (@TicketId)` (por ticket). Para rankings se necesita un **SP nuevo** que agrupe por agente y filtre por rango + empresa (no existe hoy).

### Q6. Páginas / permisos / menú (clave para "Estadisticas vs Estadísticas")
- **`Pagina`** (`basededatosservicedesk.txt:273-291` + `NombreVisible` añadido por `archive/2026-08-26-personal-administracion/migration.sql`): `Id, Nombre, NombreVisible, Descripcion, Tipo ('Menu'|'SubMenu'), Direccion, PermisosPadreId, Logo, OrdenB, Estatus, CreadoPor, FechaCreacion, ...`. `Pagina` es **catálogo GLOBAL** (sin `EmpresaId`; `ObtenerPaginas` no filtra por empresa).
- **`RolPaginaAccion`** (`:362-380`): `Id, RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar` + auditoría. **No existe `PermisoPaginaAccion`** (grep = 0); el nombre real es `RolPaginaAccion`. Existe además `UsuarioPagina` (legacy) y `PlantillaRol` (template de roles).
- **Cómo se enlaza el menú**: `ObtenerPaginasPorUsuario (@Usuario)` devuelve páginas donde `rpa.PuedeLeer=1` (y su padre). El render `MenusUser.cshtml` muestra `NombreVisible ?? Nombre`. El acceso a la página se valida en el controlador con `[Permiso("Pagina")]` (MVC) y `[Permiso("Pagina","Accion")]` (WebApi).
- **Cómo matchea el `[Permiso("...")]`**: el string del atributo se compara contra **`Pagina.Nombre`** vía `PermisosService.ValidarPermisoUsuario` → `ObtenerPaginaPorNombre(@Nombre)` → `SELECT * FROM Pagina WHERE Nombre = @Nombre AND Estatus = 1` (`:4337-4346`). Es una **comparación de igualdad de string en SQL (collation de la BD)**. No interviene `Direccion` ni ruta; **`Pagina.Nombre` no se usa en URLs** (routing usa `Direccion`).
- **¿Hay páginas con acento?** Sí: el spec `menu-etiquetas` cita `Pagina.Nombre='Áreas'` con acento como ejemplo existente. Sin embargo, la comparación `Nombre = @Nombre` depende de la collation de la BD (típicamente `SQL_Latin1_General_CP1_CI_AS` = **accent-sensitive**, o `CI_AI` = accent-insensitive, no confirmado en el entorno hosted).
- **Seed para una página nueva (patrón exacto a replicar)** — `archive/2026-08-26-vinculacion-persona-usuario/migration.sql:339-354`:
  ```sql
  IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'MisActivos')
  BEGIN
      INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
      VALUES (N'MisActivos', N'Mis Activos', N'Activos asignados al usuario', N'Menu', N'/Home/MisActivos', NULL, N'fas fa-laptop', 99, 1);
  END
  INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, ...)
  SELECT r.Id, p.Id, 1, 0, 0, 0, 0, N'migracion', GETDATE(), 1
  FROM Rol r CROSS JOIN Pagina p
  WHERE p.Nombre = N'MisActivos' AND r.Nombre = N'Usuario' AND r.Estatus = 1
    AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
  ```
  ⚠️ Nota del propio migration: la estructura real de `Pagina/RolPaginaAccion` en la BD hosted puede diferir del dump; verificar columnas antes de ejecutar.

### Q7. Precedente de dashboard / reportes / gráficas / spinner
- Precedente funcional: `DashboardController` + `DashboardService` + `DbWrapper.Dashboard.cs` + `DashboardIndicadoresDTO` (ver §1). Es el esqueleto 1:1 a replicar.
- No existe módulo de "Reportes"/"Indicadores"/"Estadísticas" aparte del dashboard del `Home`.
- **Chart.js** ya referenciado (comentado) en `_Layout.cshtml`. No hay otra librería.
- No hay spinner global; usar `spinner-border` de Bootstrap 4 + texto "Cargando datos…".

### Q8. Semántica de rangos por KPI (propuesta concreta)
Con las 2 fechas (`fechaInicio`, `fechaFin`, ambos inclusive), propongo:
| KPI / gráfica | Interpretación |
|---|---|
| Tarjeta **Total** | tickets **creados** en el rango (`Ticket.FechaCreacion`), `Estatus=1` y `EmpresaId` |
| **Nuevos / En Progreso / Resueltos / Cerrados / Rechazados** | tickets creados en el rango **agrupados por `Ticket.TicketEstatusId` actual** (snapshot del estatus al momento de consultar) |
| **Eficiencia** | `Cerrados / Total * 100` (sobre los creados en rango) |
| **Tiempo promedio de resolución** | entre los tickets **resueltos** cuyo movimiento `Resolver` (o creación) cae en el rango; regla de horas hábiles (Q3) |
| **Pie de distribución de estatus** | conteo por estatus actual de los tickets **creados** en el rango |
| **Evolución diaria (línea)** | 2 series por día del rango: **creados/día** (`Ticket.FechaCreacion`) vs **resueltos/día** (movimiento `Resolver` en `TicketAsignacion`) |
| **Ranking de Áreas** | por tickets **creados** en rango: `Total`, `AVG(Urgencia)`, `Cerrados`, `Rechazados` (por estatus actual), orden por Total desc |
| **Ranking agentes que reciben** | filas `TipoMovimiento='Reasignar'` en rango, agrupar por `UsuarioId` destino (`TicketAsignacion.UsuarioId`) |
| **Ranking agentes a quienes les quitan** | filas `TipoMovimiento='Reasignar'` en rango, agrupar por el **agente anterior** (la fila `TicketAsignacion` inmediatamente anterior activa que quedó con `EsActiva=0`) |

**Nota clave**: el "agente a quien le quitan" no está explícito en una sola fila. `TransicionarTicket` al reasignar: (1) pone `EsActiva=0` a la fila activa anterior (el agente que pierde el ticket), (2) inserta una fila nueva `Reasignar` con el nuevo agente. Por tanto: el "agente quitado" = `UsuarioId` de la fila puesta en `EsActiva=0` **cuya** fila siguiente es `Reasignar`. Alternativa más simple: contar el par (fila `Reasignar` ⇒ agente destino) y (fila anterior activa ⇒ agente origen). Esto se resolverá con un SP con `LAG()`/autounión sobre `TicketAsignacion` ordenado por `FechaCreacion`.

### Q9. Multi-tenant (aislamiento por EmpresaId)
- **Dos mecanismos coexisten**: (a) claim `empresaId` del token (`Startup.cs:208-210`) → `BaseController.ObtenerEmpresaIdDesdeClaim()` (WebApi); (b) resolución por `@Usuario` (`User.Identity.Name`) → `(SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)` dentro de los SPs. Ambos convergen en `Usuarios.EmpresaId`.
- **`EmpresaId` ya existe en las tablas de dominio** (incluida `Ticket` y `TicketAsignacion`) por `tenant-estructural`. Para el módulo, el SP de métricas **debe filtrar por `EmpresaId`** obtenido del `@Usuario` autenticado (o pasar `@EmpresaId` desde el claim). **Nunca** aceptar `EmpresaId` del body/query del cliente (riesgo de fuga ya documentado en `database-review`).
- **Recomendación**: que los SPs de métricas reciban `@Usuario` (NombreUsuario) y resuelvan internamente `@EmpresaId`, consistente con el resto de `Obtener*`.

### Q10. Arquitectura de replicación (archivos exactos a tocar)
Replicando el patrón `Dashboard` + `MisActivos` + `Permisos`, un cambio nuevo toca:
1. **Migración SQL** `openspec/changes/metricas-desempeno/migration.sql`:
   - `INSERT INTO Pagina` (Nombre=`N'Estadisticas'`, `NombreVisible=N'Estadísticas'`, `Tipo='Menu'`, `Direccion='/Estadisticas'` o `/Home/Estadisticas`, `Logo`, `OrdenB`).
   - `INSERT INTO RolPaginaAccion` (`PuedeLeer=1`) para roles **Administrador** y **Supervisor** (CROSS JOIN como el seed de `MisActivos`).
   - **1 SP nuevo** `ObtenerMetricasTickets` (`@Usuario`, `@FechaInicio`, `@FechaFin`) devolviendo los KPIs + distribución de estatus + ranking de áreas; y/o SPs pequeños: `ObtenerEvolucionDiaria`, `ObtenerRankingAreas`, `ObtenerRankingReasignaciones`. Todo idempotente (`IF OBJECT_ID ... DROP / IF NOT EXISTS`).
2. **Entities** `ServiceDeskDESIEntities`:
   - DTOs nuevos en `Tickets/` (p. ej. `MetricasTicketsDTO`, `RankingAreaDTO`, `RankingReasignacionDTO`, `EvolucionDiariaDTO`). **Registrar** en `ServiceDeskDESIEntities.csproj` (old-style `<Compile Include=...>` — obligatorio).
3. **WebApi**:
   - `DAL/DbWrapper.Estadisticas.cs` (partial) — `GetObject`/`GetObjects` + `LlenarEntidad<T>`.
   - `Services/EstadisticasService.cs` — `ModelResponse<...>` + Serilog + try/catch.
   - `Controllers/EstadisticasController.cs` — `[Authorize] [RoutePrefix("api/Estadisticas")]`, acciones `[HttpGet, Route(...)]`, `var usuario = User.Identity.Name`. Añadir `[Permiso("Estadisticas","Leer")]` en el endpoint (defensa en profundidad).
4. **MVC**:
   - `DAL/HttpClientConnection.Estadisticas.cs` (partial) — `RequestAsync<T>(...)`.
   - `Services/EstadisticasService.cs` (MVC).
   - `Controllers/EstadisticasController.cs` — hereda `BaseController`; `Index()` valida (a) `[Permiso]` de lectura y (b) jefe de departamento/administrador, y devuelve `View()`; acciones AJAX que devuelven `JsonConvert.SerializeObject(response)` (patrón `TicketController`).
   - `Views/Estadisticas/Index.cshtml` — tarjetas + `<canvas>` Chart.js + tablas de ranking + 2 inputs `datetime-local` nativos + spinner.
   - **Registrar** `Controllers/EstadisticasController.cs`, `Services/EstadisticasService.cs`, `DAL/HttpClientConnection.Estadisticas.cs` en `ServiceDeskDESIMVC.csproj` (lista explícita).
5. **Menú**: no hay cambios de código en el render (usa `ObtenerPaginasPorUsuario`); el nuevo ítem aparece automáticamente al sembrar `Pagina` + `RolPaginaAccion`.

---

## 3. Decisiones fijas del usuario → recomendación

### Decisión 1 — Acento en el nombre de página
**Recomendación: llave sin acento `Estadisticas`, etiqueta visible con acento `Estadísticas`.**
Evidencia: (a) `[Permiso("...")]` matchea contra `Pagina.Nombre` por igualdad exacta en SQL (`ObtenerPaginaPorNombre WHERE Nombre = @Nombre`), collation-dependiente; (b) la separación llave/etiqueta **ya existe** (`Pagina.NombreVisible`, patrón `menu-etiquetas`/`personal-administracion`); (c) el render del menú ya hace `NombreVisible ?? Nombre`. Con llave `Estadisticas` (sin tilde) se elimina cualquier ambigüedad de collation; con `NombreVisible='Estadísticas'` se mantiene la UI en español correcto.

### Decisión 2 — Regla de acceso ("jefe de departamento")
- **No existe un rol "Jefe de departamento"**. Los roles (PlantillaRol): Administrador, Supervisor, Agente, Usuario. El concepto de "jefe de departamento" está modelado como **usuario responsable de un área**: `Area.UsuarioResponsableId`.
- Evidencia de patrón existente: `HomeController.Configuration` (`esJefeArea = areas.Any(a => a.UsuarioResponsableId == UserID)`) y `TicketController.Index` (`esResponsableArea`).
- **Recomendación**: (1) sembrar la página "Estadisticas" (`PuedeLeer=1`) solo a los roles **Administrador** y **Supervisor** (los agentes/usuarios no la ven en el menú — el menú solo lista `PuedeLeer=1`); (2) en `EstadisticasController.Index` aplicar el chequeo runtime: `esAdmin (rol Administrador) || Area.UsuarioResponsableId == UserID`, redirigiendo a `Home/AccesoDenegado` si no cumple. Esto cubre exactamente la regla "rol con página asignada AND jefe de departamento".

### Decisión 3 — Selector 7/15/30/60/90 cancelado
Confirmado: solo 2 inputs `datetime` (fecha inicio/fecha fin). Defaults: inicio = 1 de enero del año actual, fin = hoy; `max = hoy` en ambos; validación inicio ≤ fin.

### Decisión 4 — TOP N configurable (appSettings)
Patrón a espejar: `EvidenciasMaxArchivos` / `EvidenciasMaxTamanoMB` en WebApi `Web.config` leídos con `ConfigurationManager.AppSettings["..."]` + `int.TryParse` + default (`TicketService.cs:719-726`, `EvidenciaService.cs:32-41`).
**Key propuesta**: `EstadisticasTopAgentes` (default `5`), agregar en `<appSettings>` de `ServiceDeskDESIWebApi/Web.config` (donde ya están `Evidencias*`).

### Decisión 5–7 — Verificadas (Área/Urgencia, histórico, solo reasignaciones reales)
Confirmado todo (Q4, Q5). El conteo de reasignaciones = `TipoMovimiento='Reasignar'` (excluye `Tomar`), filtrado por rango.

---

## 4. Áreas afectadas (resumen de archivos)

| Archivo | Acción |
|---|---|
| `openspec/changes/metricas-desempeno/migration.sql` (+ `rollback.sql`) | Nuevo: página + permisos + SP(s) |
| `ServiceDeskDESIEntities/Tickets/*DTO.cs` + `.csproj` | Nuevos DTOs + registro |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Estadisticas.cs` | Nuevo (partial) |
| `ServiceDeskDESIWebApi/Services/EstadisticasService.cs` | Nuevo |
| `ServiceDeskDESIWebApi/Controllers/EstadisticasController.cs` | Nuevo |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Estadisticas.cs` | Nuevo (partial) |
| `ServiceDeskDESIMVC/Services/EstadisticasService.cs` | Nuevo |
| `ServiceDeskDESIMVC/Controllers/EstadisticasController.cs` | Nuevo |
| `ServiceDeskDESIMVC/Views/Estadisticas/Index.cshtml` | Nuevo (UTF-8 **con BOM**) |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Registrar 3 .cs nuevos |
| `ServiceDeskDESIWebApi/Web.config` | `EstadisticasTopAgentes` |
| `Views/Shared/_Layout.cshtml` | (opcional) descomentar/activar Chart.js |

---

## 5. Enfoques (alternativas)

1. **Un SP agregador `ObtenerMetricasTickets`** que devuelva varios result sets (KPIs, distribución, evolución, ranking áreas) en una sola llamada.
   - Pros: 1 round-trip, consistente con SPs "que devuelven todo" del proyecto.
   - Cons: result sets múltiples requieren `reader.NextResult()` (más código de mapeo); menos reutilizable.
   - Esfuerzo: Medio.

2. **Varios SPs pequeños** (`ObtenerMetricasResumen`, `ObtenerDistribucionEstatus`, `ObtenerEvolucionDiaria`, `ObtenerRankingAreas`, `ObtenerRankingReasignaciones`) + un endpoint por bloque, cargados en paralelo por AJAX.
   - Pros: cada endpoint aislado, carga parcial independiente (mejor UX con spinner por sección), más testeable, alineado al granularismo del proyecto.
   - Cons: más llamadas HTTP.
   - Esfuerzo: Medio.

3. **Un solo endpoint `ObtenerTodo` + un solo DTO compuesto** (resumen + listas).
   - Pros: simple de consumir.
   - Cons: DTO grande y poco flexible; re-carga todo al cambiar un filtro.
   - Esfuerzo: Bajo.

**Recomendación**: enfoque **2** (varios SPs/endpoints), con un DTO de resumen único para las tarjetas + pie (una llamada) y endpoints separados para evolución, ranking de áreas y ranking de reasignaciones. Reutiliza `ModelResponse<T>` y el patrón `DashboardController`. La gráfica de evolución y los rankings pueden pedirse con `fechaInicio`/`fechaFin` como query params.

---

## 6. Riesgos

1. **Collation de la BD no confirmada** → si se usara `Nombre='Estadísticas'` con tilde, el `[Permiso]` podría no matchear si la collation es accent-sensitive. Mitigado usando llave sin tilde `Estadisticas` + `NombreVisible`.
2. **`TicketAsignacion` no está en el dump** (`basededatosservicedesk.txt` no la define; solo se ALTERA en migraciones) → su CREATE TABLE vive en la BD hosted. Verificar columnas reales (`sys.columns`) antes de escribir los SPs de métricas (mismo aviso que dejó la migración de `MisActivos`).
3. **"Agente a quien le quitan" requiere lógica de par anterior/siguiente** (LAG/autounión por `FechaCreacion`). Riesgo de mal conteo si hay reasignaciones en cascada; validar con datos.
4. **Tiempo promedio en horas hábiles**: depende de la zona horaria del `GETDATE()` del servidor y del horario laboral asumido (09:00–17:00). Documentar/parametrizar si cambia por empresa (por ahora fijo 8h).
5. **Sin test project**: verificación = MSBuild 0 errores + revisión estática (sin smoke manual posible en esta fase).
6. **Rendimiento**: los SPs agregadores sobre `Ticket`/`TicketAsignacion` sin índices no-cluster en `FechaCreacion`/`EmpresaId`/`TipoMovimiento` (hallazgo `database-review #8`) pueden ser lentos con volumen. Considerar índices o acotar por rango.
7. **`.csproj` con lista explícita**: cualquier `.cs` nuevo no registrado no compila (error ya visto con `ThemeHelper`).

---

## 7. Listo para propuesta

**Sí.** La propuesta (`sdd-propose`) y la spec (`sdd-spec`) pueden proceder sin re-leer el código: quedan documentados el catálogo de estatus, el histórico `TicketAsignacion`/`TipoMovimiento`, el concepto "jefe de departamento" (`Area.UsuarioResponsableId`), la recomendación de llave/etiqueta (`Estadisticas`/`Estadísticas`), el patrón de seed de página+permiso, el patrón appSettings (`EstadisticasTopAgentes`), el precedente `Dashboard`+Chart.js, y la lista exacta de archivos a crear/tocar.

---

## Anexo A — Configuración de horario laboral por empresa

- **Fase**: explore (solo lectura, follow-up)
- **Fecha**: 2026-09-07
- **Objetivo**: decidir dónde/cómo modelar y exponer el horario laboral (días + horas) **por empresa**, NO hardcodeado, editable por la propia empresa.

### A.1 Modelo `Empresa` (Q-A)

Tabla **`dbo.Empresa`** — `openspec/basededatosservicedesk.txt:201-225` (CREATE TABLE):

| Columna | Tipo | Notas |
|---|---|---|
| `Id` | `bigint IDENTITY(1,1)` | PK (`PK_Empresa`). **Es el tenant**: no existe columna `EmpresaId` en `Empresa`; todas las tablas hijas referencian `Empresa.Id` vía FK (`FK_*_Empresa`, `basededatosservicedesk.txt:773-795`). |
| `NombreComercial`, `RazonSocial` | `nvarchar(250)` | NOT NULL |
| `RFC` | `nvarchar(50)` | NOT NULL |
| `Responsable` | `nvarchar(250)` | NOT NULL |
| `Direccion` | `nvarchar(500)` | NOT NULL |
| `Ciudad`, `Estado` | `nvarchar(100)` | NULL |
| `CodigoPostal` | `nvarchar(10)` | NULL |
| `Telefono` | `nvarchar(50)` | NULL |
| `CorreoContacto` | `nvarchar(250)` | NOT NULL |
| `FechaVigenciaInicio`, `FechaVigenciaFin` | `datetime` | NOT NULL |
| `EsPeriodoPrueba` | `bit` | NOT NULL, default `1` (`:598`) |
| `Estatus` | `bit` | NOT NULL, default `1` (`:600`) |
| `CreadoPor` | `nvarchar(25)` | NOT NULL |
| `FechaCreacion` | `datetime` | NOT NULL |
| `ModificadoPor`, `FechaModificacion` | `nvarchar(25)` / `datetime` | NULL |

- **Identificación del tenant**: `Empresa.Id`. Se resuelve desde el usuario autenticado: `Usuarios.EmpresaId → Empresa.Id` (ej. SPs `SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1`). Las tablas de dominio (`Ticket`, `Area`, `Sucursal`, `Rol`, `Activo`, …) llevan `EmpresaId` (migración `tenant-estructural`, `basededatosservicedesk.txt:747-795`).
- **POCO** `ServiceDeskDESIEntities/Catalogos/Empresa.cs` → `Empresa : BaseObject` con `NombreComercial, RazonSocial, RFC, Responsable, Direccion, Ciudad, Estado, CodigoPostal, Telefono, CorreoContacto, FechaVigenciaInicio, FechaVigenciaFin, EsPeriodoPrueba`. `BaseObject` (`Entities/BaseObject.cs`) aporta `Id(long), CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion?, Estatus(bool)`. **No** lleva `EmpresaId` (es el propio tenant).
- ⚠️ **No confundir con `Compania`**: `dbo.Compania` (`:179-194`) + POCO `Catalogos/Compania.cs` (`Nombre, Acronimo, RFC, Direccion`) es un **catálogo de "Razón Social"**, distinto del tenant `Empresa`. Tiene CRUD propio (`CompaniaController/Service/DbWrapper.Compania.cs`, UI MVC `Catalogs/Company.cshtml`) y **no** tiene `EmpresaId` (es global). Para el horario laboral el sujeto correcto es **`Empresa`** (el tenant), no `Compania`.

### A.2 UI existente de "información de empresa" (Q-B)

**No existe** una pantalla donde un admin edite los datos de **su propia** `Empresa` (tenant). Lo que hay:

1. **Registro (pre-login)**: `HomeController.NewCompany` / `GuardarNuevaEmpresa` (`HomeController.cs:142, 362`) → `EmpresaService.RegistrarEmpresa` → `api/Empresas/Registrar` (`[AllowAnonymous]`). Solo crea empresa nueva; `Views/Home/NewCompany.cshtml`.
2. **Lectura de licencia (solo display)**: `HomeController.Configuration` (`HomeController.cs:162-196`) lee `_empresaService.ObtenerEmpresaPorId(tokenCookie.EmpresaID)` y muestra `NombreComercial`, `FechaVigenciaInicio/Fin`, `EsPeriodoPrueba` en la card "Licencia" de `Views/Home/Configuration.cshtml` (solo si `esJefeArea = Area.UsuarioResponsableId == UserID`). **Solo lectura.**
3. **`Mi Perfil`** (`/User/MyProfile`, `Views/User/MyProfile.cshtml`) edita al **usuario**, no a la empresa.
4. **"Compañías"** (página `Pagina.Id=9`, `Tipo='SubMenu'`, `Direccion='/Catalogs/Company'`) gobierna el catálogo **`Compania`** (Razón Social), NO `Empresa`. MVC `CatalogsController` (`Company`/`GuardarOActualizarCompanias`/`EliminarCompanias`, `CatalogsController.cs:95-107,696-715`) + `Views/Catalogs/Company.cshtml`.

- El WebApi `EmpresaController` expone `GuardarOActualizarEmpresa`/`ObtenerEmpresasPorId`/`EliminarEmpresa` bajo `[Permiso("Compañías")]` (`EmpresaController.cs:25-79`), y MVC `HttpClientConnection.Empresa.cs`/`Services/EmpresaService.cs` tienen `GuardarOActualizarEmpresa`/`EliminarEmpresa`, pero **ninguna acción MVC los invoca** (grep: solo `HomeController` llama `ObtenerEmpresaPorId`). Es decir, el endpoint de edición de `Empresa` existe pero **no está cableado a ninguna pantalla**.
- `Home/Configuration` **no** es una entrada del menú `Pagina`/`RolPaginaAccion`: se llega por el dropdown del usuario (`Views/Shared/_Layout.cshtml:212`), solo protegida por autenticación de sesión (la card de Licencia se muestra solo a jefes de área). No requiere seed de `Pagina`.

**Dónde vive de forma natural el editor de "días laborables y horas"**: en `Home/Configuration` (la pantalla de "Configuración" ya existente, que ya muestra info de la empresa al responsable), como una card/sección nueva "Horario laboral". Alternativa: pantalla dedicada nueva (requeriría `Pagina` + `RolPaginaAccion` + controlador/vista MVC nuevos). Ver A.5.

### A.3 Precedentes de configuración por empresa (Q-C)

- **No hay** tabla genérica `Configuracion`/`Parametros`/`EmpresaConfiguracion` (grep = 0). Los parámetros **globales** de la app (`EvidenciasMaxArchivos`, `EvidenciasMaxTamanoMB`) viven en `ServiceDeskDESIWebApi/Web.config` `<appSettings>` y se leen con `ConfigurationManager.AppSettings` + `int.TryParse` (`TicketService.cs:714-726`, `EvidenciaService.cs:27-41`).
- **Precedente real de config por empresa = `Foliador`** (`archive/2026-08-24-foliador-tickets/migration.sql:8-19`): tabla hija `Foliador (EmpresaId BIGINT NOT NULL, FechaActualizacion, Nombre, Descripcion, Consecutivo)` con PK `(EmpresaId, Nombre)` y `FK → Empresa(Id)`. SPs `ConsultarFoliador`/`ActualizarFoliador` resuelven `@EmpresaId` desde `@Usuario`. **Seed por empresa** con `INSERT ... SELECT Id FROM Empresa WHERE NOT EXISTS` (`:176-181`). Este es el patrón exacto a espejar.
- `PlantillaRol` (`provisioning-template/migration.sql:12-34`) es un **template GLOBAL** (no por empresa), usado para sembrar roles al registrar empresa nueva.
- **No hay uso de columnas JSON** en la BD (grep `FOR JSON`/`json` en schema = 0; el JSON solo aparece en serialización C#). Un blob/JSON sería una desviación del estilo (tablas hijas normalizadas + columnas explícitas).
- **Patrón de SPs**: `Obtener* (@Usuario)` resuelve `@EmpresaId` y filtra por tenant; `Guardar*` valida tenant y hace `INSERT`/`UPDATE` + `SELECT SCOPE_IDENTITY()`/`SELECT @Id`. `DbWrapper` usa `LlenarEntidad<T>` (mapeo por nombre de columna) y `ObtenerParametrosSQL` (reflexión). Capa service envuelve en `ModelResponse<T>`.

### A.4 Diseño de esquema recomendado (Q-D)

**Recomendación: Opción 1 — tabla hija `EmpresaHorarioLaboral`** (una fila por día y por empresa, con flag `Estatus`). Razones:
- Consistente con el estilo del repo (tablas hijas por empresa: `Foliador`, `TicketAsignacion`, `CategoriaResponsable`); **sin** precedente JSON/bitmask.
- El SP de métricas la lee con un simple `WHERE EmpresaId=... AND DiaSemana=... AND Estatus=1`.
- El editor (7 checkboxes Lun–Dom + horas) mapea 1:1 a filas.
- La Opción 2 (columnas en `Empresa`: bitmask + ventana única `HoraInicio`/`HoraFin`) es más simple de leer pero (a) mezcla config en el tenant y toca `GuardarOActualizarEmpresa`/`ObtenerEmpresaPorId`/POCO `Empresa`, (b) una sola ventana diaria no permite horarios distintos por día, (c) el bitmask es feo de editar en UI y de consultar en SQL.

**Variante de representación de días**: en lugar de "filas solo para días laborables" (con DELETE/INSERT al alternar), se recomienda **7 filas por empresa** (`DiaSemana 1..7`) con `Estatus = 1` (laborable) / `0` (no laborable). Es idempotente de sembrar, y el editor queda como una lista fija de 7 checkboxes.

```sql
IF OBJECT_ID(N'[dbo].[EmpresaHorarioLaboral]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EmpresaHorarioLaboral](
        [Id]               [bigint] IDENTITY(1,1) NOT NULL,
        [EmpresaId]        [bigint] NOT NULL,
        [DiaSemana]        [tinyint] NOT NULL,            -- 1=Lunes .. 7=Domingo
        [HoraInicio]       [time] NOT NULL,               -- o [nvarchar](5) 'HH:mm'
        [HoraFin]          [time] NOT NULL,
        [Estatus]          [bit] NOT NULL CONSTRAINT DF_EHL_Estatus DEFAULT ((1)),
        [CreadoPor]        [nvarchar](25) NULL,
        [FechaCreacion]    [datetime] NOT NULL CONSTRAINT DF_EHL_FechaCreacion DEFAULT (GETDATE()),
        [ModificadoPor]    [nvarchar](25) NULL,
        [FechaModificacion][datetime] NULL,
        CONSTRAINT [PK_EmpresaHorarioLaboral] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_EHL_Empresa_Dia] UNIQUE ([EmpresaId], [DiaSemana]),
        CONSTRAINT [FK_EHL_Empresa] FOREIGN KEY ([EmpresaId]) REFERENCES [dbo].[Empresa]([Id])
    ) ON [PRIMARY]
END
GO
```
Nota de tipo: `time` es nativo y facilita el cálculo en el SP de métricas; el `DbWrapper.LlenarEntidad<T>` lo mapea a `TimeSpan`. Si se prefiere cero fricción en JSON/UI, usar `nvarchar(5)` 'HH:mm' (el SP convierte con `CONVERT(time, ...)`).

**SPs a añadir** (nombres propuestos; equivalentes a `ObtenerConfiguracionHorario`/`GuardarConfiguracionHorario`):
- `ObtenerHorarioLaboral (@Usuario NVARCHAR(25))` → `SELECT DiaSemana, HoraInicio, HoraFin, Estatus FROM EmpresaHorarioLaboral WHERE EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1) ORDER BY DiaSemana`.
- `GuardarHorarioLaboralDia (@Usuario NVARCHAR(25), @DiaSemana TINYINT, @HoraInicio TIME, @HoraFin TIME, @EsLaboral BIT)` → valida tenant, `UPDATE ... WHERE EmpresaId=@EmpresaId AND DiaSemana=@DiaSemana`; si no existe, `INSERT`; devuelve la fila o `SCOPE_IDENTITY()`. Se llama **7×** dentro de la transacción (loop en C#, patrón `foreach rol` de `GuardarNuevaEmpresaConDatosIniciales`). Alternativa sin loop: un solo SP con TVP (table-valued parameter), pero **no hay precedente de TVP** en el repo.

### A.5 Alcance de UI (Q-E)

**Recomendación**: el almacenamiento + lectura (para el SP de métricas) es **IN SCOPE** de `metricas-desempeno` (el KPI depende de que el horario no esté hardcodeado). El **editor** es pequeño y se puede incluir aquí como una card en `Home/Configuration` (pantalla existente) — **sin** seed de `Pagina`/`RolPaginaAccion` — o separarse como follow-up si el cambio crece.

Archivos a tocar (editor en `Home/Configuration`):
- **MVC**: `Controllers/HomeController.cs` (nuevas acciones `ObtenerHorarioLaboral`/`GuardarHorarioLaboral` + pasar datos a `Configuration`), `Views/Home/Configuration.cshtml` (card "Horario laboral": 7 checkboxes + 2 inputs `time` + botón guardar), `DAL/HttpClientConnection.Empresa.cs` (o partial nuevo `HttpClientConnection.Horario.cs`), `Services/EmpresaService.cs` (o `HorarioLaboralService.cs`).
- **WebApi**: `DAL/DbWrapper.Horario.cs` (partial nuevo), `Services/HorarioLaboralService.cs`, `Controllers/HorarioLaboralController.cs` (o añadir endpoints a `EmpresaController`).
- **Entities**: `Catalogos/HorarioLaboral.cs` (+ DTO si hace falta), registrado en `ServiceDeskDESIEntities.csproj` y en `ServiceDeskDESIMVC.csproj` (listas explícitas — obligatorio).
- **Migración**: `openspec/changes/metricas-desempeno/migration.sql` (+ `rollback.sql`).

Gating de acceso sugerido para el editor: solo `Administrador` (rol) **o** `esJefeArea` (`Area.UsuarioResponsableId == UserID`), reutilizando el chequeo ya presente en `HomeController.Configuration`.

### A.6 Defaults y migración (Q-F)

- **Backfill de empresas existentes** (idempotente, espeja el seed de `Foliador` en `foliador-tickets/migration.sql:176-181`):
```sql
INSERT INTO [dbo].[EmpresaHorarioLaboral] (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
SELECT e.Id, d.Dia, d.Inicio, d.Fin, CASE WHEN d.Dia BETWEEN 1 AND 5 THEN 1 ELSE 0 END, N'migracion', GETDATE()
FROM [dbo].[Empresa] e
CROSS JOIN (VALUES
    (1,CAST('09:00' AS time),CAST('17:00' AS time)),
    (2,CAST('09:00' AS time),CAST('17:00' AS time)),
    (3,CAST('09:00' AS time),CAST('17:00' AS time)),
    (4,CAST('09:00' AS time),CAST('17:00' AS time)),
    (5,CAST('09:00' AS time),CAST('17:00' AS time)),
    (6,CAST('09:00' AS time),CAST('17:00' AS time)),
    (7,CAST('09:00' AS time),CAST('17:00' AS time))
) d(Dia, Inicio, Fin)
WHERE NOT EXISTS (SELECT 1 FROM [dbo].[EmpresaHorarioLaboral] h WHERE h.EmpresaId = e.Id);
```
(Default: **Lun–Vie 09:00–17:00**, Sáb/Dom con `Estatus=0`.)

- **Empresas nuevas**: **NO hay precedente de triggers** en la BD (grep `CREATE TRIGGER` = 0; solo `SET RECURSIVE_TRIGGERS OFF` en `basededatosservicedesk.txt:45`). El registro de empresa ya usa una **transacción explícita** con siembra por pasos en `EmpresaService.GuardarNuevaEmpresaConDatosIniciales` (`ServiceDeskDESIWebApi/Services/EmpresaService.cs:267-638`): PASO 1 guarda `Empresa` (`GuardarNuevaEmpresa`) → PASO 2 `Sucursal` → PASO 3 `Area "TI"` → PASO 4 usuario admin → PASO 5 roles desde `PlantillaRol` (`GuardarRolParaNuevaEmpresa`) → PASO 5.1 `Mis Activos` al rol Usuario → PASO 6 `AsignarRolUsuarioParaNuevaEmpresa` → PASO 7 `InsertarRolPaginaAccion` (todas las páginas al rol Administrador) → PASO 8 `InsertarUsuarioPaginaParaNuevaEmpresa`, todo bajo `_dbWrapper.BeginTransaction()/CommitTransaction()`.
- **Recomendación**: sembrar el horario por **inserción a nivel de aplicación** (no trigger), añadiendo un "PASO 5.2" (tras PASO 5) dentro de la misma transacción: `foreach (día 1..7)` llamar `GuardarHorarioLaboralDia(empresaGuardada.Id, dia, "09:00", "17:00", diaEntre1y5)` — espeja el loop `foreach rol` existente. Trigger se descarta (sin precedente, efectos ocultos, y la transacción ya centraliza la siembra).

### A.7 Riesgos específicos de este anexo

1. **`Empresa` no se edita hoy en ningún lado** (el endpoint `GuardarOActualizarEmpresa` existe pero no está cableado a UI). Si el horario se modelara como columnas en `Empresa`, habría que crear igualmente el editor desde cero → refuerza la Opción 1 (tabla hija + editor en `Configuration`).
2. **`DbWrapper.LlenarEntidad<T>`** mapea por nombre de columna y usa `Convert.ChangeType`: una columna `time` debe mapearse a `TimeSpan` (o exponerla como `string` HH:mm vía `nvarchar(5)`). Validar el mapeo al implementar.
3. **Tipo de dato de hora**: elegir `time` vs `nvarchar(5)` antes de escribir el SP de métricas (el cálculo de intersección día×ventana depende de esto). Recomendado `time`.
4. **`.csproj` legacy**: cualquier POCO/DAL/Service/Controller nuevos deben registrarse manualmente en `ServiceDeskDESIEntities.csproj` y `ServiceDeskDESIMVC.csproj` (listas `<Compile Include>` explícitas) o no compilan.
5. **Zona horaria**: el horario se guarda sin zona; los timestamps de tickets se generan con `GETDATE()` del servidor. Si la empresa y el servidor están en zonas distintas, el KPI de horas hábiles puede desviarse (mismo riesgo ya anotado en §6.4 del explore principal).
