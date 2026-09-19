# Exploration: Vinculación Persona ↔ Usuario ("usuario básico") + vista "Mis Activos"

## Current State (verificado contra código real y BD)

### 0. Resumen del hallazgo
NO existe hoy ninguna relación entre `Persona` y `Usuarios`. La vinculación 1:1 es neta-nueva:
- `Usuarios` **no tiene** `PersonaId` (dump `basededatosservicedesk.txt:540-563`; entidad `ServiceDeskDESIEntities/Autenticacion/Usuario.cs:10-24` sin `PersonaId`).
- `Persona` **no tiene** `UsuarioId` (dump `:298-314`; `Catalogos/Persona.cs:10-19` solo `Nombre, Apellido, Correo, Telefono, PuestoId`).
- El patrón 1:1 con FK hacia `Usuarios` YA existe como precedente: `Area.UsuarioResponsableId` (`Catalogos/Area.cs:14`, `long?`). También `CategoriaResponsable.UsuarioId` (`Catalogos/CategoriaResponsable.cs:9`) y `Usuario.AreaId` (`Autenticacion/Usuario.cs:22`).

---

### 1. Flujo de creación de usuarios (dónde enchufa "vincular Persona + asignar rol Usuario")

**Cadena completa (admin):**
```
MVC UserController.GuardarOActualizarUsuarioAdmin (Controllers/UserController.cs:239-276)
  → UsuarioService.GuardarOActualizarUsuarioAdmin (Services/UsuarioService.cs:45-48)
    → HttpClientConnection.GuardarOActualizarUsuarioAdmin (DAL/HttpClientConnection.User.cs:25-28 → POST api/Autentication/User/Empresa? ver :30-33)
      → WebApi AutenticationController.GuardarOActualizarUsuarioAdmin (Controllers/AutenticationController.cs:163-170, [Permiso("Usuarios")])
        → AutenticacionService.GuardarOActualizarUsuarioAdmin (Services/AutenticacionService.cs:156-197)
          → DbWrapper.GuardarOActualizarUsuarioAdmin (DAL/DbWrapper.Autenticacion.cs:214-282)
            → SP [dbo].[GuardarOActualizarUsuarioAdmin] (dump 3340-3451)
```

- **SP `GuardarOActualizarUsuarioAdmin`** (dump `:3340-3451`): valida admin → empresa; retorna `0` (sin permiso), `-1` (nombre usuario duplicado), `-2` (correo duplicado), o `SCOPE_IDENTITY()` (nuevo Id). **NO asigna rol ni PersonaId** — solo inserta/actualiza la fila `Usuarios`.
- **SP `GuardarOActualizarUsuario`** (dump `:3256-3333`): variante self-service/empresa, sin validación de admin. Tampoco toca rol ni Persona.
- **La asignación de rol es POSTERIOR y en MVC**, no en el SP: `UserController.cs:256-272` lee `Request.Form["RolId"]`, obtiene las filas junction con `_rolService.ObtenerUsuarioRolesPorUsuario(usuarioGuardado.Id)`, las elimina una a una con `_rolService.EliminarRolUsuario(usuarioRol.Id)` y luego `_rolService.AsignarRolUsuario(usuarioGuardado.Id, rolId)`.
- **SP `AsignarRolUsuario`** (dump `:998-1068`): exige que `@AsignadoPor` sea `Administrador` de la misma empresa, que el rol destino pertenezca a la empresa (`r.EmpresaId = @EmpresaId`), y devuelve `-1` si el usuario ya tiene ese rol activo. Cadena WebApi: `DbWrapper.Rol.cs:167-209` → MVC `RolService.AsignarRolUsuario` (`Services/RolService.cs:44-47`) → `HttpClientConnection.Rol.AsignarRolUsuario` (`DAL/HttpClientConnection.Rol.cs:45-61`, POST `api/Rol/Asignar`).

⇒ **Punto de inserción natural**: (a) extender el SP `GuardarOActualizarUsuarioAdmin` (y/o `GuardarOActualizarUsuario`) para aceptar `@PersonaId` y persistirlo en `Usuarios.PersonaId`; (b) en `UserController.GuardarOActualizarUsuarioAdmin` (MVC), cuando el formulario traiga `PersonaId`, tras guardar asignar el rol **"Usuario"** (buscar el `Rol` con `Nombre='Usuario'` y `EmpresaId = tokenCookie.EmpresaID`). Alternativa: hacer el rol-link en un solo SP transaccional nuevo.

