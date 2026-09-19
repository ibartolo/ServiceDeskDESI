# Design: Vinculación Persona ↔ Usuario, aceptación autenticada de activos y "Mis Activos"

- **Change**: `vinculacion-persona-usuario`
- **Fase**: design
- **Fecha**: 2026-08-26
- **Entradas**: `proposal.md`, `explore.md`, specs `VPU-001..006`, `NAA-006/007` (+ NAA-001 modificado), `MA-001..004`, `CRA-006..009` (+ CRA-001/002/005 modificados), y `asignacion-activos/migration.sql|design.md|proposal.md` (este change REESCRIBE partes).

## Contexto verificado (no re-implementar)

- `Usuarios` **no** tiene `PersonaId`; `Persona` **no** tiene `UsuarioId`. La relación es nueva. Precedente 1:1 nullable: `Area.UsuarioResponsableId` (`long?`).
- `ObtenerUsuarios` (SP) ya existe y retorna `u.*` + `SucursalNombre/AreaNombre/EmpresaNombre` filtrado por empresa → **reutilizable tal cual** para el modal de sync (nombre usuario, nombre, apellido, correo). `ObtenerUsuarioPorId`/`ObtenerUsuarioPorNombreUsuario` retornan `u.*` → expondrán `PersonaId` automáticamente al añadir la columna.
- `AsignarActivoPersona` (SP) hoy devuelve `SCOPE_IDENTITY()` / `-1` / `0`; `DbWrapper.AsignarActivoPersona` interpreta `<= -1` como "ya asignado" y `<= 0` como fallo. **Hay que insertar la rama `-2` antes de `-1`**.
- `PersonaActivoService.AsignarActivoPersona` persiste → token → correo (un solo destinatario) → bitácora → compensación (`DesvincularActivoPersona`). Se conserva todo salvo el cambio de destinatarios y la URL.
- Login: `HomeController.LogIn` (POST) → `AutenticarUsuario` → `GetToken` (`/token`) → `SessionHelper.CreateSession` (FormsAuth + `TokenCookie`). La página anónima replica este patrón.
- `[Permiso("Personas")]` deniega a rol "Usuario"; `MisActivos` NO lleva `[Permiso]` (solo `[Authorize]` heredado).
- `.csproj` legacy: toda entidad `.cs` nueva y vista `.cshtml`/template nuevo requieren `<Compile Include>`/`<Content Include>`.
- Roles "Usuario" por-empresa (IDs 3 global, 31 y otros). `InsertarRolPaginaAccion` existe (lo usa `EmpresaService` Paso 7/8). Provisioning de nuevas empresas: `GuardarNuevaEmpresaConDatosIniciales` (Paso 5 crea roles, Paso 7 asigna páginas al rol Administrador).

## Data Flow (resumen)

```
Asignación (Status 1):
  MVC CatalogsController.AsignarActivoPersona
    → WebApi PersonaActivoController.Asignar  [Permiso Personas/Editar]
      → SP AsignarActivoPersona  (valida -2 "persona sin usuario vinculado")
      → Persistir token → Email ADMIN (informativo) + Email USUARIO (liga VerAsignacion)
      → BitacoraCorreo ; si falla → DesvincularActivoPersona (compensación)

Aceptación (Status 2):
  Email → VerAsignacion/{token} (anónima, standalone)  → GET AsignacionPorToken (anónimo, muestra info)
    → "Acepto" → modal login → POST Home/LogIn (FormsAuth+TokenCookie)
      → POST AceptarAsignacion → WebApi confirmarRecepcion (AUTENTICADO, valida PersonaId) → FechaConfirmacion
      → redirect MisActivos

Desvincular:
  Admin Assets → "Desvincular" (confirm) → POST IniciarDesvinculacion → Email usuario (liga VerAsignacion?accion=desvincular)
    → usuario abre → VerAsignacion discierne "desvincular" → login → POST DesvincularAsignacion
      → WebApi desvincularConfirmacion (AUTENTICADO, valida PersonaId) → FechaFin

Sync Persona↔Usuario:
  Persona.cshtml → modal (ObtenerUsuarios) → Sincronizar → WebApi Persona/VincularUsuario → SP VincularPersonaUsuario
    → Usuarios.PersonaId = PersonaId + sobrescribe Nombre/Apellido/Correo/Telefono de Persona (PuestoId intacto)
    → campos bloqueados en la vista

Mis Activos:
  Sidebar (RolPaginaAccion) → Home/MisActivos → WebApi GET PersonaActivo/MisActivos (sin [Permiso])
    → SP ObtenerPersonaIdPorUsuario(@Usuario) → SP ObtenerActivosPorPersona (enriquecido)
```

