# Plan manual de QA de HelpDesk

## Alcance y leyenda

No existe proyecto ni ejecutor de pruebas automatizadas identificado. Este plan es manual y se ejecuta en ambiente controlado. `E` significa evidencia estática de código y especificación; `V` requiere verificación de despliegue, datos o infraestructura. Cada caso incluye éxito y rechazo esperado; registrar incidente cuando difieran.

## Prerrequisitos y evidencia

- Dos empresas de prueba, A y B, con datos distinguibles.
- Cuentas: administrador, supervisor, responsable de área, agente, solicitante, solo lectura, sin permiso y usuario sin persona vinculada.
- Área con responsable, dos agentes y categorías/subcategorías; activos disponibles, asignados y de baja; tickets en cada estado.
- Correo de prueba accesible y SMTP con escenario controlado de falla.
- Migraciones registradas, incluyendo activos, configuración, tickets, evidencias e indicadores cuando apliquen.
- Capturar por caso: ejecución, cuenta/empresa, fecha, pasos, UI, respuesta HTTP/JSON, ID/folio y consulta controlada de bitácora. No capturar contraseñas, tokens completos ni secretos.

## Trazabilidad

| Área | Especificación o evidencia |
|---|---|
| Persona-usuario | `vinculacion-persona-usuario`, `permisos-edicion-usuarios-personas` |
| Activos | `serial-unico-activo`, `mantenimiento-activo`, `mis-activos`, `notificacion-asignacion-activo`, `confirmacion-recepcion-activo` |
| Tickets | `foliador-tickets`, cambio `tickets-ciclo-vida`, controlador y servicio de tickets |
| Configuración e indicadores | `configuracion-empresa`, `metricas-desempeno` |
| Seguridad | `autorizacion-e2e`, `tenant-isolation`, `contrasenas`, `sesion-expiracion` |

## Casos sugeridos

