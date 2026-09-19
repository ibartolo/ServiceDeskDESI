# Tasks: mapeo-reflection

## 1. Mapeo por reflection (W10, E3)
- [x] 1.1 Endurecer `LlenarEntidad<T>`: `Convert.ChangeType` + unwrap de nullables + guard en enum.
- [x] 1.2 Endurecer `MapearPorpiedades<T>`: `Convert.ChangeType` + unwrap de nullables.

## 2. Verificación
- [x] 2.1 Compilar `ServiceDeskDESI.sln` (0 errores).
- [ ] 2.2 Smoke: `GET api/Ticket/Estatus/List` devuelve el catálogo de estatus (ya no `IsSuccess=false`).

## 3. Pendiente (cambio separado)
- [ ] 3.1 E2: decidir y refactorizar FKs navegación → escalares `*Id` (8 entidades, 18 relaciones).
