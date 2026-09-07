# Proposal: Módulo "Estadísticas" (Métricas y Desempeño)

> **ESTADO: PAUSADO** (2026-09-07) — El usuario decidió implementar primero un change nuevo `configuracion-empresa` (página de configuración y administración de la empresa: horario laboral, logo, textos footer "by DESi", etc.). Al retomar: resolver Open Questions, integrar tabla `EmpresaHorarioLaboral` (diseño en Anexo A de explore.md) y continuar con sdd-spec.

- **Change**: `metricas-desempeno`
- **Fase**: propose (PAUSADO)
- **Fecha**: 2026-09-07
- **Origen**: `nuevas reglas.txt` (reglas de negocio) + `explore.md` (Q1–Q10) + decisiones fijas D1–D8.

## Intent

Proveer a Administradores y jefes de área ("responsables") un panel de solo lectura con KPIs accionables sobre el desempeño del equipo de soporte (volumen, eficiencia, tiempo de resolución, ranking de áreas y de reasignaciones), sin exportar a hojas de cálculo externas. Es un módulo nuevo de consulta; no altera el flujo de tickets existente.

## Scope

### In Scope
- Página global `Pagina.Nombre='Estadisticas'` (llave sin acento), `NombreVisible='Estadísticas'`, `Tipo='Menu'`; seed `RolPaginaAccion` con `PuedeLeer=1` para roles **Administrador** y **Supervisor** (patrón `MisActivos`).
- Regla de acceso en runtime en el controlador MVC: rol con página asignada (PuedeLeer=1) **Y** (`Area.UsuarioResponsableId == UserID` **O** rol `Administrador`); si no cumple → redirect `Home/AccesoDenegado`. "Jefe de departamento" no es rol, se modela como `Area.UsuarioResponsableId`.
- Vista principal (tarjetas): Total, Nuevos, En Progreso, Resueltos, Cerrados, Rechazados, Eficiencia (%), Tiempo promedio de resolución (h, 1 decimal).
- Gráfica de pastel (distribución de estatus) y gráfica de líneas (evolución diaria: creados vs resueltos) con Chart.js.
- Ranking de Áreas (Área, Total, Promedio urgencia, Cerrados, Rechazados) y Ranking de Reasignaciones (2 listas), TOP N = 5 configurable vía `EstadisticasTopAgentes` en `Web.config`.
- Filtros globales: exactamente 2 `datetime` nativos (inicio/fin), default 1-ene-año-actual → hoy, sin fechas futuras, validación inicio ≤ fin.
- UX: spinner "Cargando datos…", estados "Sin datos", textos en español con acentos, `.cshtml` UTF-8 con BOM.

### Out of Scope
- Proceso externo de auto-cierre por inactividad (futuro).
- Actualización en tiempo real (push / SignalR).
- Exportación (Excel/PDF).
- Operaciones de escritura (módulo 100 % consulta).
- Selector 7/15/30/60/90 días (CANCELADO).

## Capabilities

### New Capabilities
- `metricas-desempeno`: panel "Estadísticas" (acceso, filtros, KPIs, gráficas y rankings) con resolución multi-tenant por `@Usuario`.

### Modified Capabilities
- None (ningún spec existente en `openspec/specs/` cambia a nivel de requisitos).

## Approach

Replicar el precedente `Dashboard` (DashboardController → DashboardService → DbWrapper.Dashboard.cs → SP; MVC DashboardService → HttpClientConnection) + seed de página de `migration.sql:339-354`.

- **SPs pequeños dedicados por bloque** (enfoque recomendado por explore §5): `ObtenerMetricasResumen`, `ObtenerDistribucionEstatus` (o fusionado al resumen), `ObtenerEvolucionDiaria`, `ObtenerRankingAreas`, `ObtenerRankingReasignaciones`. Cada uno con `@Usuario`, `@FechaInicio`, `@FechaFin`; el `EmpresaId` se resuelve internamente desde `@Usuario` (`Usuarios.EmpresaId`), **nunca** desde input del cliente.
- **Backend WebApi**: `DbWrapper.Estadisticas.cs` (partial) → `EstadisticasService` → `EstadisticasController` (`[Authorize] [RoutePrefix("api/Estadisticas")]`, `var usuario = User.Identity.Name`). `[Permiso("Estadisticas","Leer")]` como defensa en profundidad.
- **Frontend MVC**: `HttpClientConnection.Estadisticas.cs` (partial) → `EstadisticasService` → `EstadisticasController` (valida acceso y jefe de área) → `Views/Estadisticas/Index.cshtml` (tarjetas + canvas + tablas + 2 inputs `datetime`).
- **DTOs** nuevos en `ServiceDeskDESIEntities/Tickets/`, registrados en el `.csproj` (old-style, lista explícita).
- **Menú**: sin cambios de render; el ítem aparece al sembrar `Pagina` + `RolPaginaAccion`.

## Definiciones de negocio (semántica por KPI)

