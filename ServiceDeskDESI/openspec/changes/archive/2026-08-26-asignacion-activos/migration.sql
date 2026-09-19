-- ============================================================
-- Migration: asignacion-activos
-- Asignar activos a personas (fecha inicio auto = hoy; fecha fin auto al desvincular).
-- Fecha: 2026-08-19
-- ============================================================

-- 1. Tabla PersonaActivo
IF OBJECT_ID(N'dbo.PersonaActivo', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PersonaActivo] (
        Id BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        PersonaId BIGINT NOT NULL,
        ActivoId BIGINT NOT NULL,
        FechaInicio DATETIME NOT NULL,
        FechaFin DATETIME NULL,
        CreadoPor NVARCHAR(25) NOT NULL,
        FechaCreacion DATETIME NOT NULL,
        ModificadoPor NVARCHAR(25) NULL,
        FechaModificacion DATETIME NULL,
        Estatus BIT NOT NULL,
        EmpresaId BIGINT NOT NULL
    );
END
GO

-- 2. Índice para búsqueda de activo vigente
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PersonaActivo_ActivoVigente' AND object_id = OBJECT_ID(N'dbo.PersonaActivo'))
    CREATE INDEX IX_PersonaActivo_ActivoVigente ON [dbo].[PersonaActivo] (ActivoId, FechaFin, Estatus);
GO

