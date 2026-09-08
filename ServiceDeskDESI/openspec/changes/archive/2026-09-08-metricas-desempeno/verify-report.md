# Verification Report — `metricas-desempeno`

- **Change**: metricas-desempeno
- **Versión spec**: MD-001..MD-013 (42 escenarios)
- **Modo**: Standard (strict_tdd deshabilitado — sin test project; validación = revisión estática + build registrado)
- **Fecha**: 2026-09-07

---

## Completeness (tasks)

| Métrica | Valor |
|---|---|
| Tareas totales | 28 |
| Tareas completas `[x]` | 28 |
| Tareas incompletas `[ ]` | 0 |

Todas las tareas T1–T28 están marcadas `[x]` en `tasks.md`. No hay tareas incompletas.

---

## Build & Tests

- **Build**: ✅ **PASS (registrado, no re-ejecutado)**. Según `apply-progress` (G6/T27, engram #343): `MSBuild.exe ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug` → **exit code 0, 0 errores** en los 3 proyectos. Warnings: solo los 4 pre-existentes (CS0168 `UserController.cs:128`, `CatalogsController.cs:647`; CS1998 `Startup.cs:163/186`). Sin warnings nuevos.
- **Tests**: ➖ No aplica (sin test project).
- **Coverage**: ➖ No disponible.

---

## Spec Compliance Matrix (static review — MD por MD)

| Req | Veredicto | Evidencia |
|---|---|---|
| **MD-001** Página de menú independiente | ✅ PASS | `migration.sql:86-90` — seed idempotente `IF NOT EXISTS`: `Nombre='Estadisticas'` (sin tilde), `NombreVisible='Estadísticas'`, `Tipo='Menu'`, `Direccion='/Estadisticas'`, `PermisosPadreId=NULL`, `Logo='fas fa-chart-pie'`, `OrdenB=10`, `Estatus=1`. Ítem de primer nivel (no anidado). |
| **MD-002** Seed permisos + visibilidad | ✅ PASS | `migration.sql:97-101` — `INSERT RolPaginaAccion` CROSS JOIN `Rol`×`Pagina`, solo `r.Nombre IN ('Administrador','Supervisor')`, `PuedeLeer=1` y `PuedeCrear/Editar/Eliminar/Exportar=0`, guard `NOT EXISTS` (idempotente). Visibilidad delegada al render existente (`ObtenerPaginasPorUsuario` filtra `PuedeLeer=1`), sin cambios requeridos. |
| **MD-003** Control de acceso runtime | ✅ PASS | MVC `EstadisticasController.cs:31` `[Permiso("Estadisticas","Leer")]`; gate `Index()` líneas 34-66: sesión (`tokenCookie==null||UserID<=0`→`Home/Autentication`), `esAdmin`/`esSupervisor` (`RolService.ObtenerRolesPorUsuario`), `esJefeArea` (`AreaService.ConsultarTodasAreas().Any(a=>a.UsuarioResponsableId==UserID)`), y `if(!esAdmin&&!esSupervisor&&!esJefeArea) RedirectToAction("AccesoDenegado","Home")`. **Opción A**: Supervisor siempre entra (SPs sin filtro por área ⇒ ve todas las áreas). |
| **MD-004** Filtros globales de fecha | ✅ PASS | `Index.cshtml:24,28` — 2 `<input type="date">` con `max="@hoy"`; defaults desde `ViewBag` (`EstadisticasController.cs:68-69` → 1-ene-año-actual y `DateTime.Today`); validación JS `inicio>fin` → `Swal` error (líneas 490-497) y vacío → `Swal` (481-488). |
| **MD-005** Tarjetas KPI | ✅ PASS | 8 tarjetas (`Index.cshtml:48-95`); SP `ObtenerMetricasResumen` conteos por `TicketEstatusId` 1..5 sobre creados en rango (`migration.sql:134-143`); `Eficiencia = ROUND(@Cerrados*100.0/NULLIF(@Total,0),1)` (`:145`); "0" por defecto vía `|| 0` en JS (`renderResumen`). |
| **MD-006** Tiempo promedio en horas hábiles | ✅ PASS (corregido) | Fórmula ISO **corregida** `((DATEPART(weekday, dia.Dia) + @@DATEFIRST - 2) % 7) + 1` (`migration.sql:207`) — mapea `1=Lun..7=Dom`, coincidente con `EmpresaHorarioLaboral.DiaSemana`. Fallback Lun–Vie 09:00–17:00 si `#Horario` vacío (`:156-161`). Day-walk con intersección acotada `dh.Inicio/dh.Fin` (`:193-219`, `CASE WHEN dh.Fin > dh.Inicio` ⇒ sin horas negativas). Promedio `ROUND(SUM/COUNT,1)` decimal(10,1) (`:187-191`), `0` si no hay resueltos. **El off-by-one CRITICAL detectado en G6 quedó corregido en `migration.sql` y el usuario re-aplicó la migración.** |
| **MD-007** Pie distribución de estatus | ✅ PASS | SP `ObtenerDistribucionEstatus` (`:241-260`) — 5 filas `LEFT JOIN`, `Cantidad=0`, `ORDER BY te.Orden`, labels/colors DB-driven. `Index.cshtml` pie `estatusChart` (`:110-113, 245-290`), tooltip `nombre + conteo` (`:280-283`), overlay "Sin datos" si `ΣCantidad==0` (`:251`). |
| **MD-008** Evolución diaria (línea) | ✅ PASS | SP `ObtenerEvolucionDiaria` (`:270-300`) — tally CTE días continuos + `LEFT JOIN`/`ISNULL(...,0)`; `Creados` por `Ticket.FechaCreacion`, `Resueltos` por `TicketAsignacion.TipoMovimiento='Resolver'` (`COUNT(DISTINCT TicketId)`). `Index.cshtml:292-350` — 2 datasets, "Sin datos" si no hay actividad (`:302`). |
| **MD-009** Ranking de Áreas | ✅ PASS | SP `ObtenerRankingAreas` (`:308-332`) — `INNER JOIN Area` (áreas sin tickets omitidas), `ORDER BY Total DESC`, sin TOP N, `PromUrgencia = ROUND(AVG(CAST(Urgencia AS decimal(5,1))),1)`. Tabla + `formatDecimal` (1 decimal) en `Index.cshtml:144-160, 352-375`. |
| **MD-010** Ranking de Reasignaciones | ✅ PASS | SP `ObtenerRankingReasignaciones` (`:341-389`) — solo `TipoMovimiento='Reasignar'` (excluye `Tomar`); **Reciben** = agrupado por `UsuarioId` destino; **Quitan** = `LAG(UsuarioId) OVER (PARTITION BY TicketId ORDER BY FechaCreacion, Id)`; `UNION ALL` + `ROW_NUMBER() OVER (PARTITION BY Tipo ORDER BY Cantidad DESC, UsuarioId) <= @TopN`. TopN: `Web.config:37` `EstadisticasTopAgentes=5` + `EstadisticasService.cs:156-158` `int.TryParse` fallback 5. Vacío "No hay reasignaciones registradas." (`Index.cshtml:206, 396-400`). |
| **MD-011** UX | ✅ PASS | Spinner `spinner-border` + "Cargando datos…" por sección (`Index.cshtml:42-45, 105-108, 122-125, 139-142, 169-172`), `.show()` al disparar / `.always(.hide())` al responder. Mensajes vacíos con acentos: "Aún no hay tickets registrados…" (`:160`), "No hay reasignaciones registradas." (`:206`). `Index.cshtml` **UTF-8 CON BOM verificado (EF BB BF)**. Chart.js activo en `_Layout.cshtml:43` (FullCalendar comentado `:42`). |
| **MD-012** Aislamiento multi-tenant | ✅ PASS | Los 5 SPs reciben solo `@Usuario` (más fechas; el de reasignaciones también `@TopN`) y resuelven `@EmpresaId` interno vía `Usuarios.EmpresaId` (p. ej. `migration.sql:122-123`). Ningún SP acepta `@EmpresaId`. WebApi pasa `User.Identity.Name`; MVC envía solo `fechaInicio/fechaFin` en query string. |
| **MD-013** Solo lectura | ✅ PASS | Solo endpoints `[HttpGet]` + `[Permiso("Estadisticas","Leer")]` (WebApi y MVC). Sin botones/endpoints crear/editar/eliminar/exportar. Sin SignalR; los datos se recargan al aplicar filtro/recargar. |

**Cumplimiento**: 13/13 requisitos PASS (42/42 escenarios cubiertos estructuralmente).

---

## Correctness (estructural)

| Req | Estado | Notas |
|---|---|---|
| MD-001 | ✅ Implementado | Seed página idempotente con llave sin tilde/etiqueta con tilde. |
| MD-002 | ✅ Implementado | RolPaginaAccion solo Admin+Supervisor, lectura=1. |
| MD-003 | ✅ Implementado | `[Permiso]` + gate Opción A + redirección. |
| MD-004 | ✅ Implementado | 2 inputs date con defaults/max/validación. |
| MD-005 | ✅ Implementado | 8 tarjetas + Eficiencia + "0". |
| MD-006 | ✅ Implementado | ISO weekday corregido + fallback + 1 decimal. |
| MD-007 | ✅ Implementado | Pie DB-driven + tooltip + "Sin datos". |
| MD-008 | ✅ Implementado | Línea 2 series + días continuos + "Sin datos". |
| MD-009 | ✅ Implementado | Ranking completo Total DESC, urgencia 1 decimal. |
| MD-010 | ✅ Implementado | Solo 'Reasignar' + LAG + TOP N configurable. |
| MD-011 | ✅ Implementado | Spinner + mensajes vacíos + BOM + Chart.js. |
| MD-012 | ✅ Implementado | Resolución interna por `@Usuario`, sin `EmpresaId` del cliente. |
| MD-013 | ✅ Implementado | Solo lectura, sin exportación ni push. |

---

## Coherence (Design)

| Decisión | Seguida | Notas |
|---|---|---|
| D1. Horas hábiles en SP (day-walk T-SQL) | ✅ Sí | `ObtenerMetricasResumen` implementa el day-walk completo. |
| D2. SP propio de distribución | ✅ Sí | `ObtenerDistribucionEstatus` (labels/colores DB-driven). |
| D3. Ranking reasignaciones: columna `Tipo` | ✅ Sí | `UNION ALL` + columna `Tipo` ('Reciben'/'Quitan'), JS filtra. |
| D4. Conjunto KPI tiempo = resueltos en rango | ✅ Sí | Filtra por `FechaCreacion` del último `Resolver` en rango. |
| D5. Empresa completa (sin filtro por área) | ✅ Sí | SPs solo filtran `@EmpresaId`; sin `@AreaId`. |
| D6. TopN desde appSettings | ✅ Sí | `EstadisticasTopAgentes=5` + `int.TryParse` fallback 5. |

File-by-file del design: verificado (5 DTOs + 5 SPs + DbWrapper/Service/Controller WebApi + HttpClientConnection/Service/Controller MVC + Index.cshtml + `_Layout.cshtml` + csproj ×3 + Web.config). Todos los `.cs` nuevos registrados en sus `.csproj` old-style (grep confirmado: `MetricasResumenDTO.cs`…`RankingReasignacionDTO.cs`, `DbWrapper.Estadisticas.cs`, `EstadisticasService.cs`, `EstadisticasController.cs`, `HttpClientConnection.Estadisticas.cs`, `<Content Include="Views\Estadisticas\Index.cshtml">`).

---

## Issues Found

**CRITICAL**: Ninguna. (El off-by-one de MD-006 detectado en G6 ya está corregido en `migration.sql:207`.)

**WARNING**: Ninguna.

**SUGGESTION** (no bloqueante):
1. **Precheck `Area` incompleto** — `migration.sql:73-78` (y el comentario de columnas `:28`) lista `Area` con `Id, Nombre, Estatus`, pero el gate de acceso de MD-003 depende de `Area.UsuarioResponsableId` (columna real, confirmada en `ServiceDeskDESIEntities/Catalogos/Area.cs:14`). Añadir `UsuarioResponsableId` al SELECT de precheck para que la verificación de drift cubra la columna que usa el gate en runtime.
2. **Nuance semántico de "Quitan"** — el LAG (`migration.sql:355`) atribuye al `UsuarioId` de la fila inmediatamente anterior sin validar que dicha fila tuviera `EsActiva=1`. Es exactamente lo que especifica el design (D3/D5 + snippet SQL), por lo que es fiel; en el flujo normal (Tomar → Reasignar) coincide con "la fila activa puesta en EsActiva=0". Solo en una secuencia patológica (dos `Reasignar` consecutivos sin `Tomar` intermedio) la atribución iría al destino del `Reasignar` previo. Registrado para conocimiento, no es bloqueante.
3. **`<input type="date">` vs "datetime nativo"** — MD-004 dice "2 inputs datetime"; el design asume `type="date"` (supuesto documentado, granularidad diaria suficiente para todos los bloques). Documentado y consistente; sin acción.

---

## Verdict

**PASS**

Cambio `metricas-desempeno` verificado: 28/28 tareas completas, build 0 errores (registrado), 13/13 requisitos (42 escenarios) cubiertos estructuralmente, fiel al design, y el único defecto CRITICAL conocido (MD-006 off-by-one de día de semana) está corregido en `migration.sql` y re-aplicado por el usuario. Sin issues CRITICAL ni WARNING. Listo para `sdd-archive`.
