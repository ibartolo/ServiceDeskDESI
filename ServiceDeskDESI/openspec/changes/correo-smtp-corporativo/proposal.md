# Proposal: Correo saliente con SMTP corporativo (noreply@desipr.com.mx)

- **Change**: `correo-smtp-corporativo`
- **Fecha**: 2026-09-09
- **Origen**: Solicitud del usuario — usar buzón corporativo del hosting (SmarterASP) en lugar del correo Gmail personal para todos los envíos del sistema (bienvenida de empresa, recuperación de contraseña, asignación/desvinculación de activos).

## Intent

Cambiar la configuración SMTP del WebApi de Gmail (`desi56677@gmail.com`) al buzón corporativo `noreply@desipr.com.mx` creado en el hosting SmarterASP, de modo que los correos enviados por el sistema salgan del dominio de la empresa (`desipr.com.mx`). No requiere cambios de código: `EmailHelper` ya lee `smtpClient`, `port`, `userEmail` y `passEmail` de `appSettings`.

## Hecho

- `ServiceDeskDESIWebApi/Web.config` y `ServiceDeskDESIWebApi/bin/ServiceDeskDESIWebApi.dll.config` (config publicado que viaja al hosting):
  - `smtpClient` → `mail5010.site4now.net` (SMTP seguro SmarterASP; puerto 25 bloqueado por el ISP)
  - `port` → `587` (SSL/STARTTLS; compatible con `EnableSsl = true` que fuerza `EmailHelper`)
  - `userEmail` → `noreply@desipr.com.mx` (SmarterASP exige remitente = buzón autenticado)
  - `passEmail` → contraseña del buzón corporativo
- Credenciales Gmail anteriores quedaron **comentadas** en ambos archivos como respaldo.

## Datos SMTP del hosting (fuente: `información del correo.txt`)

- Webmail: `https://mail5010.site4now.net`
- SMTP plano: `mail.desipr.com.mx` — puertos 25/8889 (descartado: puerto 25 bloqueado por red del cliente)
- SMTP seguro: `mail5010.site4now.net` — puertos SSL SMTP 465/587 (elegido 587)
- MX: `igw10.site4now.net` | SPF: `v=spf1 a mx include:_spf.site4now.net -all` | DMARC: `p=reject`

## Verificación

- ✅ Envío de prueba real desde `noreply@desipr.com.mx` → `bartolocastro@gmail.com` recibido correctamente (SMTP autenticó y entregó sin errores).

## Archivos

- `ServiceDeskDESIWebApi/Web.config`
- `ServiceDeskDESIWebApi/bin/ServiceDeskDESIWebApi.dll.config`
- `información del correo.txt` (referencia del hosting, carpeta raíz)

## Nota de despliegue

El config publicado (`bin/ServiceDeskDESIWebApi.dll.config`) ya quedó actualizado, pero el sitio en el hosting debe republicarse (o subir los configs corregidos) para que producción use el correo corporativo.
