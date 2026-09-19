# Tasks: Vinculación Persona ↔ Usuario, aceptación autenticada de activos y "Mis Activos"

Orden: SQL → Entities → WebApi DAL → WebApi Services → WebApi Controllers → MVC (sync) → MVC (página anónima) → MVC (Mis Activos + desvincular) → Build → BD hosted → Verificación.
Nota: `ObtenerUsuarios` ya existe y se reutiliza tal cual para el modal (no se reescribe).

## Lote 1: SQL / migración

- [x] T1 — `openspec/changes/vinculacion-persona-usuario/migration.sql`: `ALTER TABLE Usuarios ADD PersonaId BIGINT NULL` + FK `FK_Usuarios_Persona`→`Persona(Id)` + índice único filtrado `UX_Usuarios_PersonaId` (`WHERE PersonaId IS NOT NULL`); todo idempotente con guardas `sys.columns`/`sys.foreign_keys`/`sys.indexes`. (D1.1, VPU-001)
- [x] T2 — `migration.sql`: reescribir `AsignarActivoPersona` (DROP/CREATE) insertando `IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE PersonaId=@PersonaId AND Estatus=1 AND EmpresaId=@EmpresaId) BEGIN SELECT -2; RETURN; END` **después** del check de Persona y **antes** del check de Activo. (D1.2, NAA-006)
- [x] T3 — `migration.sql`: nuevo `VincularPersonaUsuario(@PersonaId,@UsuarioId,@Usuario)` atómico: valida Persona+Usuario misma empresa `Estatus=1`; si ya vinculada a OTRO usuario → `-3`; si no `UPDATE Usuarios SET PersonaId=@PersonaId` + sobrescribe `Persona.Nombre/Apellido/Correo/Telefono` desde Usuario (`u.Celular→p.Telefono`), `PuestoId` intacto; retorna `1`/`0`/`-3`. (D1.3, VPU-004)
- [x] T4 — `migration.sql`: nuevo `DesvincularPersonaUsuario(@PersonaId,@Usuario)` → `UPDATE Usuarios SET PersonaId=NULL WHERE PersonaId=@PersonaId AND EmpresaId=@EmpresaId`; retorna `@@ROWCOUNT`. (D1.3)
- [x] T5 — `migration.sql`: nuevo `ObtenerPersonaIdPorUsuario(@Usuario)` → `SELECT PersonaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1`. (D1.3, MA-002)
- [x] T6 — `migration.sql`: nuevo `ObtenerAsignacionPorToken(@TokenConfirmacion)` → fila única `PersonaActivoId, PersonaId, FechaInicio, FechaConfirmacion, FechaFin, ActivoNombre, ActivoSerial, TipoActivoNombre, MarcaNombre, ModeloNombre, PersonaNombre, PersonaApellido, AsignadorNombre` (join Activo/TipoActivo/Marca/Modelo/Persona; `AsignadorNombre=u.Nombre+' '+u.Apellido` vía `LEFT JOIN Usuarios u ON pa.CreadoPor=u.NombreUsuario`); sin `@Usuario`. (D1.3, CRA-006)
- [x] T7 — `migration.sql`: reescribir `ConfirmarRecepcionActivo(@TokenConfirmacion,@Usuario)` autenticado; retornos `0` desconocido · `1` confirmado ahora · `2` ya confirmado · `3` no autorizado (valida `Usuarios.PersonaId = PersonaActivo.PersonaId`). (D1.4, CRA-001)
- [x] T8 — `migration.sql`: nuevo `DesvincularActivoPersonaConfirmacion(@TokenConfirmacion,@Usuario)`: valida `FechaFin IS NULL` + titularidad; `SET FechaFin=GETDATE(), ModificadoPor=@Usuario, FechaModificacion=GETDATE()`; retorna `0`/`1`/`3`. (D1.5, CRA-009)
- [x] T9 — `migration.sql`: enriquecer `ObtenerActivosPorPersona` (DROP/CREATE): añadir `ta.Nombre AS TipoActivoNombre, m.Nombre AS MarcaNombre, mo.Nombre AS ModeloNombre, pa.FechaConfirmacion, p.Nombre AS PersonaNombre, p.Apellido AS PersonaApellido, pa.CreadoPor AS AsignadoPor` (joins TipoActivo/Marca/Modelo/Persona); mantiene filtro `FechaFin IS NULL AND Estatus=1`. (D1.6, MA-003)
- [x] T10 — `migration.sql`: enriquecer `ObtenerPersonas` (DROP/CREATE): `LEFT JOIN Usuarios u ON u.PersonaId=p.Id AND u.Estatus=1` + `u.Id AS UsuarioId, u.NombreUsuario AS NombreUsuarioVinculado`. (D1.7, VPU-004)
- [x] T11 — `migration.sql`: `INSERT Pagina` "Mis Activos" (`Nombre='MisActivos', Tipo='Menu', Direccion='/Home/MisActivos'`) + `INSERT RolPaginaAccion` (PuedeLeer=1) para TODOS los roles `Nombre='Usuario'` (`r.Estatus=1`), ambos con guard `NOT EXISTS`. **FLAG: verificar estructura real de `Pagina`/`RolPaginaAccion` en BD hosted (columnas `Estatus`/`CreadoPor`/`FechaCreacion`; el dump no refleja `EmpresaId`/`NombreVisible`) ANTES de escribir el INSERT.** (D1.8, MA-001)
- [x] T12 — `openspec/changes/vinculacion-persona-usuario/rollback.sql` orden inverso: `DELETE RolPaginaAccion`/`Pagina` "Mis Activos" → `DROP PROCEDURE` nuevos → restaurar `ObtenerActivosPorPersona`/`ObtenerPersonas`/`AsignarActivoPersona`/`ConfirmarRecepcionActivo` → `DROP INDEX UX_Usuarios_PersonaId` → `DROP CONSTRAINT FK_Usuarios_Persona` → `DROP COLUMN Usuarios.PersonaId`. (D1.9)

