# Mission Status

## Mission
Implementar el cambio OpenSpec `tickets-estatus-espera` (estatus 6 "Pendiente de Materiales" y 7 "En Espera de Terceros").

## Progress
- .opencode/todo.md: **17/17** (14 sub-tasks `[x]` + 3 milestones `status: completed`) — 100%
- openspec/changes/tickets-estatus-espera/tasks.md: **22/22** `[x]`
- Issues: **0 unresolved** (sin sync-issues)
- Workers: 0 active
- Verification Strategy: MSBuild VS2022 (no hay proyecto de tests)
- Execution Status: **pass**

## Current Phase
COMPLETE — implementación terminada y verificada.

## Build Evidence
- Command: `MSBuild.exe ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug /nologo /verbosity:minimal`
- Result: **0 errores**, exit code 0.
- Outputs: `ServiceDeskDESIEntities.dll`, `ServiceDeskDESIMVC.dll`, `ServiceDeskDESIWebApi.dll`.
- Warnings: solo preexistentes CS0168 (UserController.cs:128, CatalogsController.cs:647) y CS1998 (Startup.cs:163,186) — fuera del cambio.

## Static Spec Verification — PASS
- Entities: `TicketAsignacion.FechaEstimada`, `TicketDTO.FechaEstimada`.
- WebApi: `DbWrapper.PausarTicket/ReanudarTicket` (SP `TransicionarTicket` + `@FechaEstimada`), validación de motivo y comentario 1..300, endpoints `Route("Pausar")`/`Route("Reanudar")` + `PausarTicketRequest`.
- MVC: espejo HTTP (`api/Ticket/Pausar`/`Reanudar`), acciones JSON con `[Permiso("Tickets","Editar")]`.
- Vistas: `_PausarTicket.cshtml` (modal), `Index.cshtml` (botones Pausar/Reanudar + fecha estimada), `_DetalleTicket.cshtml` (columna fecha estimada).
- SQL `migration.sql`: estatus 6/7; bloqueo de Resolver/Cerrar/Rechazar/Reasignar desde 6/7; Reanudar 6/7→2; histórico con TipoMovimiento/estatus/FechaEstimada; dashboard ActivosSemana IN (1,2,6,7), Trabajando=2.

## Notes
- SQL se entrega como `migration.sql`; el usuario lo ejecuta en la BD (sin acceso a BD en este entorno).
- `lsp_diagnostics` no disponible; MSBuild es la verificación definitiva de C#.
- `openspec/changes/tickets-estatus-espera/verify-report.md` contiene el detalle (PASS).

## Cambio adicional `tickets-notificacion-estatus` (2026-09-11) — COMPLETE
- Notificación por correo al creador en **cada** cambio de estatus (9 movimientos), best-effort.
- Solo `ServiceDeskDESIWebApi/Services/TicketService.cs` (sin BD/Entities/MVC). Reutiliza `ObtenerTicketPorId` + `ObtenerUsuarioPorNombreUsuario` + `Template_CambioEstatusTicket.html`.
- Build `ServiceDeskDESI.sln` Debug → **0 errores**.

## Correcciones post-verificación (2026-09-11)
- Todas las clases request inline se movieron de los controllers de la WebApi a `ServiceDeskDESIWebApi/Models/` (11 archivos) + `<Compile Include>` en `ServiceDeskDESIWebApi.csproj`. `RestablecerContraseniaRequest` se reutiliza desde `ServiceDeskDESIEntities.Seguridad` (ya existía).
- `_PausarTicket.cshtml` re-guardado como UTF-8 con BOM (único `.cshtml` sin BOM → acentos rotos en Razor).
- Rebuild `ServiceDeskDESI.sln` Debug → **0 errores**.
