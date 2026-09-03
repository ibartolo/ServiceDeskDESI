# Revisión del Modelo de Datos — ServiceDeskDESI (SQL Server)

- **Change**: `database-review`
- **Fase**: explore (revisión del script de BD, solo lectura)
- **Fecha**: 2026-08-17
- **Origen**: análisis del script `openspec/basededatosservicedesk.txt`

> Nota: el script real tiene **5.181 líneas** (no 4.919), **107 stored procedures**, **21 tablas**, **67 `ALTER TABLE`** (defaults + FKs), **0 views/triggers/functions** y **28 sentencias `INSERT`** — pero **todas** están DENTRO de los stored procedures. **No existe ningún `INSERT` de seed data a nivel de esquema**: los catálogos (`Pagina`, `TicketEstatus`, `Rol`, etc.) se pueblan 100% desde la aplicación. Compatibilidad `150` (SQL Server 2019).

## Estado actual

Base de datos `db_9c7990_servicedeskdesi` (SQL Server 2019) para un service desk multi-empresa. Acceso exclusivo vía stored procedures parametrizados (ADO.NET crudo desde `BaseDbWrapper`). El modelo gira en torno a dos conceptos centrales: (1) **tenant** = `Empresa`, que solo está anclada a `Usuarios.EmpresaId`; y (2) **multi-tenancy por inferencia**: ninguna tabla de dominio tiene columna `EmpresaId` — la pertenencia se deduce uniendo `tabla.CreadoPor = Usuarios.NombreUsuario`. El período de prueba se modela con `Empresa.FechaVigenciaInicio`, `FechaVigenciaFin` y `EsPeriodoPrueba`. No hay views, triggers ni funciones; toda la lógica vive en 107 procedures (muchos clones).

## Resumen del modelo de datos

### Las 21 tablas agrupadas por dominio

**Seguridad / usuarios / RBAC (7)**
| Tabla | Rol | Notas |
|-------|-----|-------|
| `Usuarios` | Usuarios (ancla de tenant) | `EmpresaId` (nullable, **sin FK**), `Contrasena nvarchar(250)` (ciphertext reversible), `CreadoPor/FechaCreacion/...` |
| `Rol` | Roles | `PuedeAtenderTickets bit` |
| `UsuarioRol` | M:N usuario↔rol | |
| `Pagina` | Catálogo de páginas/menú | `PermisosPadreId` (self-ref), `Direccion`, `OrdenB` |
| `RolPaginaAccion` | Permisos rol→página→acciones | `PuedeLeer/Crear/Editar/Eliminar/Exportar` |
| `UsuarioPagina` | Acceso directo usuario→página (legacy) | Redundante con `RolPaginaAccion` |
| `TokenRecuperacion` | Tokens de recuperación de contraseña | `Usado bit`, `FechaExpiracion` |

**Tenant / empresa (2)**
| Tabla | Rol | Notas |
|-------|-----|-------|
| `Empresa` | Tenant | `FechaVigenciaInicio/Fin`, `EsPeriodoPrueba`, `Estatus` |
| `Compania` | ¿Segunda entidad de empresa? | **Sin FK a Empresa, sin EmpresaId** — redundante/ambigua |

**Organización / catálogos (6)**
| Tabla | Rol | Notas |
|-------|-----|-------|
| `Area` | Departamentos | sin `EmpresaId` (tenant vía `CreadoPor`) |
| `Sucursal` | Sucursales | sin `EmpresaId` |
| `Puesto` | Puestos de trabajo | sin `EmpresaId` |
| `Persona` | Directorio de personas (no son usuarios) | `PuestoId` FK |
| `Categoria` | Categorías (self-ref `CategoriaPadreId`) | `AreaId` FK |
| `CategoriaResponsable` | Agente responsable por categoría | `EsPrincipal bit` (sin constraint único) |

