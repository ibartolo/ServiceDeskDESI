# Tasks: Contrato de respuesta tipado — `ModelResponse<T>`

> Cambio: `modelresponse-tipado` (cierra E8). Migración por entidad/dominio: DAL+Service+Controller (WebApi) + HttpClientConnection+Service+Controller (MVC) del mismo dominio SIEMPRE juntos. Compilar `ServiceDeskDESI.sln` al cerrar cada lote.

## Fase 1: Fundaciones (primero)

- [x] 1.1 `ServiceDeskDESIEntities/Seguridad/ModelResponse.cs`: cambiar `IsSuccess = true` → `IsSuccess = false` en el constructor (default) y añadir nueva clase `public class ModelResponse<T> { public bool IsSuccess { get; set; } /*default false*/; public string Message { get; set; }; public T Response { get; set; } }` (independiente, SIN herencia, SIN atributos de serialización). Mismo namespace `ServiceDeskDESIEntities.Seguridad`.
- [x] 1.2 `ServiceDeskDESIMVC/DAL/HttpClientBase.cs`: añadir overload `public async Task<ModelResponse<TResponse>> RequestAsync<TResponse>(string endPoint, HttpMethod method, object content, string token = "", string contentType = "application/json")` que deserializa el string crudo **una vez** a `ModelResponse<TResponse>` y conserva el manejo de error no-2xx actual (devolver `ModelResponse<TResponse>` con `IsSuccess=false` + mensaje de error HTTP, sin NRE).
- [x] 1.3 **Auditoría `IsSuccess`** (riesgo default `false`): verificar que TODO `new ModelResponse()` en WebApi (112) y MVC (6) setea `IsSuccess = true` explícito en cada camino de éxito. Sospechosos a revisar y corregir si falta seteo: `AutenticacionService.ValidarRecetearContrasenia` (:316), `AutenticacionService.RestablecerContrasenia` (:386), `EmpresaService.GuardarNuevaEmpresaConDatosIniciales` (:247), MVC `UserController.ActualizarPerfilUsuario` (:84), `CatalogsController.GuardarPerfil` (:598), `HomeController.LogIn` (:75), `TicketController.CambiarEstatusTicket`/`AsignarTicketAgente` (:180/:205). Grep `return modelResponse;` (y `return result;`) dentro de `try` sin `IsSuccess` previo en `DbWrapper.*.cs`.

## Fase 2: Lotes por dominio (cada lote compila al cerrarse)

> Notación: `Método → ModelResponse<T>` (tipado) · `→ ModelResponse` (sin payload, Eliminar*/escalar). Los `Eliminar*` y escalares (`Convert.ToInt64`) NO migran a genérico.

### Lote 2.1 — Autenticacion / Usuario

