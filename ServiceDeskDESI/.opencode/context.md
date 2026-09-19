# Project Context

## Environment
- Solution: legacy **.NET Framework 4.8** MVC project (`ServiceDeskDESIMVC`) + WebApi + Entities (C# / langversion 7.3).
- Platform: Windows (win32), PowerShell 5.1.
- Build: VS 2022 MSBuild — `"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ServiceDeskDESIMVC.csproj /t:Build /p:Configuration=Debug`
- Test: **NO test project exists** in the solution → verification = MSBuild compilation (0 errores) + static symbol/integration checks.
- LSP diagnostics: NOT available in this environment (Rust tool process failure) → use MSBuild as definitive C# check.
- Git: not available on this machine; no VCS diff possible.
- **IMPORTANT**: `.csproj` uses EXPLICIT file list (`<Compile Include=...>`) — new .cs files MUST be registered or they won't compile into assembly (caused runtime error with ThemeHelper).

## Project Type
- Application (ASP.NET MVC 5 web app) + Web API backend + Entities class library.

## Structure
- Source: `ServiceDeskDESIMVC/` (Controllers, Services, DAL/HttpClientConnection, Helpers, Views)
- Entities: `ServiceDeskDESIEntities/` (Autenticacion/Usuario, Seguridad/TokenCookie, Seguridad/ModelResponse)
- WebApi: `ServiceDeskDESIWebApi/` (Services, DAL/DbWrapper)
- DataTables 2.3.7 via CDN in ~15 views (Users, WorkArea, TypeActive, Puesto, Persona, Ticket/Index, Model, Branch, Mark, Active, Company, Category, Role, Permisos, MisActivos, CategoriaResponsable, _DetalleTicket tblHistorial). i18n: `/Content/datatables/i18n/es-ES.json`.

## Key Patterns (OBSERVE from existing code)
- **Session (TokenCookie)**: stored in FormsAuthentication ticket userData. Read: `SessionHelper.GetSessionUser()` (SessionHelper.cs:43). Write: `SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie))` (SessionHelper.cs:60).
- **Profile image**: `TokenCookie.ProfileImage` (TokenCookie.cs:15); `Usuario.ImagenPerfil` (Usuario.cs:14); `UserAvatar` = initials HTML (HomeController.GenerarAvatarIniciales).
- **Navbar avatar**: `_Layout.cshtml:193-204` reads `SessionHelper.GetSessionUser().ProfileImage`; `<img>` if `File.Exists(Server.MapPath("~"+ProfileImage))` else `UserAvatar`.
- **Services**: controllers call `*Service` → `HttpClientConnection` (HTTP) — return `ModelResponse<T>` with `IsSuccess`, `Message`, `Response`.
- Error handling: try/catch with `ModelResponse` + JSON serialize; catch `ex` unused (CS0168 warnings pre-existing).
- **Sidebar menu**: loaded via AJAX `$("#sidebar").load("/Home/MenusUser")` (partial `Views/Home/MenusUser.cshtml`).
- **CSS**: `CSS/Comun/TemplatePage.css` — `:root` vars (`--primary: #4e73df` azul, light bg `#f0f2f5`). Dark theme = `body.dark-theme` overrides in same file. CSS verified balanced 153/153 braces.

## Current Status (2026-08-30)
### Feature 1: Navbar avatar muestra imagen de perfil (DONE, verified by user)
- `UserController.ActualizarPerfilUsuario` (~L104-117): refreshes session `TokenCookie.ProfileImage` + `CreateSession` after success.
- `UserController.MyProfile` GET: syncs session ProfileImage with fresh DB value.
- `_Layout.cshtml:196`: `Server.MapPath("~" + usuario.ProfileImage)`.
- User confirmed works after re-login.

### Feature 2: Tema claro/oscuro con cookie (DONE, user confirmed "se ve mejor")
- **NEW** `Helpers/ThemeHelper.cs` (REGISTERED in .csproj — was the runtime error fix): cookie `TemaUsuario_{UserID}`, 1-year expiry renewed each change. Methods: `GetCookieName`, `GetTema` (default 'light'), `GetTemaClase` ('dark-theme'|'').
- **NEW** `HomeController.GuardarTema(tema)` POST: validates light/dark, cookie `Expires = DateTime.Now.AddYears(1)`, returns `{IsSuccess, Tema}`.
- `Views/Home/Configuration.cshtml`: compact `stat-card` + radio buttons ☀️ Claro / 🌙 Oscuro, default Claro, JS `CambiarTema(tema)` → `$('body').toggleClass('dark-theme', ...)` immediate + `$.post('/Home/GuardarTema')`.
- `_Layout.cshtml:170`: `<body class="@(ThemeHelper.GetTemaClase(Request, SessionHelper.GetSessionUser()))">`; menu "Configuración" → `/Home/Configuration` (was href="#").
- **`TemplatePage.css` `body.dark-theme` covers**: bg `#171722`, cards `#232334`, inputs, dropdowns, alerts, text-muted, sidebar (gradient `#232334→#1a1a2e`), buttons (btn-success green glow, btn-warning amber glow, btn-secondary `#3a3a52`), modals (content, header/footer, title, labels, hr, btn-close inverted, backdrop), **DataTables 2.x** (`--dt-*` vars, `.dt-layout-row`, `.dt-length select`, `.dt-search input`, `table.dataTable` thead/tbody/stripe/hover/selected, `.dt-info`, `.dt-paging-button` + current/disabled), **Bootstrap table vars in `.table` scope** (`--bs-table-bg: transparent`, `--bs-table-color`, `--bs-table-striped-bg`, `--bs-table-hover-bg`, `--bs-table-border-color`), `.modal .table`/`table.dataTable` transparent, `--bs-body-bg/--bs-body-color/--bs-border-color`.

### Fix: acentos en es-ES.json (DONE)
- `Content/datatables/i18n/es-ES.json` was CORRUPTED (accents stored as `?`, ASCII). Rewrote full file UTF-8 with correct accents (Ningún, búsqueda, Último, Colección, Añadir condición, Vacío, ¿Está seguro, Próximo, Mié, Sáb, sangría, conservarán, información). JSON valid, UTF-8 confirmed. The 3 remaining `?` are legitimate question marks ("¿Está seguro...?").

## Other Feature Notes (unrelated to current mission — informational, not action items)
- User to verify: DataTables accents fixed + tables inside modals fully dark (last request delivered; awaiting user confirmation).
- IF user wants: darken SweetAlert popups (hardcoded `background:'white'` in Swal.fire calls) — prefer CSS override `.swal2-popup { background:#232334; color:#d5d7e3 }`.
- IF user wants: sidebar dark gradient blue-navy instead of gray.

## Notes
- User ahora trabaja con OpenSpec (`openspec/changes/`): proposal/design/tasks/specs + `migration.sql`. Mantener respuestas concisas en español. (La nota previa de "NO spec process" quedó obsoleta.)
- .opencode/todo.md + work-log.md exist from earlier Reviewer work (ses_1/ses_2) — feature 1 closed.

## Current Status (2026-09-11) — MISSION `tickets-estatus-espera` COMPLETE
Task IDs (background agents): Planner `task_6c134052`; Workers `task_ef41bc72` (Entities+WebApi), `task_ebea06ab` (MVC backend), `task_c119c62b` (Frontend), `task_2099dd55` (fix `using System;`); Reviewers `task_d3ed914e`, `task_6cae70c0`, `task_c0557b76`, `task_5dac0b37`.
OpenSpec artifacts: `openspec/changes/tickets-estatus-espera/` = proposal.md, design.md, tasks.md (22/22), specs/ticket-estatus-espera/spec.md, migration.sql, rollback.sql, verify-report.md (PASS).
Final build re-run by Commander: `MSBuild ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug` → 0 errores.
OpenSpec change: `openspec/changes/tickets-estatus-espera/` (estatus 6 "Pendiente de Materiales" y 7 "En Espera de Terceros").
- **TODO**: `.opencode/todo.md` = 17/17 (14 sub-tasks [x] + 3 milestones completed).
- **Build**: `MSBuild.exe ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug` → **exit 0, 0 errores**, 3 assemblies (`ServiceDeskDESIEntities.dll`, `ServiceDeskDESIMVC.dll`, `ServiceDeskDESIWebApi.dll`). Warnings only pre-existing CS0168 (UserController.cs:128, CatalogsController.cs:647) + CS1998 (Startup.cs:163,186).
- **Spec**: 11/11 PASS vs `spec.md`.
- **Sync issues**: `.opencode/sync-issues.md` = none (all resolved).
- **Files changed (11 manifest)**: Entities `TicketAsignacion.cs`/`TicketDTO.cs` (+`FechaEstimada`); WebApi `DAL/DbWrapper.Ticket.cs` (Pausar/Reanudar via SP `TransicionarTicket`), `Services/TicketService.cs` (validación), `Controllers/TicketController.cs` (routes `Pausar`/`Reanudar` + `PausarTicketRequest`); MVC `DAL/HttpClientConnection.Ticket.cs` (POST `api/Ticket/Pausar`/`Reanudar`), `Services/TicketService.cs`, `Controllers/TicketController.cs` (JSON actions); views `_PausarTicket.cshtml` (CREATE), `Index.cshtml`, `_DetalleTicket.cshtml`. Also `ServiceDeskDESIMVC.csproj` (+`<Content Include="Views\Ticket\_PausarTicket.cshtml" />` line 236).
- **Blockers resolved**: SYNC-1 `TicketDTO.cs` missing `using System;`; SYNC-2 MVC `Services/TicketService.cs` missing `using System;`; SYNC-3 `_PausarTicket.cshtml` not registered in WAP content list.
- **SQL**: `migration.sql` estatus 6/7, `TicketAsignacion.FechaEstimada`, SP transitions (2→6/7, 6/7→2; Resolver/Cerrar/Rechazar/Reasignar blocked from 6/7), dashboard `ActivosSemana IN (1,2,6,7)` / `Trabajando=2`.
- **Correcciones post-verificación (feedback del usuario, 2026-09-11)**: (a) TODAS las clases request inline se sacaron de los controllers WebApi → `ServiceDeskDESIWebApi/Models/` (Ticket: `TomarTicketRequest`, `ReasignarTicketRequest`, `TransicionTicketRequest`, `PausarTicketRequest`; PersonaActivo: `AsignarActivoRequest`, `DesvincularActivoRequest`, `ConfirmarRecepcionRequest`; Persona: `VincularUsuarioRequest`, `DesvincularUsuarioRequest`; Rol: `AsignarRolRequest`, `EliminarRolUsuarioRequest`) y se registraron en `ServiceDeskDESIWebApi.csproj` (`<Compile Include>` explícito). `RestablecerContraseniaRequest` NO se duplicó: ya existía idéntico en `ServiceDeskDESIEntities.Seguridad` y se reutiliza. (b) `_PausarTicket.cshtml` era el ÚNICO `.cshtml` sin BOM → acentos rotos en Razor; se re-guardó como UTF-8 **con BOM**. Build re-verificado: **0 errores**.
- **Regla de encoding**: los `.cshtml` del proyecto MUST guardarse como UTF-8 **con BOM** (Razor/legacy CodeDom lee sin BOM como ANSI y rompe acentos). Los `.cs` con BOM también; los `.sql` del repo van sin BOM (convención existente).
- **Estado final (2026-09-11)**: 11 archivos en `ServiceDeskDESIWebApi/Models/` (todos con BOM) + 12 includes en `ServiceDeskDESIWebApi.csproj` (11 `<Compile>` + `<Folder Include="Models\" />`); 0 clases request declaradas en controllers de la WebApi; `_PausarTicket.cshtml` con BOM; manual de ayuda `Views/Home/Ayuda.cshtml` actualizado (estatus 6/7 + sección "Pausar y reanudar" + roles + FAQ + glosario + data-tags); `MSBuild ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug` → **0 errores**. Misión + correcciones completas.

## Current Status (2026-09-11) — cambio `tickets-notificacion-estatus` COMPLETE
- OpenSpec change: `openspec/changes/tickets-notificacion-estatus/` (proposal, design, tasks, specs/ticket-notificacion-estatus/spec.md). **Sin cambios de BD.**
- Implementado en `ServiceDeskDESIWebApi/Services/TicketService.cs`: `using ServiceDeskDESIWebApi.Helpers;` + `NotificarCambioEstatus(ticketId, usuario, tipoMovimiento, comentario, fechaEstimada)` (best-effort, `try/catch` + log) + `ObtenerMensajeYNota`/`ObtenerPrioridadTexto`/`ObtenerPrioridadColor` + **8 llamadas** tras cada transición exitosa (`Tomar`, `Reasignar`, `Resolver`, `Rechazar`, `Cerrar`, `Retomar`, `Pausar`[tipoMovimiento], `Reanudar`).
- Reutiliza `ObtenerTicketPorId` + `ObtenerUsuarioPorNombreUsuario` (trae `Correo`) y el template existente `ServiceDeskDESIWebApi/Template/Template_CambioEstatusTicket.html` (placeholders rellenados). URL = `AppSettings["BaseUri"] + "Ticket/Index"`.
- Build: `MSBuild ServiceDeskDESI.sln /t:Rebuild /p:Configuration=Debug` → **0 errores**. BOM conservado.

## Pending Tasks
- [ ] EXTERNAL (out of code scope): user must execute `openspec/changes/tickets-estatus-espera/migration.sql` against the live DB (no DB access in this environment).
- [ ] Optional/informational (unrelated to mission): verify DataTables accents + dark tables in modals; optionally darken SweetAlert popups; optionally sidebar dark gradient.
