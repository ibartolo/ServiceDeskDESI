# Design: Módulo "Configuración de Empresa" (horario laboral + logotipo)

- **Change**: `configuracion-empresa`
- **Fase**: design
- **Fecha**: 2026-09-07

## Overview

Nueva página de menú independiente `ConfiguracionEmpresa` (llave sin tilde, etiqueta "Configuración de Empresa") donde el usuario autenticado **ve** los datos generales de su `Empresa` (solo lectura) y **edita** dos criterios: (a) horario laboral (7 días) y (b) logotipo (SVG/PNG). Se replica el patrón MVC → `HttpClientConnection` → WebApi → `DbWrapper` (ADO.NET) → SP (precedentes: `Dashboard`, `MisActivos`, foto de perfil, `GuardarPermisosRolMasivo`). Sin operaciones de eliminación. Footer estático "by DESi" en `_Layout.cshtml`. Acceso por `RolPaginaAccion` (seed solo a rol `Administrador`), aislamiento multi-tenant por usuario autenticado.

```
MVC (ConfiguracionEmpresaController)
   │  HorarioLaboralService / EmpresaService (MVC)
   ▼
HttpClientConnection.HorarioLaboral / .Empresa  ──(ModelResponse<T> JSON)──►  WebApi
                                                                             [Authorize][Permiso]
                                                                             HorarioLaboralController / EmpresaController
                                                                                    ▼
                                                                             HorarioLaboralService / EmpresaService
                                                                                    ▼
                                                                             DbWrapper (BeginTransaction + ExecuteScalar/GetObjects)
                                                                                    ▼
                                                                             SPs: ObtenerHorarioLaboral, GuardarHorarioLaboralDia, GuardarLogoEmpresa
```

Logo: el binario **no** viaja por WebApi. El MVC lo guarda en disco (`Uploads/Logos/{empresaId}/{guid}.ext`) y persiste solo la URL parcial `Empresa.LogoUrl` vía SP `GuardarLogoEmpresa`.

---

## Architecture Decisions

### Decision: Guardado de horario = `GuardarHorarioLaboralDia` × 7 en transacción C#

**Choice**: un SP atómico por día `GuardarHorarioLaboralDia(@Usuario, @DiaSemana, @HoraInicio, @HoraFin, @EsLaboral)` (INSERT/UPDATE según exista), invocado 7 veces dentro de `_dbWrapper.BeginTransaction()`/`CommitTransaction()` desde un único método `DbWrapper.GuardarHorarioLaboral(usuario, List<HorarioLaboral>)`. Espeja `GuardarPermisosRolMasivo` (`DbWrapper.Permisos.cs:303-367`).
**Alternatives considered**: (a) un SP de reemplazo de semana con TVP o JSON; (b) `DELETE` + `INSERT` de 7 filas en un solo SP con 7×3 parámetros explícitos.
**Rationale**: TVP/JSON no tienen precedente en el repo (grep = 0); la transacción C# reutiliza `BeginTransaction` (una sola `SqlConnection`, evita escalación a MSDTC) y mantiene atomicidad "todo o nada" (CE-004). El mismo primitivo `GuardarHorarioLaboralDia` se reutiliza para la siembra de empresas nuevas (PASO 5.2) **sin** abrir otra transacción (ya hay transacción ambiente).

### Decision: Columnas `HoraInicio`/`HoraFin` como `datetime NULL` con ancla fija `1900-01-01`

**Choice**: `HoraInicio`/`HoraFin` `[datetime] NULL`; la fecha se normaliza a `1900-01-01` (ancla única) tanto en C# como en SQL.
**Alternatives considered**: `time` + `TimeSpan` (recomendado en explore Q5) — descartado por decisión fija del usuario (proposal: "decisión fija, NO `time`"); `nvarchar(5)` 'HH:mm'.
**Rationale**: solo la componente hora es significativa (CE-004). `CAST('09:00' AS datetime)` en SQL produce `1900-01-01 09:00`, por lo que el ancla `1900-01-01` mantiene **idénticos** los valores del backfill SQL y los generados en C# (`new DateTime(1900,1,1,hh,mm,0)`), sin fricción de serialización. Exigir `HoraFin > HoraInicio` (misma fecha ancla) garantiza ventana de un solo día y rechaza `HoraFin == HoraInicio` (CE-004, supuesto 3).

### Decision: Logo = archivo en disco MVC + columna `Empresa.LogoUrl` (URL parcial)

