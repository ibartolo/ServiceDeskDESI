# Design: Módulo "Estadísticas" (Métricas y Desempeño)

- **Change**: `metricas-desempeno`
- **Fase**: design
- **Fecha**: 2026-09-07
- **Dependencia**: consume `EmpresaHorarioLaboral` + SP `ObtenerHorarioLaboral` creados por el change archivado `2026-09-07-configuracion-empresa`.

## Overview

Panel de **solo lectura** `Estadisticas` (llave sin tilde, etiqueta "Estadísticas") para Administradores y jefes de área. Réplica 1:1 del precedente `Dashboard` (MVC → `HttpClientConnection` → WebApi → `DbWrapper` → SP) con 5 SPs dedicados por bloque, Chart.js (ya referenciado en `_Layout.cshtml`), tarjetas KPI, pie de estatus, línea de evolución diaria y rankings de áreas y reasignaciones. Sin escritura, sin exportación, sin push. Resolución multi-tenant por `@Usuario` (nunca `EmpresaId` del cliente).

```
Browser (Index.cshtml + JS/Chart.js)
   │  AJAX (fechaInicio/fechaFin) — spinner "Cargando datos…"
   ▼
EstadisticasController (MVC) [Permiso("Estadisticas","Leer")] + gate admin/jefe
   │  EstadisticasService (MVC)
   ▼
HttpClientConnection.Estadisticas ──(ModelResponse<T> JSON)──► WebApi [Authorize][Permiso("Estadisticas","Leer")]
                                                                EstadisticasController
                                                                      ▼
                                                                EstadisticasService (appSettings EstadisticasTopAgentes)
                                                                      ▼
                                                                DbWrapper.Estadisticas (GetObject/GetObjects + LlenarEntidad<T>)
                                                                      ▼
                                                                SPs: ObtenerMetricasResumen, ObtenerDistribucionEstatus,
                                                                     ObtenerEvolucionDiaria, ObtenerRankingAreas, ObtenerRankingReasignaciones
```

---

## Architecture Decisions

| Decisión | Opciones | Decisión | Racional |
|---|---|---|---|
| **D1. Dónde corre el cálculo de horas hábiles** | (a) SP con day-walk en T-SQL; (b) SP devuelve pares (inicio, fin) + horario crudo y C# calcula | **(a) SP** | Convención del repo = "toda lógica en SP" (DbWrapper solo delega; los services C# solo validan + `ModelResponse` + Serilog). Mantiene WebApi/MVC delgados y el KPI reutilizable. El volumen (tickets resueltos en rango × días) es pequeño para un dashboard. |
| **D2. Distribución de estatus: SP propio vs fusionado al resumen** | (a) fusionar (el resumen ya tiene los 5 conteos); (b) SP `ObtenerDistribucionEstatus` propio | **(b) SP propio** | El pie necesita `Nombre` + `Color` desde `TicketEstatus` (labels/colores DB-driven, no hardcodeados → evita drift si cambia "Rechazado"). El resumen es 1 objeto de 8 escalares; la distribución es una lista de 5 filas → formas distintas. |
| **D3. Ranking de reasignaciones: 2 listas** | (a) un SP con 2 result sets (`reader.NextResult()`); (b) un SP, 1 result set con columna discriminadora `Tipo`; (c) 2 SPs | **(b) columna `Tipo`** | `GetObjects`/`LlenarEntidad<T>` mapean **un solo** result set; `NextResult()` no tiene helper en `DbWrapper`. `UNION ALL` + `ROW_NUMBER() PARTITION BY Tipo` da ambas listas en una llamada; el JS filtra por `Tipo`. |
| **D4. Conjunto del KPI "tiempo promedio de resolución"** | (a) tickets **creados** en rango que están Resueltos; (b) tickets **resueltos** en rango (última fila `Resolver` con `FechaCreacion` en rango) | **(b) resueltos en rango** | MD-006 mide "tiempo de resolución" y su escenario "Sin tickets resueltos en el rango" fija el filtro por la fecha del movimiento `Resolver`. El intervalo es `[Ticket.FechaCreacion → FechaCreacion(último Resolver)]`. |
| **D5. Alcance de datos en fase 1** | (a) filtrar por `AreaId` del jefe; (b) empresa completa | **(b) empresa completa (sin filtro por área)** | MD-003 "Supervisor ve todas las áreas", MD-009 "todas las áreas con tickets". El gate de acceso (admin/jefe) restringe *quién entra*, no *qué ve*. Los SPs filtran solo por `@EmpresaId`. |
| **D6. Top N reasignaciones** | hardcode 5 vs appSettings | **appSettings `EstadisticasTopAgentes` (default 5)** en WebApi `Web.config`, leído con `int.TryParse` (espeja `EvidenciasMax*`) y pasado como `@TopN` al SP | MD-010/supuesto 5; configurable sin recompilar. |