## Lote 2: Entities

- [x] T13 — `ServiceDeskDESIEntities/Autenticacion/Usuario.cs`: añadir `public long? PersonaId { get; set; }` (hereda a `UsuarioDTO`; `ObtenerUsuarioPorId`/`ObtenerUsuarios`/`ObtenerUsuarioPorNombreUsuario` lo mapean por `u.*`). (D2, VPU-006)
- [x] T14 — `ServiceDeskDESIEntities/Catalogos/PersonaDTO.cs`: añadir `public long? UsuarioId { get; set; }` y `public string NombreUsuarioVinculado { get; set; }`. (D2, VPU-004)
- [x] T15 — `ServiceDeskDESIEntities/Catalogos/PersonaActivoDTO.cs`: añadir `TipoActivoNombre, MarcaNombre, ModeloNombre, DateTime? FechaConfirmacion, PersonaNombre, PersonaApellido, AsignadoPor`. (D2, MA-003)
- [x] T16 — Crear `ServiceDeskDESIEntities/Catalogos/AsignacionActivoDetalleDTO.cs`: `Id, PersonaId, FechaInicio, DateTime? FechaConfirmacion/FechaFin, ActivoNombre, ActivoSerial, TipoActivoNombre, MarcaNombre, ModeloNombre, PersonaNombre, PersonaApellido, AsignadorNombre`. (D2, CRA-006)
- [x] T17 — `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj`: `<Compile Include="Catalogos\AsignacionActivoDetalleDTO.cs" />`. (D2)

## Lote 3: WebApi DAL