- [x] 2.1.1 WebApi `DAL/DbWrapper.Autenticacion.cs`: `ObtenerUsuarios→ModelResponse<List<Usuario>>`, `ObtenerUsuarioPorId→<Usuario>`, `ObtenerUsuarioPorNombreUsuario→<Usuario>`, `ObtenerUsuarioPorCorreo→<Usuario>`, `GuardarOActualizarUsuario→<Usuario>`, `GuardarOActualizarUsuarioAdmin→<Usuario>`, `ActualizarPerfilUsuario→<Usuario>`, `AutenticarUsuario→<Usuario>`, `ObtenerTokenRecuperacion→<TokenRecuperacionDTO>`; dejan `ModelResponse`: `EliminarUsuario`, `InsertarTokenRecuperacion` (escalar long), `ActualizarTokenUsado`, `ActualizarContrasena`.
- [x] 2.1.2 WebApi `Services/AutenticacionService.cs`: mismas firmas passthrough tipadas (14 métodos); `ValidarRecetearContrasenia`/`RestablecerContrasenia`/`GuardarNuevaEmpresaConDatosIniciales` (en EmpresaService) conservan `ModelResponse`.
- [x] 2.1.3 WebApi `Controllers/AutenticationController.cs`: tipo de retorno tipado en `ObtenerUsuarios`, `ObtenerUsuarioPorId`, `GuardarOActualizarUsuario`, `GuardarUsuarioEmpresa`, `ActualizarPerfilUsuario`, `AutenticarUsuario`, `GuardarOActualizarUsuarioAdmin`; `EliminarUsuario`, `ValidarRecetearContrasenia`, `ValidarTokenRecuperacion`, `RestablecerContrasenia` quedan `ModelResponse`.
- [x] 2.1.4 MVC `DAL/HttpClientConnection.Autentication.cs` (6) y `HttpClientConnection.User.cs` (6): parse único → `AutenticarUsuario→<Usuario>`, `ActualizarPerfilUsuario→<Usuario>`, `GuardarOActualizarUsuarioAdmin→<Usuario>`, `ObtenerUsuarios→<List<Usuario>>`, `ObtenerUsuarioPorId→<Usuario>`, `GuardarOActualizarUsuario→<Usuario>`, `GuardarUsuarioEmpresa→<Usuario>`; `ValidarTokenRecuperacion`, `RestablecerContrasenia`, `ValidarRecetearContrasenia`, `EliminarUsuario`, `ObtenerSucursales` (dead, en User.cs) quedan `ModelResponse`.
- [x] 2.1.5 MVC `Services/AutenticacionService.cs` + `Services/UsuarioService.cs`: quitar reparse; `ObtenerUsuarioPorId`→`Task<Usuario>` usa `.Response`; `ObtenerUsuarios`/`Guardar*`→`Task<ModelResponse<T>>`.
- [x] 2.1.6 MVC `Controllers/HomeController.cs` (`.Response.ToString()` L53 paginas, L104 usuarioAutenticado) y `Controllers/UserController.cs` (L51, L130, L137, L147, L156, L164, L259, L269): reemplazar `JsonConvert.DeserializeObject<T>(r.Response.ToString())` → `r.Response` tipado.

### Lote 2.2 — Empresa

- [x] 2.2.1 WebApi `DAL/DbWrapper.Empresa.cs`: `ObtenerEmpresaPorId→<Empresa>`, `ObtenerEmpresaPorRFC→<Empresa>`, `ObtenerEmpresaPorCorreoContacto→<Empresa>`, `ObtenerEmpresaPorNombreComercial→<Empresa>`, `ObtenerEmpresaPorRazonSocial→<Empresa>`, `GuardarOActualizarEmpresa→<Empresa>`, `GuardarNuevaEmpresa→<Empresa>`, `ObtenerPlantillaRoles→<List<Rol>>`; dejan `ModelResponse`: `EliminarEmpresa`, `GuardarRolParaNuevaEmpresa`, `AsignarRolUsuarioParaNuevaEmpresa`, `InsertarUsuarioPaginaParaNuevaEmpresa` (escalares).
- [x] 2.2.2 WebApi `Services/EmpresaService.cs`: firmas passthrough tipadas (7); `GuardarNuevaEmpresaConDatosIniciales` conserva `ModelResponse`.
- [x] 2.2.3 WebApi `Controllers/EmpresaController.cs`: `ObtenerEmpresasPorId→<Empresa>`, `GuardarOActualizarEmpresa`, `GuardarNuevaEmpresa`, `GuardarNuevaEmpresaCompleta`, `Registrar` tipadas; `EliminarEmpresa` queda `ModelResponse`.
- [x] 2.2.4 MVC `DAL/HttpClientConnection.Empresa.cs` (6): parse único tipado (`ObtenerEmpresaPorId`, `GuardarOActualizarEmpresa`, `GuardarNuevaEmpresa`, `RegistrarEmpresa`, `GuardarNuevaEmpresaCompleta`, `EliminarEmpresa`→`ModelResponse`).
- [x] 2.2.5 MVC `Services/EmpresaService.cs`: quitar reparse; `ObtenerEmpresaPorId`→`Task<Empresa>` usa `.Response`; `ObtenerPermisosParaEmpresa` usa `List<PermisosViewModel>`.
- [x] 2.2.6 MVC `Controllers/HomeController.cs` (flujo `NewCompany`/`Configuration`) y `CatalogsController.cs` (acción `Company`, L103): usar `.Response` tipado.

