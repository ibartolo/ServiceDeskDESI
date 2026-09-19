-- ============================================================
-- Rollback: foliador-tickets (deshacer la migración)
-- Ejecutar SOLO si se requiere revertir la migración.
-- Orden inverso: DROP SPs nuevos -> DROP Ticket.Folio -> DROP Foliador
-- -> restaurar GuardarOActualizarTicket previo.
-- ============================================================

-- 1. Eliminar SPs nuevos
IF OBJECT_ID(N'[dbo].[ConsultarFoliador]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ConsultarFoliador];
GO
IF OBJECT_ID(N'[dbo].[ActualizarFoliador]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ActualizarFoliador];
GO

-- 2. Quitar columna Ticket.Folio
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Ticket]') AND name = 'Folio')
    ALTER TABLE [dbo].[Ticket] DROP COLUMN [Folio];
GO

-- 3. Eliminar tabla Foliador
IF OBJECT_ID(N'[dbo].[Foliador]', N'U') IS NOT NULL DROP TABLE [dbo].[Foliador];
GO

-- 4. Restaurar GuardarOActualizarTicket previo (sin @Folio)
IF OBJECT_ID(N'[dbo].[GuardarOActualizarTicket]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[GuardarOActualizarTicket];
GO
CREATE PROCEDURE [dbo].[GuardarOActualizarTicket]
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
