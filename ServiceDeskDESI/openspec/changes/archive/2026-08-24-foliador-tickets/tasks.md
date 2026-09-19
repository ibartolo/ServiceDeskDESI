# Tasks: Foliador de Tickets (folio/consecutivo)

Orden: BD → Entities → WebApi DAL → WebApi Services → WebApi Controllers → MVC → Build.

## Lote 1: BD / migración

- [x] T1 — Crear `openspec/changes/foliador-tickets/migration.sql`: `CREATE TABLE Foliador` (EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo) con PK `(EmpresaId,Nombre)`, FK→Empresa, defaults `Consecutivo=0`/`FechaActualizacion=GETDATE`; idempotente `IF OBJECT_ID(...) IS NULL`. (FOL-001)
- [x] T2 — `migration.sql`: `ALTER TABLE Ticket ADD Folio NVARCHAR(50) NULL` guardado por `sys.columns` (sin backfill). (FOL-006)
- [x] T3 — `migration.sql`: `CREATE PROCEDURE ConsultarFoliador` (@Nombre, @Usuario→EmpresaId) devuelve fila o vacío. (FOL-002)
- [x] T4 — `migration.sql`: `CREATE PROCEDURE ActualizarFoliador` — upsert defensivo + `UPDATE ... WITH (UPDLOCK,HOLDLOCK) SET Consecutivo=Consecutivo+1 OUTPUT INSERTED.Consecutivo`. (FOL-003)
- [x] T5 — `migration.sql`: `ALTER GuardarOActualizarTicket` + `@Folio NVARCHAR(50)=NULL`; INSERT inserta `Folio`, UPDATE no lo toca. (FOL-004, FOL-006)
- [x] T6 — `migration.sql`: seed `INSERT Foliador SELECT e.Id,...,'Ticket',...,0 FROM Empresa e WHERE NOT EXISTS(...)`. (FOL-001)
- [x] T7 — Crear `rollback.sql` orden inverso: DROP SPs, `ALTER Ticket DROP Folio`, DROP Foliador, restaurar `GuardarOActualizarTicket`. (FOL-001…FOL-006)

## Lote 2: Entities

- [x] T8 — Crear `ServiceDeskDESIEntities/Catalogos/Foliador.cs` (EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo) + registrar `<Compile Include>` en `ServiceDeskDESIEntities.csproj`. (FOL-001, FOL-002)
- [x] T9 — Crear `ServiceDeskDESIEntities/Catalogos/FoliadorDTO.cs` (`: Foliador` + `string FolioSiguiente`) + `.csproj`. (FOL-005)
- [x] T10 — Modificar `ServiceDeskDESIEntities/Tickets/Ticket.cs`: + `public string Folio { get; set; }` (hereda a `TicketDTO`). (FOL-006)

## Lote 3: WebApi DAL

- [x] T11 — Crear `ServiceDeskDESIWebApi/DAL/DbWrapper.Foliador.cs`: `ConsultarFoliador(nombre,usuario)`→`GetObject`, `ActualizarFoliador(nombre,usuario)`→`ExecuteScalar` + `.csproj`. (FOL-002, FOL-003)
- [x] T12 — Modificar `ServiceDeskDESIWebApi/DAL/DbWrapper.Ticket.cs`: añadir `t.Folio` al objeto anónimo `parametrosObj` de `GuardarOActualizarTicket`. (FOL-004, FOL-006)

## Lote 4: WebApi Services

- [x] T13 — Crear `ServiceDeskDESIWebApi/Services/FoliadorService.cs`: ctor `FoliadorService(DbWrapper)`, `ConsultarConsecutivo` (público), `ActualizarConsecutivo` (internal), `static string FormatearFolio(int c) => $"T-{c:00000}"` + `.csproj`. (FOL-002, FOL-003, FOL-005)
- [x] T14 — Modificar `ServiceDeskDESIWebApi/Services/TicketService.cs` `GuardarTicketConEvidencias`: `new FoliadorService(_dbWrapper)` (instancia compartida); dentro del `BeginTransaction`, antes de `GuardarOActualizarTicket`, `ActualizarConsecutivo` + `ticket.Folio = FormatearFolio(c)`. (FOL-004)

## Lote 5: WebApi Controllers

- [x] T15 — Crear `ServiceDeskDESIWebApi/Controllers/FoliadorController.cs`: `[Authorize] [RoutePrefix("api/Foliador")]`, `[HttpGet, Route("Consultar")] Consultar(string nombre)` → `ModelResponse<FoliadorDTO>` con `FolioSiguiente=FormatearFolio(c+1)`; SIN action de actualizar + `.csproj`. (FOL-002, FOL-005, FOL-007)
- [x] T16 — Modificar `ServiceDeskDESIWebApi/Controllers/TicketController.cs` `LeerTicketDesdeForm`: + `ticket.Folio = form["Folio"];`. (FOL-004, FOL-006)

## Lote 6: MVC

- [x] T17 — Crear `ServiceDeskDESIMVC/DAL/HttpClientConnection.Foliador.cs`: `ConsultarFoliador()` → `RequestAsync<FoliadorDTO>("api/Foliador/Consultar?nombre=Ticket", GET)` + `.csproj`. (FOL-002, FOL-005)
- [x] T18 — Crear `ServiceDeskDESIMVC/Services/FoliadorService.cs`: `ConsultarFolioSiguiente()` wrapper + `.csproj`. (FOL-005)
- [x] T19 — Modificar `ServiceDeskDESIMVC/Services/TicketService.cs` `GuardarTicketConEvidencias`: + `form.Add(new StringContent(ticket.Folio ?? string.Empty), "Folio");` (HttpClientConnection.Ticket.cs no cambia: `PostMultipartAsync` es genérico). (FOL-004)
- [x] T20 — Modificar `ServiceDeskDESIMVC/Controllers/TicketController.cs`: inyectar `FoliadorService`, + `[HttpGet] ConsultarFoliador()` → JSON preview. (FOL-005)
- [x] T21 — Modificar `ServiceDeskDESIMVC/Views/Ticket/_CapturarTicket.cshtml`: campo `#Folio` (disabled/readonly) + populate en `shown.bs.modal` vía `GetMVC('/Ticket/ConsultarFoliador')`. (FOL-005)

## Lote 7: Build / verificación

- [x] T22 — Compilar `ServiceDeskDESI.sln` (MSBuild VS2022, Debug) → 0 errores; verificación manual: guardar ticket → `Folio='T-00001'`; abrir captura → preview; `GET api/Foliador/Consultar` OK y sin endpoint de actualizar (404). (FOL-001…FOL-007)

## Follow-up (post-verify)

- [x] T23 — `GuardarOActualizarTicket` (WebApi): generar folio atómicamente al CREAR (`ticket.Id <= 0`) dentro de `BeginTransaction`/`CommitTransaction`/`RollbackTransaction`; al ACTUALIZAR se conserva el folio existente. (FOL-004)
- [x] T24 — `Views/Ticket/Index.cshtml`: columna `Folio` en `<thead>` y en `InicializarDataTable()`. (FOL-006)
- [x] T25 — `Views/Ticket/_DetalleTicket.cshtml` + `VerTicket(id)`: mostrar folio en el detalle. (FOL-006)