| ID | Estado | Acción | Éxito esperado | Rechazo esperado | Evidencia | Pri. |
|---|---|---|---|---|---|---|
| AUT-01 | E+V | Iniciar sesión con credenciales válidas | Crea sesión y menú autorizado | Contraseña incorrecta no crea sesión y da error seguro | UI y respuesta | P0 |
| AUT-02 | E+V | Solicitar token con cliente válido e inválido | Cliente válido continúa | Cliente inválido no entrega token | HTTP sin secretos | P0 |
| AUT-03 | V | Probar HTTPS y HTTP en liberación | HTTPS opera | HTTP inseguro se rechaza/redirige | URL y cabeceras | P0 |
| AUT-04 | E+V | Consumir API protegida sin token | Ninguna operación ocurre | HTTP 401 | Traza HTTP | P0 |
| AUT-05 | E+V | Ejecutar escritura con token sin acción | Usuario autorizado completa | HTTP 403 o denegación; sin persistencia | HTTP y consulta | P0 |
| AUT-06 | V | Probar origen CORS permitido y ajeno | Permitido recibe CORS válido | Ajeno no recibe cabecera permisiva | Cabeceras | P1 |
| AUT-07 | E+V | Expirar sesión y abrir acción protegida | Sesión vigente opera | Expirada va a acceso; no ejecuta | UI y registro | P0 |
| AUT-08 | E+V | Abrir acción pública y acción fuera de allowlist sin sesión | Pública responde | Fuera de lista redirige al acceso | URL y HTTP | P0 |
| AUT-09 | V | Entrar con periodo vencido y vigente | Vigente opera | Vencido no entra ni revela detalle | UI | P0 |
| AUT-10 | E+V | Como A pedir datos/ID de B | A ve propios | B no se devuelve ni modifica | JSON A/B | P0 |
| PER-01 | E+V | Asignar página y acciones a rol | Menú y operación coinciden | Página visible sin acción no escribe | UI y 403 | P0 |
| PER-02 | E+V | Abrir URL directa sin lectura | Con lectura abre | Sin lectura va a denegado | URL/UI | P0 |
| PER-03 | E+V | Editar usuario/persona con lectura sin edición | Consulta disponible | Campos bloqueados y guardado rechazado | Antes/después | P0 |
| PER-04 | E+V | Crear con Crear sin Editar | Nuevo guarda | Existente no se altera | IDs/UI | P1 |
| PER-05 | E+V | Cambiar roles en selector de permisos | Conteos propios se mantienen | Sin contaminación ni fallo de tema oscuro | Video/UI | P2 |
| PERS-01 | E+V | Vincular persona a usuario existente | Advierte y sincroniza | Segundo vínculo/inexistente se rechaza | UI/datos | P1 |
| PERS-02 | E+V | Revisar persona vinculada y puesto | Campos definidos bloqueados; puesto se conserva | Cambio bloqueado no persiste | Antes/después | P1 |
| CAT-01 | E+V | Crear área, categoría y subcategoría | Se guardan relacionadas en A | Referencia inválida/ajena se rechaza | UI/JSON | P1 |
| CAT-02 | E+V | Crear tipo, marca y modelo; filtrar por marca | Catálogos coherentes | Datos de B no aparecen ni se aceptan | UI A/B | P1 |
| CAT-03 | E+V | Crear sucursal, puesto y compañía | Alta/listado por permiso | Sin Crear/Editar no persiste | ID/respuesta | P2 |
| EMP-01 | E+V | Registrar empresa con datos únicos | Crea datos iniciales | RFC, correo, razón o nombre duplicado rechaza | Correo/respuesta | P0 |
| EMP-02 | E+V | Consultar configuración como A y B | Solo datos propios | URL/parámetro ajeno no cambia B | UI A/B | P0 |
| EMP-03 | E+V | Guardar semana válida de siete días | Persiste todo | Menos de siete, inválida o fin <= inicio rechaza todo | Antes/después | P1 |
| EMP-04 | E+V | Desmarcar día laboral | Limpia horas y queda no laboral | No persiste horas residuales | UI/consulta | P1 |
| EMP-05 | E+V | Subir PNG/SVG válido | Vista previa, guardado y menú muestran logo | Vacío, extensión, MIME o peso inválido no guardan | UI/respuesta | P1 |
| EMP-06 | E+V | Reemplazar/quitar logo confirmando | Sustituye o restaura fallback | Cancelar/sin edición no modifica | UI/archivos | P2 |
| ACT-01 | E+V | Crear activo con serial único en A | Guarda y lista | Duplicado vigente en A rechaza amigablemente | JSON/UI | P0 |
| ACT-02 | E+V | Repetir serial en B, vacío y tras baja | B, nulo y reutilizado tras baja aceptan | Duplicado vigente A rechaza | Datos A/B | P0 |
| ACT-03 | E+V | Guardar serie local y notas de 250 | Serie opcional/no única; notas persisten | >250 no acepta ni trunca silencioso | Antes/después | P1 |
| ACT-04 | E+V | Guardar mantenimiento con comentario | Fecha automática e historial descendente | Vacío/sin edición no inserta | UI/historial | P1 |
| ACT-05 | E+V | Consultar mantenimiento A como B | A ve propio | B no ve ni registra A | JSON A/B | P0 |
| ASG-01 | E+V | Asignar activo a persona vinculada | Pendiente y notificaciones | Ya asignado/sin usuario rechaza | ID/correo | P0 |
| ASG-02 | V | Forzar falla SMTP en asignación | Normal registra bitácora | Falla devuelve error y compensa; nunca éxito | Bitácora/estado | P0 |
| ASG-03 | E+V | Abrir enlace válido e inválido | Muestra asignador/activo | GUID inválido no cambia estado | UI/HTTP | P0 |
| ASG-04 | E+V | Confirmar como titular vinculado | Marca aceptado y aparece en Mis Activos | Credencial ajena/incorrecta no confirma | Estado | P0 |
| ASG-05 | E+V | Reabrir enlace aceptado | Es idempotente | No duplica ni modifica | UI/consulta | P1 |
| ASG-06 | E+V | Iniciar y confirmar desvinculación | Titular finaliza relación | Admin no acepta; token ajeno no desvincula | Correo/estado | P0 |
| ASG-07 | E+V | Abrir Mis Activos con y sin vínculo | Muestra pendientes/aceptados del titular | Sin vínculo lista vacía, sin acceso a Personas | UI/JSON | P1 |
| TKT-01 | E+V | Crear ticket con datos válidos | Nuevo con folio persistido | Obligatorios, urgencia inválida o título >250 rechazan | Folio/JSON | P0 |
| TKT-02 | E+V | Elegir área, categoría y subcategoría | Cascada limita opciones | Relación ajena no se acepta | UI | P1 |
| TKT-03 | E+V | Crear simultáneamente en misma empresa | Folios únicos/secuenciales | Sin duplicado ni hueco por error transaccional | Folios | P0 |
| TKT-04 | E+V | Previsualizar y crear tras otro usuario | Guardado usa folio autoritativo | Vista previa obsoleta no colisiona | Dos sesiones | P1 |
| TKT-05 | E+V | Agente toma Nuevo de su área | En Progreso y agente asignado | Otro agente, área ajena o tomado no cambia | Estado/historial | P0 |
| TKT-06 | E+V | Resolver con comentario 1..300 | Resuelto con historial | Vacío, >300 o agente ajeno rechaza | Historial/JSON | P0 |
| TKT-07 | E+V | Cerrar como solicitante con comentario válido | Cerrado con actor | Ajeno, estado distinto, vacío o >300 rechaza | Historial/JSON | P0 |
| TKT-08 | E+V | Rechazar y retomar | Conserva movimientos | Actor/estado no elegible no cambia | Historial | P0 |
| TKT-09 | E+V | Reasignar como responsable a agente del área | Nueva asignación activa y En Progreso | Sin comentario, destino ajeno, no responsable o estado inválido rechaza | Historial/JSON | P0 |
| TKT-10 | E+V | Consultar detalle tras ciclo completo | Detalle solo lectura y asignación coherente | ID ajeno no filtra datos B | UI/JSON A/B | P0 |
| TKT-11 | E+V | Adjuntar archivos permitidos al crear | Ticket/evidencias se guardan juntos | Tipo, tamaño, exceso o fallo revierte todo | Folio/archivos | P0 |
| TKT-12 | E+V | Subir/descargar evidencia autorizada | Lista y descarga correcta | Evidencia/ticket B da no encontrado o denegado | HTTP/UI | P0 |
| TKT-13 | E+V | Eliminar ticket ajeno o sin Eliminar | Propio autorizado sigue regla | Ajeno no cambia; sin permiso rechaza | Estado | P0 |
| TKT-14 | V | Comparar visibilidad de acciones con Editar | Rol configurado permite | Botón visible sin Editar y 403 es defecto de coherencia | UI/403 | P1 |
| DASH-01 | E+V | Abrir panel principal con/sin datos | Valores o ceros seguros | Falla controlada, sin traza | UI/JSON | P2 |
| MET-01 | E+V | Abrir indicadores por rol | Roles autorizados con lectura acceden | No autorizado no ve menú ni URL | UI por rol | P0 |
| MET-02 | E+V | Aplicar rango válido, futuro e invertido | Válido recalcula | Futuro/invertido no consulta | UI/red | P1 |
| MET-03 | E+V | Comparar resumen contra datos preparados | Total, estados y eficiencia coinciden | Vacíos muestran cero sin división inválida | Cálculo | P1 |
| MET-04 | V | Validar viernes-lunes y horario empresa | Promedio usa horas hábiles y decimal | No suma fuera de jornada | Cálculo manual | P1 |
| MET-05 | E+V | Revisar pie, evolución y rankings | Orden, vacío y TOP N correctos | Tomas iniciales no cuentan; B no aparece en A | UI/datos A-B | P1 |
| MEN-01 | E+V | Revisar etiquetas de menú | Personal/Administración visibles | Llave interna y permisos no cambian | UI/rol | P2 |
| SEC-01 | V | Provocar validación y excepción controlada | Mensaje útil y registro seguro | Sin secretos, SQL ni stack trace | HTTP/log seguro | P0 |
| SEC-02 | V | Revisar recuperación, cambio y perfil | Secreto no aparece en HTML/JSON | Contraseña en claro/reversible/log es P0 | Captura controlada | P0 |
| SEC-03 | V | Revisar POST MVC contra CSRF | Operación legítima protegida | Cruce no autorizado no persiste | HTTP | P1 |
| UX-01 | V | Revisar acentos, temas y móvil | Legible y utilizable | Mojibake, contraste o desborde se registra | Capturas | P2 |

