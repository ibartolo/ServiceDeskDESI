-- ============================================================
-- Migration: evidencias-tickets
-- Adjuntos (evidencias) a tickets: almacenar metadatos + relación multi-tenant.
-- Fecha: 2026-08-20
-- ============================================================

-- 1. Tabla TicketEvidencia
IF OBJECT_ID(N'dbo.TicketEvidencia', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[TicketEvidencia] (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TicketId      BIGINT NOT NULL,
        EmpresaId     BIGINT NOT NULL,
        NombreArchivo NVARCHAR(500) NOT NULL,   -- nombre original del archivo
        RutaArchivo   NVARCHAR(1000) NOT NULL,  -- ruta parcial relativa a ~/ (sin '~/')
        FechaSubida   DATETIME NOT NULL CONSTRAINT DF_TicketEvidencia_FechaSubida DEFAULT (GETDATE()),
        CreadoPor     NVARCHAR(25) NULL,
        FechaCreacion DATETIME NOT NULL CONSTRAINT DF_TicketEvidencia_FechaCreacion DEFAULT (GETDATE()),
        Estatus       BIT NOT NULL CONSTRAINT DF_TicketEvidencia_Estatus DEFAULT ((1)),
        CONSTRAINT FK_TicketEvidencia_Ticket  FOREIGN KEY (TicketId)  REFERENCES [dbo].[Ticket]([Id]),
        CONSTRAINT FK_TicketEvidencia_Empresa FOREIGN KEY (EmpresaId) REFERENCES [dbo].[Empresa]([Id])
    );
END
GO

-- 2. Índices no clúster por TicketId y por EmpresaId
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TicketEvidencia_TicketId' AND object_id = OBJECT_ID(N'dbo.TicketEvidencia'))
    CREATE INDEX IX_TicketEvidencia_TicketId ON [dbo].[TicketEvidencia] (TicketId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TicketEvidencia_EmpresaId' AND object_id = OBJECT_ID(N'dbo.TicketEvidencia'))
    CREATE INDEX IX_TicketEvidencia_EmpresaId ON [dbo].[TicketEvidencia] (EmpresaId);
GO

-- 3. SP GuardarEvidencia (valida ticket en empresa del usuario; inserta; devuelve SCOPE_IDENTITY())
IF OBJECT_ID(N'dbo.GuardarEvidencia', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarEvidencia;
GO
CREATE PROCEDURE [dbo].[GuardarEvidencia]
(
    @TicketId BIGINT,
    @NombreArchivo NVARCHAR(500),
    @RutaArchivo NVARCHAR(1000),
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

    IF NOT EXISTS(SELECT 1 FROM Ticket WHERE Id = @TicketId AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN SELECT 0; RETURN; END

    INSERT INTO TicketEvidencia (TicketId, EmpresaId, NombreArchivo, RutaArchivo, FechaSubida, CreadoPor, FechaCreacion, Estatus)
    VALUES (@TicketId, @EmpresaId, @NombreArchivo, @RutaArchivo, GETDATE(), @Usuario, GETDATE(), 1);

    SELECT SCOPE_IDENTITY();
END
GO

-- 4. SP ObtenerEvidenciasPorTicket (solo de la empresa del usuario, Estatus=1)
IF OBJECT_ID(N'dbo.ObtenerEvidenciasPorTicket', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerEvidenciasPorTicket;
GO
CREATE PROCEDURE [dbo].[ObtenerEvidenciasPorTicket]
(
    @TicketId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT e.Id, e.TicketId, e.EmpresaId, e.NombreArchivo, e.RutaArchivo, e.FechaSubida, e.CreadoPor, e.FechaCreacion, e.Estatus
    FROM TicketEvidencia e
    WHERE e.TicketId = @TicketId AND e.EmpresaId = @EmpresaId AND e.Estatus = 1
    ORDER BY e.FechaSubida DESC;
END
GO

-- 5. SP ObtenerEvidencia (una evidencia por Id, filtrada por empresa del usuario y Estatus=1)
IF OBJECT_ID(N'dbo.ObtenerEvidencia', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerEvidencia;
GO
CREATE PROCEDURE [dbo].[ObtenerEvidencia]
(
    @Id BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT e.Id, e.TicketId, e.EmpresaId, e.NombreArchivo, e.RutaArchivo, e.FechaSubida, e.CreadoPor, e.FechaCreacion, e.Estatus
    FROM TicketEvidencia e
    WHERE e.Id = @Id AND e.EmpresaId = @EmpresaId AND e.Estatus = 1;
END
GO