| KPI / bloque | Regla |
|---|---|
| Tarjeta **Total** | tickets **creados** en rango (`Ticket.FechaCreacion`), `Estatus=1`, `EmpresaId` |
| **Nuevos/En Progreso/Resueltos/Cerrados/Rechazados** | creados en rango agrupados por `TicketEstatusId` actual |
| **Eficiencia** | `Cerrados / Total * 100` |
| **Tiempo promedio resolución** | tickets resueltos (último `TicketAsignacion.TipoMovimiento='Resolver'`); horas hábiles Lun–Vie 09:00–17:00 (8h), promedio con 1 decimal |
| **Pie de distribución** | conteo por estatus actual de creados en rango |
| **Evolución diaria** | 2 series por día: creados/día vs resueltos/día |
| **Ranking Áreas** | creados en rango: Total, `AVG(Urgencia)`, Cerrados, Rechazados; orden Total desc |
| **Ranking reciben** | filas `TipoMovimiento='Reasignar'` en rango, agrupadas por `UsuarioId` destino |
| **Ranking quitan** | agente de la fila anterior activa puesta en `EsActiva=0` antes de un `Reasignar` (LAG/autounión) |

## Affected Areas

| Área | Impacto | Descripción |
|---|---|---|
| `openspec/changes/metricas-desempeno/migration.sql` (+`rollback.sql`) | Nuevo | `Pagina` + `RolPaginaAccion` + SP(s) idempotentes |
| `ServiceDeskDESIEntities/Tickets/*DTO.cs` + `.csproj` | Nuevo | DTOs + registro `<Compile Include>` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Estadisticas.cs` | Nuevo | `GetObject(s)` + `LlenarEntidad<T>` |
| `ServiceDeskDESIWebApi/Services/EstadisticasService.cs` | Nuevo | `ModelResponse<T>` + Serilog |
| `ServiceDeskDESIWebApi/Controllers/EstadisticasController.cs` | Nuevo | Endpoints `[Authorize]` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Estadisticas.cs` | Nuevo | `RequestAsync<T>` |
| `ServiceDeskDESIMVC/Services/EstadisticasService.cs` | Nuevo | Lógica MVC |
| `ServiceDeskDESIMVC/Controllers/EstadisticasController.cs` | Nuevo | Acceso + jefe de área |
| `ServiceDeskDESIMVC/Views/Estadisticas/Index.cshtml` | Nuevo | UTF-8 con BOM |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Mod | Registrar 3 `.cs` |
| `ServiceDeskDESIWebApi/Web.config` | Mod | `EstadisticasTopAgentes` (default 5) |
| `Views/Shared/_Layout.cshtml` | Mod (opcional) | Activar Chart.js |

## Risks

| Riesgo | Prob. | Mitigación |
|---|---|---|
| Collation BD no confirmada → `[Permiso]` falla con tilde | Baja | Llave sin acento `Estadisticas` + `NombreVisible` |
| `TicketAsignacion` no está en el dump (CREATE vive en BD hosted) | Media | Verificar `sys.columns` antes de escribir SPs |
| Conteo "agente a quien le quitan" (par anterior/siguiente) | Media | LAG/autounión por `FechaCreacion`; validar con datos |
| Horas hábiles dependen de `GETDATE()` (zona) y 09:00–17:00 fijo | Media | Documentar; parametrizar por empresa si cambia |
| SPs agregadores sin índices en `FechaCreacion`/`EmpresaId`/`TipoMovimiento` | Media | Acotar por rango; evaluar índices |
| `.csproj` legacy sin registrar nuevos `.cs` → no compila | Media | Registrar manualmente (patrón `ThemeHelper`) |
| Sin test project (verificación = MSBuild + revisión) | Media | 0 errores de build + revisión estática |

## Rollback Plan

- SPs y página son aditivos: revertir = `DROP PROCEDURE` + `DELETE` de `Pagina`/`RolPaginaAccion` "Estadisticas" (rollback.sql). Sin cambios destructivos sobre datos.
- Código: quitar controller/service/DAO/view nuevos y su registro en `.csproj`; quitar `EstadisticasTopAgentes` de `Web.config`.
- Chart.js (si se activa en `_Layout.cshtml`): volver a comentar la línea.

## Dependencies

- Migración SQL ejecutada en la BD antes del despliegue.
- Patrón previo `Dashboard` + `MisActivos` (existente, no bloqueante).

## Success Criteria

- [ ] La página "Estadísticas" aparece en el menú solo para Administrador y Supervisor (PuedeLeer=1).
- [ ] El controlador redirige a `Home/AccesoDenegado` si el usuario no es Administrador ni `Area.UsuarioResponsableId == UserID`.
- [ ] Tarjetas, pie, evolución diaria y rankings se cargan con spinner "Cargando datos…" y muestran "Sin datos" cuando no hay registros.
- [ ] Los 2 filtros `datetime` (default 1-ene → hoy) respetan: sin fechas futuras, inicio ≤ fin.
- [ ] Tiempo promedio de resolución usa horas hábiles (Lun–Vie 09:00–17:00) con 1 decimal.
- [ ] Ranking de reasignaciones cuenta solo `TipoMovimiento='Reasignar'`, TOP N desde `EstadisticasTopAgentes`.
- [ ] `ServiceDeskDESI.sln` compila sin errores (0 errores).

## Open Questions

1. **Ranking de Áreas**: ¿aplica TOP N o lista completa de áreas con tickets? (regla de negocio solo define TOP N para reasignaciones).
2. **Horas hábiles**: ¿confirmar jornada 09:00–17:00 (8h) como valor fijo, o parametrizarla por empresa a futuro?
3. **Rol Supervisor**: solo Administrador está confirmado como "ver todo"; ¿Supervisor también ve todas las áreas o se filtra por su área (fase 1 ve todo)?
