# Revisión del Modelo de Entidades — ServiceDeskDESIEntities

- **Change**: `entities-review`
- **Fase**: explore (revisión del proyecto de entidades, solo lectura)
- **Fecha**: 2026-08-18
- **Origen**: análisis del proyecto `ServiceDeskDESIEntities` (POCOs compartidos), cruzado con `database-review` (21 tablas), `webapi-review` (mapeo por reflection) y `mvc-review`.

## Estado actual

`ServiceDeskDESIEntities` es una **biblioteca de clases .NET Framework 4.8** (OutputType=Library, sin paquetes NuGet, solo referencias al framework) que contiene los POCOs compartidos entre el MVC y el WebApi. Tiene **27 clases en 26 archivos .cs** (ignorando bin/obj): 1 clase base (`BaseObject`), 18 entidades de dominio, y 8 clases auxiliares/DTO (`ModelResponse`, `Token`, `TokenCookie`, `RestablecerContraseniaRequest`, y 4 DTOs de permisos dentro de `PermisoRequest.cs`). Organización por carpetas: raíz, `Autenticacion/`, `Catalogos/`, `Seguridad/`, `Tickets/`.

Características clave del modelo:
- **Todos los 18 POCOs de dominio heredan de `BaseObject`**, que centraliza `Id` (`long`), `CreadoPor`, `FechaCreacion`, `ModificadoPor`, `FechaModificacion` y `Estatus` (soft-delete). Ninguna entidad duplica estos campos.
- **Sin atributos de ningún tipo**: cero `DataAnnotations` (ni `Required`, ni `StringLength`, ni `Display`), cero `[JsonProperty]`, cero `[Table]`/`[Column]`, y **cero enums en toda la solución**. Son POCOs 100% "dumb".
- **Las relaciones FK se modelan como propiedades de navegación** (`Usuario.Empresa`, `Ticket.Area`, `Ticket.Categoria`, etc.), no como escalares `*Id`. Esta decisión no coincide con el esquema de la BD (que usa columnas `*Id`) ni con el mapeo por reflection del WebApi.

## Resumen del modelo de entidades

### Correspondencia entidad ↔ tabla

**Entidades de dominio (18)** — todas heredan `BaseObject`:

| Entidad | Tabla | Correspondencia de campos |
|---|---|---|
| `Usuario` (Autenticacion) | `Usuarios` | ⚠️ `Sucursal`/`Area`/`Empresa` (nav) ↔ `SucursalId`/`AreaId`/`EmpresaId`; el resto exacto |
| `Rol` | `Rol` | ✅ exacto (incl. `PuedeAtenderTickets` bool↔bit) |
| `Pagina` | `Pagina` | ✅ casi exacto: `PermisosPadreId` es escalar `long?` (coincide con la columna); `OrdenB int` ↔ `int NULL` |
| `Empresa` | `Empresa` | ✅ exacto (13 campos, incl. fechas de vigencia y `EsPeriodoPrueba`) |
| `Compania` | `Compania` | ✅ exacto (4 campos) |
| `Area` | `Area` | ✅ exacto |
| `Sucursal` | `Sucursal` | ✅ exacto |
| `Puesto` | `Puesto` | ✅ exacto |
| `Persona` | `Persona` | ⚠️ `Puesto` (nav) ↔ `PuestoId` |
| `Categoria` | `Categoria` | ⚠️ `CategoriaPadre`/`Area` (nav) ↔ `CategoriaPadreId`/`AreaId` |
| `CategoriaResponsable` | `CategoriaResponsable` | ⚠️ `Categoria`/`Usuario` (nav) ↔ `CategoriaId`/`UsuarioId` |
| `Marca` | `Marca` | ✅ exacto |
| `Modelo` | `Modelo` | ⚠️ `Marca` (nav) ↔ `MarcaId` |
| `Activo` | `Activo` | ⚠️ `TipoActivo`/`Marca`/`Modelo` (nav) ↔ `TipoActivoID`/`MarcaID`/`ModeloID`; `FechaCompra DateTime` ↔ `datetime NULL` |
| `TipoActivo` | `TipoActivo` | ✅ exacto |
| `TicketEstatus` | `TicketEstatus` | 🔴 **`Id` `long` (BaseObject) ↔ `Id int` (BD)** — mismatch de tipo |
| `Ticket` | `Ticket` | ⚠️ `Area`/`Categoria`/`Subcategoria`/`TicketEstatus` (nav) ↔ `AreaId`/`CategoriaId`/`SubcategoriaId`/`TicketEstatusId`; `Urgencia int` (magic number) |
| `UsuarioPagina` | `UsuarioPagina` | ⚠️ `Usuarios` (plural, nav)/`Pagina` (nav) ↔ `UsuarioID`/`PaginaID` |