**UI de administración de usuarios**: `Views/User/Users.cshtml` (catálogo de usuarios, edición/alta). El DDL de rol se manda como `RolId` (hidden/DDL). Aquí se agregaría el selector/búsqueda de `Persona`.

### 2. Login: claims del token OAuth e identidad en MVC

- **Claims que emite el token** — `ServiceDeskDESIWebApi/App_Start/Startup.cs:186-227` (`GrantResourceOwnerCredentials`):
  - `ClaimTypes.Name` = `NombreUsuario` (`:206`)
  - `"usuarioId"` = `usuario.Id` (`:207`)
  - `"empresaId"` = `usuario.EmpresaId` (`:208-211`)
  - `ClaimTypes.Role` = `rol.Nombre` por cada rol (`:213-221`, vía `ObtenerRolesPorUsuario`)
  - **NO hay `personaId`** en el token.
- **MVC resuelve el usuario logueado** vía `SessionHelper.GetSessionUser()` (`Helpers/SessionHelper.cs:43-58`) → deserializa `TokenCookie` desde el ticket de `FormsAuthentication`. `TokenCookie.cs:11-16` → `Token, UserID, EmpresaID, UserName, ProfileImage, UserAvatar`. Se construye en `HomeController.LogIn:171-181`.
- `BaseController` (`Controllers/BaseController.cs:22-27`) expone `tokenCookie` a todos los controladores.

⇒ **Para resolver el `PersonaId` de un usuario básico logueado** hay dos vías (ver Approaches):
  1. Añadir claim `personaId` en `Startup.cs` (requiere que `AutenticarUsuario` devuelva `u.PersonaId`).
  2. Obtener `Usuario` por `tokenCookie.UserID` con `ObtenerUsuarioPorId` (el MVC ya lo hace en `UserController.MyProfile:46`) y leer `PersonaId` — requiere añadir `PersonaId` a `Usuario`/`UsuarioDTO` y al `SELECT u.*` del SP `ObtenerUsuarioPorId` (dump `:5078-5096`, ya retorna `u.*`, así que el mapeo `LlenarEntidad<UsuarioDTO>` lo tomaría automáticamente).

### 3. Estructura de menús (sidebar) y cómo agregar "Mis Activos"

- **Sidebar**: `Views/Home/MenusUser.cshtml` (51 líneas). Recibe `List<Pagina>`; separa `Tipo=="Menu"` con `PermisosPadreId==null` (padres) y `Tipo=="SubMenu"` con `PermisosPadreId==menu.Id` (hijos). Un menú sin hijos se renderiza como enlace directo a `@menu.Direccion` (`:43-48`); los hijos enlazan a `@sub.Direccion` (`:29`).
- **Origen de datos**: `HomeController.MenusUser:104-113` → `httpClientConnection.ObtenerPaginasPorUsuario()` (`DAL/HttpClientConnection.Pagina.cs:15-18`, GET `api/Pagina/List`) → WebApi `PaginaController.ObtenerPaginasPorUsuario` (`Controllers/PaginaController.cs:27-33`) → `PaginaService` (`Services/PaginaService.cs:18-40`) → `DbWrapper.Paginas.cs:13-39` → **SP `ObtenerPaginasPorUsuario`** (dump `:4372-4402`).
- **SP `ObtenerPaginasPorUsuario`** resuelve el menú **por ROL**, vía `RolPaginaAccion` (join `UsuarioRol → Rol → RolPaginaAccion → Pagina`, filtro `rpa.PuedeLeer=1`), NO por `UsuarioPagina`. Devuelve las páginas con acceso + sus padres.
- `Pagina` (entidad `Seguridad/Pagina.cs:5-15`): `Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB`. Tabla dump `:273-291` (nota: `NombreVisible` se añadió por migración `personal-administracion`, no está en el dump).
- La tabla `UsuarioPagina` y el SP `ValidarAccesoPagina` (dump `:5156-5177`) existen pero **el sidebar NO los usa** (legado/per-usuario).