**Choice**: la subida es MVC-side (`HttpPostedFileBase`, espeja `UserController.ActualizarPerfilUsuario`); el archivo se guarda en `ServiceDeskDESIMVC/Uploads/Logos/{empresaId}/{guid}.{ext}` con nombre GUID generado en servidor; se persiste la URL **relativa** `"/Uploads/Logos/{empresaId}/{guid}.ext"` en `Empresa.LogoUrl NVARCHAR(500) NULL` vía WebApi → SP `GuardarLogoEmpresa(@Usuario, @LogoUrl)` con `[Permiso("ConfiguracionEmpresa","Editar")]`. El `<img>` del sidebar usa la URL relativa servida por los estáticos del MVC.
**Alternatives considered**: varbinary en BD (sin precedente, peor para servir inline); tabla genérica `EmpresaConfiguracion` clave/valor (desviación del estilo; no existe en repo).
**Rationale**: coherencia con foto de perfil (servida inline como estático MVC); el binario no hace round-trip por WebApi; la URL relativa evita acoplar el dominio de la API en BD (MVC y WebApi siguen siendo despliegues separados). El servidor genera `{guid}.ext` → sin path traversal desde el cliente.

### Decision: Sidebar obtiene `LogoUrl` en `HomeController.MenusUser()`, no en login

**Choice**: `HomeController.MenusUser()` (acción que ya tiene `_empresaService`) carga `Empresa` por `TokenCookie.EmpresaID` y expone `ViewBag.LogoUrl` + `ViewBag.NombreComercial` a `MenusUser.cshtml`.
**Alternatives considered**: guardar `LogoUrl` en `TokenCookie` (requiere cambiar login para obtener la `Empresa`, prohibido por CE-006 "El login NO cambia"); recargar el sidebar vía AJAX adicional.
**Rationale**: `MenusUser` ya se carga vía `$("#sidebar").load("/Home/MenusUser")` en cada página (`_Layout.cshtml:52`); añadir ahí la lectura de la empresa es el punto natural y no toca login. Costo = un round-trip extra (aceptable; el sidebar ya hace `ObtenerPaginasPorUsuario`).

---

## DB Design (DDL + SPs)

### Tabla `EmpresaHorarioLaboral`

```sql
CREATE TABLE [dbo].[EmpresaHorarioLaboral](
    [Id] [bigint] IDENTITY(1,1) NOT NULL,
    [EmpresaId] [bigint] NOT NULL,
    [DiaSemana] [tinyint] NOT NULL,            -- 1=Lun .. 7=Dom
    [HoraInicio] [datetime] NULL,              -- ancla 1900-01-01
    [HoraFin] [datetime] NULL,
    [Estatus] [bit] NOT NULL CONSTRAINT [DF_EmpresaHorarioLaboral_Estatus] DEFAULT ((1)),
    [CreadoPor] [nvarchar](25) NOT NULL,
    [FechaCreacion] [datetime] NOT NULL,
    [ModificadoPor] [nvarchar](25) NULL,
    [FechaModificacion] [datetime] NULL,
    CONSTRAINT [PK_EmpresaHorarioLaboral] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_EmpresaHorarioLaboral_EmpresaDia] UNIQUE ([EmpresaId], [DiaSemana]),
    CONSTRAINT [FK_EmpresaHorarioLaboral_Empresa] FOREIGN KEY ([EmpresaId]) REFERENCES [dbo].[Empresa]([Id])
)
GO
```

### Columna en `Empresa`

```sql
ALTER TABLE [dbo].[Empresa] ADD [LogoUrl] [nvarchar](500) NULL;
```

### SPs

**`ObtenerHorarioLaboral`** — lectura (tenant desde `@Usuario`):

```sql
CREATE PROCEDURE [dbo].[ObtenerHorarioLaboral] @Usuario NVARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DiaSemana, HoraInicio, HoraFin, Estatus
    FROM [dbo].[EmpresaHorarioLaboral]
    WHERE EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY DiaSemana;
END
```

**`GuardarHorarioLaboralDia`** — upsert por día (idempotente; tenant desde `@Usuario`):

```sql
CREATE PROCEDURE [dbo].[GuardarHorarioLaboralDia]
    @Usuario NVARCHAR(25), @DiaSemana TINYINT,
    @HoraInicio DATETIME, @HoraFin DATETIME, @EsLaboral BIT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END
    IF @EsLaboral = 0 BEGIN SET @HoraInicio = NULL; SET @HoraFin = NULL; END
    IF EXISTS (SELECT 1 FROM EmpresaHorarioLaboral WHERE EmpresaId = @EmpresaId AND DiaSemana = @DiaSemana)
        UPDATE EmpresaHorarioLaboral
           SET HoraInicio = @HoraInicio, HoraFin = @HoraFin, Estatus = @EsLaboral,
               ModificadoPor = @Usuario, FechaModificacion = GETDATE()
         WHERE EmpresaId = @EmpresaId AND DiaSemana = @DiaSemana;
    ELSE
        INSERT INTO EmpresaHorarioLaboral (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
        VALUES (@EmpresaId, @DiaSemana, @HoraInicio, @HoraFin, @EsLaboral, @Usuario, GETDATE());
    SELECT 1;
END
```