---

## D1 — Migración DB (aditiva, idempotente) + rollback

**D1.1 — `Usuarios.PersonaId` + FK + índice único filtrado** (sin flag, deduce "usuario básico"):

```sql
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id=OBJECT_ID(N'dbo.Usuarios') AND name=N'PersonaId')
    ALTER TABLE dbo.Usuarios ADD PersonaId BIGINT NULL;
GO
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name=N'FK_Usuarios_Persona')
    ALTER TABLE dbo.Usuarios ADD CONSTRAINT FK_Usuarios_Persona FOREIGN KEY (PersonaId) REFERENCES dbo.Persona(Id);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name=N'UX_Usuarios_PersonaId' AND object_id=OBJECT_ID(N'dbo.Usuarios'))
    CREATE UNIQUE INDEX UX_Usuarios_PersonaId ON dbo.Usuarios(PersonaId) WHERE PersonaId IS NOT NULL;
GO
```

**D1.2 — Reescribir `AsignarActivoPersona`** (DROP/CREATE) añadiendo la rama `-2` inmediatamente después de la validación de `Persona` (antes del check de `Activo`):

```sql
IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE PersonaId = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
    BEGIN SELECT -2; RETURN; END  -- -2 = persona sin usuario vinculado
```

**D1.3 — SPs nuevos** (todos DROP/CREATE, guard idempotente):

- `VincularPersonaUsuario(@PersonaId BIGINT, @UsuarioId BIGINT, @Usuario NVARCHAR(25))` — atómico: valida Persona+Usuario (misma empresa, `Estatus=1`); si la persona ya está vinculada a OTRO usuario → `-3`; si no, `UPDATE Usuarios SET PersonaId=@PersonaId WHERE Id=@UsuarioId` y **sobrescribe** `Persona.Nombre/Apellido/Correo/Telefono` desde el Usuario (`u.Celular→p.Telefono`), dejando `PuestoId` intacto; retorna `1` (ok) / `0` (fallo) / `-3` (ya vinculada a otro).
- `DesvincularPersonaUsuario(@PersonaId BIGINT, @Usuario NVARCHAR(25))` — `UPDATE Usuarios SET PersonaId=NULL WHERE PersonaId=@PersonaId AND EmpresaId=@EmpresaId`; retorna `@@ROWCOUNT`.
- `ObtenerPersonaIdPorUsuario(@Usuario NVARCHAR(25))` — `SELECT PersonaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1` (scalar; `NULL` si no hay vínculo).
- `ObtenerAsignacionPorToken(@TokenConfirmacion UNIQUEIDENTIFIER)` — fila única para renderizar la página anónima: `PersonaActivoId, PersonaId, FechaInicio, FechaConfirmacion, FechaFin, ActivoNombre, ActivoSerial, TipoActivoNombre, MarcaNombre, ModeloNombre, PersonaNombre, PersonaApellido, AsignadorNombre` (join `Activo/TipoActivo/Marca/Modelo/Persona`; `AsignadorNombre = u.Nombre+' '+u.Apellido` vía `LEFT JOIN Usuarios u ON pa.CreadoPor=u.NombreUsuario`). Sin `@Usuario` (anónimo).

**D1.4 — Reescribir `ConfirmarRecepcionActivo`** (de anónimo a **autenticado con validación de titularidad**):