⇒ **Para agregar "Mis Activos" visible solo a usuarios básicos**: dado que la visibilidad es **por rol**, basta con:
  1. Insertar una fila `Pagina` ("Mis Activos", `Tipo='Menu'` con `Direccion` apuntando al nuevo action MVC —p. ej. `/Home/MisActivos`— o `Tipo='SubMenu'` bajo un padre existente, `Logo`, `OrdenB`).
  2. Insertar `RolPaginaAccion` (`PuedeLeer=1`, resto 0) para **el rol "Usuario" de cada empresa**. Como el rol "Usuario" es por-empresa (IDs 3 global y 31 por-empresa según facts conocidos), hay que insertar la fila de permisos para cada rol con `Nombre='Usuario'`.
  - **Vía admin existente**: `GuardarPermisosRol` / `GuardarPermisosRolMasivo` (SP `GuardarPermisosRol` dump `:3495-3559`; `PermisosService` `Services/PermisosService.cs:131-223`) o `InsertarRolPaginaAccion` (dump `:3607-3656`).
  - **Provisión de nuevas empresas**: hoy `GuardarNuevaEmpresa` crea roles vía `GuardarRolParaNuevaEmpresa` (dump `:3571-3586`) y `PlantillaRol` (dump `:583-588`). **No hay** un SP equivalente "para nueva empresa" de `RolPaginaAccion` que asigne automáticamente páginas al rol "Usuario"; los permisos por página se gestionan desde la pantalla `Security/Permisos` (admin). ⇒ la nueva página + permiso al rol "Usuario" debe cubrirse en migración para empresas existentes y en el provisioning para futuras (punto abierto).

### 4. `ObtenerActivosPorPersona` existente (reutilizable para "Mis Activos")

Cadena completa ya implementada (cambio `asignacion-activos`):
- SP `ObtenerActivosPorPersona` (`openspec/changes/asignacion-activos/migration.sql:90-107`) → retorna activos vigentes (`FechaFin IS NULL`, `Estatus=1`, `EmpresaId` del usuario) + `ActivoNombre`, `ActivoSerial`.
- WebApi: `Controllers/PersonaActivoController.cs:28-35` → `GET ActivosPorPersona/{personaId}` con **`[Permiso("Personas", "Leer")]`**; `Services/PersonaActivoService.cs:27-50`; `DAL/DbWrapper.PersonaActivo.cs:14-43`.
- MVC: `DAL/HttpClientConnection.PersonaActivo.cs:15-18`; `Services/PersonaActivoService.cs:19-22`; `Controllers/CatalogsController.cs:798-803`.
- DTO: `Catalogos/PersonaActivoDTO.cs:5-6` (`ActivoNombre`, `ActivoSerial`).

⚠️ **Clave para "Mis Activos"**: el endpoint `ActivosPorPersona` exige `[Permiso("Personas","Leer")]`. Un "usuario básico" (rol "Usuario") **no tiene** ese permiso (ni debe tenerlo, o vería el catálogo de Personas). Por tanto, **no es reutilizable tal cual**: se recomienda un endpoint nuevo (p. ej. `GET api/PersonaActivo/MisActivos`) que derive el `PersonaId` desde `Usuarios.PersonaId` del usuario autenticado (no desde parámetro), y sin exigir `Personas/Leer`. El SP `ObtenerActivosPorPersona` y el `PersonaActivoDTO` sí son reutilizables tal cual.

### 5. Precedente de relaciones Persona↔Usuario / joins 1:1

- `Area.UsuarioResponsableId` (`Catalogos/Area.cs:14`, `long?`) — FK nullable 1:1 hacia `Usuarios` (responsable del área). **Este es el precedente directo** a seguir para `Usuarios.PersonaId BIGINT NULL` (columna nullable, sin propiedad de navegación; mapeo por `LlenarEntidad<T>` case-insensitive).
- `CategoriaResponsable.UsuarioId` (`Catalogos/CategoriaResponsable.cs:9`, `long`) — M:N Categoría↔Usuario (responsables). Cadena DAL: `DbWrapper.CategoriaResponsable.cs:77-129` (usa `@UsuarioId` como FK a Usuarios).
- `Usuario.AreaId` (`Autenticacion/Usuario.cs:22`) — FK inversa.
- Enlaces blandos por correo: `Area.Correo` (`Area.cs:13`) y `Persona.Correo` (`Persona.cs:15`); **no** hay join automático por correo en el código (se usa correo solo para notificaciones, p. ej. `PersonaActivoService`).
- No existe hoy ningún join Persona↔Usuario. La relación es nueva; seguir el estilo de `Area.UsuarioResponsableId`.

