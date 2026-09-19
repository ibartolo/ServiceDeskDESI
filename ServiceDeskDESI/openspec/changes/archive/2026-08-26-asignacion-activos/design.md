# Design: Asignación de Activos — notificación, confirmación de recepción y bitácora

- **Change**: `asignacion-activos`
- **Fase**: design
- **Fecha**: 2026-08-26
- **Entradas**: `proposal.md`, `explore.md`, specs `NAA-001..005` y `CRA-001..005` (todas autoritativas; decisiones resueltas del usuario respetadas).

## Contexto verificado (no re-implementar)

- El núcleo **asignar/desvincular** ya está desplegado: `PersonaActivo` existe, 5 SPs viven en producción, UI completa (`_AsignarActivoPersona.cshtml`, `CatalogsController`).
- `AsignarActivoPersona` (SP) devuelve `SCOPE_IDENTITY()` (éxito), `-1` (ya asignado) o `0` (fallo). `DesvincularActivoPersona` devuelve `@PersonaActivoId` o `0`.
- `DbWrapper.AsignarActivoPersona` ya expone `ModelResponse` con `Response = (long)newId` en éxito.
- `EmailHelper.EnvioEmaiil(para, asunto, mensaje, ssl=false, attachment="")` re-lanza excepción al fallar; `EnableSsl` hardcodeado en `true`.
- Patrón de correo a replicar (`AutenticacionService.ValidarRecetearContrasenia` / `EmpresaService.EnviarCorreoBienvenida`): `MapPath("~/Template/...")` → `File.ReadAllText` → `Replace("{{x}}", ...)` → `EnvioEmaiil(...)`.
- Precedente anónimo: `AutenticationController` usa `[AllowAnonymous]` sobre `[Authorize]` a nivel de controlador; `HomeController` + `FilterConfig.PublicActions` manejan las páginas públicas MVC. `BaseUri` (Web.config) ya se usa para armar links `{BaseUri}Home/...`.
- `LlenarEntidad<T>` mapea columnas por nombre case-insensitive; los `.csproj` son legacy (sin SDK), las entidades nuevas requieren `<Compile Include>` manual.

---

## D1 — Esquema de BD (migración aditiva, idempotente)

### D1.1 — Columnas nuevas en `PersonaActivo`

```sql
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'FechaConfirmacion')
    ALTER TABLE dbo.PersonaActivo ADD FechaConfirmacion DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'TokenConfirmacion')
    ALTER TABLE dbo.PersonaActivo ADD TokenConfirmacion UNIQUEIDENTIFIER NULL;
GO
```

- **Tipos**: `DATETIME NULL` y `UNIQUEIDENTIFIER NULL`. Aditivas y `NULL` (no rompen la migración ya aplicada ni los `INSERT`/`SELECT *` existentes).
- **Índice** (para el lookup por token en confirmación):

```sql
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_PersonaActivo_TokenConfirmacion'
                 AND object_id = OBJECT_ID(N'dbo.PersonaActivo'))
    CREATE INDEX IX_PersonaActivo_TokenConfirmacion ON dbo.PersonaActivo (TokenConfirmacion);
GO
```

> **NO es UNIQUE**: la mayoría de filas tienen `TokenConfirmacion = NULL`, y un índice único rechazaría múltiples `NULL`. El token es GUID (`UNIQUEIDENTIFIER`) generado en C#, por lo que la unicidad práctica está garantizada por diseño; se documenta, no se fuerza.

### D1.2 — Tabla `BitacoraCorreo`

```sql
IF OBJECT_ID(N'dbo.BitacoraCorreo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BitacoraCorreo (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TipoCorreo    NVARCHAR(50)  NOT NULL,
        Destinatario  NVARCHAR(250) NOT NULL,
        Asunto        NVARCHAR(250) NOT NULL,
        Estado        NVARCHAR(20)  NOT NULL,          -- 'Enviado' | 'Fallido'
        Error         NVARCHAR(MAX) NULL,
        FechaEnvio    DATETIME      NOT NULL,
        ReferenciaId  BIGINT        NULL               -- soft reference → PersonaActivoId
    );
END
GO
```

