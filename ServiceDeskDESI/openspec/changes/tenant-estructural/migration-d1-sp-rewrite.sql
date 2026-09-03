/* ============================================================================
   ServiceDeskDESI — Migración: Tenant de primera clase (D1) — SPs + NOT NULL
   ----------------------------------------------------------------------------
   Cambio: tenant-estructural (compleción D1)
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi

   CONTEXTO: esta es la SEGUNDA parte de D1. La primera (columnas EmpresaId NULL,
   backfill, FKs, índice único, SPs de registro y GuardarOActualizarActivo/Area/
   Categoria) ya está aplicada. Este script:
     1. Endurece EmpresaId a NOT NULL (re-backfill por seguridad).
     2. Reescribe TODOS los SPs restantes para filtrar/insertar por EmpresaId
        (en vez del JOIN por CreadoPor).
   Usa CREATE OR ALTER => idempotente.
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

-- 1. Re-backfill (idempotente) por si hay filas con EmpresaId NULL
UPDATE a SET a.EmpresaId = u.EmpresaId FROM Activo a INNER JOIN Usuarios u ON a.CreadoPor = u.NombreUsuario WHERE a.EmpresaId IS NULL
GO
UPDATE a SET a.EmpresaId = u.EmpresaId FROM Area a INNER JOIN Usuarios u ON a.CreadoPor = u.NombreUsuario WHERE a.EmpresaId IS NULL
GO
UPDATE c SET c.EmpresaId = u.EmpresaId FROM Categoria c INNER JOIN Usuarios u ON c.CreadoPor = u.NombreUsuario WHERE c.EmpresaId IS NULL
GO
UPDATE cr SET cr.EmpresaId = u.EmpresaId FROM CategoriaResponsable cr INNER JOIN Usuarios u ON cr.CreadoPor = u.NombreUsuario WHERE cr.EmpresaId IS NULL
GO
UPDATE m SET m.EmpresaId = u.EmpresaId FROM Marca m INNER JOIN Usuarios u ON m.CreadoPor = u.NombreUsuario WHERE m.EmpresaId IS NULL
GO
UPDATE mo SET mo.EmpresaId = u.EmpresaId FROM Modelo mo INNER JOIN Usuarios u ON mo.CreadoPor = u.NombreUsuario WHERE mo.EmpresaId IS NULL
GO
UPDATE p SET p.EmpresaId = u.EmpresaId FROM Persona p INNER JOIN Usuarios u ON p.CreadoPor = u.NombreUsuario WHERE p.EmpresaId IS NULL
GO
UPDATE p SET p.EmpresaId = u.EmpresaId FROM Puesto p INNER JOIN Usuarios u ON p.CreadoPor = u.NombreUsuario WHERE p.EmpresaId IS NULL
GO
UPDATE r SET r.EmpresaId = u.EmpresaId FROM Rol r INNER JOIN Usuarios u ON r.CreadoPor = u.NombreUsuario WHERE r.EmpresaId IS NULL
GO
UPDATE s SET s.EmpresaId = u.EmpresaId FROM Sucursal s INNER JOIN Usuarios u ON s.CreadoPor = u.NombreUsuario WHERE s.EmpresaId IS NULL
GO
UPDATE t SET t.EmpresaId = u.EmpresaId FROM Ticket t INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario WHERE t.EmpresaId IS NULL
GO
UPDATE ta SET ta.EmpresaId = u.EmpresaId FROM TipoActivo ta INNER JOIN Usuarios u ON ta.CreadoPor = u.NombreUsuario WHERE ta.EmpresaId IS NULL
GO

-- 2. NOT NULL
ALTER TABLE [dbo].[Activo] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Area] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Categoria] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[CategoriaResponsable] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Marca] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Modelo] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Persona] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Puesto] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Rol] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Sucursal] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[Ticket] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO
ALTER TABLE [dbo].[TipoActivo] ALTER COLUMN [EmpresaId] [bigint] NOT NULL
GO

-- ============================================================================
-- 3. SPs reescritos para filtrar/insertar por EmpresaId
--    (los que no aparecen aquí ya quedaron en la primera migración de D1)
-- ============================================================================

-- GuardarOActualizarMarca
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarMarca]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(250) = NULL,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Marca WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @Id AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE Marca SET Nombre=@Nombre, Descripcion=@Descripcion, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        INSERT INTO Marca (Nombre, Descripcion, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarModelo
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarModelo]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(250) = NULL, @MarcaId BIGINT,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Modelo WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Modelo mo WHERE mo.Id = @Id AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @MarcaId AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE Modelo SET Nombre=@Nombre, Descripcion=@Descripcion, MarcaId=@MarcaId, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @MarcaId AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        INSERT INTO Modelo (Nombre, Descripcion, MarcaId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @MarcaId, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarPersona
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarPersona]
(
    @Id BIGINT, @Nombre NVARCHAR(150), @Apellido NVARCHAR(250), @Correo NVARCHAR(250) = NULL,
    @Telefono NVARCHAR(50) = NULL, @PuestoId BIGINT, @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL, @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Puesto p WHERE p.Id = @PuestoId AND p.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END

    IF EXISTS(SELECT * FROM Persona WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Persona p WHERE p.Id = @Id AND p.EmpresaId = @EmpresaId)
        BEGIN SELECT 0 RETURN END
        UPDATE Persona SET Nombre=@Nombre, Apellido=@Apellido, Correo=@Correo, Telefono=@Telefono, PuestoId=@PuestoId, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        INSERT INTO Persona (Nombre, Apellido, Correo, Telefono, PuestoId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Apellido, @Correo, @Telefono, @PuestoId, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarPuesto
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarPuesto]
(
    @Id BIGINT, @Nombre NVARCHAR(100), @Descripcion NVARCHAR(250) = NULL,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END

    IF EXISTS(SELECT * FROM Puesto WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Puesto p WHERE p.Id = @Id AND p.EmpresaId = @EmpresaId)
        BEGIN SELECT 0 RETURN END
        UPDATE Puesto SET Nombre=@Nombre, Descripcion=@Descripcion, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF EXISTS(SELECT 1 FROM Puesto p WHERE p.Nombre = @Nombre AND p.EmpresaId = @EmpresaId)
        BEGIN SELECT -1 RETURN END
        INSERT INTO Puesto (Nombre, Descripcion, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarRol
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarRol]
(
    @Id BIGINT, @Nombre NVARCHAR(50), @Descripcion NVARCHAR(250) = NULL, @PuedeAtenderTickets BIT,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF EXISTS(SELECT * FROM Rol WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Rol r WHERE r.Id = @Id AND r.EmpresaId = @EmpresaId)
        BEGIN SELECT 0 RETURN END
        UPDATE Rol SET Nombre=@Nombre, Descripcion=@Descripcion, PuedeAtenderTickets=@PuedeAtenderTickets, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF EXISTS(SELECT 1 FROM Rol r WHERE r.Nombre = @Nombre AND r.EmpresaId = @EmpresaId)
        BEGIN SELECT -1 RETURN END
        INSERT INTO Rol (Nombre, Descripcion, PuedeAtenderTickets, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @PuedeAtenderTickets, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarSucursal
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarSucursal]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(500) = NULL,
    @Calle NVARCHAR(100) = NULL, @Ciudad NVARCHAR(100) = NULL, @Colonia NVARCHAR(100) = NULL,
    @CodigoPostal NVARCHAR(10) = NULL, @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Sucursal WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Sucursal s WHERE s.Id = @Id AND s.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE Sucursal SET Nombre=@Nombre, Descripcion=@Descripcion, Calle=@Calle, Ciudad=@Ciudad, Colonia=@Colonia, CodigoPostal=@CodigoPostal, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        INSERT INTO Sucursal (Nombre, Descripcion, Calle, Ciudad, Colonia, CodigoPostal, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @Calle, @Ciudad, @Colonia, @CodigoPostal, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarTicket
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarTicket]
(
    @Id BIGINT, @Area BIGINT, @Categoria BIGINT, @Subcategoria BIGINT = NULL, @Urgencia INT,
    @Titulo NVARCHAR(250), @Descripcion NVARCHAR(MAX), @TicketEstatusId INT,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Area a WHERE a.Id = @Area AND a.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Categoria c WHERE c.Id = @Categoria AND c.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM TicketEstatus WHERE Id = @TicketEstatusId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END

    IF EXISTS(SELECT * FROM Ticket WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Ticket t WHERE t.Id = @Id AND t.EmpresaId = @EmpresaId)
        BEGIN SELECT 0 RETURN END
        UPDATE Ticket SET AreaId=@Area, CategoriaId=@Categoria, SubcategoriaId=@Subcategoria, Urgencia=@Urgencia, Titulo=@Titulo, Descripcion=@Descripcion, TicketEstatusId=@TicketEstatusId, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        INSERT INTO Ticket (AreaId, CategoriaId, SubcategoriaId, Urgencia, Titulo, Descripcion, TicketEstatusId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Area, @Categoria, @Subcategoria, @Urgencia, @Titulo, @Descripcion, @TicketEstatusId, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarTipoActivo
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarTipoActivo]
(
    @Id BIGINT, @Nombre NVARCHAR(250), @Descripcion NVARCHAR(250) = NULL,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM TipoActivo WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM TipoActivo ta WHERE ta.Id = @Id AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        BEGIN SELECT 0 RETURN END
        UPDATE TipoActivo SET Nombre=@Nombre, Descripcion=@Descripcion, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @CreadoPor AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1) AND Estatus = 1)
        BEGIN SELECT 0 RETURN END
        INSERT INTO TipoActivo (Nombre, Descripcion, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@Nombre, @Descripcion, @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- GuardarOActualizarCategoriaResponsable
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarCategoriaResponsable]
(
    @Id BIGINT, @CategoriaId BIGINT, @UsuarioId BIGINT, @EsPrincipal BIT,
    @CreadoPor NVARCHAR(25), @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL, @FechaModificacion DATETIME = NULL,
    @Estatus BIT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE Id = @UsuarioId AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND r.Estatus = 1 AND ur.Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Categoria c WHERE c.Id = @CategoriaId AND c.EmpresaId = @EmpresaId AND c.Estatus = 1)
    BEGIN SELECT 0 RETURN END

    IF EXISTS(SELECT * FROM CategoriaResponsable WHERE Id = @Id)
    BEGIN
        UPDATE CategoriaResponsable SET CategoriaId=@CategoriaId, UsuarioId=@UsuarioId, EsPrincipal=@EsPrincipal, ModificadoPor=@ModificadoPor, FechaModificacion=@FechaModificacion, Estatus=@Estatus WHERE Id=@Id
        SELECT @Id
    END
    ELSE
    BEGIN
        IF @EsPrincipal = 1
        BEGIN
            UPDATE CategoriaResponsable SET EsPrincipal = 0 WHERE CategoriaId = @CategoriaId AND Estatus = 1
        END
        INSERT INTO CategoriaResponsable (CategoriaId, UsuarioId, EsPrincipal, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES (@CategoriaId, @UsuarioId, @EsPrincipal, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)
        SELECT SCOPE_IDENTITY()
    END
END
GO

-- EliminarActivo
CREATE OR ALTER PROCEDURE [dbo].[EliminarActivo]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Activo a WHERE a.Id = @Id AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Activo SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarArea
CREATE OR ALTER PROCEDURE [dbo].[EliminarArea]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Area a WHERE a.Id = @Id AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Area SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarCategoria
CREATE OR ALTER PROCEDURE [dbo].[EliminarCategoria]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Categoria c WHERE c.Id = @Id AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Categoria SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarMarca
CREATE OR ALTER PROCEDURE [dbo].[EliminarMarca]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Marca m WHERE m.Id = @Id AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Marca SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarModelo
CREATE OR ALTER PROCEDURE [dbo].[EliminarModelo]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Modelo mo WHERE mo.Id = @Id AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Modelo SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarPersona
CREATE OR ALTER PROCEDURE [dbo].[EliminarPersona]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @ModificadoPor AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Persona p WHERE p.Id = @Id AND p.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    UPDATE Persona SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarPuesto
CREATE OR ALTER PROCEDURE [dbo].[EliminarPuesto]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @ModificadoPor AND EmpresaId = @EmpresaId AND Estatus = 1)
    BEGIN SELECT 0 RETURN END
    IF NOT EXISTS(SELECT 1 FROM Puesto p WHERE p.Id = @Id AND p.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    UPDATE Puesto SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarRol
CREATE OR ALTER PROCEDURE [dbo].[EliminarRol]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1
    IF NOT EXISTS(SELECT 1 FROM Rol r WHERE r.Id = @Id AND r.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    UPDATE Rol SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarSucursal
CREATE OR ALTER PROCEDURE [dbo].[EliminarSucursal]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Sucursal s WHERE s.Id = @Id AND s.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Sucursal SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarTicket
CREATE OR ALTER PROCEDURE [dbo].[EliminarTicket]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Ticket t WHERE t.Id = @Id AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE Ticket SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarTipoActivo
CREATE OR ALTER PROCEDURE [dbo].[EliminarTipoActivo]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM TipoActivo ta WHERE ta.Id = @Id AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN SELECT 0 RETURN END
    UPDATE TipoActivo SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- EliminarCategoriaResponsable
CREATE OR ALTER PROCEDURE [dbo].[EliminarCategoriaResponsable]
(
    @Id BIGINT, @ModificadoPor NVARCHAR(25), @FechaModificacion DATETIME, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1
    IF NOT EXISTS(SELECT 1 FROM CategoriaResponsable cr WHERE cr.Id = @Id AND cr.EmpresaId = @EmpresaId)
    BEGIN SELECT 0 RETURN END
    UPDATE CategoriaResponsable SET Estatus = 0, ModificadoPor = @ModificadoPor, FechaModificacion = @FechaModificacion WHERE Id = @Id
    SELECT @Id
END
GO

-- ObtenerActivoPorId / ObtenerActivos / ObtenerAreaPorId / ObtenerAreas
CREATE OR ALTER PROCEDURE [dbo].[ObtenerActivoPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT a.*, ta.Nombre as TipoActivoNombre, ta.Descripcion as TipoActivoDescripcion,
           m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion,
           mo.Nombre as ModeloNombre, mo.Descripcion as ModeloDescripcion
    FROM Activo a
    INNER JOIN TipoActivo ta ON a.TipoActivoID = ta.Id
    INNER JOIN Marca m ON a.MarcaID = m.Id
    INNER JOIN Modelo mo ON a.ModeloID = mo.Id
    WHERE a.Id = @Id AND a.Estatus = 1 AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerActivos]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT a.*, ta.Nombre as TipoActivoNombre, ta.Descripcion as TipoActivoDescripcion,
           m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion,
           mo.Nombre as ModeloNombre, mo.Descripcion as ModeloDescripcion
    FROM Activo a
    INNER JOIN TipoActivo ta ON a.TipoActivoID = ta.Id
    INNER JOIN Marca m ON a.MarcaID = m.Id
    INNER JOIN Modelo mo ON a.ModeloID = mo.Id
    WHERE a.Estatus = 1 AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY a.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerAreaPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT a.* FROM Area a WHERE a.Id = @Id AND a.Estatus = 1 AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerAreas]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT a.* FROM Area a WHERE a.Estatus = 1 AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY a.Nombre
END
GO

-- ObtenerCategoriaPorId / ObtenerCategorias / ObtenerCategoriasPorArea / ObtenerCategoriasPorPadre
CREATE OR ALTER PROCEDURE [dbo].[ObtenerCategoriaPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT c.*, a.Nombre as AreaNombre, cp.Nombre as CategoriaPadreNombre
    FROM Categoria c
    INNER JOIN Area a ON c.AreaId = a.Id
    LEFT JOIN Categoria cp ON c.CategoriaPadreId = cp.Id
    WHERE c.Id = @Id AND c.Estatus = 1 AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerCategorias]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT c.*, a.Nombre as AreaNombre, cp.Nombre as CategoriaPadreNombre,
           CASE WHEN c.CategoriaPadreId IS NULL THEN 1 ELSE 2 END as Nivel
    FROM Categoria c
    INNER JOIN Area a ON c.AreaId = a.Id
    LEFT JOIN Categoria cp ON c.CategoriaPadreId = cp.Id
    WHERE c.Estatus = 1 AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY c.CategoriaPadreId, c.Orden
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerCategoriasPorArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT c.*, a.Nombre as AreaNombre, cp.Nombre as CategoriaPadreNombre,
           CASE WHEN c.CategoriaPadreId IS NULL THEN 1 ELSE 2 END as Nivel
    FROM Categoria c
    INNER JOIN Area a ON c.AreaId = a.Id
    LEFT JOIN Categoria cp ON c.CategoriaPadreId = cp.Id
    WHERE c.AreaId = @AreaId AND c.Estatus = 1 AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY c.CategoriaPadreId, c.Orden
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerCategoriasPorPadre]
(
    @CategoriaPadreId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT c.*, a.Nombre as AreaNombre
    FROM Categoria c
    INNER JOIN Area a ON c.AreaId = a.Id
    WHERE c.CategoriaPadreId = @CategoriaPadreId AND c.Estatus = 1 AND c.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY c.Orden
END
GO

-- ObtenerMarca / ObtenerMarcaPorId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerMarca]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT m.* FROM Marca m WHERE m.Estatus = 1 AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY m.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerMarcaPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT m.* FROM Marca m WHERE m.Id = @Id AND m.Estatus = 1 AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

-- ObtenerModelo / ObtenerModeloPorId / ObtenerModelosPorMarca / ObtenerModelosPorMarcaId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerModelo]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT mo.*, m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion
    FROM Modelo mo
    INNER JOIN Marca m ON mo.MarcaId = m.Id
    WHERE mo.Estatus = 1 AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY m.Nombre, mo.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerModeloPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT mo.*, m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion
    FROM Modelo mo
    INNER JOIN Marca m ON mo.MarcaId = m.Id
    WHERE mo.Id = @Id AND mo.Estatus = 1 AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerModelosPorMarca]
(
    @MarcaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT mo.*, m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion
    FROM Modelo mo
    INNER JOIN Marca m ON mo.MarcaId = m.Id
    WHERE mo.MarcaId = @MarcaId AND mo.Estatus = 1 AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY mo.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerModelosPorMarcaId]
(
    @MarcaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT m.* FROM Modelo m WHERE m.Estatus = 1 AND m.MarcaId = @MarcaId AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY m.Nombre
END
GO

-- ObtenerPersonaPorId / ObtenerPersonas
CREATE OR ALTER PROCEDURE [dbo].[ObtenerPersonaPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.*, pu.Nombre as PuestoNombre
    FROM Persona p
    INNER JOIN Puesto pu ON p.PuestoId = pu.Id
    WHERE p.Id = @Id AND p.Estatus = 1 AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerPersonas]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.*, pu.Nombre as PuestoNombre
    FROM Persona p
    INNER JOIN Puesto pu ON p.PuestoId = pu.Id
    WHERE p.Estatus = 1 AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY p.Nombre, p.Apellido
END
GO

-- ObtenerPuestoPorId / ObtenerPuestos
CREATE OR ALTER PROCEDURE [dbo].[ObtenerPuestoPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.* FROM Puesto p WHERE p.Id = @Id AND p.Estatus = 1 AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerPuestos]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.* FROM Puesto p WHERE p.Estatus = 1 AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY p.Nombre
END
GO

-- ObtenerRoles / ObtenerRolPorId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerRoles]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT r.* FROM Rol r WHERE r.Estatus = 1 AND r.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY r.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerRolPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT r.* FROM Rol r WHERE r.Id = @Id AND r.Estatus = 1 AND r.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

-- ObtenerSucursales / ObtenerSucursalPorId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerSucursales]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT s.* FROM Sucursal s WHERE s.Estatus = 1 AND s.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY s.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerSucursalPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT s.* FROM Sucursal s WHERE s.Id = @Id AND s.Estatus = 1 AND s.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

-- ObtenerTipoActivo / ObtenerTipoActivoPorId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerTipoActivo]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT ta.* FROM TipoActivo ta WHERE ta.Estatus = 1 AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY ta.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTipoActivoPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT ta.* FROM TipoActivo ta WHERE ta.Id = @Id AND ta.Estatus = 1 AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

-- Tickets: el JOIN a Usuarios se mantiene para el nombre del creador; el filtro de tenant ahora usa t.EmpresaId
CREATE OR ALTER PROCEDURE [dbo].[ObtenerTicketPorId]
(
    @Id BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.Id = @Id AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTickets]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTicketsPorArea]
(
    @AreaId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.AreaId = @AreaId AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTicketsPorEstatus]
(
    @TicketEstatusId INT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.TicketEstatusId = @TicketEstatusId AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTicketsPorUrgencia]
(
    @Urgencia INT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.Urgencia = @Urgencia AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerTicketsPorUsuario]
(
    @CreadoPor NVARCHAR(25), @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT t.*, a.Nombre as AreaNombre, c.Nombre as CategoriaNombre, sc.Nombre as SubcategoriaNombre,
           u.Nombre as UsuarioCreadorNombre, u.Apellido as UsuarioCreadorApellido, te.Nombre as EstatusNombre, te.Color as EstatusColor
    FROM Ticket t
    INNER JOIN Area a ON t.AreaId = a.Id
    INNER JOIN Categoria c ON t.CategoriaId = c.Id
    LEFT JOIN Categoria sc ON t.SubcategoriaId = sc.Id
    INNER JOIN Usuarios u ON t.CreadoPor = u.NombreUsuario
    INNER JOIN TicketEstatus te ON t.TicketEstatusId = te.Id
    WHERE t.CreadoPor = @CreadoPor AND t.Estatus = 1 AND t.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY t.FechaCreacion DESC
END
GO

-- ObtenerPermisosPorRol / EliminarPermisosRol (validación de rol por EmpresaId)
CREATE OR ALTER PROCEDURE [dbo].[ObtenerPermisosPorRol]
(
    @RolId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF NOT EXISTS(SELECT 1 FROM Rol r WHERE r.Id = @RolId AND r.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))
    BEGIN
        SELECT NULL AS Error
        RETURN
    END
    SELECT rpa.*, p.Nombre AS PaginaNombre, p.Direccion
    FROM RolPaginaAccion rpa
    INNER JOIN Pagina p ON rpa.PaginaId = p.Id
    WHERE rpa.RolId = @RolId AND rpa.Estatus = 1 AND p.Estatus = 1
    ORDER BY p.Nombre
END
GO

CREATE OR ALTER PROCEDURE [dbo].[EliminarPermisosRol]
(
    @RolId BIGINT, @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1
    IF NOT EXISTS(SELECT 1 FROM Rol r WHERE r.Id = @RolId AND r.EmpresaId = @EmpresaId)
    BEGIN
        SELECT 0 AS Resultado
        RETURN
    END
    DELETE FROM RolPaginaAccion WHERE RolId = @RolId
    SELECT @@ROWCOUNT AS Resultado
END
GO