```sql
CREATE PROCEDURE dbo.ConfirmarRecepcionActivo (@TokenConfirmacion UNIQUEIDENTIFIER, @Usuario NVARCHAR(25))
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @PersonaId BIGINT, @Id BIGINT, @PersonaIdUsuario BIGINT;
  SELECT @Id=Id, @PersonaId=PersonaId FROM PersonaActivo WHERE TokenConfirmacion=@TokenConfirmacion;
  IF @Id IS NULL BEGIN SELECT 0; RETURN; END                              -- token desconocido
  IF EXISTS(SELECT 1 FROM PersonaActivo WHERE Id=@Id AND FechaConfirmacion IS NOT NULL)
      BEGIN SELECT 2; RETURN; END                                         -- ya confirmado (idempotente)
  SELECT @PersonaIdUsuario=PersonaId FROM Usuarios WHERE NombreUsuario=@Usuario AND Estatus=1;
  IF @PersonaIdUsuario IS NULL OR @PersonaIdUsuario <> @PersonaId
      BEGIN SELECT 3; RETURN; END                                         -- 3 = no autorizado (no es la persona)
  UPDATE PersonaActivo SET FechaConfirmacion=GETDATE() WHERE Id=@Id AND FechaConfirmacion IS NULL;
  SELECT 1;
END
```

- **Retornos**: `0` desconocido · `1` confirmado ahora · `2` ya confirmado · `3` no autorizado.

**D1.5 — `DesvincularActivoPersonaConfirmacion(@TokenConfirmacion, @Usuario)`** (nuevo, autenticado): resuelve la asignación por token, valida `FechaFin IS NULL` y titularidad (`Usuarios.PersonaId = PersonaActivo.PersonaId`), `SET FechaFin=GETDATE(), ModificadoPor=@Usuario, FechaModificacion=GETDATE()`. Retornos: `0` desconocido/ya desvinculado · `1` ok · `3` no autorizado. (El SP legado `DesvincularActivoPersona` permanece como primitivo para la compensación.)

**D1.6 — Enriquecer `ObtenerActivosPorPersona`** (DROP/CREATE, aditivo): añadir al SELECT `ta.Nombre AS TipoActivoNombre`, `m.Nombre AS MarcaNombre`, `mo.Nombre AS ModeloNombre`, `pa.FechaConfirmacion`, `p.Nombre AS PersonaNombre`, `p.Apellido AS PersonaApellido`, `pa.CreadoPor AS AsignadoPor` (joins a `TipoActivo/Marca/Modelo/Persona`). Mantiene el filtro `FechaFin IS NULL AND Estatus=1` (cubre "por aceptar" y "vigente"). Reutilizado por `ActivosPorPersona` (admin) y `MisActivos`.

**D1.7 — Enriquecer `ObtenerPersonas`** (DROP/CREATE): `LEFT JOIN Usuarios u ON u.PersonaId = p.Id AND u.Estatus=1` añadiendo `u.Id AS UsuarioId, u.NombreUsuario AS NombreUsuarioVinculado` (para saber si la persona ya está sincronizada y bloquear campos).

**D1.8 — `Pagina` "Mis Activos" + `RolPaginaAccion`** para TODOS los roles existentes `Nombre='Usuario'` (por-empresa):

```sql
INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
SELECT 'MisActivos', 'Mis Activos', 'Activos asignados al usuario', 'Menu', '/Home/MisActivos', NULL, 'fas fa-laptop', 99, 1
WHERE NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre='MisActivos');
GO
INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion)
SELECT r.Id, p.Id, 1,0,0,0,0, 'migracion', GETDATE()
FROM Rol r CROSS JOIN Pagina p
WHERE p.Nombre='MisActivos' AND r.Nombre='Usuario' AND r.Estatus=1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId=r.Id AND rpa.PaginaId=p.Id);
GO
```

> Nota: `Pagina`/`RolPaginaAccion` deben tener columnas `Estatus`/`CreadoPor`/`FechaCreacion`; ajustar INSERT a la estructura real verificada en BD hosted (el dump no refleja `EmpresaId` ni `NombreVisible`).