- **PK/identity**: `Id BIGINT IDENTITY(1,1) PRIMARY KEY` (consistente con `PersonaActivo.Id`).
- **Sin FK** sobre `ReferenciaId`: es una **soft reference** a `PersonaActivoId`. Evita acoplar la bitácora al ciclo de vida de `PersonaActivo` (la bitácora debe sobrevivir aunque la asignación se desvincule/elimine lógicamente) y no bloquea el `INSERT` de un registro `Fallido` cuando la fila ya fue compensada.
- **Sin columnas de auditoría** (`CreadoPor`/`Estatus`/etc.): es una tabla de log de solo-escritura, de ciclo de vida append-only. `FechaEnvio` se fija `GETDATE()` dentro del SP.

### D1.3 — SPs nuevos (3)

Se **CREAN** (no re-DROP/CREATE los 5 existentes). Para idempotencia se usa el mismo guard que la migración original:

```sql
IF OBJECT_ID(N'dbo.GenerarTokenConfirmacion', N'P') IS NOT NULL DROP PROCEDURE dbo.GenerarTokenConfirmacion;
GO
```

**a) `GenerarTokenConfirmacion`** — persiste el token generado en C#. Decisión: **SP separado** (no se pliega al SP de asignación porque eso exigiría re-DROP/CREATE de `AsignarActivoPersona`, prohibido por alcance). El GUID se genera en C# (`Guid.NewGuid()`) siguiendo el precedente de reset (`Guid.NewGuid().ToString()` + persistencia), de modo que el servicio ya dispone del token para construir la URL.

```sql
CREATE PROCEDURE dbo.GenerarTokenConfirmacion
(
    @PersonaActivoId   BIGINT,
    @TokenConfirmacion UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PersonaActivo
    SET TokenConfirmacion = @TokenConfirmacion
    WHERE Id = @PersonaActivoId AND FechaFin IS NULL;
    SELECT @@ROWCOUNT;   -- 1 = ok; 0 = fila inexistente o ya desvinculada
END
GO
```

**b) `ConfirmarRecepcionActivo`** — idempotente, sin `@Usuario` (es anónimo; el token GUID es el único secreto). Decisión de contrato de retorno: **señal tri-estado escalar** (no solo row count), para que el endpoint distinga "confirmado ahora" vs "ya confirmado":

```sql
CREATE PROCEDURE dbo.ConfirmarRecepcionActivo
(
    @TokenConfirmacion UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PersonaActivo WHERE TokenConfirmacion = @TokenConfirmacion)
        BEGIN SELECT 0; RETURN; END   -- token desconocido

    IF EXISTS (SELECT 1 FROM PersonaActivo
               WHERE TokenConfirmacion = @TokenConfirmacion AND FechaConfirmacion IS NOT NULL)
        BEGIN SELECT 2; RETURN; END   -- ya confirmado (idempotente, sin cambio)

    UPDATE PersonaActivo
    SET FechaConfirmacion = GETDATE()
    WHERE TokenConfirmacion = @TokenConfirmacion AND FechaConfirmacion IS NULL;

    SELECT 1;                         -- confirmado ahora
END
GO
```

- **Retorno**: `0` = desconocido, `1` = confirmado ahora, `2` = ya confirmado.
- **No actualiza** `ModificadoPor`/`FechaModificacion` (no hay usuario autenticado; evita fabricar auditoría).
- **No filtra por `EmpresaId`** (sin `@Usuario` anónimo); el token GUID es globalmente único, por lo que no hay riesgo de fuga entre tenants.

**c) `RegistrarBitacoraCorreo`** — INSERT append-only:

```sql
CREATE PROCEDURE dbo.RegistrarBitacoraCorreo
(
    @TipoCorreo   NVARCHAR(50),
    @Destinatario NVARCHAR(250),
    @Asunto       NVARCHAR(250),
    @Estado       NVARCHAR(20),
    @Error        NVARCHAR(MAX) = NULL,
    @ReferenciaId BIGINT        = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO BitacoraCorreo (TipoCorreo, Destinatario, Asunto, Estado, Error, FechaEnvio, ReferenciaId)
    VALUES (@TipoCorreo, @Destinatario, @Asunto, @Estado, @Error, GETDATE(), @ReferenciaId);
    SELECT SCOPE_IDENTITY();
END
GO
```

### D1.4 — `migration.sql` y `rollback.sql`

