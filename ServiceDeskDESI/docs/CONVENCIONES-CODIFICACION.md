# Convenciones de idioma y codificación (ServiceDeskDESI)

> Regla de oro: **todo texto de la aplicación se escribe SIEMPRE con acentos y
> caracteres correctos del español**: á é í ó ú ü ñ, ¿ ¡, viñetas, comillas
> españolas, etc. Nunca "a" por "á", nunca "n" por "ñ", nunca texto sin acentos
> "para evitar problemas".

## Por qué

El proyecto (vistas `.cshtml`, templates `.html`, controladores `.cs`, correos)
contiene textos en español. Si un archivo se guarda con la codificación
incorrecta, los acentos se ven así en pantalla:

- ❌ `Ãndice rÃ¡pido:` (mojibake: UTF-8 leído como Windows-1252)
- ❌ `Configuraci�n`, `secci�n`
- ✅ `Índice rápido:`, `Configuración`, `sección`

## Reglas técnicas

1. **Los archivos `.cshtml` (vistas Razor) deben guardarse en UTF-8 CON BOM**
   (firma). Sin BOM, ASP.NET/Razor los interpreta como Windows-1252 y los
   acentos se ven como `Ã` / `�`.

   - Visual Studio: **Archivo → Guardar como → Guardar con codificación… →
     "Unicode (UTF-8 con firma) - página de códigos 65001"**.
   - Las vistas existentes del proyecto ya tienen BOM; una vista nueva debe
     tenerlo también.

2. **Los demás archivos** (`.cs`, `.html`, `.config`, `.sql`, `.md`) también se
   guardan en UTF-8 (con BOM recomendado en `.cshtml` y `.html`).

3. La codificación es responsabilidad del **guardado del archivo**, no del
   contenido: escribe siempre el acento correcto y asegúrate de que el archivo
   quede en UTF-8. No "traduzcas" quitando acentos.

4. **Al crear o editar** cualquier archivo con texto en español (vistas,
   templates de correo, manuales, mensajes), verifica:
   - que el acento se vea bien en el editor, y
   - que el archivo tenga **BOM UTF-8** (revisar el primer byte o el diálogo de
     codificación del editor).

5. **Si aparece mojibake** (tipo `Ãndice`), el problema es de codificación de
   lectura → agrega el **BOM UTF-8** al archivo y recarga (Ctrl+F5 / reinicia el
   sitio de desarrollo).

## Ejemplo rápido de verificación (PowerShell)

```powershell
$b = [System.IO.File]::ReadAllBytes("ruta\Vista.cshtml")
$tieneBom = $b.Length -ge 3 -and $b[0] -eq 0xEF -and $b[1] -eq 0xBB -and $b[2] -eq 0xBF
$tieneBom   # True = correcto
```