**Tickets / activos (6)**
| Tabla | Rol | Notas |
|-------|-----|-------|
| `Ticket` | Tickets | `AreaId/CategoriaId/SubcategoriaId`, `Urgencia int` (magic number), `TicketEstatusId` — **sin EmpresaId** |
| `TicketEstatus` | Catálogo de estatus | global (sin tenant) |
| `Activo` | Activos/inventario | `TipoActivoID/MarcaID/ModeloID` |
| `TipoActivo` | Catálogo tipo de activo | |
| `Marca` | Catálogo marcas | |
| `Modelo` | Catálogo modelos | `MarcaId` FK |

### Claves foráneas clave

- Declaradas (19 FKs): `Activo→Marca/Modelo/TipoActivo`, `Categoria→Area/CategoriaPadre`, `CategoriaResponsable→Categoria/Usuarios`, `Modelo→Marca`, `Persona→Puesto`, `RolPaginaAccion→Pagina/Rol`, `Ticket→Area/Categoria/Subcategoria(Categoria)/TicketEstatus`, `TokenRecuperacion→Usuarios`, `UsuarioPagina→Pagina/Usuarios`, `UsuarioRol→Rol/Usuarios`, `Usuarios→Area/Sucursal`.
- **AUSENTE (clave): no existe `FK_Usuarios_Empresa`** — `Usuarios.EmpresaId` es un `bigint NULL` sin integridad referencial. Tampoco hay FK de `Compania→Empresa` ni `EmpresaId` en ninguna tabla hija.

### Patrón de auditoría y soft-delete

- Campos de auditoría (`CreadoPor/FechaCreacion/ModificadoPor/FechaModificacion`) presentes en las 21 tablas (consistente). `CreadoPor`/`ModificadoPor` guardan el **`NombreUsuario` (string)**, no el `Id`.
- Soft-delete con `Estatus bit`, pero **inconsistente**: `NOT NULL` en ~11 tablas y `NULL` en ~10 (`Activo`, `Area`, `Marca`, `Modelo`, `Pagina`, `Sucursal`, `TipoActivo`, `Compania`, `UsuarioPagina`, `Usuarios`). Un `Estatus NULL` no se trata como borrado de forma uniforme.

## Hallazgos CRÍTICOS

1. **Aislamiento de tenant frágil: se basa en `CreadoPor` (string) y colapsa con colisión de `NombreUsuario`.** Ninguna tabla de dominio (`Ticket`, `Area`, `Sucursal`, `Activo`, `Categoria`, `Rol`, …) tiene `EmpresaId`. El filtro es siempre `INNER JOIN Usuarios u ON <tabla>.CreadoPor = u.NombreUsuario` + `u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario)`. Como `NombreUsuario` **no es único global** (solo se valida dentro de la misma empresa en `GuardarOActualizarUsuarioAdmin`), dos empresas pueden tener un usuario `admin`/`juan` idéntico. En `ObtenerTickets`/`ObtenerActivos`/etc., un ticket creado por `admin` de la empresa A también hace JOIN con el `admin` de la empresa B, y al filtrar `u.EmpresaId = B` **el ticket de A aparece para los usuarios de B**. Además, si el `NombreUsuario` se renombra o se borra lógicamente, los registros quedan huérfanos (invisibles o reasignados).

2. **Procedures de lectura SIN filtro multi-tenant (fuga de datos entre empresas).** Devuelven datos de TODAS las empresas sin validar la identidad:
   - `ObtenerModelos` → todos los modelos de todas las empresas.
   - `ObtenerUsuarioPagina` y `ObtenerUsuarioPaginaPorId` → todas las asignaciones usuario↔página.
   - `ObtenerUsuarioPorCorreo` y `ObtenerUsuarioPorNombreUsuario` → devuelven `u.*` (incluida `Contrasena`) de cualquier usuario por correo/nombre.
   - `ObtenerEmpresas`, `ObtenerEmpresasPorPeriodoPrueba`, `ObtenerEmpresaPorRFC` → todas las empresas (aceptable solo para admin/billing, pero sin protección).
   - `EliminarTicket` → borra cualquier ticket por `Id` **sin validación de propiedad ni empresa** (IDOR).

