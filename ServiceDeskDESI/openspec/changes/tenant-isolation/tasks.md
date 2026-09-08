# Tasks: tenant-isolation

## 1. Base de datos (SPs)
- [x] 1.1 `ObtenerUsuarioPagina`: añadir `@Usuario` + filtro de tenant vía `UsuarioId → Usuarios`.
- [x] 1.2 `ObtenerUsuarioPaginaPorId`: añadir `@Usuario` + filtro de tenant.
- [x] 1.3 `ObtenerUsuarioPorNombreUsuario`: añadir `@Usuario` + filtro de tenant.
- [x] 1.4 `EliminarTicket`: añadir `@Usuario` + validación de propiedad (patrón `EliminarTipoActivo`).
- [x] 1.5 Añadir SPs de dedupe: `ObtenerEmpresaPorCorreoContacto`, `ObtenerEmpresaPorNombreComercial`, `ObtenerEmpresaPorRazonSocial`.

## 2. WebApi — claim de tenant
- [x] 2.1 `Startup.cs`: emitir claim `empresaId`.

## 3. WebApi — DbWrapper
- [x] 3.1 `DbWrapper.UsuarioPagina.cs`: hilar `usuario` en `ObtenerUsuarioPagina`/`PorId`.
- [x] 3.2 `DbWrapper.Autenticacion.cs`: `ObtenerUsuarioPorNombreUsuario(nombreUsuario, usuario)`.
- [x] 3.3 `DbWrapper.Empresa.cs`: quitar `ObtenerTodasLasEmpresas`; añadir 3 métodos de dedupe.

## 4. WebApi — Services
- [x] 4.1 `EmpresaService.cs`: quitar `ObtenerTodasLasEmpresas`; dedupe puntual en `RegistrarEmpresa`.
- [x] 4.2 `PermisosService.cs`: `ObtenerUsuarioPorNombreUsuario(usuario, usuario)`.
- [x] 4.3 `AutenticacionService.cs`: `ObtenerUsuarioPorNombreUsuario(nombreUsuario, nombreUsuario)`.

## 5. WebApi — Controllers
- [x] 5.1 `EmpresaController.cs`: quitar `List` y `RFC`.
- [x] 5.2 `UsuarioPaginaController.cs`: pasar `User.Identity.Name`.
- [x] 5.3 `RolController.cs`: `ObtenerUsuarioPorNombreUsuario(usuario, usuario)`.

## 6. MVC — limpieza de métodos muertos
- [x] 6.1 `HttpClientConnection.Empresa.cs`: quitar `ObtenerTodasLasEmpresas` y `ObtenerEmpresasPorRFC`.
- [x] 6.2 `Services/EmpresaService.cs`: quitar wrappers muertos.

## 7. Verificación
- [x] 7.1 Compilar `ServiceDeskDESI.sln` (0 errores).
- [ ] 7.2 Smoke test manual por endpoint (404 en List/RFC, cruce de tenant, IDOR).
