/* ============================================================================
   ServiceDeskDESI — Migración: FKs escalares (E2) — rename de parámetros en SPs
   ----------------------------------------------------------------------------
   Cambio: fk-escalares
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi

   CONTEXTO: el cambio fk-escalares convirtió las FKs de navegación a escalares
   *Id en las entidades, por lo que el DAL ahora genera parámetros con casing
   normalizado (@AreaId, @TipoActivoId, etc.). Los SPs de escritura deben
   coincidir con esos nombres.
     1. GuardarOActualizarActivo: @TipoActivoID/@MarcaID/@ModeloID -> *Id
     2. GuardarOActualizarTicket: @Area/@Categoria/@Subcategoria -> *Id
   Usa CREATE OR ALTER => idempotente.
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

-- 1. Guardar o actualizar activo (normalización de casing *ID -> *Id)
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarActivo]
(
    @Id BIGINT,
    @Nombre NVARCHAR(50),
    @Descripcion NVARCHAR(250) = NULL,
    @TipoActivoId BIGINT,
    @Serial NVARCHAR(50) = NULL,
    @MarcaId BIGINT,
    @ModeloId BIGINT,
    @Notas NVARCHAR(250) = NULL,
    @FechaCompra DATETIME = NULL,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL,
    @Estatus BIT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    IF EXISTS(SELECT * FROM Activo WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Activo a
            WHERE a.Id = @Id
                AND a.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM TipoActivo ta
            WHERE ta.Id = @TipoActivoId
                AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Marca m
            WHERE m.Id = @MarcaId
                AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Modelo mo
            WHERE mo.Id = @ModeloId
                AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        UPDATE Activo
        SET Nombre = @Nombre,
            Descripcion = @Descripcion,
            TipoActivoID = @TipoActivoId,
            Serial = @Serial,
            MarcaID = @MarcaId,
            ModeloID = @ModeloId,
            Notas = @Notas,
            FechaCompra = @FechaCompra,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion,
            Estatus = @Estatus
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Usuarios
            WHERE NombreUsuario = @CreadoPor
                AND EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
                AND Estatus = 1
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM TipoActivo ta
            WHERE ta.Id = @TipoActivoId
                AND ta.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Marca m
            WHERE m.Id = @MarcaId
                AND m.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF NOT EXISTS(
            SELECT 1
            FROM Modelo mo
            WHERE mo.Id = @ModeloId
                AND mo.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
        )
        BEGIN
            SELECT 0
            RETURN
        END

        INSERT INTO Activo
        (Nombre, Descripcion, TipoActivoID, Serial, MarcaID, ModeloID, Notas, FechaCompra,
         CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES
        (@Nombre, @Descripcion, @TipoActivoId, @Serial, @MarcaId, @ModeloId, @Notas, @FechaCompra,
         @CreadoPor, @FechaCreacion, @Estatus, (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1))

        SELECT SCOPE_IDENTITY()
    END
END
GO

-- 2. Guardar o actualizar ticket (rename @Area/@Categoria/@Subcategoria -> *Id)
CREATE OR ALTER PROCEDURE [dbo].[GuardarOActualizarTicket]
(
    @Id BIGINT,
    @AreaId BIGINT,
    @CategoriaId BIGINT,
    @SubcategoriaId BIGINT = NULL,
    @Urgencia INT,
    @Titulo NVARCHAR(250),
    @Descripcion NVARCHAR(MAX),
    @TicketEstatusId INT,
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL,
    @Estatus BIT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    IF NOT EXISTS(
        SELECT 1
        FROM Usuarios
        WHERE NombreUsuario = @CreadoPor
            AND EmpresaId = @EmpresaId
            AND Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    IF NOT EXISTS(
        SELECT 1
        FROM Area a
        WHERE a.Id = @AreaId
            AND a.EmpresaId = @EmpresaId
    )
    BEGIN
        SELECT 0
        RETURN
    END

    IF NOT EXISTS(
        SELECT 1
        FROM Categoria c
        WHERE c.Id = @CategoriaId
            AND c.EmpresaId = @EmpresaId
    )
    BEGIN
        SELECT 0
        RETURN
    END

    IF NOT EXISTS(SELECT 1 FROM TicketEstatus WHERE Id = @TicketEstatusId AND Estatus = 1)
    BEGIN
        SELECT 0
        RETURN
    END

    IF EXISTS(SELECT * FROM Ticket WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Ticket t
            WHERE t.Id = @Id
                AND t.EmpresaId = @EmpresaId
        )
        BEGIN
            SELECT 0
            RETURN
        END

        UPDATE Ticket
        SET AreaId = @AreaId,
            CategoriaId = @CategoriaId,
            SubcategoriaId = @SubcategoriaId,
            Urgencia = @Urgencia,
            Titulo = @Titulo,
            Descripcion = @Descripcion,
            TicketEstatusId = @TicketEstatusId,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion,
            Estatus = @Estatus
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        INSERT INTO Ticket
        (AreaId, CategoriaId, SubcategoriaId, Urgencia, Titulo, Descripcion,
         TicketEstatusId, CreadoPor, FechaCreacion, Estatus, EmpresaId)
        VALUES
        (@AreaId, @CategoriaId, @SubcategoriaId, @Urgencia, @Titulo, @Descripcion,
         @TicketEstatusId, @CreadoPor, @FechaCreacion, @Estatus, @EmpresaId)

        SELECT SCOPE_IDENTITY()
    END
END
GO
