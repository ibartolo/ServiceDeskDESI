# Archive Report — metricas-desempeno

- **Change**: `metricas-desempeno`
- **Archived on**: 2026-09-08
- **Archive location**: `openspec/changes/archive/2026-09-08-metricas-desempeno/`
- **Artifact store**: openspec (file-based)
- **Veredicto**: **PASS** (0 issues CRÍTICOS, 0 WARNINGS, 3 sugerencias no bloqueantes)

---

## Resumen del cambio

Nuevo módulo de solo lectura "Estadísticas" (Métricas y Desempeño) — página de menú independiente (`Pagina.Nombre='Estadisticas'`, `NombreVisible='Estadísticas'`, `Tipo='Menu'`, `PermisosPadreId=NULL`, ícono `fa-chart-pie`, `OrdenB=10`) visible solo para roles **Administrador** y **Supervisor** (`RolPaginaAccion.PuedeLeer=1`, resto de acciones 0, idempotente). El controlador MVC hace gate de acceso en runtime (`[Permiso("Estadisticas","Leer")]` + `esAdmin || esSupervisor || esJefeArea`, siendo "jefe de área" modelado como `Area.UsuarioResponsableId == UserID`); si no cumple redirige a `Home/AccesoDenegado`. El panel muestra: 8 tarjetas KPI (Total, Nuevos, En Progreso, Resueltos, Cerrados, Rechazados, Eficiencia %, Tiempo promedio en horas hábiles 1 decimal), gráfica de pastel de distribución de estatus y de líneas de evolución diaria (Chart.js), ranking de áreas (sin TOP N, ordenado por Total desc) y ranking de reasignaciones (Reciben/Quitan, TOP N configurable `EstadisticasTopAgentes` default 5). Filtros globales: 2 inputs `datetime` nativos (default 1-ene-año-actual → hoy, sin fechas futuras, inicio ≤ fin). Aislamiento multi-tenant: todos los SPs resuelven `EmpresaId` internamente desde `@Usuario` (nunca desde el cliente). Sin operaciones de escritura ni exportación ni push en tiempo real.

Implementado sobre el patrón N-capas existente (MVC → HttpClient → WebApi → `DbWrapper` → SPs), replicando el precedente `Dashboard`. 5 SPs dedicados: `ObtenerMetricasResumen`, `ObtenerDistribucionEstatus`, `ObtenerEvolucionDiaria`, `ObtenerRankingAreas`, `ObtenerRankingReasignaciones`. 5 DTOs nuevos en `ServiceDeskDESIEntities/Tickets/`.

## Sync de specs (delta → main specs)

La capability `metricas-desempeno` era **nueva** (no existía main spec previo en `openspec/specs/`), y su delta es un spec completo (sin anotaciones "(Previously: …)"). Acción: **copia íntegra** (verificada por hash SHA256 byte-idéntico, acentos y español preservados).

| Delta (origen) | Main spec (destino) | Acción |
|---|---|---|
| `specs/metricas-desempeno/spec.md` | `openspec/specs/metricas-desempeno/spec.md` | Copiado íntegro (13 reqs MD-001..MD-013, 42 escenarios) |

## Estado de la migración

**APLICADA por el usuario manualmente** (constraint de sdd-apply: el agente nunca ejecuta SQL contra BD). `migration.sql` es idempotente (guards `IF NOT EXISTS`/`NOT EXISTS`), escrito contra el esquema real hosted: seed `Pagina` 'Estadisticas' + `RolPaginaAccion` (solo `Administrador` y `Supervisor`, `PuedeLeer=1`) y los 5 SPs multi-tenant. `rollback.sql` en orden inverso con guards (`DROP PROCEDURE` los 5 SPs → `DELETE` `RolPaginaAccion`/`Pagina`).

> **Nota histórica**: la migración fue aplicada inicialmente el 2026-09-07 y **re-aplicada por el usuario el 2026-09-08** tras corregir el off-by-one de día de semana en MD-006 (`migration.sql:207` — fórmula ISO `((DATEPART(weekday, dia.Dia) + @@DATEFIRST - 2) % 7) + 1`). El único defecto CRÍTICO detectado en G6 quedó corregido y re-verificado.

## Veredicto de verificación

**PASS** — 28/28 tareas `[x]`, build MSBuild VS2022 Debug con **0 errores** en los 3 proyectos (Entities / MVC / WebApi), 13/13 requisitos (MD-001..MD-013) y 42/42 escenarios cubiertos estructuralmente, 6/6 decisiones de diseño seguidas (D1–D6). Registros `.csproj` old-style completos. Sin issues CRÍTICOS ni WARNING. El off-by-one CRÍTICO de MD-006 detectado en G6 quedó corregido en `migration.sql` y re-aplicado.

### Sugerencias (no bloqueantes)

1. **Precheck `Area` incompleto** — `migration.sql:73-78` lista `Area` con `Id, Nombre, Estatus`, pero el gate MD-003 depende de `Area.UsuarioResponsableId`; añadir esa columna al precheck para que la verificación de drift cubra la columna usada en runtime.
2. **Nuance semántico de "Quitan"** — el `LAG` atribuye al `UsuarioId` de la fila inmediatamente anterior sin validar `EsActiva=1`; fiel al design, solo diverge en una secuencia patológica (dos `Reasignar` consecutivos sin `Tomar` intermedio).
3. **`<input type="date">` vs "datetime nativo"** — MD-004 pide "2 inputs datetime"; el design asume `type="date"` (granularidad diaria, documentado). Consistente y sin acción.

## Next steps

1. (Ya hecho) `migration.sql` aplicado y re-aplicado por el usuario tras el fix MD-006.
2. **Smoke-test** manual de `/Estadisticas` (menú solo Admin/Supervisor, gate de acceso, tarjetas, pie/línea, rankings, filtros y estados vacíos).
3. (Opcional) atender las 3 sugerencias de verificación (precheck `Area`, nuance "Quitan", `type="date"`).

## Contenido del archivo

| Artefacto | Presente |
|-----------|----------|
| `proposal.md` | ✅ |
| `design.md` | ✅ |
| `explore.md` | ✅ |
| `tasks.md` (28/28 `[x]`) | ✅ |
| `verify-report.md` (PASS) | ✅ |
| `migration.sql` | ✅ |
| `rollback.sql` | ✅ |
| `specs/metricas-desempeno/spec.md` | ✅ |

## Notas de trazabilidad

- Origen: `nuevas reglas.txt` (reglas de negocio) + `explore.md` (Q1–Q10) + decisiones fijas D1–D8.
- Regla respetada: llave de página sin tilde (`Estadisticas`) + `NombreVisible` con tilde (`Estadísticas`) por seguridad de collation; ningún SP acepta `EmpresaId` del cliente; módulo 100 % consulta.
- Dependencia resuelta: consume `EmpresaHorarioLaboral` (creada en el change `configuracion-empresa`, archivado 2026-09-07) para las horas hábiles, con fallback Lun–Vie 09:00–17:00.
- El `proposal.md` conserva su nota histórica "ESTADO: PAUSADO (2026-09-07)" del momento en que se priorizó `configuracion-empresa`; el cambio fue retomado, implementado, verificado (PASS) y ahora archivado.