**D1.9 — `rollback.sql`** (orden inverso): `DELETE` `RolPaginaAccion`/`Pagina` "Mis Activos" → `DROP PROCEDURE` de los nuevos → restaurar `ObtenerActivosPorPersona`/`ObtenerPersonas`/`AsignarActivoPersona`/`ConfirmarRecepcionActivo` (definiciones previas) → `DROP INDEX UX_Usuarios_PersonaId` → `DROP CONSTRAINT FK_Usuarios_Persona` → `DROP COLUMN Usuarios.PersonaId`.

## D2 — Entidades (`ServiceDeskDESIEntities`)

- `Autenticacion/Usuario.cs`: añadir `public long? PersonaId { get; set; }`. `UsuarioDTO` hereda → lo gana (y `ObtenerUsuarioPorId`/`ObtenerUsuarios`/`ObtenerUsuarioPorNombreUsuario` lo mapean por `u.*`).
- `Catalogos/PersonaDTO.cs`: añadir `public long? UsuarioId { get; set; }` y `public string NombreUsuarioVinculado { get; set; }` (para lock/bloqueo en `Persona.cshtml`).
- `Catalogos/PersonaActivoDTO.cs`: añadir `TipoActivoNombre`, `MarcaNombre`, `ModeloNombre`, `DateTime? FechaConfirmacion`, `PersonaNombre`, `PersonaApellido`, `AsignadoPor`.
- **Nuevo** `Catalogos/AsignacionActivoDetalleDTO.cs` (para la página anónima): `long Id, PersonaId`, `DateTime FechaInicio`, `DateTime? FechaConfirmacion, FechaFin`, `string ActivoNombre, ActivoSerial, TipoActivoNombre, MarcaNombre, ModeloNombre, PersonaNombre, PersonaApellido, AsignadorNombre`.
- `ServiceDeskDESIEntities.csproj`: `<Compile Include="Catalogos\AsignacionActivoDetalleDTO.cs" />`.

## D3 — WebApi DAL (`DbWrapper.*`)

- `DbWrapper.PersonaActivo.cs`:
  - `AsignarActivoPersona`: insertar rama **antes** de `<= -1`: `if (resultadoLong == -2) → IsSuccess=false, Message="La persona no tiene un usuario vinculado."`.
  - `ConfirmarRecepcionActivo(Guid token, string usuario)` → `ExecuteScalar`; `Response=(long)estado`, `IsSuccess=(estado==1 || estado==2)`; mapear `3` → mensaje "No autorizado…".
  - `DesvincularActivoPersonaConfirmacion(Guid token, string usuario)` → `ExecuteScalar`.
  - `ObtenerAsignacionPorToken(Guid token)` → `GetObject` → `AsignacionActivoDetalleDTO`.
  - `ObtenerPersonaIdPorUsuario(string usuario)` → `ExecuteScalar` → `long?`.
- `DbWrapper.Persona.cs`: `VincularPersonaUsuario(long personaId, long usuarioId, string usuario)` (rama `-3` → "La persona ya está vinculada a otro usuario") y `DesvincularPersonaUsuario(long personaId, string usuario)`.

## D4 — WebApi Services