---

## DB Design (SPs + seed)

### Seed página + permiso (idempotente)

```sql
IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'Estadisticas')
BEGIN
    INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
    VALUES (N'Estadisticas', N'Estadísticas', N'KPIs de desempeño del equipo de soporte', N'Menu', N'/Estadisticas', NULL, N'fas fa-chart-pie', 10, 1);
END
GO
-- Solo lectura: PuedeLeer=1, resto 0. Roles Administrador y Supervisor.
INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
SELECT r.Id, p.Id, 1, 0, 0, 0, 0, N'migracion', GETDATE(), 1
FROM Rol r CROSS JOIN Pagina p
WHERE p.Nombre = N'Estadisticas' AND r.Nombre IN (N'Administrador', N'Supervisor') AND r.Estatus = 1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
GO
```

> ⚠️ Verificar columnas reales de `Pagina`/`RolPaginaAccion` en BD hosted antes de aplicar (drift conocido). Migración **manual futura** (configuracion-empresa ya archivado); `migration.sql`/`rollback.sql` son archivos nuevos de este change.

### SPs (cada uno: `@Usuario`, `@FechaInicio`, `@FechaFin`; tenant resuelto internamente)

Convención de rango **inclusive**: `FechaCreacion >= @FechaInicio AND FechaCreacion < DATEADD(day,1,@FechaFin)`.

**1. `ObtenerMetricasResumen @Usuario NVARCHAR(25), @FechaInicio DATETIME, @FechaFin DATETIME`** → 1 fila `{ Total, Nuevos, EnProgreso, Resueltos, Cerrados, Rechazados, Eficiencia, HorasPromedioResolucion }`.

- Conteos: tickets creados en rango (`Ticket.FechaCreacion`), `Estatus=1`, `EmpresaId=@EmpresaId`, agrupados por `TicketEstatusId` actual (1/2/3/4/5). `Eficiencia = ROUND(Cerrados*100.0/NULLIF(Total,0),1)` (0 si Total=0).
- `HorasPromedioResolucion`: algoritmo de horas hábiles (ver §Business-hours). Devuelve `decimal(10,1)`, `0` si no hay resueltos.

**2. `ObtenerDistribucionEstatus @Usuario, @FechaInicio, @FechaFin`** → 5 filas `{ EstatusId, Nombre, Color, Cantidad }` (todas las de `TicketEstatus` con `Estatus=1`, `Cantidad=0` si vacío, `ORDER BY te.Orden`). `LEFT JOIN Ticket` creados en rango sobre `TicketEstatusId`.

**3. `ObtenerEvolucionDiaria @Usuario, @FechaInicio, @FechaFin`** → 1 fila por día del rango `{ Fecha, Creados, Resueltos }`. Serie de días continua (tally), `LEFT JOIN` conteos: `Creados` = `Ticket.FechaCreacion` por día; `Resueltos` = `TicketAsignacion.TipoMovimiento='Resolver'` por día (`ISNULL(...,0)`).

**4. `ObtenerRankingAreas @Usuario, @FechaInicio, @FechaFin`** → filas `{ AreaId, AreaNombre, Total, PromUrgencia, Cerrados, Rechazados }`, `ORDER BY Total DESC` (sin TOP N). `PromUrgencia = ROUND(AVG(CAST(Urgencia AS decimal(5,1))),1)`. Áreas sin tickets no aparecen.

**5. `ObtenerRankingReasignaciones @Usuario, @FechaInicio, @FechaFin, @TopN INT`** → filas `{ Tipo, UsuarioId, Nombre, Apellido, NombreUsuario, Cantidad }`, máximo `@TopN` por `Tipo` (`ROW_NUMBER() OVER (PARTITION BY Tipo ORDER BY Cantidad DESC) <= @TopN`).