### Lote 2.3 — Compania

- [x] 2.3.1 WebApi `DAL/DbWrapper.Compania.cs`: `ObtenerCompanias→<List<Compania>>`, `ObtenerCompaniaPorId→<Compania>`, `GuardarOActualizarCompania→<Compania>`; `EliminarCompania`→`ModelResponse`.
- [x] 2.3.2 WebApi `Services/CompaniaService.cs` (4) + `Controllers/CompaniaController.cs` (4): firmas tipadas.
- [x] 2.3.3 MVC `DAL/HttpClientConnection.Compania.cs` (4): parse único tipado.
- [x] 2.3.4 MVC `Services/CompaniaService.cs` (quitar reparse en `ObtenerCompaniaPorId` y `ObtenerPermisosParaCompania`) + `Controllers/CatalogsController.cs` (L103).

### Lote 2.4 — Area

- [x] 2.4.1 WebApi `DAL/DbWrapper.Area.cs`: `ObtenerAreas→<List<Area>>`, `ObtenerAreaPorId→<Area>`, `GuardarOActualizarArea→<Area>`, `GuardarNuevaAreaParaEmpresa→<Area>`; `EliminarArea`→`ModelResponse`.
- [x] 2.4.2 WebApi `Services/AreaService.cs` (4) + `Controllers/AreaController.cs` (4): firmas tipadas.
- [x] 2.4.3 MVC `DAL/HttpClientConnection.Area.cs` (4): parse único tipado.
- [x] 2.4.4 MVC `Services/AreaService.cs`: `ObtenerAreaPorId`→`Task<Area>` (`.Response`); `ConsultarTodasAreas`→`Task<ModelResponse<List<Area>>>`; `ObtenerPermisosParaArea` usa `.Response`. `Controllers/CatalogsController.cs` acción `WorkArea` (sin reparse directo de Area; revisar).

### Lote 2.5 — Categoria

- [x] 2.5.1 WebApi `DAL/DbWrapper.Categoria.cs`: `ObtenerCategorias→<List<CategoriaDTO>>`, `ObtenerCategoriasPorArea→<List<CategoriaDTO>>`, `ObtenerCategoriaPorId→<CategoriaDTO>`, `ObtenerCategoriasPorPadre→<List<CategoriaDTO>>`, `GuardarOActualizarCategoria→<Categoria>`; `EliminarCategoria`→`ModelResponse`.
- [x] 2.5.2 WebApi `Services/CategoriaService.cs` (6) + `Controllers/CatalogsController.cs` (6 acciones Categoria): firmas tipadas.
- [x] 2.5.3 MVC `DAL/HttpClientConnection.Categoria.cs` (6): parse único tipado.
- [x] 2.5.4 MVC `Services/CategoriaService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L463, L487, L524, L533) y `TicketController.cs` (L54, L72).

### Lote 2.6 — CategoriaResponsable

- [x] 2.6.1 WebApi `DAL/DbWrapper.CategoriaResponsable.cs`: `ObtenerResponsablesPorCategoria→<List<CategoriaResponsableDTO>>`, `ObtenerCategoriasPorResponsable→<List<CategoriaResponsableDTO>>`, `GuardarOActualizarCategoriaResponsable→<CategoriaResponsable>`; `EliminarCategoriaResponsable`→`ModelResponse`.
- [x] 2.6.2 WebApi `Services/CategoriaResponsableService.cs` (4) + `Controllers/CatalogsController.cs` (4 acciones): firmas tipadas.
- [x] 2.6.3 MVC `DAL/HttpClientConnection.CategoriaResponsable.cs` (4): parse único tipado.
- [x] 2.6.4 MVC `Services/CategoriaResponsableService.cs` (passthrough) + `Controllers/CatalogsController.cs` (L556).

### Lote 2.7 — Marca

- [x] 2.7.1 WebApi `DAL/DbWrapper.Marca.cs`: `ObtenerMarcas→<List<Marca>>`, `ObtenerMarcaPorId→<Marca>`, `GuardarOActualizarMarca→<Marca>`; `EliminarMarca`→`ModelResponse`.
- [x] 2.7.2 WebApi `Services/MarcaService.cs` (4) + `Controllers/MarcaController.cs` (4): firmas tipadas.
- [x] 2.7.3 MVC `DAL/HttpClientConnection.Marca.cs` (4): parse único tipado.
- [x] 2.7.4 MVC `Services/MarcaService.cs` (quitar reparse `ObtenerMarcaPorId`, `ObtenerPermisosParaMarca`) + `Controllers/CatalogsController.cs` (L257-258, L354, L395).

### Lote 2.8 — Modelo

- [x] 2.8.1 WebApi `DAL/DbWrapper.Modelo.cs`: `ObtenerModelos→<List<ModeloDTO>>`, `ObtenerModeloPorId→<ModeloDTO>`, `GuardarOActualizarModelo→<Modelo>`, `ObtenerModelosPorMarcaId→<List<Modelo>>`; `EliminarModelo`→`ModelResponse`.
- [x] 2.8.2 WebApi `Services/ModeloService.cs` (5) + `Controllers/ModeloController.cs` (5): firmas tipadas.
- [x] 2.8.3 MVC `DAL/HttpClientConnection.Modelo.cs` (5): parse único tipado.
- [x] 2.8.4 MVC `Services/ModeloService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L249-250, L340, L844).

