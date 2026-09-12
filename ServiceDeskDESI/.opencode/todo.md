# Mission: Implementar el cambio OpenSpec `tickets-estatus-espera` (estatus 6 Pendiente de Materiales y 7 En Espera de Terceros)

## Context
- Proyecto: .NET Framework 4.8 MVC + WebApi + Entities. Verificación = MSBuild VS2022 (no hay proyecto de tests).
- OpenSpec change: `openspec/changes/tickets-estatus-espera/`.
- SQL se entrega como `migration.sql` (el usuario lo ejecuta en la BD); no hay acceso a BD.

## File Manifest
| Action | File Path | Description |
|--------|-----------|-------------|
| MODIFY | ServiceDeskDESIEntities/Tickets/TicketAsignacion.cs | +FechaEstimada |
| MODIFY | ServiceDeskDESIEntities/Tickets/TicketDTO.cs | +FechaEstimada |
| MODIFY | ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs | PausarTicket/ReanudarTicket |
| MODIFY | ServiceDeskDESIWebApi/Services/TicketService.cs | Métodos + validación |
| MODIFY | ServiceDeskDESIWebApi/Controllers/TicketController.cs | Endpoints + PausarTicketRequest |
| MODIFY | ServiceDeskDESIMVC/DAL/HttpClientConnection.Ticket.cs | Espejo HTTP |
| MODIFY | ServiceDeskDESIMVC/Services/TicketService.cs | Espejo |
| MODIFY | ServiceDeskDESIMVC/Controllers/TicketController.cs | Acciones JSON |
| MODIFY | ServiceDeskDESIMVC/Views/Ticket/Index.cshtml | Botones + render fecha |
| CREATE | ServiceDeskDESIMVC/Views/Ticket/_PausarTicket.cshtml | Modal de pausa |
| MODIFY | ServiceDeskDESIMVC/Views/Ticket/_DetalleTicket.cshtml | Columna fecha estimada |

## M1: Backend (BD ya entregada en migration.sql) | status: completed
### T1.1: Entities/DTO | agent:Worker
- [x] S1.1.1: TicketAsignacion.cs + FechaEstimada | size:S
- [x] S1.1.2: TicketDTO.cs + FechaEstimada | size:S
### T1.2: WebApi | agent:Worker | depends:T1.1
- [x] S1.2.1: DbWrapper.Ticket.cs Pausar/Reanudar | size:M
- [x] S1.2.2: TicketService.cs (WebApi) validación | size:M
- [x] S1.2.3: TicketController.cs (WebApi) endpoints + request | size:M
### T1.3: MVC backend | agent:Worker
- [x] S1.3.1: HttpClientConnection.Ticket.cs espejo | size:M
- [x] S1.3.2: Services/TicketService.cs (MVC) espejo | size:S
- [x] S1.3.3: Controllers/TicketController.cs (MVC) acciones | size:M

## M2: Frontend | status: completed
### T2.1: Vistas | agent:Worker
- [x] S2.1.1: Crear _PausarTicket.cshtml | size:M
- [x] S2.1.2: Index.cshtml botones Pausar/Reanudar + render fecha | size:M
- [x] S2.1.3: _DetalleTicket.cshtml columna fecha estimada | size:S

## M3: Verificación | status: completed
### T3.1: Build + verificación estática | agent:Reviewer | depends:M1,M2
- [x] S3.1.1: Compilar ServiceDeskDESI.sln (MSBuild Debug) 0 errores | size:S
- [x] S3.1.2: Verificar transiciones/bloqueos/dashboard según spec | size:M
- [x] S3.1.3: Marcar [x] y actualizar work-log/status | size:S