- **Reciben** (destino): `COUNT(*)` de `TicketAsignacion.TipoMovimiento='Reasignar'` agrupado por `UsuarioId`.
- **Quitan** (origen): con `LAG` sobre `TicketAsignacion` ordenado por `FechaCreacion, Id`, `PARTITION BY TicketId`:

```sql
;WITH Movs AS (
    SELECT TicketId, UsuarioId, TipoMovimiento, FechaCreacion,
           LAG(UsuarioId) OVER (PARTITION BY TicketId ORDER BY FechaCreacion, Id) AS AgenteAnterior
    FROM TicketAsignacion
    WHERE Estatus = 1 AND EmpresaId = @EmpresaId)
SELECT u.Id AS UsuarioId, u.Nombre, u.Apellido, u.NombreUsuario, 'Quitan' AS Tipo, COUNT(*) AS Cantidad
FROM Movs m JOIN Usuarios u ON u.Id = m.AgenteAnterior
WHERE m.TipoMovimiento = 'Reasignar' AND m.FechaCreacion >= @FechaInicio AND m.FechaCreacion < DATEADD(day,1,@FechaFin)
GROUP BY u.Id, u.Nombre, u.Apellido, u.NombreUsuario
```

Ambas listas se combinan con `UNION ALL` y se aplica `@TopN` por tipo. Filtro de rango **sobre la fecha del evento `Reasignar`** en ambas.

---

## Entities / DTO Design (`ServiceDeskDESIEntities/Tickets/`, 5 archivos nuevos)

```csharp
public class MetricasResumenDTO {
    public int Total { get; set; }
    public int Nuevos { get; set; }
    public int EnProgreso { get; set; }
    public int Resueltos { get; set; }
    public int Cerrados { get; set; }
    public int Rechazados { get; set; }
    public decimal Eficiencia { get; set; }            // SQL decimal(5,1)
    public decimal HorasPromedioResolucion { get; set; } // SQL decimal(10,1)
}
public class DistribucionEstatusDTO {
    public int EstatusId { get; set; }
    public string Nombre { get; set; }
    public string Color { get; set; }
    public int Cantidad { get; set; }
}
public class EvolucionDiariaDTO {
    public DateTime Fecha { get; set; }
    public int Creados { get; set; }
    public int Resueltos { get; set; }
}
public class RankingAreaDTO {
    public long AreaId { get; set; }
    public string AreaNombre { get; set; }
    public int Total { get; set; }
    public decimal PromUrgencia { get; set; }  // SQL decimal(4,1)
    public int Cerrados { get; set; }
    public int Rechazados { get; set; }
}
public class RankingReasignacionDTO {
    public string Tipo { get; set; }         // 'Reciben' | 'Quitan'
    public long UsuarioId { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string NombreUsuario { get; set; }
    public int Cantidad { get; set; }
}
```

Notas de mapeo: `LlenarEntidad<T>` mapea por nombre case-insensitive con `Convert.ChangeType`; los alias SQL deben coincidir exactamente con los nombres de propiedad (`EnProgreso`, `PromUrgencia`, `HorasPromedioResolucion`, etc.). `decimal` SQL ↔ `decimal` C#; `datetime` ↔ `DateTime`; `COUNT/SUM` int ↔ `int`; `AVG`/`ROUND` forzados a `decimal` vía `CAST` para evitar `decimal`→`double` desajustes.

---

## WebApi Design

### `Controllers/EstadisticasController.cs` (nuevo)