-- 3. SP AsignarActivoPersona (fecha inicio = GETDATE(), no editable)
IF OBJECT_ID(N'dbo.AsignarActivoPersona', N'P') IS NOT NULL DROP PROCEDURE dbo.AsignarActivoPersona;
GO
CREATE PROCEDURE [dbo].[AsignarActivoPersona]
(
    @PersonaId BIGINT,
    @ActivoId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

    IF NOT EXISTS(SELECT 1 FROM Persona WHERE Id = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT 0; RETURN; END
    IF NOT EXISTS(SELECT 1 FROM Activo WHERE Id = @ActivoId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT 0; RETURN; END
    IF EXISTS(SELECT 1 FROM PersonaActivo WHERE ActivoId = @ActivoId AND FechaFin IS NULL AND Estatus = 1)
        BEGIN SELECT -1; RETURN; END  -- -1 = activo ya asignado

    INSERT INTO PersonaActivo (PersonaId, ActivoId, FechaInicio, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@PersonaId, @ActivoId, GETDATE(), @Usuario, GETDATE(), 1, @EmpresaId);

    SELECT SCOPE_IDENTITY();
END
GO

-- 4. SP DesvincularActivoPersona (fecha fin = GETDATE(), auto)
IF OBJECT_ID(N'dbo.DesvincularActivoPersona', N'P') IS NOT NULL DROP PROCEDURE dbo.DesvincularActivoPersona;
GO
CREATE PROCEDURE [dbo].[DesvincularActivoPersona]
(
    @PersonaActivoId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

    IF NOT EXISTS(SELECT 1 FROM PersonaActivo WHERE Id = @PersonaActivoId AND Estatus = 1 AND EmpresaId = @EmpresaId AND FechaFin IS NULL)
        BEGIN SELECT 0; RETURN; END

    UPDATE PersonaActivo
    SET FechaFin = GETDATE(), ModificadoPor = @Usuario, FechaModificacion = GETDATE()
    WHERE Id = @PersonaActivoId AND FechaFin IS NULL;

    SELECT @PersonaActivoId;
END
GO

-- 5. SP ObtenerActivosPorPersona (solo vigentes)
IF OBJECT_ID(N'dbo.ObtenerActivosPorPersona', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerActivosPorPersona;
GO
CREATE PROCEDURE [dbo].[ObtenerActivosPorPersona]
(
    @PersonaId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT pa.Id, pa.PersonaId, pa.ActivoId, pa.FechaInicio, pa.FechaFin,
           a.Nombre AS ActivoNombre, a.Serial AS ActivoSerial
    FROM PersonaActivo pa
    INNER JOIN Activo a ON pa.ActivoId = a.Id
    WHERE pa.PersonaId = @PersonaId AND pa.FechaFin IS NULL AND pa.Estatus = 1 AND pa.EmpresaId = @EmpresaId
    ORDER BY pa.FechaInicio DESC;
END
GO

-- 6. SP ObtenerActivosDisponibles (activos sin asignación vigente)
IF OBJECT_ID(N'dbo.ObtenerActivosDisponibles', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerActivosDisponibles;
GO
CREATE PROCEDURE [dbo].[ObtenerActivosDisponibles]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT a.Id, a.Nombre, a.Serial
    FROM Activo a
    WHERE a.Estatus = 1 AND a.EmpresaId = @EmpresaId
      AND NOT EXISTS (SELECT 1 FROM PersonaActivo pa WHERE pa.ActivoId = a.Id AND pa.FechaFin IS NULL AND pa.Estatus = 1)
    ORDER BY a.Nombre;
END
GO

-- 7. ObtenerActivos: añadir "Asignado a" (LEFT JOIN asignación vigente + Persona)
IF OBJECT_ID(N'dbo.ObtenerActivos', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerActivos;
GO
CREATE PROCEDURE [dbo].[ObtenerActivos]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;

    SELECT a.*, ta.Nombre as TipoActivoNombre, ta.Descripcion as TipoActivoDescripcion,
           m.Nombre as MarcaNombre, m.Descripcion as MarcaDescripcion,
           mo.Nombre as ModeloNombre, mo.Descripcion as ModeloDescripcion,
           p.Nombre AS PersonaNombre, p.Apellido AS PersonaApellido
    FROM Activo a
    INNER JOIN TipoActivo ta ON a.TipoActivoID = ta.Id
    INNER JOIN Marca m ON a.MarcaID = m.Id
    INNER JOIN Modelo mo ON a.ModeloID = mo.Id
    LEFT JOIN PersonaActivo pa ON pa.ActivoId = a.Id AND pa.FechaFin IS NULL AND pa.Estatus = 1
    LEFT JOIN Persona p ON pa.PersonaId = p.Id
    WHERE a.Estatus = 1 AND a.EmpresaId = @EmpresaId
    ORDER BY a.Nombre;
END
GO

-- ============================================================
-- Extensión aditiva: notificación, confirmación de recepción y bitácora.
-- NO re-DROP/CREA los 5 SPs existentes. Los 3 SPs nuevos usan DROP/CREATE.
-- ============================================================

-- 8. Columnas nuevas en PersonaActivo (FechaConfirmacion, TokenConfirmacion)
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'FechaConfirmacion')
    ALTER TABLE dbo.PersonaActivo ADD FechaConfirmacion DATETIME NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.PersonaActivo') AND name = N'TokenConfirmacion')
    ALTER TABLE dbo.PersonaActivo ADD TokenConfirmacion UNIQUEIDENTIFIER NULL;
GO

-- 9. Índice NO único para lookup por token de confirmación (múltiples NULL permitidos)
IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_PersonaActivo_TokenConfirmacion'
                 AND object_id = OBJECT_ID(N'dbo.PersonaActivo'))
    CREATE INDEX IX_PersonaActivo_TokenConfirmacion ON dbo.PersonaActivo (TokenConfirmacion);
GO

-- 10. Tabla BitacoraCorreo (append-only, soft reference a PersonaActivoId, SIN FK)
IF OBJECT_ID(N'dbo.BitacoraCorreo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BitacoraCorreo (
        Id            BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        TipoCorreo    NVARCHAR(50)  NOT NULL,
        Destinatario  NVARCHAR(250) NOT NULL,
        Asunto        NVARCHAR(250) NOT NULL,
        Estado        NVARCHAR(20)  NOT NULL,          -- 'Enviado' | 'Fallido'
        Error         NVARCHAR(MAX) NULL,
        FechaEnvio    DATETIME      NOT NULL,
        ReferenciaId  BIGINT        NULL               -- soft reference → PersonaActivoId
    );
END
GO

-- 11. SP GenerarTokenConfirmacion (persiste el token GUID generado en C#)
IF OBJECT_ID(N'dbo.GenerarTokenConfirmacion', N'P') IS NOT NULL DROP PROCEDURE dbo.GenerarTokenConfirmacion;
GO
CREATE PROCEDURE [dbo].[GenerarTokenConfirmacion]
(
    @PersonaActivoId   BIGINT,
    @TokenConfirmacion UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE PersonaActivo
    SET TokenConfirmacion = @TokenConfirmacion
    WHERE Id = @PersonaActivoId AND FechaFin IS NULL;
    SELECT @@ROWCOUNT;   -- 1 = ok; 0 = fila inexistente o ya desvinculada
END
GO

-- 12. SP ConfirmarRecepcionActivo (tri-estado idempotente, anónimo, sin @Usuario)
IF OBJECT_ID(N'dbo.ConfirmarRecepcionActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ConfirmarRecepcionActivo;
GO
CREATE PROCEDURE [dbo].[ConfirmarRecepcionActivo]
(
    @TokenConfirmacion UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM PersonaActivo WHERE TokenConfirmacion = @TokenConfirmacion)
        BEGIN SELECT 0; RETURN; END   -- token desconocido

    IF EXISTS (SELECT 1 FROM PersonaActivo
               WHERE TokenConfirmacion = @TokenConfirmacion AND FechaConfirmacion IS NOT NULL)
        BEGIN SELECT 2; RETURN; END   -- ya confirmado (idempotente, sin cambio)

    UPDATE PersonaActivo
    SET FechaConfirmacion = GETDATE()
    WHERE TokenConfirmacion = @TokenConfirmacion AND FechaConfirmacion IS NULL;

    SELECT 1;                         -- confirmado ahora
END
GO

-- 13. SP RegistrarBitacoraCorreo (INSERT append-only)
IF OBJECT_ID(N'dbo.RegistrarBitacoraCorreo', N'P') IS NOT NULL DROP PROCEDURE dbo.RegistrarBitacoraCorreo;
GO
CREATE PROCEDURE [dbo].[RegistrarBitacoraCorreo]
(
    @TipoCorreo   NVARCHAR(50),
    @Destinatario NVARCHAR(250),
    @Asunto       NVARCHAR(250),
    @Estado       NVARCHAR(20),
    @Error        NVARCHAR(MAX) = NULL,
    @ReferenciaId BIGINT        = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO BitacoraCorreo (TipoCorreo, Destinatario, Asunto, Estado, Error, FechaEnvio, ReferenciaId)
    VALUES (@TipoCorreo, @Destinatario, @Asunto, @Estado, @Error, GETDATE(), @ReferenciaId);
    SELECT SCOPE_IDENTITY();
END
GO
