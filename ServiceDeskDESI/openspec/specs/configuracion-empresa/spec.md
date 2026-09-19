# configuracion-empresa Specification

## Purpose

Nueva página de menú independiente `ConfiguracionEmpresa` (etiqueta visible "Configuración de Empresa") donde el usuario autenticado **ve** los datos generales de su empresa (solo lectura) y **edita** únicamente dos criterios: (a) el horario laboral y (b) el logotipo (SVG/PNG). Incluye control de acceso por `RolPaginaAccion`, editor transaccional de horario (7 días), subida y visualización de logotipo, un footer estático "by DESi" en el layout y aislamiento multi-tenant por usuario autenticado. No existen operaciones de eliminación.

## Requirements

### Requirement: CE-001 — Página de menú independiente

El sistema MUST crear la página `Pagina.Nombre='ConfiguracionEmpresa'` (llave sin tilde) con `NombreVisible='Configuración de Empresa'` (con tilde), `Tipo='Menu'`, `Direccion='/ConfiguracionEmpresa'`, `Logo='fa-cog'`, `OrdenB=9` y `PermisosPadreId=NULL` (independiente, no dentro de Administración).

#### Scenario: Ítem visible en el menú

- GIVEN un usuario con un rol con `PuedeLeer=1` sobre `ConfiguracionEmpresa`
- WHEN inicia sesión y se renderiza el menú
- THEN ve "Configuración de Empresa" con ícono `fa-cog` en el menú principal

#### Scenario: Llave sin tilde, etiqueta con tilde

- GIVEN la fila `Pagina` sembrada
- THEN `Nombre='ConfiguracionEmpresa'` y `NombreVisible='Configuración de Empresa'`

### Requirement: CE-002 — Control de acceso por permisos

El sistema MUST sembrar `RolPaginaAccion` por defecto SOLO al rol `Administrador` (de todas las empresas existentes) con `PuedeLeer=1` y `PuedeEditar=1` (resto de flags en 0). El menú MUST mostrar el ítem únicamente cuando `PuedeLeer=1`. La vista MUST exigir `[Permiso("ConfiguracionEmpresa","Leer")]`; guardar horario, guardar logo y subir logo MUST exigir `[Permiso("ConfiguracionEmpresa","Editar")]` en MVC y WebApi. No existe acción "Eliminar". Si `PuedeLeer=0`, MUST redirigir a `Home/AccesoDenegado`.

#### Scenario: Administrador con acceso total

- GIVEN un usuario del rol Administrador
- WHEN abre "Configuración de Empresa"
- THEN puede ver y editar horario y logotipo

#### Scenario: Rol sin permiso no ve el menú

- GIVEN un rol sin `PuedeLeer` sobre `ConfiguracionEmpresa`
- WHEN el usuario inicia sesión
- THEN no ve el ítem en el menú

#### Scenario: Acceso directo sin permiso de lectura

- GIVEN un usuario sin `PuedeLeer=1`
- WHEN navega a `/ConfiguracionEmpresa`
- THEN es redirigido a `Home/AccesoDenegado`

#### Scenario: Editar sin permiso de edición

- GIVEN un usuario con `PuedeLeer=1` pero `PuedeEditar=0`
- WHEN intenta guardar horario o subir logotipo
- THEN la acción es rechazada (botones ocultos y endpoints protegidos)

### Requirement: CE-003 — Card de datos generales (solo lectura)

El sistema MUST mostrar los datos generales de la empresa autenticada — `NombreComercial`, `RazonSocial`, `RFC`, `Responsable`, `Direccion`, `Ciudad`, `Estado`, `CodigoPostal`, `Telefono`, `CorreoContacto`, `FechaVigenciaInicio`, `FechaVigenciaFin`, `EsPeriodoPrueba` — en modo solo lectura, sin permitir editarlos.

#### Scenario: Visualización de datos propios

- GIVEN un usuario autorizado
- WHEN abre la página
- THEN ve los datos generales de SU empresa

#### Scenario: No editables

- GIVEN la card de datos generales
- THEN ninguno de esos campos ofrece edición (solo horario y logotipo son editables)

### Requirement: CE-004 — Editor de horario laboral

El sistema MUST mostrar 7 filas (Lun=1…Dom=7) con checkbox "labora" + hora de inicio y fin por día, y un único botón "Guardar" que persiste la semana completa en una única transacción. Solo la componente de hora es significativa; la fecha se normaliza a un ancla fija. Los campos de hora MUST capturarse con formato 12h: tres listas desplegables por campo (Hora 1–12, Minuto en pasos de 5, y AM/PM); no se usa `datetime-local`.

