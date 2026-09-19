# Convenciones observadas de codificación

Este documento describe patrones visibles en la solución; no convierte recomendaciones en reglas ya establecidas.

## Estructura

- La solución usa .NET Framework 4.8, MVC 5, Web API 2 y entidades compartidas.
- Flujo habitual: controlador MVC -> servicio MVC -> `HttpClientConnection` -> controlador API -> servicio API -> `DbWrapper` ADO.NET -> procedimiento almacenado.
- MVC y API agrupan conexiones y datos en archivos parciales por dominio, por ejemplo `HttpClientConnection.Ticket.cs` y `DbWrapper.Ticket.cs`.
- Entidades y DTO se organizan en `Catalogos`, `Seguridad` y `Tickets`. Reutilice el tipo que represente el contrato real antes de crear duplicados.

## Controladores, servicios y datos

- Los controladores construyen servicios y devuelven vistas o JSON con Newtonsoft.Json.
- Servicios API validan negocio y registran con Serilog. Valide antes de invocar datos.
- `DbWrapper` encapsula parámetros SQL, procedimientos y mapeo de lectores. No coloque SQL ad hoc en controladores.
- Para varias escrituras relacionadas, el patrón existente usa la misma instancia de `DbWrapper` y transacción explícita. Mantenga atomicidad y compense archivos físicos si la base revierte.

## Seguridad y contratos

- API usa `[Authorize]` y `[Permiso]`; MVC combina filtro global de sesión y `[Permiso]` por acción. La UI no sustituye el control servidor.
- Obtenga usuario y empresa desde sesión o identidad. No confíe empresa, actor o rutas físicas proporcionadas por cliente.
- El contrato observado es `ModelResponse` o `ModelResponse<T>`: `IsSuccess`, `Message`, `Response`; el éxito inicia en falso.
- En validaciones devuelva `IsSuccess=false` y mensaje accionable. En excepciones, registre detalle seguro y no exponga secretos, SQL ni stack traces.

## Proyectos clásicos

Los `.csproj` son de formato clásico y registran cada `Compile`, `Content` y referencia. Al añadir `.cs`, `.cshtml`, recurso o partial:

1. Inclúyalo explícitamente en el `.csproj` correcto.
2. Conserve `ProjectReference` y `DependentUpon` cuando correspondan.
3. No asuma descubrimiento automático de archivos de formatos SDK modernos.

## Idioma y codificación

- Use español con acentos correctos; no los elimine como solución de codificación.
- Verifique UTF-8 con BOM al editar vistas Razor con texto acentuado. El historial documenta riesgo de mojibake y las vistas nuevas de los cambios revisados lo requieren.
- Mantenga nombres existentes, incluso si combinan inglés y español; una normalización amplia requiere cambio separado y pruebas.

## Recomendaciones pendientes

- `DataAnnotations` y `ModelState` no son una práctica generalizada observada.
- Manejador global de excepciones, HTTP homogéneo, antiforgery/CSRF y paginación son deuda documentada, no capacidades confirmadas.
- Antes de cambiar procedimiento o entidad compartida, trace impacto en entidad, `DbWrapper`, servicio/controlador API, conexión/servicio/controlador MVC y vista.