### Lote 2.9 — Activo

- [x] 2.9.1 WebApi `DAL/DbWrapper.Activo.cs`: `ObtenerTodosLosActivos→<List<ActivoDTO>>`, `ObtenerActivoPorId→<ActivoDTO>`, `GuardarOActualizarActivo→<Activo>`; `EliminarActivo`→`ModelResponse`.
- [x] 2.9.2 WebApi `Services/ActivoServices.cs` (4) + `Controllers/ActivoController.cs` (4): firmas tipadas.
- [x] 2.9.3 MVC `DAL/HttpClientConnection.Activo.cs` (4): parse único tipado.
- [x] 2.9.4 MVC `Services/ActivoService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L266).

### Lote 2.10 — TipoActivo

- [x] 2.10.1 WebApi `DAL/DbWrapper.TipoActivo.cs`: `ObtenerTodosLosTipoActivos→<List<TipoActivo>>`, `ObtenerTipoActivoPorId→<TipoActivo>`, `GuardarOActualizarTipoActivo→<TipoActivo>`; `EliminarTipoActivo`→`ModelResponse`.
- [x] 2.10.2 WebApi `Services/TipoActivoService.cs` (4) + `Controllers/TipoActivoController.cs` (4): firmas tipadas.
- [x] 2.10.3 MVC `DAL/HttpClientConnection.TipoActivo.cs` (4): parse único tipado.
- [x] 2.10.4 MVC `Services/TipoActivoService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L179, L241-242).

### Lote 2.11 — Puesto

- [x] 2.11.1 WebApi `DAL/DbWrapper.Puesto.cs`: `ObtenerTodosLosPuestos→<List<Puesto>>`, `ObtenerPuestoPorId→<Puesto>`, `GuardarOActualizarPuesto→<Puesto>`; `EliminarPuesto`→`ModelResponse`.
- [x] 2.11.2 WebApi `Services/PuestoService.cs` (4) + `Controllers/PuestoController.cs` (4): firmas tipadas.
- [x] 2.11.3 MVC `DAL/HttpClientConnection.Puesto.cs` (4): parse único tipado.
- [x] 2.11.4 MVC `Services/PuestoService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L127).

### Lote 2.12 — Persona

- [x] 2.12.1 WebApi `DAL/DbWrapper.Persona.cs`: `ObtenerTodasLasPersonas→<List<PersonaDTO>>`, `ObtenerPersonaPorId→<PersonaDTO>`, `GuardarOActualizarPersona→<Persona>`; `EliminarPersona`→`ModelResponse`.
- [x] 2.12.2 WebApi `Services/PersonaService.cs` (4) + `Controllers/PersonaController.cs` (4): firmas tipadas.
- [x] 2.12.3 MVC `DAL/HttpClientConnection.Persona.cs` (4): parse único tipado.
- [x] 2.12.4 MVC `Services/PersonaService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L150).