- `migration.sql`: orden = columnas (`D1.1`) → índice → tabla `BitacoraCorreo` (`D1.2`) → 3 SPs (`D1.3`). Idempotente por guards `IF NOT EXISTS` / `IF OBJECT_ID ... IS NULL`; los 3 SPs usan DROP/CREATE (son nuevos, no los 5 vivos).
- `rollback.sql` (orden inverso): `DROP PROCEDURE` de los 3 SPs → `DROP INDEX IX_PersonaActivo_TokenConfirmacion` → `DROP TABLE BitacoraCorreo` → `DROP COLUMN TokenConfirmacion` → `DROP COLUMN FechaConfirmacion` (cada `DROP COLUMN` con guard `IF EXISTS` en `sys.columns`).

---

## D2 — Entidades (`ServiceDeskDESIEntities`)

### D2.1 — `PersonaActivo.cs` (+ DTO)

Añadir dos propiedades (el DTO hereda de `PersonaActivo`, así que las gana automáticamente; no se modifica `PersonaActivoDTO.cs`):

```csharp
public DateTime? FechaConfirmacion { get; set; }
public Guid? TokenConfirmacion { get; set; }
```

- `TokenConfirmacion` se declara `Guid?` (nullable) porque la mayoría de filas tienen `NULL`.
- Los SPs de lectura existentes (`ObtenerActivosPorPersona`, etc.) **no** devuelven estas columnas; `LlenarEntidad<T>` las deja en `null` sin error (comportamiento deseado: no se expone el token en listados).

### D2.2 — `BitacoraCorreo.cs` (nuevo)

Nuevo archivo `Catalogos/BitacoraCorreo.cs`. **No hereda `BaseObject`** (es una entidad de log mínima; sus columnas no coinciden con las de auditoría de `BaseObject`). Sin DTO (no hay endpoint de lectura en alcance):

```csharp
using System;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class BitacoraCorreo
    {
        public long Id { get; set; }
        public string TipoCorreo { get; set; }
        public string Destinatario { get; set; }
        public string Asunto { get; set; }
        public string Estado { get; set; }
        public string Error { get; set; }
        public DateTime FechaEnvio { get; set; }
        public long? ReferenciaId { get; set; }
    }
}
```

### D2.3 — Registro en `.csproj`

En `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj`, añadir (junto a las otras entidades de `Catalogos`):

```xml
<Compile Include="Catalogos\BitacoraCorreo.cs" />
```

---

## D3 — Orquestación WebApi (`PersonaActivoService.AsignarActivoPersona`)

### D3.1 — Secuencia exacta

Tras el éxito del SP de asignación (se conserva la lógica actual de validación y el early-return en `IsSuccess=false`):

1. `newId = (long)result.Response` (id devuelto por `AsignarActivoPersona`).
2. Obtener datos (métodos ya existentes en `DbWrapper`):
   - `persona = _dbWrapper.ObtenerPersonaPorId(personaId, usuario)` → `PersonaDTO` (`Nombre, Apellido, Correo, PuestoNombre`).
   - `activo = _dbWrapper.ObtenerActivoPorId(activoId, usuario)` → `ActivoDTO` (`Nombre, Serial, TipoActivoNombre, MarcaNombre, ModeloNombre`).
   - `asignador = _dbWrapper.ObtenerUsuarioPorNombreUsuario(usuario, usuario)` → `UsuarioDTO` (`Nombre, Apellido`).
   - Si alguna de las tres lecturas falla → compensar (desvincular) + devolver error (mismo invariante que el fallo de correo; ver D3.3).
3. `Guid token = Guid.NewGuid();` y persistir: `_dbWrapper.GenerarTokenConfirmacion(newId, token)` (nuevo método DAL, ver D3.4).
   - Si `@@ROWCOUNT == 0` → compensar + error.
4. Construir URL: `string baseUri = ConfigurationManager.AppSettings["BaseUri"];` → `string urlConfirmacion = $"{baseUri}Home/ConfirmarRecepcion/{token}";` (apunta a la **página MVC**, no al WebApi).
5. Resolver los **11 placeholders** (null-safe con `?? string.Empty`):
   - `{{NombreUsuario}}` = `$"{persona.Nombre} {persona.Apellido}"`
   - `{{AsignadoPor}}` = `$"{asignador.Nombre} {asignador.Apellido}"`
   - `{{NombreActivo}}` = `activo.Nombre`
   - `{{Serial}}` = `activo.Serial`
   - `{{TipoActivo}}` = `activo.TipoActivoNombre`
   - `{{Marca}}` = `activo.MarcaNombre`
   - `{{Modelo}}` = `activo.ModeloNombre`
   - `{{FechaAsignacion}}` = `DateTime.Now.ToString("dd/MM/yyyy HH:mm")` (≈ `GETDATE()` del SP; deriva sub-segundo aceptable, documentada)
   - `{{PuestoUsuario}}` = `persona.PuestoNombre`
   - `{{CorreoUsuario}}` = `persona.Correo`
   - `{{UrlConfirmacion}}` = `urlConfirmacion`
   - (Los placeholders `{{Descripcion}}`/`{{Notas}}` están **dentro de un comentario HTML** en el template → no se resuelven.)