- `PersonaService`: `VincularPersonaUsuario`/`DesvincularPersonaUsuario` (validaciones + passthrough DAL).
- `PersonaActivoService`:
  - `AsignarActivoPersona`: sin cambios de control (el `-2` llega como `IsSuccess=false` desde DAL y se retorna tal cual). Cambios: **2 correos** — (a) ADMIN (`ObtenerUsuarioPorNombreUsuario(usuario).Correo`) informativo **sin liga**; (b) USUARIO asociado (resolver vía `Usuarios.PersonaId` → `ObtenerUsuarioPorId`/correo) **con liga** `{BaseUri}Home/VerAsignacion/{token}`. La compensación (`CompensarAsignacionFallida`) se conserva: si falla CUALQUIERA de los 2 envíos → desvincular + bitácora `Fallido` + `IsSuccess=false`. Registra 2 filas `BitacoraCorreo` (una por correo).
  - `ObtenerMisActivos(string usuario)`: `personaId = _dbWrapper.ObtenerPersonaIdPorUsuario(usuario)`; si `null` → `ModelResponse<List<PersonaActivoDTO>> { IsSuccess=true, Response=new List<>() }` (lista vacía, sin error de permiso); si no → `ObtenerActivosPorPersona(personaId, usuario)`.
  - `ConfirmarRecepcion(Guid token, string usuario)` (autenticado): llama `_dbWrapper.ConfirmarRecepcionActivo(token, usuario)`; mapea `1`→"Recepción confirmada…", `2`→"ya fue confirmada…", `3`→`IsSuccess=false` "La asignación no corresponde a su usuario.", `0`→"enlace inválido".
  - `DesvincularConfirmacion(Guid token, string usuario)`: `_dbWrapper.DesvincularActivoPersonaConfirmacion(...)`.
  - `IniciarDesvinculacion(long personaActivoId, string usuario)`: resuelve asignación (nuevo read por id o reutiliza `ObtenerAsignacionPorToken` vía token existente) + `TokenConfirmacion` (si `NULL` → `GenerarTokenConfirmacion`) + correo del usuario vinculado; envía correo de desvinculación (liga `{BaseUri}Home/VerAsignacion/{token}?accion=desvincular`) + `BitacoraCorreo` (`TipoCorreo="DesvinculacionActivo"`). NO setea `FechaFin`. Si falla el correo → `IsSuccess=false` (sin desvincular ni compensar asignación).
  - `ObtenerAsignacionPorToken(Guid token)` (anónimo, para render de la página).

## D5 — WebApi Controllers

`PersonaActivoController` (ya `[Authorize]`, `RoutePrefix api/PersonaActivo`):

- `[HttpGet, Route("MisActivos")]` → `ObtenerMisActivos()` — **sin `[Permiso]`**.
- `[HttpGet, Route("AsignacionPorToken/{token:guid}")]` + `[AllowAnonymous]` → `ObtenerAsignacionPorToken(Guid token)` (render anónimo).
- `[HttpPost, Route("confirmarRecepcion")]` (AUTENTICADO; quitar `[AllowAnonymous]` y el `:guid` del path; body `{ token }`) → `ConfirmarRecepcion(token, User.Identity.Name)`.
- `[HttpPost, Route("desvincularConfirmacion")]` (AUTENTICADO; body `{ token }`) → `DesvincularConfirmacion(token, User.Identity.Name)`.
- `[HttpPost, Route("IniciarDesvinculacion")]` + `[Permiso("Personas","Editar")]` → `IniciarDesvinculacion(request.PersonaActivoId, User.Identity.Name)`.

`PersonaController` (`api/Persona`): `[HttpPost, Route("VincularUsuario")] [Permiso("Personas","Editar")]` y `[HttpPost, Route("DesvincularUsuario")] [Permiso("Personas","Editar")]` → `PersonaService`.

## D6 — MVC: `Persona.cshtml` (sync) + `CatalogsController`

- `CatalogsController` (región `#region persona`): acciones JSON `VincularPersonaUsuario(long personaId, long usuarioId)` y `DesvincularPersonaUsuario(long personaId)` (`[Permiso("Personas","Editar")]`) que proxean al WebApi vía `HttpClientConnection.Persona`.
- `Persona.cshtml`:
  - Botón SVG con tooltip "Sincronizar con usuario" (junto al botón Guardar o en la tabla de personas).
  - Modal con tabla de usuarios (`ObtenerUsuarios`: nombre usuario, nombre, apellido, correo) + botón "Sincronizar".
  - Al sincronizar → advertencia "Los datos se sobreescribirán…" en el modal; al guardar → advertencia de nuevo (SweetAlert) antes de persistir.
  - Tras sincronizar: `NombreUsuario` (username vinculado) **bloqueado** y `Nombre/Apellido/Correo/Telefono` **deshabilitados** con los datos del Usuario; `PuestoId` editable. La tabla `tblPersona` lee `UsuarioId`/`NombreUsuarioVinculado` del `PersonaDTO` para mostrar el estado sincronizado y el botón "vincular activo" solo si `UsuarioId != null`.