**`GuardarLogoEmpresa`** — persiste URL parcial (tenant desde `@Usuario`):

```sql
CREATE PROCEDURE [dbo].[GuardarLogoEmpresa] @Usuario NVARCHAR(25), @LogoUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END
    UPDATE Empresa SET LogoUrl = @LogoUrl, ModificadoPor = @Usuario, FechaModificacion = GETDATE()
     WHERE Id = @EmpresaId AND Estatus = 1;
    SELECT 1;
END
```

### Backfill idempotente (empresas existentes)

```sql
INSERT INTO [dbo].[EmpresaHorarioLaboral] (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
SELECT e.Id, d.Dia, d.Inicio, d.Fin, CASE WHEN d.Dia BETWEEN 1 AND 5 THEN 1 ELSE 0 END, N'migracion', GETDATE()
FROM [dbo].[Empresa] e
CROSS JOIN (VALUES
    (1, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (2, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (3, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (4, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (5, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (6, NULL, NULL),
    (7, NULL, NULL)) d(Dia, Inicio, Fin)
WHERE NOT EXISTS (SELECT 1 FROM EmpresaHorarioLaboral h WHERE h.EmpresaId = e.Id);
```

### Seed de página + permiso (solo rol Administrador de todas las empresas)

```sql
IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'ConfiguracionEmpresa')
BEGIN
    INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
    VALUES (N'ConfiguracionEmpresa', N'Configuración de Empresa', N'Datos, horario laboral y logotipo de la empresa', N'Menu', N'/ConfiguracionEmpresa', NULL, N'fa-cog', 9, 1);
END
GO
INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
SELECT r.Id, p.Id, 1, 0, 1, 0, 0, N'migracion', GETDATE(), 1
FROM Rol r CROSS JOIN Pagina p
WHERE p.Nombre = N'ConfiguracionEmpresa' AND r.Nombre = N'Administrador' AND r.Estatus = 1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
GO
```

> ⚠️ Verificar columnas reales de `Pagina`/`RolPaginaAccion` en BD hosted antes de ejecutar (drift conocido). Empresas **nuevas** reciben la página automáticamente porque `GuardarNuevaEmpresaConDatosIniciales` PASO 7 itera `ObtenerPaginas()` e inserta todos los flags en `true` para el rol Administrador.

### Hook en `EmpresaService.GuardarNuevaEmpresaConDatosIniciales`

Insertar **"PASO 5.2"** en `ServiceDeskDESIWebApi/Services/EmpresaService.cs`, entre el fin del PASO 5.1 (bloque "Mis Activos", ~línea 534) y el inicio del PASO 6 (~línea 537), **dentro de la transacción existente** (`_dbWrapper.BeginTransaction()` ya abierta en línea 311):

```csharp
// PASO 5.2/8 - SEMBRAR HORARIO LABORAL POR DEFECTO (Lun-Vie 09:00-17:00; Sáb/Dom no laborable)
for (int dia = 1; dia <= 7; dia++)
{
    bool laborable = dia >= 1 && dia <= 5;
    DateTime? inicio = laborable ? new DateTime(1900, 1, 1, 9, 0, 0) : (DateTime?)null;
    DateTime? fin    = laborable ? new DateTime(1900, 1, 1, 17, 0, 0) : (DateTime?)null;
    var r = _dbWrapper.GuardarHorarioLaboralDia(usernameAdmin, dia, inicio, fin, laborable);
    if (!r.IsSuccess) { throw new Exception("No se pudo sembrar el horario laboral por defecto."); }
}
```

> `usernameAdmin` ya existe (PASO 4, línea 402-441, `Estatus=true`, `EmpresaId=empresaGuardada.Id`); como `GuardarHorarioLaboralDia` resuelve `@EmpresaId` desde `@Usuario` en la **misma conexión/transacción**, ve la fila pendiente. **No se usa trigger** (grep `CREATE TRIGGER` = 0). `RegistrarEmpresa` ya delega en este método → cobertura automática.

---

## Entities / DTO Design

### `ServiceDeskDESIEntities/Catalogos/HorarioLaboral.cs` (nuevo POCO)

```csharp
namespace ServiceDeskDESIEntities.Catalogos
{
    public class HorarioLaboral : BaseObject   // hereda Id, CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion, Estatus
    {
        public long EmpresaId { get; set; }
        public int DiaSemana { get; set; }        // 1=Lun..7=Dom
        public DateTime? HoraInicio { get; set; } // datetime NULL (ancla 1900-01-01)
        public DateTime? HoraFin { get; set; }
    }
}
```

`LlenarEntidad<HorarioLaboral>` mapea por nombre (case-insensitive): `DiaSemana` (tinyint→byte→int), `HoraInicio`/`HoraFin` (datetime→DateTime), `Estatus` (bit→bool). Funciona sin tocar el helper.