```csharp
[Authorize]
[RoutePrefix("api/Estadisticas")]
public class EstadisticasController : BaseController
{
    private readonly EstadisticasService _service = new EstadisticasService();

    [HttpGet, Route("Resumen")]            [Permiso("Estadisticas", "Leer")]
    public ModelResponse<MetricasResumenDTO> Resumen(DateTime fechaInicio, DateTime fechaFin)
        => _service.ObtenerResumen(User.Identity.Name, fechaInicio, fechaFin);

    [HttpGet, Route("DistribucionEstatus")] [Permiso("Estadisticas", "Leer")]
    public ModelResponse<List<DistribucionEstatusDTO>> DistribucionEstatus(DateTime fechaInicio, DateTime fechaFin)
        => _service.ObtenerDistribucionEstatus(User.Identity.Name, fechaInicio, fechaFin);

    [HttpGet, Route("EvolucionDiaria")]    [Permiso("Estadisticas", "Leer")]
    public ModelResponse<List<EvolucionDiariaDTO>> EvolucionDiaria(DateTime fechaInicio, DateTime fechaFin)
        => _service.ObtenerEvolucionDiaria(User.Identity.Name, fechaInicio, fechaFin);

    [HttpGet, Route("RankingAreas")]       [Permiso("Estadisticas", "Leer")]
    public ModelResponse<List<RankingAreaDTO>> RankingAreas(DateTime fechaInicio, DateTime fechaFin)
        => _service.ObtenerRankingAreas(User.Identity.Name, fechaInicio, fechaFin);

    [HttpGet, Route("RankingReasignaciones")] [Permiso("Estadisticas", "Leer")]
    public ModelResponse<List<RankingReasignacionDTO>> RankingReasignaciones(DateTime fechaInicio, DateTime fechaFin)
        => _service.ObtenerRankingReasignaciones(User.Identity.Name, fechaInicio, fechaFin);
}
```

`[Authorize]` (401) a nivel controller + `[Permiso("Estadisticas","Leer")]` (403) por acción = defensa en profundidad. Sin endpoints de escritura (MD-013). Solo `Leer` (la página no tiene `PuedeEditar`).

### `Services/EstadisticasService.cs` (nuevo, WebApi)

- 5 métodos que validan `usuario` no vacío (`ArgumentException`) y delegan en `_dbWrapper`, envolviendo en `ModelResponse<T>` + `Serilog` (espeja `DashboardService`).
- `ObtenerRankingReasignaciones` lee el Top N: `if (!int.TryParse(ConfigurationManager.AppSettings["EstadisticasTopAgentes"], out int topN)) topN = 5;` y pasa `topN` al `DbWrapper`.

### `DAL/DbWrapper.Estadisticas.cs` (nuevo partial)

`GetObject("ObtenerMetricasResumen", ..., [@Usuario,@FechaInicio,@FechaFin], r => LlenarEntidad<MetricasResumenDTO>(r))` y `GetObjects(...)` con `LlenarEntidad<T>` para las 4 listas (espeja `DbWrapper.Dashboard.cs` / `DbWrapper.HorarioLaboral.cs`). Parámetros: `new SqlParameter("@Usuario", usuario)`, `new SqlParameter("@FechaInicio", fechaInicio)`, `new SqlParameter("@FechaFin", fechaFin)`, y `new SqlParameter("@TopN", topN)`.

---

## MVC Design

### `Controllers/EstadisticasController.cs` (nuevo, : BaseController)

Constructor cablea `_estadisticasService = new EstadisticasService(httpClientConnection)`, `_rolService`, `_areaService`.

| Acción | Permiso | Comportamiento |
|---|---|---|
| `Index()` → `View()` | `[Permiso("Estadisticas","Leer")]` | Gate de acceso (abajo); si pasa, `ViewBag.FechaInicio = new DateTime(DateTime.Now.Year,1,1)`, `ViewBag.FechaFin = DateTime.Today`; `return View()`. |
| `ObtenerResumen(DateTime fechaInicio, DateTime fechaFin)` → `string` | `[Permiso("Estadisticas","Leer")]` | `JsonConvert.SerializeObject(await _estadisticasService.ObtenerResumen(...))`. |
| `ObtenerDistribucionEstatus(...)` → `string` | idem | idem |
| `ObtenerEvolucionDiaria(...)` → `string` | idem | idem |
| `ObtenerRankingAreas(...)` → `string` | idem | idem |
| `ObtenerRankingReasignaciones(...)` → `string` | idem | idem |

**Gate de acceso (`Index`)**, espejando `HomeController.Configuration` y `TicketController.Index`:

```csharp
var tokenCookie = SessionHelper.GetSessionUser();
if (tokenCookie == null || tokenCookie.UserID <= 0) return RedirectToAction("Autentication", "Home");

var rolesResponse = await _rolService.ObtenerRolesPorUsuario(tokenCookie.UserID);
bool esAdmin = rolesResponse.IsSuccess && rolesResponse.Response != null
             && rolesResponse.Response.Any(r => r.Nombre == "Administrador");
bool esSupervisor = rolesResponse.IsSuccess && rolesResponse.Response != null
                  && rolesResponse.Response.Any(r => r.Nombre == "Supervisor");

var areasResponse = await _areaService.ConsultarTodasAreas();
bool esJefeArea = areasResponse.IsSuccess && areasResponse.Response != null
                && areasResponse.Response.Any(a => a.UsuarioResponsableId == tokenCookie.UserID);

// Opción A (decisión del usuario): el rol Supervisor SIEMPRE entra (ve todas las áreas en fase 1).
if (!esAdmin && !esSupervisor && !esJefeArea) return RedirectToAction("AccesoDenegado", "Home");
```

### `Services/EstadisticasService.cs` (nuevo, MVC) + `DAL/HttpClientConnection.Estadisticas.cs` (nuevo partial)

MVC `EstadisticasService(HttpClientConnection)` delega en `HttpClientConnection`. `HttpClientConnection.Estadisticas.cs`: `RequestAsync<MetricasResumenDTO>("api/Estadisticas/Resumen?...", HttpMethod.Get, null, token.Token.access_token)` con query string `?fechaInicio=...&fechaFin=...` (formato ISO). Listas vía `RequestAsync<List<...>>(...)`.

### `Views/Estadisticas/Index.cshtml` (nuevo, **UTF-8 CON BOM**)

1. **Filtros**: 2 `<input type="date">` nativos (default `ViewBag.FechaInicio`/`FechaFin`), `max = hoy` en ambos; validación JS `inicio <= fin` (rechaza con `Swal` si no); botón "Aplicar". (MD-004 permite `datetime-local` o `date`; se usa `date` — granularidad de día es suficiente para todos los bloques.)
2. **Tarjetas** (8): Total, Nuevos, En Progreso, Resueltos, Cerrados, Rechazados, Eficiencia (`78%`), Tiempo promedio (`4.5 h`). Valor 0 si ausente.
3. **Pie** `<canvas id="estatusChart">`: labels=`Nombre`, data=`Cantidad`, backgroundColor=`Color` (DB-driven). Si `ΣCantidad==0` → overlay "Sin datos".
4. **Línea** `<canvas id="evolucionChart">`: labels=`Fecha` (dd/MM), 2 datasets (`Creados`, `Resueltos`). Vacío → "Sin datos".
5. **Tablas de ranking**: áreas (completa) y reasignaciones (2 tablas: `filter(Tipo==='Reciben')` / `'Quitan'`).
6. **Spinner** por sección: `spinner-border` de Bootstrap + "Cargando datos…" visible durante cada AJAX; mensajes vacíos "Aún no hay tickets registrados…" / "No hay reasignaciones registradas." (MD-011).

**JS**: una función `cargarEstadisticas(inicio, fin)` que dispara las 5 peticiones `$.get` en paralelo a `/Estadisticas/Obtener*`, renderiza tarjetas/gráficas/tablas, y destruye/recrea los objetos Chart.js (`window.miChart?.destroy()`). Chart.js se instancia solo si `typeof Chart !== 'undefined'`.

### `Views/Shared/_Layout.cshtml` (modificar — activar Chart.js)

Descomentar solo Chart.js (dejar FullCalendar comentado):

```html
<!--<script src="https://cdn.jsdelivr.net/npm/fullcalendar@5.11.3/main.min.js"></script>-->
<script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js"></script>
```

(El CSS de Chart.js ya está activo en línea 15.)

---

## Business-hours Algorithm (en `ObtenerMetricasResumen`)

1. Resolver `@EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1)`.
2. Cargar horario a `#Horario` (temp): `SELECT DiaSemana, CAST(HoraInicio AS time) HoraInicio, CAST(HoraFin AS time) HoraFin FROM EmpresaHorarioLaboral WHERE EmpresaId=@EmpresaId AND Estatus=1`. Si **no hay filas** → fallback `VALUES (1..5)` con `09:00`/`17:00` (Lun–Vie).
3. Conjunto de resueltos: última fila `Resolver` por ticket dentro del rango:

```sql
;WITH Resolver AS (
   SELECT TicketId, FechaCreacion FechaFin,
          ROW_NUMBER() OVER (PARTITION BY TicketId ORDER BY FechaCreacion DESC, Id DESC) rn
   FROM TicketAsignacion
   WHERE EmpresaId=@EmpresaId AND Estatus=1 AND TipoMovimiento='Resolver'
     AND FechaCreacion >= @FechaInicio AND FechaCreacion < DATEADD(day,1,@FechaFin))
SELECT t.FechaCreacion FechaInicio, r.FechaFin
FROM Resolver r JOIN Ticket t ON t.Id = r.TicketId AND t.Estatus=1 AND t.EmpresaId=@EmpresaId
WHERE r.rn = 1
```

4. Por cada intervalo, **recorrer los días** entre `CAST(inicio AS date)` y `CAST(fin AS date)` (tally CTE), y por día `d` laborable sumar la intersección:

```
day_start = MAX(inicio, d + HoraInicio)   -- d + HoraInicio = fecha d con la hora del horario
day_end   = MIN(fin,    d + HoraFin)
horas_dia = GREATEST(0, DATEDIFF(MINUTE, day_start, day_end)) / 60.0   -- CASE WHEN day_end > day_start
```

Día laborable ⇔ `d` está en `#Horario` con `DiaSemana` ISO mapeado **independiente de `@@DATEFIRST`**: `((DATEPART(WEEKDAY, d) + @@DATEFIRST - 1) % 7) + 1`. Los días sin fila en `#Horario` (fines de semana u otros no laborables) se saltan (aportan 0).

5. `HorasPromedioResolucion = ROUND(SUM(horas) / NULLIF(COUNT(*),0), 1)`; `0` si no hay resueltos.

**Gotchas (documentar en código)**: (a) `@@DATEFIRST` varía por servidor → usar la fórmula ISO; (b) `HoraInicio`/`HoraFin` son `datetime` anclados a `1900-01-01` → extraer solo la hora con `CAST(... AS time)` y combinarla con la parte de fecha de cada día caminado; (c) timestamps reales vienen de `GETDATE()` (zona del servidor) — misma limitación anotada en explore §6.4/A.7.

---

## Multi-tenant & Security

- **WebApi**: todos los SPs reciben `@Usuario` (`User.Identity.Name`) y resuelven `@EmpresaId` internamente; **nunca** aceptan `EmpresaId` del query/body (MD-012). `[Authorize]` (401) + `[Permiso("Estadisticas","Leer")]` (403).
- **MVC**: `[Permiso("Estadisticas","Leer")]` redirige a `Home/AccesoDenegado` si `PuedeLeer=0`; gate runtime `esAdmin || esSupervisor || esJefeArea` (Opción A, decisión del usuario: Supervisor siempre autorizado). Los endpoints AJAX también llevan `[Permiso("Estadisticas","Leer")]`.
- **Menú**: sin cambios de render; el ítem aparece al sembrar `Pagina` + `RolPaginaAccion` (`ObtenerPaginasPorUsuario` filtra `PuedeLeer=1`).
- **Sin área-scoping**: los SPs no reciben `@AreaId`; fase 1 = datos de toda la empresa.

---

## Error Handling / Logging

- WebApi: `ModelResponse<T>` (`IsSuccess`/`Message`/`Response`) en todo; `Serilog` (`Log.Information/Warning/Error`) con `{Usuario}` (espeja `DashboardService`).
- MVC: acciones AJAX devuelven `JsonConvert.SerializeObject(modelResponse)`; el JS muestra `Message` con `Swal` si `IsSuccess=false`; `Serilog` en errores.
- SPs: `SET NOCOUNT ON`; si `@EmpresaId` es NULL se devuelve conjunto vacío/ceros (no hay error de tenant).

---

## File-by-file change list