6. Leer template `HostingEnvironment.MapPath("~/Template/Template_AsignacionActivo.html")` → `File.ReadAllText` → `Replace(...)` encadenado.
7. Enviar en try/catch: `EmailHelper.EnvioEmaiil(new List<string>{ persona.Correo }, "Asignación de activo - Service Desk DESI", templateHtml, false)`.
8. Registrar bitácora `Enviado` (en try/catch propio).

### D3.2 — Orden decisivo: persistir primero, enviar después

**Decisión**: la asignación + token se persisten **antes** del envío; si el envío falla, se desvincula. Razones: (a) el token debe existir en BD para que el enlace sea verificable; (b) la compensación síncrona exige que el envío ocurra dentro del mismo request; (c) si persistir token fallara, no tiene sentido enviar correo con un enlace roto.

### D3.3 — Compensación ante fallo (NAA-004/005)

Cuando `EnvioEmaiil` lanza (o cualquier paso 2–6 falla):

1. `try { _dbWrapper.DesvincularActivoPersona(newId, usuario); } catch { Log.Error(...); }` (compensación; conserva `FechaFin`/histórico, no borra).
2. `try { _dbWrapper.RegistrarBitacoraCorreo("AsignacionActivo", persona.Correo, asunto, "Fallido", ex.Message, newId); } catch { Log.Error(...); }`.
3. Retornar `new ModelResponse { IsSuccess = false, Message = "..." }`.

**Mensaje de error accionable (exacto)**:

> "No se pudo enviar el correo de confirmación de asignación. La asignación fue revertida. Verifique la configuración de correo (SMTP) e intente nuevamente."

- **Invariante**: nunca se devuelve `IsSuccess=true` cuando el correo falló. Si la compensación (desvinculación) también falla, se registra el error (`Log.Error`) y de todos modos se devuelve `IsSuccess=false` (NAA-004 "Compensación también falla").
- La excepción de `EmailHelper` se captura y **no** se propaga como error no controlado (NAA-005).
- Si `persona.Correo` es null/blank, el envío no se intenta y se trata como fallo (compensa + bitácora `Fallido` con mensaje "correo del destinatario no disponible").

### D3.4 — Nuevos métodos DAL (`DbWrapper.PersonaActivo.cs`)

- `ModelResponse GenerarTokenConfirmacion(long personaActivoId, Guid token)` → `ExecuteScalar("GenerarTokenConfirmacion", ...)`; `IsSuccess = (Convert.ToInt64(resultado) > 0)`.
- `ModelResponse ConfirmarRecepcionActivo(Guid token)` → `ExecuteScalar("ConfirmarRecepcionActivo", ...)`; expone `Response = (long)estado` (0/1/2) y `IsSuccess = (estado != 0)`.
- `ModelResponse RegistrarBitacoraCorreo(string tipo, string destinatario, string asunto, string estado, string error, long? referenciaId)` → `ExecuteScalar("RegistrarBitacoraCorreo", ...)`; `IsSuccess = (Convert.ToInt64(resultado) > 0)`.

### D3.5 — `usings` nuevos en el servicio

`PersonaActivoService.cs` requiere añadir `using ServiceDeskDESIWebApi.Helpers;` y referencias a `System.Configuration` / `System.Web.Hosting` / `System.IO` (o usar `ConfigurationManager.AppSettings[...]` y `HostingEnvironment.MapPath(...)` totalmente calificados, como ya hace `AutenticacionService`).

---

## D4 — Flujo de confirmación

### D4.1 — Endpoint WebApi (anónimo)

En `PersonaActivoController` (el controlador tiene `[Authorize]` a nivel de clase; `[AllowAnonymous]` lo anula por acción, igual que en `AutenticationController`):

```csharp
[AllowAnonymous]
[HttpGet, Route("confirmarRecepcion/{token:guid}")]
public ModelResponse ConfirmarRecepcion(Guid token)
{
    return _personaActivoService.ConfirmarRecepcion(token);
}
```