**Tablas SIN entidad (3)** — tablas técnicas/junction:
| Tabla | Cómo se maneja hoy |
|---|---|
| `RolPaginaAccion` | Sin entidad. Se usan DTOs (`PermisoRequest`, `GuardarPermisosRequest`, `GuardarPermisosMasivoRequest`) y parámetros escalares en `DbWrapper.Permisos.cs`. |
| `UsuarioRol` | Sin entidad. Solo parámetros escalares (`@UsuarioId`/`@RolId`/`@UsuarioRolId`) en `AsignarRolUsuario`/`EliminarRolUsuario` (`DbWrapper.Rol.cs`). |
| `TokenRecuperacion` | Sin entidad. Se mapea a un objeto **anónimo/dynamic** en `DbWrapper.Autenticacion.cs:535` (`ObtenerTokenRecuperacion`). |

**Clases auxiliares (8, no son tablas):** `ModelResponse`, `Token` (OAuth), `TokenCookie`, `RestablecerContraseniaRequest`, `PermisoRequest`, `ValidarPermisoRequest`, `GuardarPermisosRequest`, `GuardarPermisosMasivoRequest`, `PermisosViewModel`.

## Hallazgos CRÍTICOS

1. **`Usuario.Contrasena` es un campo público y serializable en la entidad, sin protección.** `Usuario.cs:13` declara `public string Contrasena { get; set; }` mapeado a `Usuarios.Contrasena nvarchar(250)`, que contiene **ciphertext Rijndael reversible** (no hash — ver `webapi-review` #4 y `database-review` #3). No hay `[JsonIgnore]` ni ninguna marca: la propiedad viaja en `AutenticarUsuario`, `ObtenerUsuarios`, `ObtenerUsuarioPorId`, `ObtenerUsuarioPorCorreo` y `ObtenerUsuarioPorNombreUsuario` (todas devuelven `u.*`), y se reenvía al MVC, que además la **desencripta y la muestra en el HTML** (`mvc-review` #4). La entidad debería separar el POCO de dominio del DTO de auth y jamás serializar el password (ni encriptado).

2. **`TicketEstatus.Id` es `int` en la BD pero `long` en la entidad (vía `BaseObject`) → rompe el mapeo por reflection.** La tabla `TicketEstatus` es la **única** con `[Id] [int] IDENTITY` (línea 437 del script); las otras 20 usan `bigint`. `TicketEstatus` hereda `BaseObject.Id` (`long`). El SP `ObtenerTicketEstatus` hace `SELECT * FROM TicketEstatus` (línea 4703), y `DbWrapper.Ticket.cs:465` lo mapea con `LlenarEntidad<TicketEstatus>(reader)` **sin mapeo manual**. El reflection (`DbWrapper.cs:38` `item.SetValue(e, reader[j])`) recibe un `Int32` para una propiedad `Int64` → `ArgumentException` (reflection no hace widening numérico). Resultado: **el catálogo de estatus de ticket falla siempre en runtime** (la excepción se traga en el catch y devuelve `IsSuccess=false`). El resto de la capa de datos es inconsistente con esto: `Ticket.TicketEstatusId` se mapea como `int` (`DbWrapper.Ticket.cs:51`), confirmando el mismatch de raíz en la entidad.

## Hallazgos IMPORTANTES

3. **Desajuste estructural: FKs como propiedades de navegación vs columnas `*Id`.** 8 entidades modelan 18 relaciones FK como referencias a objetos (`Usuario.Empresa/Area/Sucursal`, `Ticket.Area/Categoria/Subcategoria/TicketEstatus`, `Categoria.CategoriaPadre/Area`, `Activo.TipoActivo/Marca/Modelo`, `Modelo.Marca`, `Persona.Puesto`, `CategoriaResponsable.Categoria/Usuario`, `UsuarioPagina.Usuarios/Pagina`), mientras la BD tiene columnas escalares `*Id`. Como `LlenarEntidad<T>` empareja **por nombre exacto** (`DbWrapper.cs:35`), estas propiedades de navegación **nunca se mapean automáticamente**: cada método del `DbWrapper` las rellena a mano leyendo **columnas alias** que los SPs deben devolver (`a.Nombre as AreaNombre`, `te.Color as EstatusColor`, `e.NombreComercial as EmpresaNombre`, etc. — confirmado en los SPs). Consecuencias:
   - Duplicación masiva de código de mapeo (el bloque `new Area(){...}/new Categoria(){...}/new TicketEstatus(){...}` se repite **7 veces** solo en `DbWrapper.Ticket.cs`, y 4 veces en `DbWrapper.Autenticacion.cs`).
   - El "contrato" entidad↔SP es implícito y frágil: si un SP no devuelve el alias exacto (`EstatusNombre`, `EmpresaNombreComercial`…), falla en runtime.
   - Nombres de parámetro inconsistentes en escritura: `GuardarOActualizarTicket` envía `@Area`/`@Categoria`/`@Subcategoria` (`DbWrapper.Ticket.cs:149-151`), pero `GuardarOActualizarUsuario` envía `@SucursalId`/`@AreaId`/`@EmpresaId` (`DbWrapper.Autenticacion.cs:243-247`). El mismo concepto de FK se llama distinto según el SP.
   - `ObtenerParametrosSQL` (`DbWrapper.cs:54-74`) refleja el modelo mental "objeto": para una propiedad de navegación extrae su `.Id` y lo manda como `@<Prop>`. Solo funciona porque los SPs fueron escritos para aceptar esos nombres (y no todos).

4. **3 tablas sin entidad (dominio de permisos y recovery sin tipar).** `RolPaginaAccion`, `UsuarioRol` y `TokenRecuperacion` no tienen POCO. Se manejan con DTOs sueltos, parámetros escalares y un `dynamic`/anónimo. Asimetría: el RBAC (páginas, roles, permisos) es el corazón de la seguridad del sistema, pero su entidad principal (`RolPaginaAccion`) no existe como tipo de dominio, mientras que el `PermisosViewModel` de salida sí está modelado. `TokenRecuperacion` se devuelve como objeto anónimo (`DbWrapper.Autenticacion.cs:537-548`), perdiendo tipado y autodocumentación.

5. **NULLabilidad de la BD no reflejada en los tipos.** `BaseObject.Estatus` es `bool` y `FechaCreacion` es `DateTime` (no-nullable), pero **10 tablas** tienen `Estatus bit NULL` y `FechaCreacion datetime NULL` (`Activo`, `Area`, `Marca`, `Modelo`, `Pagina`, `Sucursal`, `TipoActivo`, `Compania`, `UsuarioPagina`, `Usuarios`). También `Activo.FechaCompra` es `DateTime` (no-nullable) vs `datetime NULL`, y `Pagina.OrdenB` es `int` vs `int NULL`. Al mapear una fila con `NULL` en esas columnas, `LlenarEntidad` ejecuta `item.SetValue(e, null)` sobre una propiedad de tipo valor → o lanza excepción o asigna el default (`false`/`DateTime.MinValue`/`0`), perdiendo el `NULL`. En escritura, si el POCO no setea esos campos, serializan como default y no como `NULL`. El modelo de entidades debería declarar `bool?`/`DateTime?`/`int?` donde la columna es nullable.

6. **Cero validación en la capa de entidades (confirma `webapi-review` #13).** No hay `DataAnnotations` ni ninguna metadato de validación: un `Empresa` sin `RFC`, un `Ticket` sin `Titulo`, o un `Usuario` sin `Correo` son estados perfectamente válidos para el tipo. Toda la validación (cuando existe) es `if` manual y duplicada en servicios/controllers. La entidad es el lugar natural para declarar `[Required]`/`[StringLength]` y hoy no aporta nada a ese respecto.

7. **Sin atributos de serialización: el contrato JSON depende de la config del host.** No hay `[JsonProperty]` en ningún POCO. El WebApi aplica `CamelCasePropertyNamesContractResolver` global (`WebApiConfig.cs:25`), así que su JSON sale camelCase. Pero el MVC serializa **los mismos POCOs** con Newtonsoft default (PascalCase) al armar request bodies y al serializar `TokenCookie` dentro de la cookie `FormsAuthentication` — funciona solo porque la deserialización de Newtonsoft es case-insensitive. El "contrato" de nombres JSON es implícito y por capa, no está documentado en el tipo.

8. **`ModelResponse.Response` es `object` (sin tipar) y `IsSuccess` nace en `true` (confirma `mvc-review` #7).** `ModelResponse.cs:11-14` inicializa `IsSuccess = true`, de modo que cualquier camino que olvide setear `false` reporta éxito (peligro en manejo de errores). `Response` es `object`, no `ModelResponse<T>`: obliga al cliente a deserializar dos veces y castear (`DeserializeObject<ModelResponse>` → `Response.ToString()` → `DeserializeObject<T>`), con los `NRE` asociados ya reportados en `mvc-review` #7.

9. **Magic numbers / magic strings sin tipo semántico (confirma `database-review` #18).** `Ticket.Urgencia` es `int` sin enum ni catálogo. `Pagina.Tipo` es `string` con valores "Menu"/"SubMenu" documentados solo en un comentario (`Pagina.cs:9`). **No existe ningún `enum` en toda la solución.** `TicketEstatus.Orden`, `Categoria.Orden` y `Pagina.OrdenB` son `int` de ordenamiento (aceptable). La falta de un `enum Urgencia` propaga el magic number hasta la UI y el SP `ObtenerTicketsPorUrgencia`.

10. **`Compania` vs `Empresa`: dos entidades "empresa" solapadas y sin relación (confirma `database-review` #17).** `Empresa` (13 campos: `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, dirección completa, teléfono, correo, fechas de vigencia, `EsPeriodoPrueba`) y `Compania` (4 campos: `Nombre`, `Acronimo`, `RFC`, `Direccion`) coexisten en el mismo namespace `Catalogos`, ambas con `RFC` y `Direccion` duplicados, sin referencia entre ellas ni FK en la BD. `Compania` no aparece en el flujo de tenant (no se usa en `Usuario` ni en `Empresa`); huele a residuo de un modelo anterior.

## NICE-TO-HAVE

11. **Organización de namespaces inconsistente con el dominio.** `Empresa` y `Compania` (tenants) están en `Catalogos`; `UsuarioPagina` (junction RBAC) está en `Catalogos`; `Usuario` está en `Autenticacion` mientras `Rol` está en `Seguridad`. Las clases auxiliares de infraestructura (`ModelResponse`, `Token`, `TokenCookie`) viven en `Seguridad` aunque no son de seguridad. No sigue la agrupación por dominio de `database-review` (Seguridad/Tenant/Catálogos/Tickets).

12. **`UsuarioPagina.Usuarios` (plural) como nombre de propiedad de navegación.** `public Usuario Usuarios { get; set; }` (`UsuarioPagina.cs:13`) referencia **un único** usuario, con nombre en plural y que además no coincide con la columna `UsuarioID`. Confunde (parece colección) y rompe la convención 1:1 con la tabla.

13. **Nombres heredados confusos.** `Pagina.OrdenB` y `Pagina.PermisosPadreId` copian literalmente los nombres de columnas de la BD (que `database-review` #19 ya marcó como confusos: `OrdenB` = "Orden"+"B"; `PermisosPadreId` es en realidad el padre jerárquico del menú).

14. **Estilo/espaciado inconsistente.** `public class Activo:BaseObject` (sin espacio antes de `:`) vs `public class TipoActivo: BaseObject`; `public  class Compania` (doble espacio); `public class Modelo:BaseObject`. Usings de plantilla sin usar (`System.Collections.Generic`, `System.Linq`, `System.Text`, `System.Threading.Tasks` en casi todos los POCOs) y `using System.Security.Permissions;` huérfano en `Persona.cs:4`.

15. **DTOs de permisos con overlap y en un único archivo.** `PermisoRequest.cs` declara 4 clases (`PermisoRequest`, `ValidarPermisoRequest`, `GuardarPermisosRequest`, `GuardarPermisosMasivoRequest`); `PermisoRequest` y `GuardarPermisosRequest` son casi idénticas (solo difieren en `RolId`). `PermisosViewModel` duplica los 5 flags `PuedeLeer/Crear/Editar/Eliminar/Exportar` que ya están en `PermisoRequest`/`GuardarPermisosRequest`.

16. **`Token` mezcla convenciones de naming.** `access_token`/`token_type`/`expires_in` en snake_case (convención OAuth) junto a `ExpirationDate` en PascalCase (`Token.cs:11-14`). Con el resolver camelCase el JSON queda `access_token` + `expirationDate` mezclado.

17. **`Ticket` no distingue solicitante de agente.** La entidad no tiene `SolicitanteId`/`AgenteId` ni referencia a `Usuario` (hereda la limitación de `database-review` #18). La asignación de agente es un placeholder sin implementar (`mvc-review` #11).

## Fortalezas

- **`BaseObject` centraliza correctamente la auditoría y el soft-delete** (`Id`, `CreadoPor`/`ModificadoPor`, `FechaCreacion`/`FechaModificacion`, `Estatus`), y **las 18 entidades de dominio heredan sin duplicar** ningún campo. Sin excepciones.
- **POCOs realmente limpios y desacoplados**: sin dependencias a EF/NHibernate/Dapper ni paquetes NuGet (a pesar de que el WebApi referencia EF Core que no usa). El proyecto compila solo contra el framework.
- **Compartida de verdad entre MVC y WebApi** (un único assembly), cumpliendo exactamente su propósito: mismo contrato de datos en ambos lados.
- **Campos escalares mayoritariamente 1:1 con las columnas** (nombres en español idénticos a la BD) para las entidades "catálogo" simples (Rol, Empresa, Area, Sucursal, Puesto, Marca, TipoActivo, Compania, Categoria).
- **Intención de separar request/response del dominio**: `RestablecerContraseniaRequest`, `PermisosViewModel` y los DTOs de permisos muestran conciencia de no exponer el POCO completo en todas las operaciones.
- `ModelResponse` es un wrapper de respuesta uniforme (`IsSuccess`/`Message`/`Response`) usado consistentemente en toda la capa de datos.

## Siguientes pasos recomendados (top impacto)

1. **Corregir el tipo de `TicketEstatus.Id`** (o alinear `BaseObject.Id`): la causa raíz del fallo en runtime de `ObtenerTicketEstatus`. Opciones: (a) hacer `TicketEstatus.Id` `int` (rompe la herencia uniforme), (b) cambiar la columna a `bigint` en BD, o (c) —mínimo— castear manualmente en `DbWrapper.Ticket.cs`. Revisar si otras columnas `int`/`bigint` tienen el mismo desajuste.
2. **Decidir el modelado de FKs: navegación vs escalar `*Id`.** Si se queda con navegación, documentar el contrato de alias y **unificar los nombres de parámetro** de escritura (`@Area` vs `@AreaId`). Lo más sano para este stack (ADO.NET + reflection): usar escalares `long? AreaId`, `long? CategoriaId`, etc., y exponer los objetos "lookup" solo en DTOs de lectura. Elimina la duplicación masiva de mapeo manual.
3. **Sacar `Contrasena` del POCO serializable**: no devolverla en respuestas (o marcar `[JsonIgnore]`), migrar a hashing, y separar el DTO de autenticación del `Usuario` de dominio.
4. **Reflejar la NULLabilidad real** en las entidades (`bool? Estatus`, `DateTime? FechaCreacion`, `DateTime? FechaCompra`, `int? OrdenB`) para que el mapeo no rompa con filas que tienen `NULL`.
5. **Añadir validación** (`[Required]`/`[StringLength]`) en las entidades como primer paso, y **tipar** `ModelResponse<T>` + `IsSuccess=false` por defecto.
6. **Crear entidades para `RolPaginaAccion`, `UsuarioRol` y `TokenRecuperacion`** (o formalizar DTOs dedicados) y **resolver `Compania` vs `Empresa`** (eliminar `Compania` si es residuo).
7. **NICE**: introducir `enum` para `Urgencia` (y `Tipo` de página), reorganizar namespaces, renombrar `UsuarioPagina.Usuarios`→`Usuario`, limpiar usings y normalizar formato.