### Lote 2.13 — Sucursal

- [x] 2.13.1 WebApi `DAL/DbWrapper.Sucursal.cs`: `ObtenerSucursales→<List<Sucursal>>`, `ObtenerSucursalPorId→<Sucursal>`, `GuardarOActualizarSucursal→<Sucursal>`, `GuardarNuevaSucursalParaEmpresa→<Sucursal>`; `EliminarSucursal`→`ModelResponse`.
- [x] 2.13.2 WebApi `Services/SucursalService.cs` (4) + `Controllers/SucursalController.cs` (4): firmas tipadas.
- [x] 2.13.3 MVC `DAL/HttpClientConnection.Sucursal.cs` (4): parse único tipado.
- [x] 2.13.4 MVC `Services/SucursalService.cs` (quitar reparse) + `Controllers/CatalogsController.cs` (L210).

### Lote 2.14 — Rol

- [x] 2.14.1 WebApi `DAL/DbWrapper.Rol.cs`: `ObtenerRoles→<List<Rol>>`, `ObtenerRolPorId→<Rol>`, `GuardarOActualizarRol→<Rol>`, `ObtenerRolesPorUsuario→<List<Rol>>`; dejan `ModelResponse` (escalares): `EliminarRol`, `AsignarRolUsuario`, `EliminarRolUsuario`.
- [x] 2.14.2 WebApi `Services/RolService.cs` (7) + `Controllers/RolController.cs` (7): firmas tipadas (escalares quedan `ModelResponse`).
- [x] 2.14.3 MVC `DAL/HttpClientConnection.Rol.cs` (7): parse único tipado (`ObtenerTodosLosRoles`, `ObtenerRolPorId`, `GuardarOActualizarRol`, `ObtenerRolesPorUsuario` tipadas; `EliminarRol`, `AsignarRolUsuario`, `EliminarRolUsuario`→`ModelResponse`).
- [x] 2.14.4 MVC `Services/RolService.cs` (quitar reparse) + `Controllers/SecurityController.cs` (L48, L78) y `UserController.cs` (L142, L159, L264).

### Lote 2.15 — Pagina

- [x] 2.15.1 WebApi `DAL/DbWrapper.Paginas.cs`: `ObtenerPaginasPorUsuario→<List<Pagina>>`, `ObtenerPaginaPorNombre→<Pagina>`, `ObtenerPaginas→<List<Pagina>>`.
- [x] 2.15.2 WebApi `Services/PaginaService.cs` (3) + `Controllers/PaginaController.cs` (3): firmas tipadas.
- [x] 2.15.3 MVC `DAL/HttpClientConnection.Pagina.cs` (1: `ObtenerPaginasPorUsuario`): parse único tipado.
- [x] 2.15.4 MVC `Controllers/HomeController.cs` (L53 `MenusUser`) y `SecurityController.cs` (L87): usar `.Response` `List<Pagina>`.

### Lote 2.16 — UsuarioPagina (+ Relacion, código muerto)

- [x] 2.16.1 WebApi `DAL/DbWrapper.UsuarioPagina.cs`: `ObtenerUsuarioPagina→<List<UsuarioPagina>>`, `GuardarOActualizarUsuarioPagina→<UsuarioPagina>`, `ObtenerUsuarioPaginaPorId→<UsuarioPagina>`; `EliminarUsuarioPagina`→`ModelResponse`.
- [x] 2.16.2 WebApi `Controllers/UsuarioPaginaController.cs` (4): firmas tipadas. (No hay `UsuarioPaginaService`: el controller llama directo al DbWrapper.)
- [x] 2.16.3 WebApi `DAL/DbWrapper.Relacion.cs` + `Controllers/RelacionController.cs` (sin uso en MVC — código muerto): `ObtenerTodasRelaciones→<List<UsuarioPagina>>`, `ObtenerRelacionPorId→<UsuarioPagina>`, `GuardarOActualizarRelacion→<UsuarioPagina>`, `EliminarRelacion`→`ModelResponse`. Migrar para mantener compilación (o dejar `ModelResponse` no-genérico si no hay consumidor).

