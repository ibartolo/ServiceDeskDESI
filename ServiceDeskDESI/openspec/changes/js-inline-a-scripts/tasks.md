# Tasks: Mover JavaScript inline de las vistas a archivos en Scripts/

- **Change**: `js-inline-a-scripts`
- **Fecha**: 2026-09-09
- **Estado**: ✅ Completado y verificado

## Tareas

### Pilotos (fijar patrón)
- [x] T1 — `Views/Home/MisActivos.cshtml` → `Scripts/Home/MisActivos.js` (sin Razor).
- [x] T2 — `Views/Catalogs/Mark.cshtml` → `Scripts/Catalogs/Mark.js` (con bloque Razor `permisosGlobal`).

### Lote A — Catalogs (parte 1)
- [x] T3 — `Active.cshtml` → `Scripts/Catalogs/Active.js` (inline: `permisosGlobal`, `tipoActivoSelect`, `marcaSelect`, `modeloSelect`, `activoId`).
- [x] T4 — `Branch.cshtml` → `Scripts/Catalogs/Branch.js` (inline: `permisosGlobal`).
- [x] T5 — `CategoriaResponsable.cshtml` → `Scripts/Catalogs/CategoriaResponsable.js` (inline: `permisosGlobal`).
- [x] T6 — `Category.cshtml` → `Scripts/Catalogs/Category.js` (inline: `permisosGlobal`).

### Lote B — Catalogs (parte 2)
- [x] T7 — `Company.cshtml` → `Scripts/Catalogs/Company.js` (inline: `permisosGlobal`).
- [x] T8 — `Model.cshtml` → `Scripts/Catalogs/Model.js` (inline: `permisosGlobal`).
- [x] T9 — `Persona.cshtml` → `Scripts/Catalogs/Persona.js` (inline: `permisosGlobal`, `personaUsuarioId`, `personaIdEdicion`, `personaNombreUsuarioVinculado`).
- [x] T10 — `Puesto.cshtml` → `Scripts/Catalogs/Puesto.js` (inline: `permisosGlobal`).

### Lote C — Catalogs (parte 3)
- [x] T11 — `TypeActive.cshtml` → `Scripts/Catalogs/TypeActive.js` (inline: `permisosGlobal`).
- [x] T12 — `WorkArea.cshtml` → `Scripts/Catalogs/WorkArea.js` (inline: `permisosGlobal`).
- [x] T13 — `_AsignarActivoPersona.cshtml` → `Scripts/Catalogs/_AsignarActivoPersona.js` (sin Razor).
- [x] T14 — `_MantenimientoActivo.cshtml` → `Scripts/Catalogs/_MantenimientoActivo.js` (sin Razor).

### Lote D — Home + ConfiguracionEmpresa
- [x] T15 — `Ayuda.cshtml` → `Scripts/Home/Ayuda.js` (sin Razor).
- [x] T16 — `Configuration.cshtml` → `Scripts/Home/Configuration.js` (sin Razor).
- [x] T17 — `NewCompany.cshtml` → `Scripts/Home/NewCompany.js` (sin Razor).
- [x] T18 — `RecoverPassword.cshtml` — sin `.js` (solo bloque Razor; lógica en `Scripts/Comun/RecoverPass.js`).
- [x] T19 — `VerAsignacion.cshtml` → `Scripts/Home/VerAsignacion.js` (inline: `tokenAsignacion`, `esDesvinculacion`).
- [x] T20 — `ConfiguracionEmpresa/Index.cshtml` → `Scripts/ConfiguracionEmpresa/Index.js` (inline: `puedeEditar`).

### Lote E — Security + User
- [x] T21 — `Permisos.cshtml` → `Scripts/Security/Permisos.js` (inline: `permisosGlobal`, `todasLasPaginas`).
- [x] T22 — `Role.cshtml` → `Scripts/Security/Role.js` (inline: `permisosGlobal`).
- [x] T23 — `MyProfile.cshtml` → `Scripts/User/MyProfile.js` (inline: `perfilTemp`).
- [x] T24 — `Users.cshtml` → `Scripts/User/Users.js` (inline: `permisosGlobal`, `empresaId`).

### Lote F — Ticket + Estadisticas + Shared
- [x] T25 — `Ticket/_CapturarTicket.cshtml` → `Scripts/Ticket/_CapturarTicket.js` (sin Razor).
- [x] T26 — `Ticket/_DetalleTicket.cshtml` → `Scripts/Ticket/_DetalleTicket.js` (sin Razor).
- [x] T27 — `Ticket/_ReasignarTicket.cshtml` → `Scripts/Ticket/_ReasignarTicket.js` (sin Razor).
- [x] T28 — `Ticket/Index.cshtml` → `Scripts/Ticket/Index.js` (inline: `permisosGlobal`, `esAgenteGlobal`, `usuarioActualIdGlobal`, `esResponsableAreaGlobal`, `evidenciaConfig`).
- [x] T29 — `Estadisticas/Index.cshtml` → `Scripts/Estadisticas/Index.js` (inline: `fechaInicioGlobal`, `fechaFinGlobal`).
- [x] T30 — `Shared/_Layout.cshtml` → `Scripts/Shared/_Layout.js` (sin Razor).

### Cierre
- [x] T31 — Registrar los 29 `.js` nuevos en `ServiceDeskDESIMVC.csproj` (`<Content Include>`).
- [x] T32 — Verificar sintaxis: `node --check` (33/33 OK).
- [x] T33 — Verificar Razor: MSBuild `/p:MvcBuildViews=true` (exit 0).
- [x] T34 — Verificar que no queden residuos de Razor en los `.js` (0).

## Pendiente / recomendaciones

- [ ] Smoke manual en navegador de los flujos por vista (DataTables, modales, filtros, validaciones) — validación de comportamiento en runtime que la verificación estática no cubre.
- [ ] Al publicar, incluir la carpeta `Scripts/` completa (el `.csproj` ya lista los archivos).