- [x] T18 — `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs` `AsignarActivoPersona`: insertar rama **antes** de `<= -1`: `if (resultadoLong == -2) → IsSuccess=false, Message="La persona no tiene un usuario vinculado."`. (D3, NAA-006)
- [x] T19 — `DbWrapper.PersonaActivo.cs`: `ConfirmarRecepcionActivo(Guid token, string usuario)` → `ExecuteScalar`; `IsSuccess=(estado==1||estado==2)`; mapear `3`→"No autorizado…", `0`→"enlace inválido", `2`→"ya confirmado". (D3, CRA-001)
- [x] T20 — `DbWrapper.PersonaActivo.cs`: `DesvincularActivoPersonaConfirmacion(Guid,string)` + `ObtenerAsignacionPorToken(Guid)`→`GetObject`→`AsignacionActivoDetalleDTO` + `ObtenerPersonaIdPorUsuario(string)`→`ExecuteScalar`→`long?`. (D3)
- [x] T21 — `ServiceDeskDESIWebApi/DAL/DbWrapper.Persona.cs`: `VincularPersonaUsuario(long personaId, long usuarioId, string usuario)` (rama `-3`→"La persona ya está vinculada a otro usuario") y `DesvincularPersonaUsuario(long, string)`. (D3, VPU)

## Lote 4: WebApi Services

- [x] T22 — `ServiceDeskDESIWebApi/Services/PersonaService.cs`: `VincularPersonaUsuario`/`DesvincularPersonaUsuario` (validaciones + passthrough DAL). (D4, VPU-002)
- [x] T23 — `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` `AsignarActivoPersona`: 2 correos — (a) ADMIN (`ObtenerUsuarioPorNombreUsuario(usuario).Correo`) informativo SIN liga; (b) USUARIO vinculado (vía `Usuarios.PersonaId`→`ObtenerUsuarioPorId`) con liga `{BaseUri}Home/VerAsignacion/{token}`; conserva compensación `CompensarAsignacionFallida` (si falla cualquiera → desvincular + bitácora `Fallido` + `IsSuccess=false`) y 2 filas `BitacoraCorreo`. (D4, NAA-001)
- [x] T24 — `PersonaActivoService.cs` `ObtenerMisActivos(string usuario)`: `personaId=_dbWrapper.ObtenerPersonaIdPorUsuario(usuario)`; si `null` → `IsSuccess=true` + lista vacía; si no → `ObtenerActivosPorPersona(personaId, usuario)`. (D4, MA-002)
- [x] T25 — `PersonaActivoService.cs` `ConfirmarRecepcion(Guid token, string usuario)`: `ConfirmarRecepcionActivo(token,usuario)`; mapea `1`→"Recepción confirmada…", `2`→"ya fue confirmada…", `3`→`IsSuccess=false` "no corresponde a su usuario", `0`→"enlace inválido". (D4, CRA-001)
- [x] T26 — `PersonaActivoService.cs` `DesvincularConfirmacion(Guid,string)` + `IniciarDesvinculacion(long,string)` (resuelve asignación + `TokenConfirmacion` (genera si NULL) + correo usuario liga `{BaseUri}Home/VerAsignacion/{token}?accion=desvincular` + `BitacoraCorreo` `TipoCorreo="DesvinculacionActivo"`; NO setea `FechaFin`; fallo correo → `IsSuccess=false`) + `ObtenerAsignacionPorToken(Guid)` anónimo. (D4, CRA-009, NAA-007)
- [x] T27 — Crear `ServiceDeskDESIWebApi/Template/Template_DesvinculacionActivo.html` (variante mínima, botón "Desvincular activo", asunto "Desvinculación de activo") + `<Content Include>` en `ServiceDeskDESIWebApi.csproj`. (D10, NAA-007)
- [x] T28 — `ServiceDeskDESIWebApi/Services/EmpresaService.cs` `GuardarNuevaEmpresaConDatosIniciales`: tras Paso 5, insertar `RolPaginaAccion` de "Mis Activos" al rol `Nombre='Usuario'` recién creado (buscar `Pagina.Nombre='MisActivos'` vía `ObtenerPaginas` + `InsertarRolPaginaAccion`). (D12)

## Lote 5: WebApi Controllers

