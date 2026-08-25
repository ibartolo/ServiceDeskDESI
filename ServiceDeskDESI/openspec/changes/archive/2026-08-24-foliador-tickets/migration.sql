-- ============================================================
-- Migration: foliador-tickets
-- Migración directa (idempotente). Ejecutar contra la BD viva.
-- Fecha: 2026-08-24
-- ============================================================

-- 1. Tabla Foliador (un consecutivo por empresa y por Nombre)
IF OBJECT_ID(N'[dbo].[Foliador]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Foliador] (
        EmpresaId          BIGINT        NOT NULL,
        FechaActualizacion DATETIME      NOT NULL CONSTRAINT DF_Foliador_FechaActualizacion DEFAULT (GETDATE()),
        Nombre             NVARCHAR(50)  NOT NULL,
        Descripcion        NVARCHAR(250) NULL,
        Consecutivo        INT           NOT NULL CONSTRAINT DF_Foliador_Consecutivo DEFAULT ((0)),
        CONSTRAINT PK_Foliador PRIMARY KEY (EmpresaId, Nombre),
        CONSTRAINT FK_Foliador_Empresa FOREIGN KEY (EmpresaId) REFERENCES [dbo].[Empresa]([Id])
    );
END
GO

-- 2. Ticket.Folio (nullable, sin backfill)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Ticket]') AND name = 'Folio')
    ALTER TABLE [dbo].[Ticket] ADD [Folio] NVARCHAR(50) NULL;
GO

-- 3. SP ConsultarFoliador (público): devuelve la fila o vacío (sin error)
IF OBJECT_ID(N'[dbo].[ConsultarFoliador]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ConsultarFoliador];
GO
CREATE PROCEDURE [dbo].[ConsultarFoliador]
    @Nombre NVARCHAR(50), @Usuario NVARCHAR(25)
AS BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    SELECT EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo
    FROM Foliador WHERE EmpresaId = @EmpresaId AND Nombre = @Nombre;
END
GO

-- 4. SP ActualizarFoliador (interno): upsert defensivo + incremento atómico; devuelve el nuevo valor
IF OBJECT_ID(N'[dbo].[ActualizarFoliador]', N'P') IS NOT NULL DROP PROCEDURE [dbo].[ActualizarFoliador];
GO
CREATE PROCEDURE [dbo].[ActualizarFoliador]
    @Nombre NVARCHAR(50), @Usuario NVARCHAR(25)
AS BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT NULL; RETURN; END
    IF NOT EXISTS (SELECT 1 FROM Foliador WHERE EmpresaId = @EmpresaId AND Nombre = @Nombre)
        INSERT INTO Foliador (EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo)
        VALUES (@EmpresaId, GETDATE(), @Nombre, NULL, 0);
    UPDATE Foliador WITH (UPDLOCK, HOLDLOCK)
    SET Consecutivo = Consecutivo + 1, FechaActualizacion = GETDATE()
    OUTPUT INSERTED.Consecutivo
    WHERE EmpresaId = @EmpresaId AND Nombre = @Nombre;
END
GO

-- 5. GuardarOActualizarTicket (+@Folio): INSERT inserta Folio; UPDATE no lo toca
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
    @Folio NVARCHAR(50) = NULL,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    DECLARE @EmpresaId BIGINT
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1

    -- Validar que el usuario creador pertenezca a la empresa del usuario autenticado
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

    -- Validar que el área pertenezca a la empresa del usuario autenticado
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

    -- Validar que la categoría pertenezca a la empresa del usuario autenticado
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

    -- Validar que el estatus exista
    IF NOT EXISTS(SELECT 1 FROM TicketEstatus WHERE Id = @TicketEstatusId AND Estatus = 1)
    BEGIN
        SELECT 0
        RETURN
    END

    IF EXISTS(SELECT * FROM Ticket WHERE Id = @Id)
    BEGIN
        -- Validar que el ticket pertenezca a la empresa del usuario autenticado
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

        -- UPDATE no modifica Folio (preservar el folio existente al editar).
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
         TicketEstatusId, CreadoPor, FechaCreacion, Estatus, Folio, EmpresaId)
        VALUES
        (@AreaId, @CategoriaId, @SubcategoriaId, @Urgencia, @Titulo, @Descripcion,
         @TicketEstatusId, @CreadoPor, @FechaCreacion, @Estatus, @Folio, @EmpresaId)

        SELECT SCOPE_IDENTITY()
    END
END
GO

-- 6. Seed: una fila Nombre='Ticket' (Consecutivo=0) por cada Empresa existente
INSERT INTO [dbo].[Foliador] (EmpresaId, FechaActualizacion, Nombre, Descripcion, Consecutivo)
SELECT e.Id, GETDATE(), N'Ticket', N'Foliador de tickets', 0
FROM [dbo].[Empresa] e
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[Foliador] f WHERE f.EmpresaId = e.Id AND f.Nombre = N'Ticket'
);
GO
