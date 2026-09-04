/* ============================================================================
   ServiceDeskDESI — Migración: Aislamiento entre Tenants (contención)
   ----------------------------------------------------------------------------
   Cambio: tenant-isolation (refs D1, D2, D15, W6, M6, M11)
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi (compatibilidad 150 / SQL Server 2019)

   Contenido:
     1. ALTERAR  EliminarTicket                  — valida propiedad de empresa (IDOR)
     2. ALTERAR  ObtenerUsuarioPagina            — filtra por tenant (@Usuario)
     3. ALTERAR  ObtenerUsuarioPaginaPorId       — filtra por tenant (@Usuario)
     4. ALTERAR  ObtenerUsuarioPorNombreUsuario  — filtra por tenant (@Usuario)
     5. CREAR    ObtenerEmpresaPorCorreoContacto — dedupe server-side (registro)
     6. CREAR    ObtenerEmpresaPorNombreComercial— dedupe server-side (registro)
     7. CREAR    ObtenerEmpresaPorRazonSocial    — dedupe server-side (registro)

   NOTA: Se usa CREATE OR ALTER para que el script sea idempotente
         (funciona tanto si el procedure ya existe como si no).
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

/* ----------------------------------------------------------------------------
   1. EliminarTicket: añade @Usuario y valida que el ticket pertenezca a la
      empresa del usuario autenticado antes del soft-delete (cierra IDOR).
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[EliminarTicket]
(
    @Id BIGINT,
    @ModificadoPor NVARCHAR(25),
    @FechaModificacion DATETIME,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    -- Validar que el ticket exista y pertenezca a la empresa del usuario autenticado
    IF NOT EXISTS(
        SELECT 1
        FROM Ticket t
        INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
        WHERE t.Id = @Id
            AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
            AND u.Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    UPDATE Ticket
    SET Estatus = 0,
        ModificadoPor = @ModificadoPor,
        FechaModificacion = @FechaModificacion
    WHERE Id = @Id

    SELECT @Id
END
GO

/* ----------------------------------------------------------------------------
   2. ObtenerUsuarioPagina: filtra por la empresa del usuario autenticado
      (vía UsuarioPagina.UsuarioId -> Usuarios.Id -> EmpresaId).
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuarioPagina]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT up.*
    FROM UsuarioPagina up
    INNER JOIN Usuarios u ON up.UsuarioId = u.Id
    WHERE u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

/* ----------------------------------------------------------------------------
   3. ObtenerUsuarioPaginaPorId: filtra por la empresa del usuario autenticado.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuarioPaginaPorId]
(
    @Id BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT up.*
    FROM UsuarioPagina up
    INNER JOIN Usuarios u ON up.UsuarioId = u.Id
    WHERE up.Id = @Id
        AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

/* ----------------------------------------------------------------------------
   4. ObtenerUsuarioPorNombreUsuario: añade @Usuario y filtra por la empresa
      del usuario autenticado.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuarioPorNombreUsuario]
(
    @NombreUsuario NVARCHAR(25),
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT u.*,
           s.Nombre as SucursalNombre,
           a.Nombre as AreaNombre,
           e.NombreComercial as EmpresaNombre
    FROM Usuarios u
    LEFT JOIN Sucursal s ON u.SucursalId = s.Id
    LEFT JOIN Area a ON u.AreaId = a.Id
    LEFT JOIN Empresa e ON u.EmpresaId = e.Id
    WHERE u.NombreUsuario = @NombreUsuario
        AND u.Estatus = 1
        AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

/* ----------------------------------------------------------------------------
   5-7. Procedures de unicidad para el dedupe server-side del registro de
       empresa (reemplazan la carga de todas las empresas). NO se exponen
       como endpoints; son lookups cross-tenant solo para validar unicidad.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerEmpresaPorCorreoContacto]
(
    @CorreoContacto NVARCHAR(250)
)
AS
BEGIN
    SELECT *
    FROM Empresa
    WHERE CorreoContacto = @CorreoContacto AND Estatus = 1
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerEmpresaPorNombreComercial]
(
    @NombreComercial NVARCHAR(250)
)
AS
BEGIN
    SELECT *
    FROM Empresa
    WHERE NombreComercial = @NombreComercial AND Estatus = 1
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerEmpresaPorRazonSocial]
(
    @RazonSocial NVARCHAR(250)
)
AS
BEGIN
    SELECT *
    FROM Empresa
    WHERE RazonSocial = @RazonSocial AND Estatus = 1
END
GO