### `ServiceDeskDESIEntities/Catalogos/Empresa.cs` (modificar)

Añadir `public string LogoUrl { get; set; }`. Como `ObtenerEmpresaPorId` usa `SELECT e.*` (`basededatosservicedesk.txt:4069`), el nuevo `LogoUrl` fluye automáticamente a `LlenarEntidad<Empresa>` sin tocar el SP de lectura.

### DTOs de frontera

| DTO | Vive en | Forma |
|---|---|---|
| `HorarioLaboral` (entidad) | Entities | Transporte MVC↔WebApi (lectura y guardado). El `EmpresaId`/`Id` del body se ignoran server-side (tenant desde `@Usuario`). |
| `HorarioViewModel` | `ServiceDeskDESIMVC/Models/` (MVC-side) | `{ int DiaSemana; bool EsLaboral; string HoraInicio; string HoraFin; }` — contrato de UI en "HH:mm". |
| `GuardarLogoRequest` | inline en `EmpresaController.cs` (WebApi) | `{ string LogoUrl }` — espeja `AsignarRolRequest` (inline en `RolController.cs`). El MVC postea un objeto anónimo `new { LogoUrl = logoUrl }`. |

---

## WebApi Design

### `Controllers/HorarioLaboralController.cs` (nuevo)

```csharp
[Authorize]
[RoutePrefix("api/HorarioLaboral")]
public class HorarioLaboralController : BaseController
{
    private readonly HorarioLaboralService _service = new HorarioLaboralService();

    [HttpGet, Route("Obtener")]
    [Permiso("ConfiguracionEmpresa", "Leer")]
    public ModelResponse<List<HorarioLaboral>> Obtener()
        => _service.ObtenerHorarioLaboral(User.Identity.Name);

    [HttpPost, Route("Guardar")]
    [Permiso("ConfiguracionEmpresa", "Editar")]
    public ModelResponse Guardar(List<HorarioLaboral> horario)
        => _service.GuardarHorarioLaboral(User.Identity.Name, horario);
}
```

### `Controllers/EmpresaController.cs` (modificar — añadir acción)

```csharp
[Permiso("ConfiguracionEmpresa", "Editar")]
[HttpPost, Route("GuardarLogo")]
public ModelResponse GuardarLogo([FromBody] GuardarLogoRequest request)
    => _empresaService.GuardarLogoEmpresa(User.Identity.Name, request.LogoUrl);
```

Con `GuardarLogoRequest { public string LogoUrl { get; set; } }` declarado inline en el mismo archivo.

### `Services/HorarioLaboralService.cs` (nuevo, WebApi)

- `ModelResponse<List<HorarioLaboral>> ObtenerHorarioLaboral(string usuario)` → delega en `_dbWrapper.ObtenerHorarioLaboral(usuario)`.
- `ModelResponse GuardarHorarioLaboral(string usuario, List<HorarioLaboral> horario)`:
  1. Validar: `horario != null && horario.Count == 7`; cada `DiaSemana ∈ [1..7]`; si `Estatus` (laborable) → `HoraInicio`/`HoraFin` no nulos y `HoraFin > HoraInicio`; si no laborable → forzar `HoraInicio = HoraFin = null`.
  2. Delegar en `_dbWrapper.GuardarHorarioLaboral(usuario, horario)` (transaccional).

### `DAL/DbWrapper.HorarioLaboral.cs` (nuevo partial)

```csharp
public partial class DbWrapper
{
    public ModelResponse<List<HorarioLaboral>> ObtenerHorarioLaboral(string usuario)
        // GetObjects("ObtenerHorarioLaboral", StoredProcedure, [@Usuario], reader => LlenarEntidad<HorarioLaboral>(reader))

    public ModelResponse GuardarHorarioLaboral(string usuario, List<HorarioLaboral> horario)
        // BeginTransaction(); foreach dia → GuardarHorarioLaboralDia(usuario, dia, ini, fin, laborable);
        //   si algún resultado != 1 → throw; CommitTransaction();  (catch → RollbackTransaction())

    public ModelResponse GuardarHorarioLaboralDia(string usuario, int diaSemana, DateTime? horaInicio, DateTime? horaFin, bool esLaboral)
        // ExecuteScalar("GuardarHorarioLaboralDia", StoredProcedure, [@Usuario, @DiaSemana,
        //   (object)horaInicio ?? DBNull.Value, (object)horaFin ?? DBNull.Value, @EsLaboral])
}
```

`GuardarHorarioLaboralDia` es `public` para ser reutilizado por la siembra PASO 5.2 **sin** abrir otra transacción (reutiliza la transacción ambiente vía `ExecuteScalar`).

### `DAL/DbWrapper.Empresa.cs` + `Services/EmpresaService.cs` (modificar)