- [x] T29 — `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs`: `[HttpGet, Route("MisActivos")]` → `ObtenerMisActivos()` — **sin `[Permiso]`**. (D5, MA-002)
- [x] T30 — `PersonaActivoController.cs`: `[HttpGet, Route("AsignacionPorToken/{token:guid}")]` + `[AllowAnonymous]` → `ObtenerAsignacionPorToken(Guid)`. (D5, CRA-006)
- [x] T31 — `PersonaActivoController.cs`: rework `confirmarRecepcion` (AUTENTICADO; quitar `[AllowAnonymous]` y el `:guid`; body `{token}` → `User.Identity.Name`); nuevo `[HttpPost, Route("desvincularConfirmacion")]` (body `{token}`); nuevo `[HttpPost, Route("IniciarDesvinculacion")]` + `[Permiso("Personas","Editar")]`. (D5, CRA-001/009)
- [x] T32 — `ServiceDeskDESIWebApi/Controllers/PersonaController.cs`: `[HttpPost, Route("VincularUsuario")]` + `[HttpPost, Route("DesvincularUsuario")]`, ambos `[Permiso("Personas","Editar")]` → `PersonaService`. (D5, VPU-002)

## Lote 6: MVC — Persona.cshtml (sync)

- [x] T33 — `ServiceDeskDESIMVC/DAL/HttpClientConnection.Persona.cs`: `VincularPersonaUsuario(long,long)` + `DesvincularPersonaUsuario(long)` (POST `api/Persona/...`); `ServiceDeskDESIMVC/Services/PersonaService.cs` wrappers. (D6)
- [x] T34 — `ServiceDeskDESIMVC/Controllers/CatalogsController.cs` (región `#region persona`): acciones JSON `VincularPersonaUsuario(long personaId,long usuarioId)` y `DesvincularPersonaUsuario(long personaId)` `[Permiso("Personas","Editar")]` que proxean al WebApi vía `HttpClientConnection.Persona`. (D6)
- [x] T35 — `ServiceDeskDESIMVC/Views/Catalogs/Persona.cshtml` (+ partial): botón SVG con tooltip "Sincronizar con usuario" + modal con tabla de usuarios (`ObtenerUsuarios`: nombre usuario, nombre, apellido, correo) + botón "Sincronizar". (D6, VPU-002)
- [x] T36 — `Persona.cshtml`: advertencia de sobrescritura "Los datos se sobreescribirán…" en el modal y de nuevo (SweetAlert) antes de guardar. (D6, VPU-003)
- [x] T37 — `Persona.cshtml`: tras sincronizar, `NombreUsuario` bloqueado y `Nombre/Apellido/Correo/Telefono` deshabilitados con datos del Usuario; `PuestoId` editable; tabla `tblPersona` lee `UsuarioId`/`NombreUsuarioVinculado`; botón "vincular activo" solo si `UsuarioId != null`. (D6, VPU-004)

## Lote 7: MVC — página anónima + login modal

- [x] T38 — `ServiceDeskDESIMVC/DAL/HttpClientConnection.PersonaActivo.cs`: espejo `MisActivos`, `AsignacionPorToken`, `ConfirmarRecepcion(token)` (POST), `DesvincularConfirmacion`, `IniciarDesvinculacion`; `ServiceDeskDESIMVC/Services/PersonaActivoService.cs` wrappers. (D7/D8)
- [x] T39 — `ServiceDeskDESIMVC/Controllers/HomeController.cs`: `[HttpGet] VerAsignacion(string token, string accion=null)` (pública, valida GUID, `ViewBag.Detalle/Accion/Token`, vista standalone sin masterpage como `RecoverPassword`); `[HttpPost] AceptarAsignacion(string token)` y `[HttpPost] DesvincularAsignacion(string token)` (autenticadas → WebApi con bearer de sesión). (D7, CRA-006/009)
- [x] T40 — Crear `ServiceDeskDESIMVC/Views/Home/VerAsignacion.cshtml` (standalone sin masterpage): muestra asignador+activo, botón "Acepto la asignación"/"Desvincular" según `accion`, modal login (usuario/contraseña → POST `Home/LogIn` → FormsAuth+TokenCookie → dispara Aceptar/Desvincular); credenciales incorrectas → error sin cambio de estado (Status 1 intacto); ya aceptado → "este activo ya fue asignado" + redirect login. (D7, CRA-006/007/008)
- [x] T41 — `ServiceDeskDESIMVC/App_Start/FilterConfig.cs`: `PublicActions` añade `"Home.VerAsignacion"`; elimina/reemplaza `"Home.ConfirmarRecepcion"`. `ServiceDeskDESIMVC.csproj`: `<Content Include>` de `VerAsignacion.cshtml`. (D9)