#### Scenario: Carga del horario

- GIVEN un usuario autorizado
- WHEN abre la página
- THEN ve las 7 filas con sus valores actuales (`Estatus`, `HoraInicio`, `HoraFin`)

#### Scenario: Guardado de semana completa

- GIVEN el usuario ajusta uno o más días
- WHEN presiona "Guardar"
- THEN los 7 días se persisten atómicamente (o todos o ninguno)

#### Scenario: Validación fin > inicio

- GIVEN un día marcado "labora" con `HoraFin <= HoraInicio`
- WHEN intenta guardar
- THEN se rechaza con mensaje de error y no se persiste nada

#### Scenario: Ventana de un solo día (sin cruzar medianoche)

- GIVEN un día con horario que cruzaría a la madrugada del día siguiente
- WHEN intenta guardar
- THEN se rechaza (solo la componente de hora importa; `HoraFin > HoraInicio` garantiza ventana dentro del mismo día)

#### Scenario: Día desmarcado

- GIVEN el usuario desmarca el checkbox "labora" de un día
- WHEN guarda
- THEN ese día se persiste con `Estatus=0` y `HoraInicio`/`HoraFin` en NULL (horas ignoradas y limpiadas)

### Requirement: CE-005 — Defaults y backfill de horario

El sistema MUST sembrar por defecto Lun–Vie 09:00–17:00 laborables (`Estatus=1`) y Sáb/Dom no laborables (`Estatus=0`, horas NULL). El backfill MUST ser idempotente para empresas existentes, y las empresas nuevas MUST recibir los defaults al crearse.

#### Scenario: Backfill idempotente

- GIVEN empresas existentes sin filas en `EmpresaHorarioLaboral`
- WHEN se ejecuta la migración
- THEN se crean 7 filas con los defaults y re-ejecutar no duplica

#### Scenario: Empresa nueva

- GIVEN se registra una empresa nueva
- WHEN se crea
- THEN recibe 7 filas de horario con los defaults (dentro de la misma transacción)

### Requirement: CE-006 — Gestión del logotipo

El sistema MUST permitir subir un logotipo SVG o PNG; MUST validar servidor-side extensión + MIME (`image/svg+xml` o `image/png`), rechazar archivos vacíos, extensiones no permitidas y archivos que excedan `LogoEmpresaMaxTamanoKB` (default 2048). Los tipos permitidos se leen de `LogoEmpresaTiposPermitidos` (default `svg,png`). El archivo MUST guardarse en `Uploads/Logos/{empresaId}/{guid}.ext` (el servidor genera `{guid}.ext`, sin aceptar rutas del cliente). El binario MUST NOT viajar por WebApi; solo se persiste la URL parcial `Empresa.LogoUrl` (NVARCHAR(500) NULL) vía SP `GuardarLogoEmpresa(@Usuario, @LogoUrl)` con `[Permiso("ConfiguracionEmpresa","Editar")]`. El sidebar (`MenusUser.cshtml`) MUST mostrar la imagen de empresa SOLO si `LogoUrl` tiene valor; en caso contrario conserva el logo DESi actual (`fa-headset` + texto). El login NO cambia. El sistema MUST permitir además quitar el logotipo: una acción "Quitar logo" (con confirmación) MUST borrar el archivo físico y persistir `Empresa.LogoUrl = NULL`, restaurando el fallback DESi.

#### Scenario: Subida válida

- GIVEN un archivo SVG o PNG válido dentro del peso permitido
- WHEN el usuario lo sube
- THEN se guarda en `Uploads/Logos/{empresaId}/` y `Empresa.LogoUrl` se actualiza con la URL relativa

#### Scenario: Formato no permitido

- GIVEN un archivo con extensión o MIME distinto de svg/png
- WHEN lo sube
- THEN se rechaza con mensaje de error y no se guarda nada

#### Scenario: Archivo vacío u oversize

- GIVEN un archivo de 0 bytes o que excede `LogoEmpresaMaxTamanoKB`
- WHEN lo sube
- THEN se rechaza con mensaje de error

#### Scenario: Visualización con fallback

- GIVEN `Empresa.LogoUrl` con valor
- WHEN se renderiza el sidebar
- THEN muestra la imagen de la empresa; si `LogoUrl` es NULL/empty, muestra el logo DESi (`fa-headset` + texto)

#### Scenario: Re-subida reemplaza

- GIVEN la empresa ya tiene logotipo
- WHEN sube uno nuevo
- THEN `LogoUrl` se actualiza a la nueva imagen (el logotipo previo deja de mostrarse)

#### Scenario: Quitar logotipo