`DbWrapper.Empresa.cs`: `public ModelResponse GuardarLogoEmpresa(string usuario, string logoUrl)` → `ExecuteScalar("GuardarLogoEmpresa", ..., [@Usuario, @LogoUrl])` (valida que devuelva 1). `EmpresaService.cs` (WebApi): `public ModelResponse GuardarLogoEmpresa(string usuario, string logoUrl)` con validación de `logoUrl` no vacío y `Serilog`.

---

## MVC Design

### `Controllers/ConfiguracionEmpresaController.cs` (nuevo)

Hereda `BaseController` (tiene `httpClientConnection` protegido). Constructor cablea `_empresaService` y `_horarioService`.

| Acción | Permiso | Comportamiento |
|---|---|---|
| `Index()` → `View(empresa)` | `[Permiso("ConfiguracionEmpresa","Leer")]` | Obtiene `TokenCookie` (null → redirect `Home/Autentication`); `_empresaService.ObtenerEmpresaPorId(EmpresaID)` como modelo; lee permisos de la página (`httpClientConnection.ObtenerPermisosPorUsuario()` → `FirstOrDefault(p => p.PaginaNombre == "ConfiguracionEmpresa")`) y setea `ViewBag.PuedeEditar`. |
| `ObtenerHorario()` → `string` (JSON) | `[Permiso("ConfiguracionEmpresa","Leer")]` | Llama `_horarioService.ObtenerHorarioLaboral()`, mapea `HorarioLaboral`→`HorarioViewModel` (`HoraInicio?.ToString("HH:mm")`), devuelve `JsonConvert.SerializeObject`. |
| `GuardarHorario(List<HorarioViewModel>)` → `string` | `[Permiso("ConfiguracionEmpresa","Editar")]` | Normaliza cada fila a `HorarioLaboral` con ancla `1900-01-01` (`DateTime.ParseExact("HH:mm")` → `new DateTime(1900,1,1,hh,mm,0)`); `EsLaboral=false` → null; valida `HoraFin>HoraInicio`; llama `_horarioService.GuardarHorarioLaboral(lista)`. Devuelve `ModelResponse` JSON. |
| `SubirLogo(HttpPostedFileBase file)` → `string` | `[Permiso("ConfiguracionEmpresa","Editar")]` | Pipeline de logo (abajo). |

### `Models/HorarioViewModel.cs` (nuevo, MVC-side)

```csharp
public class HorarioViewModel
{
    public int DiaSemana { get; set; }
    public bool EsLaboral { get; set; }
    public string HoraInicio { get; set; } // "HH:mm"
    public string HoraFin { get; set; }    // "HH:mm"
}
```

### Pipeline de subida de logo (`SubirLogo`)

1. `file == null || file.ContentLength == 0` → `ModelResponse { IsSuccess=false, Message="El archivo está vacío." }`.
2. **Extensión**: `ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant()`; debe ∈ `LogoEmpresaTiposPermitidos` (`svg,png`).
3. **MIME**: `(ext=="svg" && file.ContentType=="image/svg+xml") || (ext=="png" && file.ContentType=="image/png")`. (Nota: algunos navegadores envían `application/octet-stream` para SVG; la extensión es el guard principal — ver Riesgos.)
4. **Tamaño**: `file.ContentLength <= LogoEmpresaMaxTamanoKB * 1024` (default 2048).
5. **Borrar logo previo** (re-subida reemplaza): `_empresaService.ObtenerEmpresaPorId(EmpresaID)`; si `LogoUrl` no vacío, `File.Delete(Server.MapPath("~" + LogoUrl))` si existe.
6. **Guardar**: `guid = Guid.NewGuid()`, carpeta `Uploads/Logos/{empresaId}/` (crear si no existe, espeja foto de perfil), `file.SaveAs(...)`, `logoUrl = "/Uploads/Logos/{empresaId}/{guid}.{ext}"`.
7. **Persistir**: `_empresaService.GuardarLogoEmpresa(logoUrl)` (MVC service → HttpClientConnection → WebApi SP).
8. **Responder**: `JsonConvert.SerializeObject(response)`; el JS, en éxito, recarga el sidebar (`$("#sidebar").load("/Home/MenusUser")`) para refrescar el logo.

### `Services/HorarioLaboralService.cs` + `Services/EmpresaService.cs` (MVC, modificar/nuevo)

- MVC `HorarioLaboralService` (nuevo): `ObtenerHorarioLaboral()` y `GuardarHorarioLaboral(List<HorarioLaboral>)` delegando en `HttpClientConnection`.
- MVC `EmpresaService` (modificar): `GuardarLogoEmpresa(string logoUrl)`.

### `DAL/HttpClientConnection.HorarioLaboral.cs` (nuevo) + `.Empresa.cs` (modificar)