- **`{token:guid}`**: un token malformado no matchea la ruta → HTTP 404 (el MVC lo renderiza como error). Además el servicio valida `token == Guid.Empty`.
- **Sin `[Permiso(...)]`** (es anónimo, no hay usuario que validar).
- El servicio `ConfirmarRecepcion(Guid token)` llama `_dbWrapper.ConfirmarRecepcionActivo(token)` y mapea el tri-estado:
  - `0` → `IsSuccess=false`, `Message="El enlace de confirmación no es válido o ha sido alterado."`
  - `1` → `IsSuccess=true`, `Message="Recepción confirmada correctamente."`
  - `2` → `IsSuccess=true`, `Message="La recepción de este activo ya fue confirmada anteriormente."`

### D4.2 — MVC: acción pública + vista

- **Acción** `HomeController.ConfirmarRecepcion(string token)` (pública; añadida a `PublicActions` en `FilterConfig`):

```csharp
public async Task<ActionResult> ConfirmarRecepcion(string token)
{
    var resultado = await _personaActivoService.ConfirmarRecepcion(token); // nuevo
    ViewBag.Resultado = resultado;
    ViewBag.Token = token;
    return View();
}
```

  - `_personaActivoService` es la nueva instancia en `HomeController` (o se reutiliza el patrón existente con `httpClientConnection`). El MVC **no** toca BD: llama al WebApi vía `HttpClient`.
  - El token llega como `string` desde la URL; el MVC lo reenvía tal cual al WebApi (la validación GUID la hace el WebApi vía `:guid`).

- **Allowlist** en `FilterConfig.PublicActions`: añadir `"Home.ConfirmarRecepcion"`.

- **Cliente HTTP** (`HttpClientConnection.PersonaActivo.cs`): nuevo método anónimo (sin bearer), replicando `ValidarTokenRecuperacion`:

```csharp
public async Task<ModelResponse> ConfirmarRecepcion(string token)
{
    var result = await RequestAsync<object>($"api/PersonaActivo/confirmarRecepcion/{token}", HttpMethod.Get, null,
        new Func<string, string>((r) => r)); // sin token (anónimo)
    return JsonConvert.DeserializeObject<ModelResponse>(result.ToString());
}
```

  - Wrapper en `Services/PersonaActivoService.cs` (MVC): `ConfirmarRecepcion(string token)`.

- **Vista** `Views/Home/ConfirmarRecepcion.cshtml` (nueva, pública, **layout standalone** sin barra autenticada — misma estrategia visual que `RecoverPassword.cshtml`). Renderiza según `ViewBag.Resultado`:
  - `IsSuccess=true` mensaje normal → tarjeta verde "Recepción confirmada".
  - `IsSuccess=true` mensaje "ya fue confirmada" → tarjeta informativa (idempotencia).
  - `IsSuccess=false` → tarjeta de error "Enlace inválido" con texto claro (CRA-004).

### D4.3 — Enlace del correo y composición de `BaseUri`

- El enlace del correo apunta a **`{BaseUri}Home/ConfirmarRecepcion/{token}`** (página MVC pública). `BaseUri` se lee del `Web.config` del **WebApi** (producción: `http://servicedesk.desipr.com.mx/`), el mismo valor que usa reset de contraseña (`{BaseUri}Home/RecoverPassword/{token}`). **No** se usa `BaseUriWebApi` para el enlace.
- El token GUID viaja en el path, **no** el `PersonaActivoId` (evita enumeración de IDs).

### D4.4 — Seguridad (CRA-001/003/004/005)

- Token `UNIQUEIDENTIFIER` (GUID v4), **sin caducidad**, idempotente (`WHERE FechaConfirmacion IS NULL`).
- Token desconocido → error claro, sin cambio de estado. Token malformado → 404 (ruta `:guid`) renderizado como error.
- **Sin endpoint de confirmación administrativo**: el admin no puede confirmar; la única vía es el enlace anónimo.

---

## D5 — Template

- Renombrar `Template/Templat_AsignacionActivo.html` → `Template/Template_AsignacionActivo.html`.
- Actualizar `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` línea 206:
  ```xml
  <Content Include="Template\Template_AsignacionActivo.html" />
  ```