- GIVEN la empresa tiene un logotipo (`LogoUrl` con valor)
- WHEN el usuario presiona "Quitar logo" y confirma
- THEN se borra el archivo físico y `Empresa.LogoUrl` se persiste en NULL; el sidebar vuelve al fallback DESi

#### Scenario: Confirmación requerida para quitar

- GIVEN el usuario presiona "Quitar logo"
- THEN se muestra una confirmación antes de eliminar el logotipo

### Requirement: CE-007 — Footer estático "by DESi"

El sistema MUST añadir un footer estático en `_Layout.cshtml` después de `@RenderBody()` con logo DESi pequeño + texto "by DESi" + hipervínculos (Ayuda / Términos / Privacidad). MUST ser markup estático, no editable desde la página.

#### Scenario: Footer visible

- GIVEN una vista que usa `_Layout.cshtml`
- WHEN se renderiza
- THEN muestra el footer "by DESi" con los enlaces

#### Scenario: No editable

- GIVEN el footer
- THEN no existe UI para editarlo (markup estático)

### Requirement: CE-008 — Aislamiento multi-tenant

El sistema MUST resolver la empresa SIEMPRE desde el usuario autenticado — MVC vía `TokenCookie.EmpresaID`; WebApi vía claim `empresaId` / `@Usuario`→`Usuarios.EmpresaId` — y MUST NOT aceptar `EmpresaId` desde el cliente. Un agente de la empresa A MUST NOT ver ni modificar datos de la empresa B.

#### Scenario: Resolución por usuario

- GIVEN una petición de horario o logotipo
- WHEN se procesa
- THEN la empresa se obtiene del usuario autenticado, nunca de un parámetro del cliente

#### Scenario: Sin fuga entre empresas

- GIVEN un usuario de la empresa A
- WHEN intenta leer o escribir datos/horario/logotipo de la empresa B
- THEN la operación se resuelve solo contra la empresa A y B no se ve afectada

### Requirement: CE-009 — UX

El sistema MUST presentar textos en español con acentos correctos, views `.cshtml` UTF-8 CON BOM, un indicador de carga al guardar (p. ej. "Guardando…"), mensajes de éxito/error con el patrón de notificación existente de la app, y una vista previa del logotipo subido antes de guardar.

#### Scenario: Feedback de guardado

- GIVEN el usuario guarda horario o logotipo
- WHEN la operación está en curso
- THEN muestra spinner/"Guardando…" y, al terminar, mensaje de éxito o error según el patrón existente

#### Scenario: Vista previa del logotipo

- GIVEN el usuario selecciona un logotipo
- WHEN antes de guardar
- THEN se muestra una vista previa de la imagen

#### Scenario: Codificación del view

- GIVEN `Views/ConfiguracionEmpresa/Index.cshtml`
- THEN está en UTF-8 CON BOM

## Supuestos y decisiones (documentados para diseño/tareas)

1. **Quitar logotipo SÍ permitido** (decisión del usuario, revierte el supuesto anterior de "no permitido"): la empresa puede quitar su logotipo. La acción "Quitar logo" (con confirmación) borra el archivo físico bajo `Uploads/Logos/{empresaId}/` y persiste `Empresa.LogoUrl = NULL` reutilizando el SP `GuardarLogoEmpresa` (que ya admite NULL en su `UPDATE Empresa SET LogoUrl = @LogoUrl`). No se agrega ningún objeto nuevo de BD. El sidebar vuelve al fallback DESi (`fa-headset` + texto).
2. **Día desmarcado**: `Estatus=0` y `HoraInicio`/`HoraFin` se persisten en NULL (horas ignoradas/limpiadas), no se guardan valores residuales.
3. **Ventana de un solo día**: como la fecha se normaliza a un ancla fija y solo la componente de hora es significativa, exigir `HoraFin > HoraInicio` garantiza que no se representen turnos que crucen medianoche. `HoraFin == HoraInicio` también se rechaza.
4. **appSettings**: se usan `LogoEmpresaMaxTamanoKB` (default 2048) y `LogoEmpresaTiposPermitidos` (default `svg,png`) en `ServiceDeskDESIMVC/Web.config` (nombres y valores de la propuesta, no de explore).
5. **Editor de hora 12h (AM/PM)**: los campos `HoraInicio`/`HoraFin` se capturan con tres listas desplegables (Hora 1–12, Minuto en pasos de 5 y AM/PM), sin `datetime-local`. Internamente se convierte a/desde "HH:mm" (24h) sobre el ancla `1900-01-01`; el contrato con el backend (`List<HorarioLaboral>` / `HorarioViewModel` "HH:mm") no cambia.
