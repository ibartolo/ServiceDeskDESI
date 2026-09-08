# Tasks: FKs escalares `*Id` + DTOs de lectura (E2)

## Phase 1: Entidades → escalares `*Id`

- [x] 1.1 `ServiceDeskDESIEntities/Autenticacion/Usuario.cs`: sustituir `Sucursal`/`Area`/`Empresa` por `long? SucursalId`, `long? AreaId`, `long? EmpresaId`.
- [x] 1.2 `ServiceDeskDESIEntities/Catalogos/Persona.cs`: sustituir `Puesto` por `long PuestoId`.
- [x] 1.3 `ServiceDeskDESIEntities/Catalogos/Categoria.cs`: sustituir `CategoriaPadre`/`Area` por `long? CategoriaPadreId` y `long AreaId`.
- [x] 1.4 `ServiceDeskDESIEntities/Catalogos/CategoriaResponsable.cs`: sustituir `Categoria`/`Usuario` por `long CategoriaId` y `long UsuarioId`.
- [x] 1.5 `ServiceDeskDESIEntities/Catalogos/Modelo.cs`: sustituir `Marca` por `long? MarcaId`.
- [x] 1.6 `ServiceDeskDESIEntities/Catalogos/Activo.cs`: sustituir `TipoActivo`/`Marca`/`Modelo` por `long? TipoActivoId`, `long? MarcaId`, `long? ModeloId`.
- [x] 1.7 `ServiceDeskDESIEntities/Tickets/Ticket.cs`: sustituir `Area`/`Categoria`/`Subcategoria`/`TicketEstatus` por `long AreaId`, `long CategoriaId`, `long? SubcategoriaId`, `int TicketEstatusId`.
- [x] 1.8 `ServiceDeskDESIEntities/Catalogos/UsuarioPagina.cs`: sustituir `Usuarios`/`Pagina` por `long? UsuarioId` y `long? PaginaId`.

## Phase 2: DTOs de lectura (heredan la entidad)

- [x] 2.1 `TicketDTO` (Tickets/): hereda `Ticket`; `string AreaNombre`, `CategoriaNombre`, `SubcategoriaNombre`, `EstatusNombre`, `EstatusColor`.
- [x] 2.2 `UsuarioDTO` (Autenticacion/): hereda `Usuario`; `SucursalNombre`, `AreaNombre`, `EmpresaNombre`, `EmpresaNombreComercial`, `EmpresaRazonSocial`, `EmpresaRFC`, `EmpresaResponsable`, `EmpresaDireccion`, `EmpresaCiudad`, `EmpresaEstado`, `EmpresaCodigoPostal`, `EmpresaTelefono`, `EmpresaCorreoContacto`, `DateTime? FechaVigenciaInicio`, `FechaVigenciaFin`, `bool? EsPeriodoPrueba` (cubre `AutenticarUsuario` y `ObtenerUsuarios`).
- [x] 2.3 `ActivoDTO` (Catalogos/): hereda `Activo`; `TipoActivoNombre`, `MarcaNombre`, `ModeloNombre`.
- [x] 2.4 `CategoriaDTO` (Catalogos/): hereda `Categoria`; `AreaNombre`, `CategoriaPadreNombre`.
- [x] 2.5 `CategoriaResponsableDTO` (Catalogos/): hereda `CategoriaResponsable`; `CategoriaNombre`, `AreaNombre`, `NombreUsuario`, `Nombre`, `Apellido`, `Correo`.
- [x] 2.6 `ModeloDTO` (Catalogos/): hereda `Modelo`; `MarcaNombre`, `MarcaDescripcion`.
- [x] 2.7 `PersonaDTO` (Catalogos/): hereda `Persona`; `PuestoNombre`, `PuestoDescripcion`.

## Phase 3: DbWrapper

- [x] 3.1 `DAL/DbWrapper.cs`: simplificar `ObtenerParametrosSQL` — eliminar rama que extrae `.Id` de nav; generar siempre `@<Prop>` con `p.GetValue(o)`.
- [x] 3.2 `DAL/DbWrapper.Ticket.cs`: `ObtenerTickets`, `ObtenerTicketPorId`, `ObtenerTicketsPorArea/PorUsuario/PorUrgencia/PorEstatus` → `LlenarEntidad<TicketDTO>` (borrar bloques `new Area(){…}`×7); `GuardarOActualizarTicket` usar `AreaId`/`CategoriaId`/`SubcategoriaId`/`TicketEstatusId`.
- [x] 3.3 `DAL/DbWrapper.Autenticacion.cs`: lecturas → `LlenarEntidad<UsuarioDTO>`; `GuardarOActualizarUsuario/UsuarioAdmin/ActualizarPerfilUsuario` usar `SucursalId`/`AreaId`/`EmpresaId`; trial check `usuario.Empresa…` → `usuario.EsPeriodoPrueba`/`FechaVigenciaFin`.
- [x] 3.4 `DAL/DbWrapper.Activo.cs`: lecturas → `LlenarEntidad<ActivoDTO>`; `GuardarOActualizarActivo` usar `TipoActivoId`/`MarcaId`/`ModeloId`.
- [x] 3.5 `DAL/DbWrapper.Categoria.cs`: lecturas → `LlenarEntidad<CategoriaDTO>`; escritura usar `CategoriaPadreId`/`AreaId`.
- [x] 3.6 `DAL/DbWrapper.CategoriaResponsable.cs`: lecturas → `LlenarEntidad<CategoriaResponsableDTO>`; escritura usar `CategoriaId`/`UsuarioId`.
- [x] 3.7 `DAL/DbWrapper.Modelo.cs`: `ObtenerModeloPorId` → `LlenarEntidad<ModeloDTO>`; escritura usar `MarcaId`.
- [x] 3.8 `DAL/DbWrapper.Persona.cs`: lecturas → `LlenarEntidad<PersonaDTO>`; escritura usar `PuestoId`.
- [x] 3.9 `DAL/DbWrapper.UsuarioPagina.cs`: confirmar `GuardarOActualizarUsuarioPagina` recibe `UsuarioId`/`PaginaId` escalares.