## D7 — MVC: página anónima + login modal (una página para aceptar/desvincular)

- `HomeController`:
  - `[HttpGet] VerAsignacion(string token, string accion = null)` (pública): valida GUID; consulta `ObtenerAsignacionPorToken(token)`; `ViewBag.Detalle`, `ViewBag.Accion` (`aceptar|desvincular`), `ViewBag.Token`. Vista standalone (sin masterpage, como `RecoverPassword`).
  - `[HttpPost] AceptarAsignacion(string token)` (autenticada): llama al WebApi `confirmarRecepcion` (con bearer de la sesión) → redirige `MisActivos` o devuelve mensaje.
  - `[HttpPost] DesvincularAsignacion(string token)` (autenticada): llama a `desvincularConfirmacion` → mensaje + redirect login/`MisActivos`.
  - `MisActivos()` (autenticada): `View()` (los datos se piden vía AJAX al endpoint `MisActivos`).
- Login modal: formulario usuario/contraseña que llama al `HomeController.LogIn` existente (POST, crea sesión FormsAuth+TokenCookie). Al autenticar correctamente, el JS dispara `AceptarAsignacion`/`DesvincularAsignacion`. Credenciales incorrectas → error, sin cambio de estado (Status 1 intacto).
- Idempotencia: si ya aceptado, el render muestra "este activo ya fue asignado" y redirige a login.

## D8 — MVC: vista `MisActivos` + menú

- `Views/Home/MisActivos.cshtml` (masterpage normal): DataTable con columnas `ActivoNombre, Serial, TipoActivoNombre, MarcaNombre, ModeloNombre, FechaInicio, FechaConfirmacion, AsignadoPor` + estado ("Por aceptar" si `FechaConfirmacion==null`, "Vigente" si no) + botón "Aceptar" (solo si por aceptar) que llama al endpoint autenticado de aceptación sin re-login (MA-004).
- Menú: visible vía `RolPaginaAccion` (fila insertada en D1.8); `MenusUser.cshtml` lo renderiza sin cambios (menu directo `Direccion=/Home/MisActivos`).
- Botón "Desvincular" en la vista de activos del lado admin (`Active.cshtml`/assets) ahora invoca `IniciarDesvinculacion` (envía correo al usuario), no el `DesvincularActivoPersona` inmediato.

## D9 — MVC: `FilterConfig`

- `PublicActions` añade `"Home.VerAsignacion"`. `MisActivos` **requiere** sesión (no se añade). `Home.ConfirmarRecepcion` existente se elimina/reemplaza por `VerAsignacion` (los enlaces de correo cambian a `VerAsignacion`).

## D10 — Flujo de correo

