# Unit Test Record: UserController.ActualizarPerfilUsuario

## Target File
`ServiceDeskDESIMVC/Controllers/UserController.cs`

## Session
ses_1 | Timestamp: 2026-08-30T21:28:45Z

## Test Status
- Isolated unit test: NOT CREATED (no test project/test runner exists in this legacy .NET Framework MVC solution; creating one would require new csproj/package infrastructure, outside the single-file assignment scope per Commander instruction "No modifiques nada más del archivo").
- Verification method used instead: **full MSBuild compilation** of the MVC project.

## Build Verification
Command:
```
"C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" ServiceDeskDESIMVC.csproj /t:Build /p:Configuration=Debug
```
Result: SUCCESS
```
ServiceDeskDESIEntities -> ...\bin\Debug\ServiceDeskDESIEntities.dll
ServiceDeskDESIMVC -> ...\bin\ServiceDeskDESIMVC.dll
```
Warnings: only pre-existing CS0168 (unused variable `ex` in catch blocks — present before this change).

## Symbol Verification (pre-edit)
- `using Newtonsoft.Json;` — present (line 1) → `JsonConvert` available
- `using ServiceDeskDESIMVC.Helpers;` — present (line 5) → `SessionHelper` available
- `SessionHelper.GetSessionUser()` returns `TokenCookie` — exists (SessionHelper.cs:43)
- `SessionHelper.CreateSession(string id)` — exists (SessionHelper.cs:60); same call pattern used at HomeController.cs:221
- `TokenCookie.ProfileImage` — exists (TokenCookie.cs:15)
- `usuario.ImagenPerfil` — exists (Usuario entity)
- `_autenticacionService.ActualizarPerfilUsuario(Usuario)` returns `Task<ModelResponse<Usuario>>` → `response.IsSuccess` valid (AutenticacionService.cs:25)

## Edit Applied
Replaced:
```csharp
var response = await _autenticacionService.ActualizarPerfilUsuario(usuario);
return JsonConvert.SerializeObject(response);
```
with:
```csharp
var response = await _autenticacionService.ActualizarPerfilUsuario(usuario);

// Refrescar la sesión con la nueva imagen para que el navbar (user-avatar del _Layout) la muestre de inmediato
if (response.IsSuccess)
{
    var tokenCookie = SessionHelper.GetSessionUser();
    if (tokenCookie != null)
    {
        tokenCookie.ProfileImage = usuario.ImagenPerfil;
        SessionHelper.CreateSession(JsonConvert.SerializeObject(tokenCookie));
    }
}

return JsonConvert.SerializeObject(response);
```