## Flujos de aceptación

| Flujo | Secuencia exitosa | Fallas obligatorias |
|---|---|---|
| Ticket completo | Solicitante crea -> agente toma -> resuelve -> solicitante cierra | Actor ajeno, comentario inválido, área ajena, permiso ausente, evidencia inválida |
| Reapertura | Solicitante rechaza -> agente retoma -> resuelve -> cierra | Rechazo sin comentario, retoma fuera de área, reasignación sin responsable |
| Entrega de activo | Admin asigna -> correo/bitácora -> titular autentica -> acepta | Sin vínculo, SMTP fallido, token inválido, credencial ajena, segundo clic |
| Devolución | Admin inicia -> correo -> titular autentica -> confirma | Admin aceptando, token ajeno, correo fallido, estado finalizado |
| Configuración e indicadores | Admin ajusta horario/logo -> crea tickets -> consulta rango | Horario/archivo inválido, sin permiso, fechas inválidas, cruce A/B |

## Cierre de liberación

- [ ] P0 aprobados o con excepción formal.
- [ ] Aislamiento A/B probado en API y web.
- [ ] Migraciones, SMTP, archivos y permisos del ambiente verificados.
- [ ] Cada falla tiene evidencia, severidad, responsable y decisión.
- [ ] Casos `V` no se aprueban solo por inspección estática.