- El contenido **no cambia**: los 11 placeholders y el botón "Confirmar Recepción" apuntando a `{{UrlConfirmacion}}` ya están correctos (líneas 26–138). No se tocan `{{Descripcion}}`/`{{Notas}}` (están comentados).

---

## D6 — Aplicación de la migración (BD hosted)

- **Precondición**: aplicar `migration.sql` a `db_9c7990_servicedeskdesi` **antes** de desplegar el build nuevo (el WebApi consultará las columnas/SPs/tabla).
- **Comando** (credenciales en `Web.config` → `connectionStrings/cCon`):

```powershell
& "C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\SQLCMD.EXE" `
    -S SQL5105.site4now.net `
    -d db_9c7990_servicedeskdesi `
    -U db_9c7990_servicedeskdesi_admin `
    -P "<password de cCon>" `
    -C -i "openspec\changes\asignacion-activos\migration.sql"
```

- **Idempotencia**: re-ejecutar es seguro (guards `IF NOT EXISTS` en columnas/índice/tabla; DROP/CREATE solo de los 3 SPs nuevos). **No** re-DROP/CREA los 5 SPs vivos.
- **Rollback**: `sqlcmd ... -i rollback.sql` (orden inverso, ver D1.4). Las columnas nuevas son `NULL`, la tabla de bitácora se puede `DROP` sin perder datos de negocio (solo logs). Revertir código → la asignación vuelve a funcionar sin correo/compensación (estado previo).
- **Verificación post-migración**: `SELECT name FROM sys.columns WHERE object_id=OBJECT_ID('PersonaActivo')` debe incluir `FechaConfirmacion`/`TokenConfirmacion`; `SELECT OBJECT_ID('BitacoraCorreo')` no nulo; `SELECT OBJECT_ID('ConfirmarRecepcionActivo')` no nulo.

---

## D7 — Qué NO cambia

- **Los 5 SPs existentes** (`AsignarActivoPersona`, `DesvincularActivoPersona`, `ObtenerActivosPorPersona`, `ObtenerActivosDisponibles`, `ObtenerActivos`) — no se re-DROP/CREATE ni se alteran. `ObtenerActivos` (que hoy hace DROP/CREATE en la migración original) se deja tal cual.
- **UI de asignación/desvinculación** (`_AsignarActivoPersona.cshtml`, `Persona.cshtml`, `Active.cshtml`, `CatalogsController`) — sin cambios. La mejora se percibe sola: el `ModelResponse` de la WebApi ahora devuelve `IsSuccess=false` (con el mensaje de compensación) cuando el correo falla, y el SweetAlert existente ya muestra `Message`.
- **`EmailHelper.EnvioEmaiil`** — se conserva el nombre con typo ("Emaiil"); no se renombra (lo usan otros flujos). No se toca `EnableSsl=true`.
- **Migración ya aplicada** del núcleo (`PersonaActivo` + 5 SPs) — no se re-ejecuta la parte del DROP/CREATE de los 5 SPs.

---

## Open points / assumptions

1. **Confirmación de una asignación luego desvinculada**: el SP `ConfirmarRecepcionActivo` matchea **solo por token** (sin guard `FechaFin IS NULL`). Asunción: el token solo se envía cuando el correo salió bien (asignación activa); una confirmación tardía tras una desvinculación posterior se acepta (idempotente, registra que el usuario finalmente recibió). Si el negocio prefiere bloquearla, añadir `AND FechaFin IS NULL` al `UPDATE`.
2. **`FechaAsignacion` en el correo** usa `DateTime.Now` del servicio (no re-lee `FechaInicio` de BD, que fijó `GETDATE()` el SP). Deriva sub-segundo; si se exige exactitud, añadir un `SELECT FechaInicio` por `PersonaActivoId` (implica un mini-SP o reutilizar un lector).
3. **Índice `IX_PersonaActivo_TokenConfirmacion` no único** (por los `NULL` múltiples). La unicidad del token es por diseño (GUID), no por constraint.
4. **Latencia SMTP síncrona** en `POST Asignar` es aceptada y requerida por la compensación síncrona (riesgo documentado en proposal).
5. **`BaseUri` debe ser la URL pública del MVC**. En el `Web.config` del WebApi ya apunta a `http://servicedesk.desipr.com.mx/` en producción; verificar que coincida con el dominio real del MVC antes de desplegar.
6. La bitácora es **solo-escritura** (append-only, sin endpoint de lectura ni reintentos) — fuera de alcance confirmado.