### 6. Punto de validación en SP `AsignarActivoPersona` (dónde inyectar el check)

Cuerpo actual del SP (fuente: `openspec/changes/asignacion-activos/migration.sql:34-58`):

```sql
CREATE PROCEDURE [dbo].[AsignarActivoPersona] (@PersonaId BIGINT, @ActivoId BIGINT, @Usuario NVARCHAR(25))
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @EmpresaId BIGINT;
  SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
  IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

  IF NOT EXISTS(SELECT 1 FROM Persona WHERE Id = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
      BEGIN SELECT 0; RETURN; END
  IF NOT EXISTS(SELECT 1 FROM Activo WHERE Id = @ActivoId AND Estatus = 1 AND EmpresaId = @EmpresaId)
      BEGIN SELECT 0; RETURN; END
  IF EXISTS(SELECT 1 FROM PersonaActivo WHERE ActivoId = @ActivoId AND FechaFin IS NULL AND Estatus = 1)
      BEGIN SELECT -1; RETURN; END  -- -1 = activo ya asignado

  INSERT INTO PersonaActivo (...) VALUES (...);
  SELECT SCOPE_IDENTITY();
END
```

- **Dónde inyectar**: inmediatamente después del check de existencia de `Persona` (línea ~48) y antes del check de `Activo`, agregar:
  ```sql
  IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE PersonaId = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
      BEGIN SELECT -2; RETURN; END  -- -2 = persona sin usuario vinculado
  ```
- **Códigos actuales**: `0` = fallo genérico, `-1` = activo ya asignado. Se propone **`-2`** = "persona sin usuario vinculado".
- **Impacto en C#**: `DbWrapper.PersonaActivo.cs:73-114` (`AsignarActivoPersona`) hoy interpreta `<= -1` como "ya asignado" y `<= 0` como fallo genérico. Habrá que añadir una rama `resultadoLong == -2` para devolver mensaje específico ("La persona no tiene un usuario vinculado"). La validación **debe vivir dentro del SP** (server-side), para que aplique a cualquier llamador, no solo a la UI.
- El SP ya se despliega con DROP/CREATE en `migration.sql`; el cambio debe REESCRIBIR ese SP (o el nuevo `migration.sql` del change debe hacer DROP/CREATE del mismo) cuidando no romper la extensión aditiva de confirmación.

### 7. Confirmación BD: `PersonaId` y constraint de `UsuarioRol`

- `Usuarios` **NO tiene `PersonaId`** (dump `:540-563`; columnas: `Id, NombreUsuario, Contrasena, ImagenPerfil, Correo, Nombre, Apellido, Celular, ..., SucursalId, Firma, RFC, AreaId, EmpresaId`).
- `UsuarioRol` **NO tiene constraint unique** sobre `(UsuarioId, RolId)` — solo PK sobre `Id` (dump `:520-533`). La unicidad activa la garantiza el SP `AsignarRolUsuario` (dump `:1049-1060`, `IF EXISTS ... Estatus=1 → SELECT -1`). ⇒ Asignar rol "Usuario" **no duplicará** si la asignación pasa por el SP (salvo que la fila previa esté con `Estatus=0`).
- `Rol` en la BD real tiene `EmpresaId` (añadida por `tenant-estructural`; el dump `:341-355` está **desactualizado** y no la muestra, como tampoco muestra `EmpresaId` en `Persona`). El SP `AsignarRolUsuario` valida `r.EmpresaId = @EmpresaId` (dump `:1038-1047`).
- `Rol.PuedeAtenderTickets` (dump `:350`): el rol "Usuario" tiene `PuedeAtenderTickets=0` (PlantillaRol dump `:588`).

## Affected Areas