3. **Contraseñas reversibles y devueltas en las respuestas.** `AutenticarUsuario` compara `u.Contrasena = @Contrasena` (ciphertext, no hash) y devuelve `u.*`. `ObtenerUsuarios`, `ObtenerUsuarioPorId`, `ObtenerUsuarioPorCorreo`, `ObtenerUsuarioPorNombreUsuario` también devuelven `u.*` con `Contrasena`. La BD no hace hashing: confirma lo ya reportado en `webapi-review` (Rijndael con clave hardcodeada).

4. **Cero transacciones y cero manejo de errores en los 107 procedures** (no hay `BEGIN TRAN/COMMIT/ROLLBACK` ni `TRY/CATCH`). El registro de una empresa nueva son **8+ llamadas separadas** (`GuardarNuevaEmpresa` → `GuardarRolParaNuevaEmpresa` → `GuardarNuevaAreaParaEmpresa` → `GuardarNuevaSucursalParaEmpresa` → `GuardarOActualizarUsuario` → `AsignarRolUsuarioParaNuevaEmpresa` → `InsertarUsuarioPaginaParaNuevaEmpresa` × N, …). Si falla un paso, queda una **empresa a medio registrar** (sin rol/áreas/páginas) y sin `ROLLBACK`. La integridad depende de un `TransactionScope` en la capa de aplicación (ADO.NET crudo → riesgo de escalada a MSDTC).

5. **El período de prueba NO se hace cumplir.** `AutenticarUsuario` no valida `FechaVigenciaFin >= GETDATE()` ni `EsPeriodoPrueba`; una empresa con prueba vencida sigue autenticando y operando. No hay job/flag que desactive trials expirados.

## Hallazgos IMPORTANTES

6. `GuardarRolParaNuevaEmpresa` inserta el rol **sin `PuedeAtenderTickets`** (queda en 0 por defecto). El rol `Administrador` de la empresa nueva **no puede atender tickets**, y `GuardarOActualizarCategoriaResponsable` exige un rol con `PuedeAtenderTickets = 1` para asignar agentes → una empresa nueva no puede asignar responsables y el flujo de tickets queda roto.

7. **Dos sistemas de permisos paralelos y en conflicto**: `RolPaginaAccion` (rol→página→acciones, usado por `ValidarPermisoUsuario`, `ObtenerPermisosPorUsuario`) vs `UsuarioPagina` (legacy, usado por `ValidarAccesoPagina`). Dos fuentes de verdad para "¿puede este usuario entrar a esta página?".

8. **Sin índices no-cluster.** Solo PKs clusterizados. Las columnas más consultadas (`CreadoPor`, `EmpresaId`, `AreaId`, `CategoriaId`, `TicketEstatusId`, `FechaCreacion`) no están indexadas, y cada consulta repite el subquery correlacionado `(SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)`.

9. **Sin paginación** en ningún listado (`ObtenerTickets` devuelve todos los tickets de la empresa). `SELECT *`/`SELECT t.*` generalizado.

10. `AsignarRolUsuario` **valida mal la pertenencia del rol**: comprueba que el rol esté ya *asignado* a alguien de la empresa (`EXISTS ... FROM UsuarioRol ... WHERE ur.RolId = @RolId AND u.EmpresaId = @EmpresaId`), no que el rol *pertenezca* a la empresa (vía `Rol.CreadoPor`). Semánticamente incorrecto.

11. `ObtenerEmpresas` tiene un `INNER JOIN Usuarios` **muerto** (no usado en SELECT/WHERE): si un `CreadoPor` coincide con varios usuarios, **duplica filas** de `Empresa`.