## Lote 8: MVC — Mis Activos + desvincular (admin)

- [x] T42 — `ServiceDeskDESIMVC/Controllers/HomeController.cs`: `MisActivos()` (autenticada) → `View()`. (D7/D8, MA-001)
- [x] T43 — Crear `ServiceDeskDESIMVC/Views/Home/MisActivos.cshtml` (masterpage normal): DataTable `ActivoNombre, Serial, TipoActivoNombre, MarcaNombre, ModeloNombre, FechaInicio, FechaConfirmacion, AsignadoPor` + estado ("Por aceptar" si `FechaConfirmacion==null`, "Vigente" si no) + botón "Aceptar" (solo si por aceptar) que llama al endpoint autenticado sin re-login. Menú vía `RolPaginaAccion` (D1.8); `MenusUser.cshtml` sin cambios. (D8, MA-003/004)
- [x] T44 — vista de activos admin (`Active.cshtml`/assets): botón "Desvincular" ahora invoca `IniciarDesvinculacion` (envía correo al usuario), no el `DesvincularActivoPersona` inmediato. (D8, CRA-009)
- [x] T45 — `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj`: `<Content Include>` de `MisActivos.cshtml`. (D8)

## Lote 9: Build

- [x] T46 — Compilar `ServiceDeskDESI.sln` (MSBuild VS2022, Debug) → 0 errores. (D1..D12)

## Lote 10: BD hosted (manual)

- [x] T47 — **⚠️ Requiere confirmación del usuario antes de tocar BD hosted**: aplicar `migration.sql` a `db_9c7990_servicedeskdesi` con sqlcmd `-C` (credenciales `connectionStrings/cCon`, servidor `SQL5105.site4now.net`). Verificación post: `Usuarios.PersonaId` en `sys.columns`; `OBJECT_ID('VincularPersonaUsuario')` no nulo; `COUNT(*)` de `RolPaginaAccion` (roles `Nombre='Usuario'` + `Pagina='MisActivos'`) > 0. (D12) — **APLICADA y verificada (columna + FK + índice + 6 SPs + Pagina MisActivos + RPA rol 3 y 31).**

## Lote 11: Verificación / smoke

- [ ] T48 — Smoke sync: vincular Persona↔Usuario desde `Persona.cshtml` → campos deshabilitados con datos del Usuario; `PuestoId` intacto; asignar activo → 2 correos (admin + usuario). (VPU-004, NAA-001)
- [ ] T49 — Smoke aceptación: abrir liga correo → página anónima → modal login → Status 2 (`FechaConfirmacion` set) → redirect Mis Activos; re-clic → "ya asignado" + redirect login. (CRA-001/006/007)
- [ ] T50 — Smoke desvincular: admin inicia desvinculación → correo usuario → liga `?accion=desvincular` → login → `FechaFin` establecido. (CRA-009, NAA-007)
- [ ] T51 — Smoke Mis Activos: usuario básico ve el menú; usuario sin `PersonaId` ve lista vacía sin error; admin no ve el menú (sin `RolPaginaAccion`). (MA-001/002/003)
- [ ] T52 — Smoke permisos: `GET MisActivos` responde sin `[Permiso("Personas")]`; `confirmarRecepcion` autenticado deniega `3` si no es titular; `AsignarActivoPersona` devuelve `-2` para persona sin usuario. (MA-002, CRA-005, NAA-006)