## Phase 4: Script BD

- [x] 4.1 `openspec/basededatosservicedesk.txt` `GuardarOActualizarTicket` (líneas ~3074-3179): renombrar `@Area`→`@AreaId`, `@Categoria`→`@CategoriaId`, `@Subcategoria`→`@SubcategoriaId` en firma y cuerpo.
- [x] 4.2 (Opcional) `GuardarOActualizarActivo`: normalizar `@TipoActivoID`→`@TipoActivoId`, `@MarcaID`→`@MarcaId`, `@ModeloID`→`@ModeloId`.

## Phase 5: MVC

- [x] 5.1 Controllers: `TicketController` (`c.CategoriaPadre==null`→`CategoriaPadreId`, `ticket.Categoria.Id`→`CategoriaId`, `ticket.TicketEstatus`→`TicketEstatusId`); `CatalogsController` (`activo.TipoActivo/Modelo/Marca.Id`→`*Id`, `categoria.Area?.Id`→`AreaId`, `x.Area.Id`→`AreaId`, `m.Marca.Id`→`MarcaId`, `GuardarPerfil`); `UserController` (`usuario.Sucursal/Area.Id`→`*Id`, `usuario.Empresa=new Empresa{Id}`→`EmpresaId`); `HomeController` (`usuarioAutenticado.Empresa.Id`→`EmpresaId ?? 0`).
- [x] 5.2 Vistas bindings: `x => x.Area.Id`→`x.AreaId`, `x => x.Sucursal.Id`→`SucursalId`, `x => x.TicketEstatus.Id`→`TicketEstatusId`, `x => x.Categoria.Id`→`CategoriaId`, `x => x.Subcategoria.Id`→`SubcategoriaId`, `x => x.Marca.Id`→`MarcaId`, `x => x.TipoActivo.Id`→`TipoActivoId`, `x => x.Modelo.Id`→`ModeloId`, `x => x.Empresa.Id`→`EmpresaId` en `Ticket/Index`, `Active`, `Model`, `Category`, `User/Users`, `User/MyProfile`.
- [x] 5.3 DataTables: `data:'Area.Nombre'`→`'AreaNombre'`, `'Categoria.Nombre'`→`'CategoriaNombre'`, `'Subcategoria.Nombre'`→`'SubcategoriaNombre'`, `'TicketEstatus.Nombre/Color'`→`'EstatusNombre'/'EstatusColor'`, `'Marca.Nombre'`→`'MarcaNombre'`, `'Modelo.Nombre'`→`'ModeloNombre'`, `'TipoActivo.Nombre'`→`'TipoActivoNombre'`, `'CategoriaPadre.Nombre'`→`'CategoriaPadreNombre'`, `'Sucursal.Nombre'`→`'SucursalNombre'` en `Ticket/Index`, `Users`, `Active`, `Category`, `Model`.
- [x] 5.4 JS guardado: `Area:{Id}`→`AreaId`, `Categoria:{Id}`→`CategoriaId`, `Subcategoria:{Id}`→`SubcategoriaId`, `TicketEstatus:{Id}`→`TicketEstatusId`, `TipoActivo/Marca/Modelo:{Id}`→`*Id`, `Sucursal/Area/Empresa:{Id}`→`*Id` en `Ticket/Index`, `Active`, `Users`, `Category`, `CategoriaResponsable`, `Model`.

## Phase 6: Verificación

- [x] 6.1 Compilar `ServiceDeskDESI.sln` (Entities, WebApi, MVC): 0 errores.
- [x] 6.2 Grep de residuos: `\.(Area|Categoria|Subcategoria|TicketEstatus|Sucursal|Empresa|Puesto|Marca|TipoActivo|Modelo)\.` y `new Area(){`/`new TicketEstatus(){` en solución (0 matches fuera de DTOs).
- [ ] 6.3 Smoke test manual por catálogo: tickets, usuarios, activos, categorías, personas, responsables (listados muestran nombres/colores).
- [ ] 6.4 Login + bloqueo de trial vencido (`EsPeriodoPrueba`/`FechaVigenciaFin`).