### Lote 2.17 — Permisos

- [x] 2.17.1 WebApi `DAL/DbWrapper.Permisos.cs`: `ObtenerPermisosPorUsuario→<List<PermisosViewModel>>`, `ObtenerPermisosPorRol→<List<RolPaginaAccionDTO>>` (deviation: el DAL usa `LlenarEntidad<RolPaginaAccionDTO>`, no `PermisosViewModel`), `ValidarPermisoUsuario→<bool>`; dejan `ModelResponse` (escalares): `InsertarRolPaginaAccion`, `EliminarRolPaginaAccion`, `ActualizarRolPaginaAccion`, `GuardarPermisosRol`, `GuardarPermisosRolMasivo`.
- [x] 2.17.2 WebApi `Services/PermisosService.cs` (6) + `Controllers/PermisosController.cs` (6): firmas tipadas (`ValidarPermisoUsuario→<bool>`); escalares quedan `ModelResponse`.
- [x] 2.17.3 MVC `DAL/HttpClientConnection.Permisos.cs` (6): parse único tipado (`ObtenerPermisosPorUsuario`, `ValidarPermisoUsuario`, `ObtenerPaginas`, `ObtenerPermisosPorRol`; `GuardarPermisosRol`, `GuardarPermisosRolMasivo`→`ModelResponse`).
- [x] 2.17.4 MVC `Services/PermisosService.cs`: `ObtenerPermisosParaPagina`→`Task<List<PermisosViewModel>>` usa `.Response`; `TienePermiso` usa `.Response` (`bool`); `ObtenerPermisosParaPermisos` usa `.Response`. `Controllers/SecurityController.cs` (GuardarPermisos) y `PermissionsController.cs` (dead, constructor DI roto).

### Lote 2.18 — Ticket + TicketEstatus

- [x] 2.18.1 WebApi `DAL/DbWrapper.Ticket.cs`: `ObtenerTickets→<List<TicketDTO>>`, `ObtenerTicketPorId→<TicketDTO>`, `GuardarOActualizarTicket→<Ticket>`, `ObtenerTicketsPorArea→<List<TicketDTO>>`, `ObtenerTicketsPorUsuario→<List<TicketDTO>>`, `ObtenerTicketsPorUrgencia→<List<TicketDTO>>`, `ObtenerTicketsPorEstatus→<List<TicketDTO>>`, `ObtenerTicketEstatus→<List<TicketEstatus>>`; `EliminarTicket`→`ModelResponse`.
- [x] 2.18.2 WebApi `Services/TicketService.cs` (9) + `Controllers/TicketController.cs` (9): firmas tipadas.
- [x] 2.18.3 MVC `DAL/HttpClientConnection.Ticket.cs` (9): parse único tipado.
- [x] 2.18.4 MVC `Services/TicketService.cs` (quitar reparse `ObtenerPermisosParaTicket`) + `Controllers/TicketController.cs` (L54, L72, L194).

## Fase 3: Verificación y residuos

- [x] 3.1 Compilar `ServiceDeskDESI.sln` (0 errores) al cierre de CADA lote y al final.
- [x] 3.2 Grep residuos en MVC: `Response.ToString()` debe dar **0** resultados; `DeserializeObject<ModelResponse>` en `ServiceDeskDESIMVC/DAL` debe dar **0** (salvo Eliminar*/escalares, documentado).
- [x] 3.3 Grep `new ModelResponse()` en WebApi/MVC: confirmar que solo quedan los Eliminar*/escalares/composite (payload nulo o escalar), con `IsSuccess` explícito.
- [ ] 3.4 Smoke test manual por dominio: Autenticación (login), Área, Ticket (listar/detalle), Permisos (menú) — verificar `.Response` tipado y `IsSuccess=false` en errores.