| Archivo | Por qué se afecta |
|---|---|
| `openspec/changes/vinculacion-persona-usuario/migration.sql` (nuevo) | `ALTER TABLE Usuarios ADD PersonaId BIGINT NULL` + FK + índice único filtrado; reescritura de `GuardarOActualizarUsuarioAdmin`/`GuardarOActualizarUsuario` (aceptar `@PersonaId`); reescritura de `AsignarActivoPersona` (check `-2`); INSERT `Pagina` "Mis Activos" + `RolPaginaAccion` al rol "Usuario". |
| `ServiceDeskDESIEntities/Autenticacion/Usuario.cs` | añadir `PersonaId` (`long?`). |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.Autenticacion.cs` | `GuardarOActualizarUsuarioAdmin`/`GuardarOActualizarUsuario` incluir `PersonaId` en el objeto de parámetros. |
| `ServiceDeskDESIWebApi/DAL/DbWrapper.PersonaActivo.cs` | rama `-2` en `AsignarActivoPersona`; posible método `ObtenerActivosPorUsuario`/`ObtenerPersonaIdPorUsuario`. |
| `ServiceDeskDESIWebApi/Services/PersonaActivoService.cs` | nuevo `ObtenerMisActivos(usuario)` que derive `PersonaId`. |
| `ServiceDeskDESIWebApi/Controllers/PersonaActivoController.cs` | endpoint nuevo `GET MisActivos` (sin `[Permiso("Personas")]`). |
| `ServiceDeskDESIMVC/Controllers/UserController.cs` | en `GuardarOActualizarUsuarioAdmin`, capturar `PersonaId` y asignar rol "Usuario". |
| `ServiceDeskDESIMVC/Views/User/Users.cshtml` | selector de Persona en el alta/edición de usuario. |
| `ServiceDeskDESIMVC/Controllers/HomeController.cs` (o nuevo controlador) | acción/vista `MisActivos`. |
| `ServiceDeskDESIMVC/Views/...` (nueva vista `MisActivos`) + `ServiceDeskDESIMVC.csproj` | nueva vista y `<Compile Include>`/`<Content Include>` si aplica (csproj legado). |
| `ServiceDeskDESIWebApi/App_Start/Startup.cs` | (opcional) añadir claim `personaId`. |
| `ServiceDeskDESIEntities/Catalogos/PersonaActivoDTO.cs` | (opcional) enriquecer columnas para la vista. |

## Approaches

1. **Relación 1:1 vía `Usuarios.PersonaId` (RECOMENDADA)** — `Usuarios.PersonaId BIGINT NULL` + FK a `Persona` + índice único filtrado (`WHERE PersonaId IS NOT NULL`) para "un usuario básico por persona". "Usuario básico" = `Usuarios.PersonaId IS NOT NULL` (sin flag nuevo, como pide el requisito).
   - Pros: mínimo esquema, coherente con `Area.UsuarioResponsableId`; deducible sin flag; mapeo automático con `LlenarEntidad`.
   - Cons: hay que enriquecer los SPs de guardado y los `SELECT u.*` para exponer `PersonaId`; requiere decidir quién y cuándo lo asigna (UI de usuarios).
   - Esfuerzo: Media.

2. **Tabla intermedia `PersonaUsuario` (M:N con constraint de unicidad)** — tabla junction dedicada.
   - Pros: historial/soft-delete separado; sin tocar `Usuarios`.
   - Cons: más infraestructura (tabla + entidad + DAL + SPs) para una relación que el requisito define 1:1; rompe el patrón simple del repo.
   - Esfuerzo: Alta.

3. **Flag `EsUsuarioBasico` en `Usuarios`** — alternativa descartada por el propio requisito ("deduced, NO flag").
   - Cons: flag redundante con `PersonaId`; riesgo de estados inconsistentes.
   - Esfuerzo: Media.

**Resolución del `PersonaId` del logueado (para "Mis Activos"):** opción recomendada (A) añadir `PersonaId` a `Usuario`/`UsuarioDTO` + exponerlo vía `ObtenerUsuarioPorId`/`AutenticarUsuario` (`SELECT u.*` ya lo incluiría) y leerlo en MVC desde `tokenCookie.UserID`; opción (B) añadir claim `personaId` en `Startup.cs` (requiere tocar el flujo OAuth).

## Recommendation

1. **Esquema**: `Usuarios.PersonaId BIGINT NULL` + FK a `Persona(Id)` + índice único filtrado sobre `PersonaId` (`WHERE PersonaId IS NOT NULL`). Sin tabla nueva, sin flag.
2. **Vinculación**: en el alta/edición de usuario administrador (SP `GuardarOActualizarUsuarioAdmin` + `UserController.GuardarOActualizarUsuarioAdmin` + `Users.cshtml`), agregar selector de Persona; al vincular, asignar el rol "Usuario" de la empresa (SP `AsignarRolUsuario` ya existente, buscando `Rol.Nombre='Usuario' AND EmpresaId=@EmpresaId`).
3. **Validación de asignación de activo**: en `AsignarActivoPersona`, nuevo código de retorno `-2` ("persona sin usuario vinculado") + rama en `DbWrapper.PersonaActivo.AsignarActivoPersona`.
4. **"Mis Activos"**: nuevo endpoint `GET api/PersonaActivo/MisActivos` (sin `[Permiso("Personas")]`) que derive `PersonaId` desde `Usuarios.PersonaId` del usuario autenticado; reutilizar SP `ObtenerActivosPorPersona` + `PersonaActivoDTO`. Nueva vista MVC enlazada desde una nueva fila `Pagina` "Mis Activos" + `RolPaginaAccion` (`PuedeLeer=1`) solo al rol "Usuario".

## Risks

- **Drift del dump**: `basededatosservicedesk.txt` NO refleja `EmpresaId` en `Persona`/`Rol` (migración `tenant-estructural`). Al escribir la migración, basarse en el esquema real (hosted), no en el dump.
- **Roles "Usuario" por-empresa**: hay múltiples roles `Nombre='Usuario'` (3 global, 31 y otros por-empresa). La migración de `RolPaginaAccion` para "Mis Activos" debe insertar permisos para **todos** los roles "Usuario" existentes; y el provisioning de empresas nuevas debe cubrirse (no existe SP "para nueva empresa" de `RolPaginaAccion`).
- **`[Permiso("Personas","Leer")]`**: reutilizar `ActivosPorPersona` tal cual expondría el catálogo de Personas a un usuario básico si se le otorgara ese permiso. Usar endpoint nuevo.
- **Código de retorno**: introducir `-2` requiere tocar la interpretación en `DbWrapper.PersonaActivo.AsignarActivoPersona` (`<= -1` hoy significa "ya asignado"); si no, el mensaje será genérico/incorrecto.
- **`UsuarioRol` sin unique**: la protección contra duplicados es solo del SP; si el flujo de vinculación asigna rol por fuera del SP, puede duplicarse.
- **csproj legado**: cualquier `.cs` nuevo en los proyectos requiere `<Compile Include>` manual (no SDK-style); las vistas `.cshtml` nuevas requieren su entrada en `ServiceDeskDESIMVC.csproj`.

## Ready for Proposal

Sí. Pasar a **sdd-propose** con la Opción 1 (columna `Usuarios.PersonaId` + índice único filtrado), el endpoint nuevo "Mis Activos" y el código `-2` en `AsignarActivoPersona`, dejando explícitas las preguntas de alcance abajo.

## Scope Questions (para el proposal / usuario)

1. **Dónde se vincula** — ¿solo desde el catálogo de usuarios administrador (`Users.cshtml`), o también se permite crear el usuario desde el catálogo de Personas (`Persona.cshtml`)? ¿O ambas direcciones?
2. **Reutilización de credenciales** — al vincular Persona→Usuario, ¿se reutiliza `Persona.Correo` como `Usuarios.Correo`/`NombreUsuario` (evita correo duplicado en `GuardarOActualizarUsuarioAdmin`, que devuelve `-2`)? ¿Qué valor toma `NombreUsuario`?
3. **Contraseña del usuario básico** — ¿se genera temporal y se envía por correo (patrón `EnviarCorreoNuevoUsuario` `AutenticacionService.cs:504-546`) o se deja al admin?
4. **"Mis Activos" como menú** — ¿`Tipo='Menu'` directo en la barra lateral, o `SubMenu` bajo un padre existente? ¿Visible para todos los roles o solo para el rol "Usuario" (recomendado: solo "Usuario")?
5. **Datos del correo de confirmación** — al vincular Persona↔Usuario, el flujo de asignación de activo ya envía correo a `Persona.Correo`; ¿debe cambiar el destinatario a `Usuarios.Correo` o permanecer igual?
6. **Rol "Usuario" y permisos extra** — ¿el usuario básico debe ver también "Mi Perfil" o alguna página adicional (hoy el rol "Usuario" solo vería las páginas con `RolPaginaAccion`)? ¿Se le asigna rol "Usuario" exclusivamente o junto a otros?