```csharp
// HttpClientConnection.HorarioLaboral.cs
public async Task<ModelResponse<List<HorarioLaboral>>> ObtenerHorarioLaboral()
    => await RequestAsync<List<HorarioLaboral>>("api/HorarioLaboral/Obtener", HttpMethod.Get, null, token.Token.access_token);
public async Task<ModelResponse> GuardarHorarioLaboral(List<HorarioLaboral> horario)
    // espeja EliminarEmpresa: RequestAsync<object>("api/HorarioLaboral/Guardar", Post, horario, responseString=>responseString, token)
    //   → JsonConvert.DeserializeObject<ModelResponse>(...)

// HttpClientConnection.Empresa.cs
public async Task<ModelResponse> GuardarLogoEmpresa(string logoUrl)
    // RequestAsync<object>("api/Empresas/GuardarLogo", Post, new { LogoUrl = logoUrl }, ...) → ModelResponse
```

### `Views/ConfiguracionEmpresa/Index.cshtml` (nuevo, **UTF-8 CON BOM**)

Modelo `Empresa` (card solo lectura) + `ViewBag.PuedeEditar`. Tres bloques:

1. **Card datos generales** (CE-003, solo lectura): `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, `Direccion`, `Ciudad`, `Estado`, `CodigoPostal`, `Telefono`, `CorreoContacto`, `FechaVigenciaInicio`, `FechaVigenciaFin`, `EsPeriodoPrueba` — sin inputs.
2. **Editor horario** (CE-004/CE-009): tabla de 7 filas (Lun..Dom). Cada fila: checkbox "labora" + dos `<input type="datetime-local">` (ancla UI `1900-01-01T…`). Botón único "Guardar" (spinner/"Guardando…", `Swal` éxito/error). Carga inicial vía `GET /ConfiguracionEmpresa/ObtenerHorario` (AJAX).
3. **Subida logo** (CE-006/CE-009): `<input type="file" accept=".svg,.png">` + preview vía `FileReader.readAsDataURL` antes de guardar; botón "Subir" → `POST /ConfiguracionEmpresa/SubirLogo` (FormData). Ocultar controles de edición si `!ViewBag.PuedeEditar`.

**Mecánica JS del editor de horario**: el valor del `datetime-local` usa `"1900-01-01T" + "HH:mm"`. Al leer se extrae `value.substring(11,16)`; al escribir se setea `"1900-01-01T" + hora`. El MVC recibe/sirve "HH:mm" (el view model), el servidor normaliza la fecha a `1900-01-01`. Día desmarcado → se deshabilitan los inputs y se envía `EsLaboral=false` (el SP limpia horas a NULL).

### `Views/Home/MenusUser.cshtml` (modificar — logo swap)

En el header (`sidebar-header`):

```html
<div class="sidebar-header">
    @if (!string.IsNullOrEmpty(ViewBag.LogoUrl))
    {
        <img src="@ViewBag.LogoUrl" alt="@ViewBag.NombreComercial" style="max-width:120px; max-height:60px; object-fit:contain;" />
    }
    else
    {
        <i class="fas fa-headset" style="font-size: 3rem; color: white;"></i>
    }
    <h3>Service Desk DESI</h3>
    <p>Sistema de Gestión de Tickets</p>
</div>
```

### `Controllers/HomeController.cs` (modificar — `MenusUser()`)

Añadir tras obtener `paginas`: leer `TokenCookie.EmpresaID`, `_empresaService.ObtenerEmpresaPorId(...)`, setear `ViewBag.LogoUrl`/`ViewBag.NombreComercial` (null si no hay). No tocar login.

### `Views/Shared/_Layout.cshtml` (modificar — footer estático)

Inmediatamente después de `@RenderBody()` (línea 223), antes del cierre de `.main-content`:

```html
<footer class="app-footer">
    <span>© @DateTime.Now.Year Service Desk by <strong>DESi</strong></span>
    <a href="/Home/Ayuda">Ayuda</a> · <a href="#">Términos</a> · <a href="#">Privacidad</a>
