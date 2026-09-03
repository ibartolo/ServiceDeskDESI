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

## Pending Tasks
- [ ] User to verify: DataTables accents fixed + tables inside modals fully dark (last request delivered; awaiting user confirmation).
- [ ] IF user wants: darken SweetAlert popups (hardcoded `background:'white'` in Swal.fire calls) — prefer CSS override `.swal2-popup { background:#232334; color:#d5d7e3 }`.
- [ ] IF user wants: sidebar dark gradient blue-navy instead of gray.

## Notes
- User prefers NO spec process, NO context updates, ONLY direct code changes (told explicitly). Keep responses short in Spanish.
- .opencode/todo.md + work-log.md exist from earlier Reviewer work (ses_1/ses_2) — feature 1 closed.
