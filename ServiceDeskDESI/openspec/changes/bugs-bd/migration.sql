/* ============================================================================
   ServiceDeskDESI — Migración: Bugs de BD que rompen flujos
   ----------------------------------------------------------------------------
   Cambio: bugs-bd (refs D6, D10, D11, D12, D13)
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi (compatibilidad 150 / SQL Server 2019)

   Contenido:
     1. ALTERAR  GuardarRolParaNuevaEmpresa      — añade PuedeAtenderTickets (D6)
     2. ALTERAR  AsignarRolUsuario               — valida rol vía Rol.CreadoPor (D10)
     3. ALTERAR  ObtenerEmpresas                 — quita JOIN muerto (D11)
     4. ALTERAR  GuardarOActualizarUsuarioPagina — corrige nvarchaR + @@IDENTITY (D12)
     5. ALTERAR  ObtenerUsuarioPorId             — restaura filtro Estatus=1 (D13)
     6. ALTERAR  ObtenerUsuarios                 — añade filtro Estatus=1 (D13)

   NOTA: Se usa CREATE OR ALTER para que el script sea idempotente.
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

/* ----------------------------------------------------------------------------
   1. D6 — GuardarRolParaNuevaEmpresa: recibe PuedeAtenderTickets para que el
      rol Administrador de una empresa nueva pueda atender tickets.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[GuardarRolParaNuevaEmpresa]
(
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(250) = NULL,
    @PuedeAtenderTickets BIT,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME
)
AS
BEGIN
    INSERT INTO Rol (Nombre, Descripcion, PuedeAtenderTickets, CreadoPor, FechaCreacion, Estatus)
    VALUES (@Nombre, @Descripcion, @PuedeAtenderTickets, @CreadoPor, @FechaCreacion, 1)

    SELECT SCOPE_IDENTITY()
END
GO

/* ----------------------------------------------------------------------------
   2. D10 — AsignarRolUsuario: valida que el rol PERTENEZCA a la empresa
      (vía Rol.CreadoPor), no que ya esté asignado a alguien (UsuarioRol).
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[AsignarRolUsuario]
(
    @UsuarioId BIGINT,
    @RolId BIGINT,
    @AsignadoPor NVARCHAR(25),
    @EmpresaId BIGINT
)
AS
BEGIN
    -- Validar que el usuario que asigna sea administrador
    IF NOT EXISTS(
        SELECT 1 
        FROM Usuarios u
        INNER JOIN UsuarioRol ur ON u.Id = ur.UsuarioId
        INNER JOIN Rol r ON ur.RolId = r.Id
        WHERE u.NombreUsuario = @AsignadoPor 
            AND u.EmpresaId = @EmpresaId
            AND u.Estatus = 1
            AND r.Nombre = 'Administrador'
            AND ur.Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    -- Validar que el usuario destino pertenezca a la misma empresa
    IF NOT EXISTS(
        SELECT 1 
        FROM Usuarios 
        WHERE Id = @UsuarioId 
            AND EmpresaId = @EmpresaId
            AND Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    -- Validar que el rol pertenezca a la empresa (vía Rol.CreadoPor)
    IF NOT EXISTS(
        SELECT 1 
        FROM Rol r
        INNER JOIN Usuarios u ON r.CreadoPor = u.NombreUsuario
        WHERE r.Id = @RolId 
            AND u.EmpresaId = @EmpresaId
            AND u.Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    -- Validar que no tenga ya ese rol asignado
    IF EXISTS(
        SELECT 1 
        FROM UsuarioRol 
        WHERE UsuarioId = @UsuarioId 
            AND RolId = @RolId 
            AND Estatus = 1
    )
    BEGIN
        SELECT -1
        RETURN
    END

    INSERT INTO UsuarioRol
    (UsuarioId, RolId, CreadoPor, FechaCreacion, Estatus)
    VALUES
    (@UsuarioId, @RolId, @AsignadoPor, GETDATE(), 1)

    SELECT SCOPE_IDENTITY()
END
GO

/* ----------------------------------------------------------------------------
   3. D11 — ObtenerEmpresas: se elimina el INNER JOIN a Usuarios que no se
      usaba y que duplicaba filas si CreadoPor coincidía con varios usuarios.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerEmpresas]
AS
BEGIN
    SELECT e.*
    FROM Empresa e
    WHERE e.Estatus = 1
    ORDER BY NombreComercial
END
GO

/* ----------------------------------------------------------------------------
   4. D12 — GuardarOActualizarUsuarioPagina: se corrige el tipo inexistente
      nvarchaR por nvarchar(25) y @@IDENTITY por SCOPE_IDENTITY().
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarUsuarioPagina]
(
    @Id BIGINT,
    @UsuarioID BIGINT = NULL,
    @PaginaID BIGINT = NULL,
    @Estatus BIT = NULL,
    @CreadoPor NVARCHAR(25) = NULL,
    @FechaCreacion DATETIME = NULL,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL
)
AS
BEGIN
    IF EXISTS (SELECT * FROM UsuarioPagina WHERE Id = @Id)
    BEGIN
        UPDATE UsuarioPagina
        SET UsuarioID = @UsuarioID,
            PaginaID = @PaginaID,
            Estatus = @Estatus,
            CreadoPor = @CreadoPor,
            FechaCreacion = @FechaCreacion,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        INSERT INTO UsuarioPagina (UsuarioID, PaginaID, Estatus, CreadoPor, FechaCreacion, ModificadoPor, FechaModificacion)
        VALUES (@UsuarioID, @PaginaID, @Estatus, @CreadoPor, @FechaCreacion, @ModificadoPor, @FechaModificacion)

        SELECT SCOPE_IDENTITY()
    END
END
GO

/* ----------------------------------------------------------------------------
   5. D13 — ObtenerUsuarioPorId: se restaura el filtro Estatus=1 para no
      devolver usuarios borrados lógicamente.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuarioPorId]
(
    @Id BIGINT,
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
    WHERE u.Id = @Id 
        AND u.Estatus = 1
        AND u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

/* ----------------------------------------------------------------------------
   6. D13 — ObtenerUsuarios: se añade el filtro Estatus=1 para no listar
      usuarios borrados lógicamente.
---------------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE [dbo].[ObtenerUsuarios]
(
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
    WHERE u.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        AND u.Estatus = 1
    ORDER BY u.NombreUsuario
END
GO
