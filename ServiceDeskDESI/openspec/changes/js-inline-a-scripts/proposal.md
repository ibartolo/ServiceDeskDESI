# Proposal: Mover JavaScript inline de las vistas a archivos en Scripts/

- **Change**: `js-inline-a-scripts`
- **Fecha**: 2026-09-09
- **Origen**: Requerimiento de QA/cliente — el JavaScript no debe permanecer dentro de las vistas Razor del MVC.

## Intent

Sacar todo el JavaScript que estaba embebido en los bloques `<script>` inline de las vistas `.cshtml` del `ServiceDeskDESIMVC` y llevarlo a archivos `.js` externos bajo `Scripts/`, replicando la ruta de la vista (`Views/<Folder>/<View>.cshtml` → `Scripts/<Folder>/<View>.js`). Se busca separar markup de comportamiento, mejorar cacheo del navegador y facilitar mantenimiento.

El JavaScript que depende de Razor (renderizado server-side) NO puede moverse: se conserva en un bloque `<script>` inline mínimo dentro de la vista, **antes** de la referencia al `.js` externo, para que las variables inyectadas por el servidor existan cuando corra el script externo.

## Alcance

- **30 vistas** del MVC con script inline, **29 archivos `.js` nuevos** creados.
- Handlers inline en el HTML (`onclick="..."`, `onchange="..."`) se dejan en la vista (fuera de alcance por decisión del usuario).
- Etiquetas `<script src="...">` de CDN se dejan intactas.
- No hay `BundleConfig` en el proyecto: la referencia se hace con `<script src="@Url.Content("~/Scripts/<Folder>/<View>.js")"></script>`.

## Patrón aplicado

**Sin dependencias de Razor:**
```html
<script src="@Url.Content("~/Scripts/Home/MisActivos.js")"></script>
```

**Con dependencias de Razor:**
```html
@* Datos de Razor expuestos a JavaScript (deben declararse antes del script externo) *@
<script>
    var permisosGlobal = @Html.Raw(JsonConvert.SerializeObject(permisos));
</script>
<script src="@Url.Content("~/Scripts/Catalogs/Mark.js")"></script>
```

## Vistas afectadas (30)

- **Catalogs (13):** Active, Branch, CategoriaResponsable, Category, Company, Mark, Model, Persona, Puesto, TypeActive, WorkArea, _AsignarActivoPersona, _MantenimientoActivo
- **Home (5):** Ayuda, Configuration, MisActivos, NewCompany, VerAsignacion
- **Security (2):** Permisos, Role
- **Ticket (4):** Index, _CapturarTicket, _DetalleTicket, _ReasignarTicket
- **User (2):** MyProfile, Users
- **ConfiguracionEmpresa (1):** Index
- **Estadisticas (1):** Index
- **Shared (1):** _Layout

> `Home/RecoverPassword.cshtml` no generó `.js`: su único bloque inline era solo Razor (`const token = '@ViewBag.Token';`) y su lógica ya vivía en `Scripts/Comun/RecoverPass.js`.

## Casos especiales resueltos

- **Estadisticas/Index**: `cargarEstadisticas('@fechaInicio','@fechaFin')` → hoisted a `fechaInicioGlobal`/`fechaFinGlobal` en el bloque inline; el `.js` llama `cargarEstadisticas(fechaInicioGlobal, fechaFinGlobal)`.
- **User/Users**: `EmpresaId: @ViewBag.EmpresaId` → `var empresaId = @ViewBag.EmpresaId;` inline y `EmpresaId: empresaId` en el `.js`.
- **Security/Permisos**: `todasLasPaginas` resuelto con `@if (paginas != null)` inline.
- **Catalogs/Active**: `var tablaActivo;` (JS puro) al `.js`; solo las asignaciones Razor quedan inline.
- **Shared/_Layout**: script del `<head>` movido a `Scripts/Shared/_Layout.js` (sin Razor), referencia en el mismo lugar (después de jQuery y `Comun.js`).

## Archivos

- `ServiceDeskDESIMVC/Views/**` (30 vistas editadas)
- `ServiceDeskDESIMVC/Scripts/**` (29 `.js` nuevos)
- `ServiceDeskDESIMVC/ServiceDeskDESIMVC.csproj` — 29 `<Content Include="Scripts\...\*.js" />` agregados

## Verificación

- ✅ `node --check` sobre los 33 `.js` de `Scripts/` → 33/33 sin errores de sintaxis.
- ✅ Compilación Razor con MSBuild `/p:MvcBuildViews=true` → exit 0, sin errores en vistas.
- ✅ Sin residuos de Razor (`@`) en los `.js` nuevos.
- ✅ Los únicos `<script>` inline restantes son bloques de datos Razor.

## Nota de despliegue

El `.csproj` usa lista explícita de `<Content Include>` (no globs). Los `.js` nuevos quedaron registrados en el `.csproj`; al publicar debe incluirse la carpeta `Scripts/` completa con sus subcarpetas (`Catalogs`, `Home`, `Security`, `Ticket`, `User`, `Estadisticas`, `ConfiguracionEmpresa`, `Shared`).