</footer>
```

Markup estático (CE-007); `Términos`/`Privacidad` son hipervínculos placeholder (no existen páginas aún).

### `Web.config` (MVC, modificar — appSettings)

```xml
<add key="LogoEmpresaMaxTamanoKB" value="2048" />
<add key="LogoEmpresaTiposPermitidos" value="svg,png" />
```

Lectura con `ConfigurationManager.AppSettings[...]` (espeja el patrón `Evidencias*`, pero en el Web.config del MVC porque la subida es MVC-side).

---

## Multi-tenant & Security

- **MVC**: `TokenCookie.EmpresaID` (de `SessionHelper.GetSessionUser()`), nunca del request. Determina carpeta de logo y la lectura de empresa.
- **WebApi**: los 3 SPs nuevos reciben `@Usuario` (`User.Identity.Name`) y resuelven `(SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)`; **nunca** aceptan `EmpresaId` del body/query (CE-008). `GuardarLogoEmpresa` y `GuardarHorarioLaboralDia` devuelven `SELECT 0` si no hay tenant.
- **Permisos**: MVC `[Permiso("ConfiguracionEmpresa","Leer")]` redirige a `Home/AccesoDenegado` si `PuedeLeer=0` (`Filters/PermisoAttribute.cs:57-64`); `[Permiso(...,"Editar")]` protege guardado/subida. WebApi `[Permiso(...,"Leer"/"Editar")]` devuelve 403 (`ValidarPermisoUsuario` mapea Leer→PuedeLeer, Editar→PuedeEditar). Sin acción "Eliminar".

---

## Error Handling / Logging

- WebApi: `ModelResponse<T>` (`IsSuccess`, `Message`, `Response`) en todos los endpoints; `Serilog` (`Log.Information/Warning/Error`) con contexto (usuario). Patrón `EmpresaService`/`DbWrapper` existente.
- MVC: `try/catch` en las acciones AJAX devolviendo `JsonConvert.SerializeObject(modelResponse)` con mensaje amigable (espeja `UserController.ActualizarPerfilUsuario`); `Serilog` en errores.
- Transacción: `GuardarHorarioLaboral` hace `RollbackTransaction()` en `catch` (espeja `GuardarPermisosRolMasivo`).

---

## File-by-file change list

| Archivo | Acción | Responsabilidad |
|---|---|---|
| `openspec/changes/configuracion-empresa/migration.sql` | Create | Tabla `EmpresaHorarioLaboral` + `ALTER Empresa ADD LogoUrl` + seed `Pagina`/`RolPaginaAccion` + 3 SPs + backfill |
| `openspec/changes/configuracion-empresa/rollback.sql` | Create | DROP SPs + DELETE página/permiso + DROP tabla + DROP columna `LogoUrl` |
| `ServiceDeskDESIEntities/Catalogos/HorarioLaboral.cs` | Create | POCO `HorarioLaboral : BaseObject` |
| `ServiceDeskDESIEntities/Catalogos/Empresa.cs` | Modify | + `LogoUrl string` |
| `ServiceDeskDESIEntities/ServiceDeskDESIEntities.csproj` | Modify | Registrar `<Compile Include="Catalogos\HorarioLaboral.cs" />` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.HorarioLaboral.cs` | Create | `ObtenerHorarioLaboral`, `GuardarHorarioLaboral` (transaccional), `GuardarHorarioLaboralDia` |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Empresa.cs` | Modify | + `GuardarLogoEmpresa` |
| `ServiceDeskDESIWebApi/Services/HorarioLaboralService.cs` | Create | Validación + `ModelResponse` + Serilog |
| `ServiceDeskDESIWebApi/Services/EmpresaService.cs` | Modify | + `GuardarLogoEmpresa`; + hook PASO 5.2 en `GuardarNuevaEmpresaConDatosIniciales` |
| `ServiceDeskDESIWebApi/Controllers/HorarioLaboralController.cs` | Create | `[Authorize]` + `[Permiso]`, `User.Identity.Name` |
| `ServiceDeskDESIWebApi/Controllers/EmpresaController.cs` | Modify | + `GuardarLogo` + `GuardarLogoRequest` inline |
| `ServiceDeskDESIWebApi/ServiceDeskDESIWebApi.csproj` | Modify | Registrar `DbWrapper.HorarioLaboral.cs`, `HorarioLaboralService.cs`, `HorarioLaboralController.cs` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.HorarioLaboral.cs` | Create | `ObtenerHorarioLaboral`, `GuardarHorarioLaboral` |
| `ServiceDeskDESIMVC/DAL/HttpClientConnection.Empresa.cs` | Modify | + `GuardarLogoEmpresa` |
| `ServiceDeskDESIMVC/Services/HorarioLaboralService.cs` | Create | Lógica MVC |
| `ServiceDeskDESIMVC/Services/EmpresaService.cs` | Modify | + `GuardarLogoEmpresa` |
| `ServiceDeskDESIMVC/Models/HorarioViewModel.cs` | Create | View model "HH:mm" |
| `ServiceDeskDESIMVC/Controllers/ConfiguracionEmpresaController.cs` | Create | `Index` + AJAX horario + subida logo |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` | Modify | `MenusUser()` expone `LogoUrl`/`NombreComercial` |
| `ServiceDeskDESIMVC/Views/ConfiguracionEmpresa/Index.cshtml` | Create | UTF-8 CON BOM: card + editor + subida/preview |
| `ServiceDeskDESIMVC/Views/Home/MenusUser.cshtml` | Modify | `<img>` logo con fallback `fa-headset` |
| `ServiceDeskDESIMVC/Views/Shared/_Layout.cshtml` | Modify | Footer estático "by DESi" |
| `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` | Modify | Registrar 4 archivos MVC nuevos |
| `ServiceDeskDESIMVC/Web.config` | Modify | appSettings `LogoEmpresaMaxTamanoKB`, `LogoEmpresaTiposPermitidos` |

**Registro `.csproj` obligatorio** (old-style `<Compile Include>`): cualquier `.cs` no registrado no compila (precedente `ThemeHelper`).

---

## Verification Plan (sin test project)

- **Build**: `"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ServiceDeskDESI.sln /t:Build /p:Configuration=Debug` → **0 errores** en los 3 proyectos (confirma los `<Compile Include>` manuales).
- **Revisión estática** contra spec (CE-001..CE-009, 28 escenarios): página sembrada con llave/etiqueta/ícono/orden/`Direccion` correctos; `[Permiso("ConfiguracionEmpresa","Leer"/"Editar")]` en MVC y WebApi; card solo lectura; 7 filas con checkbox + horas, botón único transaccional; validación `HoraFin > HoraInicio`; día desmarcado → `Estatus=0` + horas NULL; defaults Lun-Vie 09:00-17:00 + backfill idempotente + hook empresa nueva; logo svg/png con validación ext/MIME/peso, carpeta `Uploads/Logos/{empresaId}/`, `Empresa.LogoUrl` relativo, re-subida reemplaza, sin limpiar; footer estático; resolución multi-tenant por usuario (nunca `EmpresaId` del cliente); `Index.cshtml` UTF-8 CON BOM.
- **Smoke manual (post-deploy)**: menú visible solo para Administrador; `/ConfiguracionEmpresa` redirige a `AccesoDenegado` sin `PuedeLeer`; guardar horario persiste 7 días; subir/visualizar logo en sidebar con fallback DESi.