12. `GuardarOActualizarUsuarioPagina` tiene el typo **`@CreadoPor nvarchaR (25)`** (tipo inexistente → el procedure no compilaría al desplegar) y usa **`@@IDENTITY`** en lugar de `SCOPE_IDENTITY()`.

13. `ObtenerUsuarioPorId` tiene comentado `--AND u.Estatus = 1` → devuelve **usuarios borrados lógicamente** (solo filtra por empresa).

14. `InsertarUsuarioPagina` y `InsertarUsuarioPaginaParaNuevaEmpresa` son **lógicamente idénticos** (solo cambia el comentario). Igual duplicación entre `ObtenerModelo`/`ObtenerModelos` y `ObtenerModelosPorMarca`/`ObtenerModelosPorMarcaId`.

15. `ObtenerUsuarioPorId`/`ObtenerUsuarios` no filtran `Estatus = 1` sobre el usuario destino (ver #13), y los procedures "`...ParaNuevaEmpresa`" son invocables con IDs arbitrarios sin autenticación (escalada si quedan expuestos vía API).

## NICE-TO-HAVE

16. **No hay seed data en el script** (0 `INSERT` a nivel de esquema): `Pagina`, `TicketEstatus`, `Rol` se pueblan solo desde la app → entornos no reproducibles y riesgo de drift entre ambientes.

17. `Compania` vs `Empresa`: dos entidades de "empresa" sin relación entre sí y con propósitos confusos.

18. `Ticket.Urgencia` es `int` sin catálogo (magic number); `SubcategoriaId` es FK a `Categoria` (doble jerarquía junto a `CategoriaPadreId`); `Ticket` no distingue "solicitante" de "agente".

19. Naming inconsistente: `ObtenerMarca` (singular) devuelve una lista; `Pagina.OrdenB` y `Pagina.PermisosPadreId` son nombres confusos; tipografías mixtas `create procedure`/`CREATE PROCEDURE`.

20. `CategoriaResponsable.EsPrincipal` sin constraint único: la unicidad de "un principal por categoría" se garantiza solo en código (`GuardarOActualizarCategoriaResponsable`), con race conditions posibles.

21. `Estatus` `NULL` vs `NOT NULL` inconsistente entre tablas (ver patrón de soft-delete).

## Evaluación de la idea de free trial + multi-tenancy

**La idea de negocio** (empresa se registra → recibe ~30 días gratis → se generan INSERTs de seed por empresa → los procedures filtran por usuario autenticado) es **correcta en concepto**, pero la implementación actual tiene fallas de fondo.

1. **"INSERT de seed por empresa": no es un problema de "seed", es *provisioning de tenant*, y hoy está mal ejecutado.**
   Crear catálogos por empresa (área inicial, sucursal, rol admin, mapeos usuario↔página) es normal y necesario. Lo incorrecto es el **mecanismo**: 8+ procedures sueltos, sin validación, sin transacción, con lógica duplicada. Recomendación: consolidar en **un único procedure transaccional `RegistrarEmpresa`** (o servicio) que clone un **template**: una tabla `PlantillaRol`/`PlantillaRolPagina` con los roles/permisos por defecto. Así, al agregar una página/rol nuevo, se edita el template y no cada ruta de registro. El "INSERT por empresa" como script SQL suelto **no escala** y genera drift.

2. **"Filtrado por usuario autenticado" como límite de tenant: necesario pero NO suficiente, y hoy es vulnerable.**
   - El filtro no es por "usuario autenticado", es por un **parámetro `@Usuario`/`@NombreUsuario` que el cliente envía** y que la WebApi no valida contra el token (ver `webapi-review` hallazgo #3: no hay autorización real). Un cliente puede enviar cualquier username → el filtro es *spoofeable*.
   - La resolución de tenant vía `CreadoPor` (string) es frágil (colisión de usernames → fuga entre tenants; rename/soft-delete → orfandad).
   - Varios procedures no filtran en absoluto (#2). **Recomendación**: añadir `EmpresaId` a todas las tablas de dominio, resolver el tenant **server-side desde la identidad OAuth** (no desde un parámetro), y hacer `NombreUsuario` único global (o migrar `CreadoPor` a `Usuarios.Id`).

3. **Modelado del período de prueba: parcialmente bien, sin enforcement.**
   Tener `FechaVigenciaInicio/FechaVigenciaFin/EsPeriodoPrueba` en `Empresa` es un buen comienzo. Falta: (a) validar la vigencia en `AutenticarUsuario` (hoy un trial vencido sigue operando); (b) un mecanismo para **desactivar automáticamente** trials expirados (job o check en login); (c) distinguir "trial vs pagado" más allá de un bit (una tabla `Plan`/`Suscripcion` con renovación). Idealmente: `FechaVigenciaFin` + estado de suscripción, y bloqueo activo en autenticación.

4. **Tenant isolation: SÍ hay riesgo real de que un usuario de la empresa A vea datos de la empresa B**, por tres vías: los procedures sin filtro (#2), la colisión de `NombreUsuario` (#1) y la ausencia de enforcement en la API. Con el modelo actual, la separación es "de buena fe" (depende de que el cliente pase el `@Usuario` correcto y de que no haya usernames repetidos), no una garantía.

**Veredicto**: la idea del free trial + multi-tenancy es viable, pero para que funcione en producción hay que (a) hacer el tenant un ciudadano de primera clase (`EmpresaId` en todas las tablas), (b) resolver el tenant desde la identidad autenticada, (c) transaccionalizar y templatizar el provisioning, y (d) hacer cumplir la expiración del trial en login.

## Fortalezas

- 100% stored procedures parametrizados: **no hay SQL dinámico, `EXEC()`, concatenación en WHERE/ORDER BY ni inyección posible a nivel de BD** (consistente con `webapi-review`).
- Campos de auditoría consistentes en las 21 tablas y patrón de soft-delete (`Estatus`) uniforme en la mayoría de operaciones.
- La intención de multi-tenancy está presente en la mayoría de los `Obtener*` (filtran por empresa vía `CreadoPor`), con validaciones de misma-empresa en los `Eliminar*`/`GuardarOActualizar*`.
- RBAC con granularidad de acciones (Leer/Crear/Editar/Eliminar/Exportar) bien pensado en `RolPaginaAccion`.
- `CategoriaResponsable` con `EsPrincipal` + `PuedeAtenderTickets` (intención correcta de separar agentes de solicitantes).
- El modelo de trial ya tiene los campos de fecha/vigencia en `Empresa`.

## Siguientes pasos recomendados (top impacto)

1. **Reforzar tenant isolation**: añadir `EmpresaId` (FK NOT NULL) a todas las tablas de dominio y resolver el tenant desde la identidad OAuth en el servicio, eliminando el patrón `CreadoPor = NombreUsuario`. Hacer `NombreUsuario` único global.
2. **Cerrar los procedures sin filtro** (`ObtenerModelos`, `ObtenerUsuarioPagina`, `ObtenerUsuarioPorCorreo/NombreUsuario`, `EliminarTicket`, etc.) o restringirlos a admin/billing con autorización explícita.
3. **Transaccionalizar el registro de empresa** en un único procedure con `BEGIN TRY/TRAN/COMMIT/ROLLBACK` y **templatizar** los catálogos por defecto.
4. **Enforce el trial**: validar vigencia en `AutenticarUsuario` + job de expiración; dejar de devolver `Contrasena` y migrar a hashing (PBKDF2/bcrypt).
5. **Rendimiento**: índices en `CreadoPor`/`EmpresaId`/FKs calientes + paginación en listados; corregir `GuardarRolParaNuevaEmpresa` (setear `PuedeAtenderTickets`), el typo `nvarchaR`, `@@IDENTITY` y el JOIN muerto de `ObtenerEmpresas`.
