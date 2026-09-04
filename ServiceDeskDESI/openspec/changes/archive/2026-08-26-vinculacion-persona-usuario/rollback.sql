-- ============================================================
-- Rollback: vinculacion-persona-usuario
-- Orden inverso a migration.sql.
-- ⚠️ Solo para revertir la migración de este change en la BD hosted (manual).
-- ============================================================

-- 1. Quitar RolPaginaAccion / Pagina "Mis Activos"
DELETE rpa
FROM RolPaginaAccion rpa
INNER JOIN Pagina p ON rpa.PaginaId = p.Id
WHERE p.Nombre = N'MisActivos';
GO

DELETE FROM Pagina WHERE Nombre = N'MisActivos';
GO

-- 2. DROP PROCEDURE de los SPs nuevos
IF OBJECT_ID(N'dbo.VincularPersonaUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.VincularPersonaUsuario;
GO
IF OBJECT_ID(N'dbo.DesvincularPersonaUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.DesvincularPersonaUsuario;
GO
IF OBJECT_ID(N'dbo.ObtenerPersonaIdPorUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonaIdPorUsuario;
GO
IF OBJECT_ID(N'dbo.ObtenerAsignacionPorToken', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerAsignacionPorToken;
GO
IF OBJECT_ID(N'dbo.ObtenerPersonaActivoPorId', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonaActivoPorId;
GO
IF OBJECT_ID(N'dbo.DesvincularActivoPersonaConfirmacion', N'P') IS NOT NULL DROP PROCEDURE dbo.DesvincularActivoPersonaConfirmacion;
GO

-- 3. Restaurar SPs modificados a sus definiciones previas
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

IF OBJECT_ID(N'dbo.ObtenerPersonas', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonas;
GO
CREATE PROCEDURE [dbo].[ObtenerPersonas]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.*,
           pu.Nombre as PuestoNombre
    FROM Persona p
    INNER JOIN Puesto pu ON p.PuestoId = pu.Id
    WHERE p.Estatus = 1
        AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY p.Nombre, p.Apellido
END
GO

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

-- 4. Quitar índice único filtrado
IF EXISTS (SELECT 1 FROM sys.indexes
           WHERE name = N'UX_Usuarios_PersonaId' AND object_id = OBJECT_ID(N'dbo.Usuarios'))
    DROP INDEX UX_Usuarios_PersonaId ON dbo.Usuarios;
GO

-- 5. Quitar FK
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuarios_Persona')
    ALTER TABLE dbo.Usuarios DROP CONSTRAINT FK_Usuarios_Persona;
GO

-- 6. Quitar columna
IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(N'dbo.Usuarios') AND name = N'PersonaId')
    ALTER TABLE dbo.Usuarios DROP COLUMN PersonaId;
GO
