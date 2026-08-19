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
