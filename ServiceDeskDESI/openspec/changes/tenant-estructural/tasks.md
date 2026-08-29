# Tasks: tenant-estructural

## 1. Esquema
- [x] 1.1 Añadir `EmpresaId` (nullable) + FK a 12 tablas de dominio.
- [x] 1.2 Backfill de `EmpresaId` desde `CreadoPor`.
- [x] 1.3 Índice único global `UX_Usuarios_NombreUsuario`.

## 2. Registro (C#)
- [x] 2.1 SPs `GuardarNueva*ParaEmpresa` con `@EmpresaId`.
- [x] 2.2 `DbWrapper.{Area,Sucursal,Empresa}.cs` pasan `empresaId`.
- [x] 2.3 `EmpresaService` pasa `empresaGuardada.Id`.

## 3. Escrituras migradas a EmpresaId
- [x] 3.1 `GuardarOActualizarActivo`.
- [x] 3.2 `GuardarOActualizarArea`.
- [x] 3.3 `GuardarOActualizarCategoria`.

## 4. Pendiente
- [ ] 4.1 Resto de `GuardarOActualizar*` (CategoriaResponsable, Marca, Modelo, Persona, Puesto, Rol, Sucursal, Ticket, TipoActivo).
- [ ] 4.2 `Eliminar*` (12 SPs).
- [ ] 4.3 `Obtener*` (18 SPs).
- [ ] 4.4 Endurecer `EmpresaId` a `NOT NULL`.

## 5. Verificación
- [x] 5.1 Compilar `ServiceDeskDESI.sln` (0 errores).
- [ ] 5.2 Smoke: registro de empresa nuevo + catálogos por tenant.