---

## Assumptions / Open items

- El MIME real de SVG puede llegar como `application/octet-stream` en algunos navegadores; el guard principal es la extensión + `accept=".svg,.png"` en el input (se mantiene la validación MIME `image/svg+xml`/`image/png` como refuerzo; si hay falsos negativos en smoke, relajar a "extensión permitida + peso", documentándolo).
- `Términos`/`Privacidad` son hipervínculos placeholder (`#`) — no existen páginas destino hoy (fuera de alcance).
- `Empresa` ya no se filtra correctamente si dos empresas comparten `NombreUsuario` (`CreadoPor`) — limitación heredada de `ObtenerEmpresaPorId` (`INNER JOIN ... ON e.CreadoPor = u.NombreUsuario`), fuera del alcance de este change (ver `database-review`/`tenant-estructural`).

---

## Amendment note (post revisión visual G5)

Tras la revisión visual del lote G5 se incorporaron dos cambios de UX y sus ajustes documentales (lote de enmienda, sin objetos nuevos de BD):

1. **Quitar logotipo** (revierte el supuesto "limpiar logo NO permitido"): nueva acción MVC `ConfiguracionEmpresaController.QuitarLogo()` `[Permiso("ConfiguracionEmpresa","Editar")]` que (a) resuelve el `LogoUrl` actual desde `Empresa`, (b) borra el archivo físico bajo `Uploads/Logos/{empresaId}/` (sin fallar si ya no existe), y (c) persiste `Empresa.LogoUrl = NULL` reutilizando el pipeline existente `GuardarLogoEmpresa` (SP `GuardarLogoEmpresa` hace `UPDATE Empresa SET LogoUrl = @LogoUrl`, que admite NULL). Para permitir el NULL se RELAJÓ la validación del servicio WebApi `EmpresaService.GuardarLogoEmpresa` (se eliminó el guard `IsNullOrWhiteSpace(logoUrl)`). No se creó endpoint nuevo ni ningún objeto de BD. La vista `Index.cshtml` añade el botón "Quitar logo" (visible solo si existe logo; confirmación Swal), y al confirmar recarga el sidebar (`$("#sidebar").empty().load("/Home/MenusUser")`) para que aparezca el fallback DESi de inmediato.
2. **Editor de hora 12h (AM/PM)**: en `Index.cshtml` se reemplazaron los `<input type="datetime-local">` por tres `<select>` por campo (Hora 1–12, Minuto 00–55 en pasos de 5, y AM/PM), mediante un `@helper HoraSelect`. El JS convierte "HH:mm" (24h) ↔ 12h+AM/PM al cargar/guardar, y sigue enviando el contrato "HH:mm" que el backend ya espera (`HorarioViewModel` / `List<HorarioLaboral>` con ancla `1900-01-01`). Sin cambios en backend ni en el contrato WebApi. Los selects se deshabilitan cuando el día no es laborable, igual que antes.

No se ejecutó migración (ya aplicada por el usuario) ni se corrió la app.