| Archivo | Acción | Responsabilidad |
|---|---|---|
| `openspec/changes/metricas-desempeno/migration.sql` | Create | Seed `Pagina`/`RolPaginaAccion` (idempotente) + 5 SPs (`DROP`/`CREATE`) |
| `openspec/changes/metricas-desempeno/rollback.sql` | Create | `DROP` 5 SPs + `DELETE` página/permiso "Estadisticas" |
| `ServiceDeskDESIEntities/Tickets/MetricasResumenDTO.cs` | Create | DTO resumen (8 valores) |
| `ServiceDeskDESIEntities/Tickets/DistribucionEstatusDTO.cs` | Create | DTO distribución |
| `ServiceDeskDESIEntities/Tickets/EvolucionDiariaDTO.cs` | Create | DTO evolución diaria |
| `ServiceDeskDESIEntities/Tickets/RankingAreaDTO.cs` | Create | DTO ranking áreas |
| `ServiceDeskDESIEntities/Tickets/RankingReasignacionDTO.cs` | Create | DTO ranking reasignaciones |
| `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` | Modify | Registrar 5 `<Compile Include>` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Estadisticas.cs` | Create | `GetObject`/`GetObjects` + `LlenarEntidad<T>` |
| `ServiceDeskDESIWebApi/Services/EstadisticasService.cs` | Create | `ModelResponse<T>` + Serilog + appSettings TopN |
| `ServiceDeskDESIWebApi/Controllers/EstadisticasController.cs` | Create | 5 endpoints `[Authorize]` + `[Permiso]` |
| `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` | Modify | Registrar 3 `.cs` nuevos |
| `ServiceDeskDESIWebApi/Web.config` | Modify | `<add key="EstadisticasTopAgentes" value="5" />` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Estadisticas.cs` | Create | `RequestAsync<T>` × 5 |
| `ServiceDeskDESIMVC/Services/EstadisticasService.cs` | Create | Lógica MVC (delegación) |
| `ServiceDeskDESIMVC/Controllers/EstadisticasController.cs` | Create | `Index` (gate) + 5 AJAX |
| `ServiceDeskDESIMVC/Views/Estadisticas/Index.cshtml` | Create | UTF-8 CON BOM: filtros + tarjetas + canvas + tablas + spinner |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Modify | Registrar 3 `.cs` nuevos |
| `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml` | Modify | Descomentar Chart.js JS |

**Registro `.csproj` obligatorio** (old-style `<Compile Include>`): cualquier `.cs` no registrado no compila (precedente `ThemeHelper`). `.cshtml` no requiere registro (compila en runtime).

---

## Verification Plan (sin test project)

- **Build**: `MSBuild.exe ServiceDeskDESI.sln /t:Build /p:Configuration=Debug` → **0 errores** en los 3 proyectos (confirma los `<Compile Include>` manuales).
- **Revisión estática contra spec (MD-001..MD-013, 42 escenarios)**: página sembrada (llave sin tilde, etiqueta con tilde, `Tipo='Menu'`, `PermisosPadreId=NULL`, logo, `OrdenB`); `RolPaginaAccion` solo Admin+Supervisor con `PuedeLeer=1` y resto 0; gate `[Permiso]` + `esAdmin || esSupervisor || esJefeArea` → `AccesoDenegado`; 2 inputs fecha con defaults/max/validación; tarjetas con "0" en vacíos y Eficiencia `Cerrados/Total*100`; horas hábiles con `EmpresaHorarioLaboral` (1 decimal, fallback Lun–Vie 09–17); pie/línea/rankings con spinner y "Sin datos"; reasignaciones solo `Reasignar` + TOP N desde `EstadisticasTopAgentes`; ranking áreas completo ordenado Total desc, urgencia 1 decimal; ningún SP acepta `EmpresaId` del cliente; `Index.cshtml` UTF-8 CON BOM.
- **Smoke manual (post-deploy)**: menú visible solo Admin/Supervisor; `/Estadisticas` redirige a `AccesoDenegado` solo si no es Admin, no es Supervisor y no es jefe de área (Opción A); aplicar rango recalcula los 5 bloques; gráficas renderizan.

---

## Assumptions / Open items

- **Input de fecha**: se usa `<input type="date">` (no `datetime-local`); la spec permite ambos y todos los bloques son de granularidad diaria. (Si se exige `datetime-local`, el contrato de query y el `DATEADD(day,1,@FechaFin)` ya soportan hora.)
- **Nuance de acceso (RESUELTO — Opción A)**: el usuario decidió que el rol `Supervisor` SIEMPRE entra (sin exigir jefatura), viendo todas las áreas en fase 1. Gate: `esAdmin || esSupervisor || esJefeArea`. Un Agente/Usuario con `PuedeLeer` (otorgado manualmente) pero sin Admin/Supervisor/jefatura verá el ítem y será redirigido a `AccesoDenegado`.
- **Zona horaria** del `GETDATE()` del servidor afecta el corte de día (heredado, documentado).
- Sin test project: verificación = MSBuild 0 errores + revisión estática (sin smoke manual posible en esta fase).
