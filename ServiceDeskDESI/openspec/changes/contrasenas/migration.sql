/* ============================================================================
   ServiceDeskDESI — Migración: Hashing de contraseñas (W4/D3/M4/E1)
   ----------------------------------------------------------------------------
   Cambio: contrasenas
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi

   IMPORTANTE:
   - El SP AutenticarUsuario deja de comparar la contraseña (ciphertext) en SQL.
     Ahora solo devuelve el usuario por NombreUsuario; la verificación del hash
     PBKDF2 (con fallback a ciphertext legacy) se hace en C# (DbWrapper).
   - Las contraseñas de usuarios existentes (ciphertext Rijndael) siguen
     funcionando vía fallback en VerifyPassword; los nuevos usuarios se hashean.
   - Debe desplegarse JUNTO con el build nuevo del WebApi y del MVC.
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

CREATE OR ALTER PROCEDURE [dbo].[AutenticarUsuario]
(
    @NombreUsuario NVARCHAR(25)
)
AS
BEGIN
    SELECT u.*, 
           s.Nombre as SucursalNombre,
           s.Descripcion as SucursalDescripcion,
           s.Calle, s.Ciudad, s.Colonia, s.CodigoPostal,
           a.Nombre as AreaNombre,
           a.Descripcion as AreaDescripcion,
           a.Correo as AreaCorreo,
           e.Id as EmpresaId,
           e.NombreComercial as EmpresaNombreComercial,
           e.RazonSocial as EmpresaRazonSocial,
           e.RFC as EmpresaRFC,
           e.Responsable as EmpresaResponsable,
           e.Direccion as EmpresaDireccion,
           e.Ciudad as EmpresaCiudad,
           e.Estado as EmpresaEstado,
           e.CodigoPostal as EmpresaCodigoPostal,
           e.Telefono as EmpresaTelefono,
           e.CorreoContacto as EmpresaCorreoContacto,
           e.FechaVigenciaInicio,
           e.FechaVigenciaFin,
           e.EsPeriodoPrueba
    FROM Usuarios u
    LEFT JOIN Sucursal s ON u.SucursalId = s.Id
    LEFT JOIN Area a ON u.AreaId = a.Id
    LEFT JOIN Empresa e ON u.EmpresaId = e.Id
    WHERE u.NombreUsuario = @NombreUsuario 
        AND u.Estatus = 1
END
GO