- **Asignación**: 2 correos — admin informativo (sin liga) + usuario con liga `{BaseUri}Home/VerAsignacion/{token}`. Template `Template_AsignacionActivo.html` (placeholders ya existentes). Se reutiliza el envío try/catch + `BitacoraCorreo` + compensación (dos filas de bitácora).
- **Desvinculación**: 1 correo al usuario con liga `{BaseUri}Home/VerAsignacion/{token}?accion=desvincular`. **Decisión (punto abierto #1 resuelto)**: variante mínima derivada `Template_DesvinculacionActivo.html` (copia del de asignación, botón "Desvincular activo" y asunto "Desvinculación de activo"). Registrar en `ServiceDeskDESIWebApi.csproj` `<Content Include>`.
- `BaseUri` del `Web.config` (WebApi), mismo valor que reset de contraseña.

## D11 — Mapeo de estados

| Estado | Condición |
|---|---|
| Status 1 "Asociado/Pendiente" | `FechaFin IS NULL` AND `FechaConfirmacion IS NULL` |
| Status 2 "Aceptado" | `FechaConfirmacion IS NOT NULL` (y `FechaFin IS NULL`) |
| Desvinculado | `FechaFin IS NOT NULL` |

## D12 — Aplicación de migración + provisioning

- Aplicar `migration.sql` a `db_9c7990_servicedeskdesi` **antes** de desplegar, con sqlcmd `-C` (credenciales de `connectionStrings/cCon`, servidor `SQL5105.site4now.net`), igual que `asignacion-activos`.
- Verificación post-migración: `SELECT name FROM sys.columns WHERE object_id=OBJECT_ID('Usuarios')` incluye `PersonaId`; `OBJECT_ID('VincularPersonaUsuario')` no nulo; `SELECT COUNT(*) FROM RolPaginaAccion rpa JOIN Rol r ON ... WHERE r.Nombre='Usuario' AND PaginaId=(SELECT Id FROM Pagina WHERE Nombre='MisActivos')` > 0.
- **Provisioning empresas futuras (punto abierto #3 resuelto)**: en `EmpresaService.GuardarNuevaEmpresaConDatosIniciales`, tras el Paso 5 (creación de roles base) y antes/después del Paso 7, añadir un paso que, para el `rol` con `Nombre='Usuario'` recién creado, inserte `RolPaginaAccion` de la página "Mis Activos" (buscar `Pagina.Nombre='MisActivos'` vía `ObtenerPaginas` y reutilizar `InsertarRolPaginaAccion(rolUsuarioId, paginaMisActivos.Id, true,false,false,false,false, usernameAdmin, usernameAdmin)`). El rol Administrador ya recibe todas las páginas en el Paso 7.
- **Rollback**: `sqlcmd -i rollback.sql` (D1.9) + revertir código → vuelve al flujo anónimo `ConfirmarRecepcion` (estado `asignacion-activos` sin archivar).

## File Changes

| File | Acción | Descripción |
|---|---|---|
| `openspec/changes/vinculacion-persona-usuario/migration.sql` | Crear | D1.1–D1.8 (columnas/FK/índice, SPs, Pagina+RolPaginaAccion) |
| `openspec/changes/vinculacion-persona-usuario/rollback.sql` | Crear | D1.9 |
| `ServiceDeskDESIEntities/Autenticacion/Usuario.cs` | Mod | `PersonaId long?` |
| `ServiceDeskDESIEntities/Catalogos/PersonaDTO.cs` | Mod | `UsuarioId`, `NombreUsuarioVinculado` |
| `ServiceDeskDESIEntities/Catalogos/PersonaActivoDTO.cs` | Mod | columnas enriquecidas |
| `ServiceDeskDESIEntities/Catalogos/AsignacionActivoDetalleDTO.cs` | Crear | DTO página anónima |
| `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` | Mod | `<Compile Include>` del DTO nuevo |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs` | Mod | `-2`, `ConfirmarRecepcionActivo(token,usuario)`, `DesvincularActivoPersonaConfirmacion`, `ObtenerAsignacionPorToken`, `ObtenerPersonaIdPorUsuario` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Persona.cs` | Mod | `VincularPersonaUsuario`, `DesvincularPersonaUsuario` |
| `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` | Mod | correo dual, `ObtenerMisActivos`, `ConfirmarRecepcion` autenticado, `DesvincularConfirmacion`, `IniciarDesvinculacion`, `ObtenerAsignacionPorToken` |
| `ServiceDeskDESIWebApi/Services/PersonaService.cs` | Mod | `VincularPersonaUsuario`, `DesvincularPersonaUsuario` |
| `ServiceDeskDESIWebApi/Services/EmpresaService.cs` | Mod | provisioning "Mis Activos" al rol "Usuario" (D12) |
| `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs` | Mod | `MisActivos`, `AsignacionPorToken`, rework `confirmarRecepcion`, `desvincularConfirmacion`, `IniciarDesvinculacion` |
| `ServiceDeskDESIWebApi/Controllers/PersonaController.cs` | Mod | `VincularUsuario`, `DesvincularUsuario` |
| `ServiceDeskDESIWebApi/Template/Template_DesvinculacionActivo.html` | Crear | variante mínima desvinculación |
| `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` | Mod | `<Content Include>` template nuevo |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.PersonaActivo.cs` | Mod | espejo `MisActivos`, `AsignacionPorToken`, `ConfirmarRecepcion(token)` (POST), `DesvincularConfirmacion`, `IniciarDesvinculacion` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Persona.cs` | Mod | `VincularPersonaUsuario`, `DesvincularPersonaUsuario` |
| `ServiceDeskDESIMVC/Services/PersonaActivoService.cs` + `PersonaService.cs` | Mod | wrappers |
| `ServiceDeskDESIMVC/Controllers/CatalogsController.cs` | Mod | sync Persona↔Usuario; botón "vincular activo" solo si tiene usuario |
| `ServiceDeskDESIMVC/Views/Catalogs/Persona.cshtml` (+ partial) | Mod | botón SVG + modal + warning + campos bloqueados |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` | Mod | `VerAsignacion`, `AceptarAsignacion`, `DesvincularAsignacion`, `MisActivos` |
| `ServiceDeskDESIMVC/Views/Home/VerAsignacion.cshtml` | Crear | página anónima standalone |
| `ServiceDeskDESIMVC/Views/Home/MisActivos.cshtml` | Crear | vista Mis Activos |
| `ServiceDeskDESIMVC/App_Start/FilterConfig.cs` | Mod | `PublicActions` + `VerAsignacion`; quitar `ConfirmarRecepcion` |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Mod | `<Content Include>` de las 2 vistas nuevas |

## Testing Strategy

| Capa | Qué probar | Enfoque |
|---|---|---|
| DB | `-2` en `AsignarActivoPersona`; `VincularPersonaUsuario` sobrescribe (PuestoId intacto) y rechaza doble vínculo (`-3`); índice único filtrado; `ConfirmarRecepcionActivo` codes 0/1/2/3; `RolPaginaAccion` para todos los roles "Usuario" | Ejecutar `migration.sql` en BD local/hosted; asserts por script |
| WebApi (unit/integration) | `ObtenerMisActivos` vacío sin PersonaId; `ConfirmarRecepcion` autenticado deniega `3` si no titular; `AsignarActivoPersona` mensaje `-2`; correo dual + compensación | Invocar servicios con DbWrapper mock/real |
| MVC (E2E manual) | Página anónima render, login modal → Status 2 → redirect Mis Activos; re-clic idempotente; sync Persona deshabilita campos; menú Mis Activos visible solo rol "Usuario" | Navegación manual + verificar `FechaConfirmacion`/`FechaFin` en BD |

## Assumptions

1. **Desvincular iniciado por admin NO setea `FechaFin`** — queda "Aceptado" hasta que el usuario confirme (CRA-009). El primitivo `DesvincularActivoPersona` se conserva para compensación.
2. **Token de desvinculación = `TokenConfirmacion`** existente (se genera en asignación); si `NULL` en filas legacy, `IniciarDesvinculacion` lo genera vía `GenerarTokenConfirmacion`.
3. **Punto abierto #2 (rol "Usuario")**: la sincronización NO asigna rol; el usuario pre-existe con su rol (creado en catálogo Usuarios). `VincularPersonaUsuario` solo setea `Usuarios.PersonaId` + copia datos, sin tocar `UsuarioRol`.
4. **Punto abierto #4 (contraseña)**: definida al crear el usuario en catálogo Usuarios; este change no la gestiona.
5. **Sin cambios en token/claims** (VPU-006): `PersonaId` se resuelve por `Usuarios.PersonaId` vía `ObtenerPersonaIdPorUsuario`, no por claim.
6. Estructura real de `Pagina`/`RolPaginaAccion` (`Estatus`, auditoría) a verificar contra BD hosted (el dump está desactualizado en `EmpresaId`/`NombreVisible`).
7. `BaseUri` debe apuntar a la URL pública del MVC (misma configuración que reset de contraseña).
