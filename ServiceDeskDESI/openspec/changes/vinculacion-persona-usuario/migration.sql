-- ============================================================
-- Migration: vinculacion-persona-usuario
-- Relación 1:1 Persona <-> Usuario (Usuarios.PersonaId), aceptación
-- autenticada de activos, "Mis Activos" y desvinculación autenticada.
-- Fecha: 2026-08-26
-- Migración ADITIVA e IDEMPOTENTE (guardas sys.columns/sys.foreign_keys/
-- sys.indexes + DROP/CREATE de SPs). NO toca datos existentes.
-- ============================================================

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- ============================================================
-- D1.1 — Usuarios.PersonaId + FK + índice único filtrado
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE object_id = OBJECT_ID(N'dbo.Usuarios') AND name = N'PersonaId')
    ALTER TABLE dbo.Usuarios ADD PersonaId BIGINT NULL;
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Usuarios_Persona')
    ALTER TABLE dbo.Usuarios
        ADD CONSTRAINT FK_Usuarios_Persona FOREIGN KEY (PersonaId) REFERENCES dbo.Persona(Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'UX_Usuarios_PersonaId' AND object_id = OBJECT_ID(N'dbo.Usuarios'))
    CREATE UNIQUE INDEX UX_Usuarios_PersonaId ON dbo.Usuarios(PersonaId) WHERE PersonaId IS NOT NULL;
GO

-- ============================================================
-- D1.2 — Reescribir AsignarActivoPersona (agrega rama -2)
--        -2 = persona sin usuario vinculado
-- ============================================================
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

    -- -2 = persona sin usuario vinculado (debe existir un usuario con este PersonaId)
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE PersonaId = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT -2; RETURN; END

    IF NOT EXISTS(SELECT 1 FROM Activo WHERE Id = @ActivoId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT 0; RETURN; END
    IF EXISTS(SELECT 1 FROM PersonaActivo WHERE ActivoId = @ActivoId AND FechaFin IS NULL AND Estatus = 1)
        BEGIN SELECT -1; RETURN; END  -- -1 = activo ya asignado

    INSERT INTO PersonaActivo (PersonaId, ActivoId, FechaInicio, CreadoPor, FechaCreacion, Estatus, EmpresaId)
    VALUES (@PersonaId, @ActivoId, GETDATE(), @Usuario, GETDATE(), 1, @EmpresaId);

    SELECT SCOPE_IDENTITY();
END
GO

-- ============================================================
-- D1.3 — SPs nuevos
-- ============================================================

-- VincularPersonaUsuario: atómico. Retorna 1 (ok) / 0 (fallo) / -3 (ya vinculada a otro)
IF OBJECT_ID(N'dbo.VincularPersonaUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.VincularPersonaUsuario;
GO
CREATE PROCEDURE [dbo].[VincularPersonaUsuario]
(
    @PersonaId BIGINT,
    @UsuarioId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;

    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

    -- Validar Persona (misma empresa, estatus 1)
    IF NOT EXISTS(SELECT 1 FROM Persona WHERE Id = @PersonaId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT 0; RETURN; END

    -- Validar Usuario objetivo (misma empresa, estatus 1)
    IF NOT EXISTS(SELECT 1 FROM Usuarios WHERE Id = @UsuarioId AND Estatus = 1 AND EmpresaId = @EmpresaId)
        BEGIN SELECT 0; RETURN; END

    -- Si la persona ya está vinculada a OTRO usuario activo → -3
    IF EXISTS(SELECT 1 FROM Usuarios WHERE PersonaId = @PersonaId AND Id <> @UsuarioId AND Estatus = 1)
        BEGIN SELECT -3; RETURN; END

    -- Vincular
    UPDATE Usuarios SET PersonaId = @PersonaId WHERE Id = @UsuarioId AND Estatus = 1;

    -- Sobrescribir datos de Persona desde el Usuario (PuestoId intacto)
    UPDATE p
    SET p.Nombre      = u.Nombre,
        p.Apellido    = u.Apellido,
        p.Correo      = u.Correo,
        p.Telefono    = u.Celular,
        p.ModificadoPor = @Usuario,
        p.FechaModificacion = GETDATE()
    FROM Persona p
    INNER JOIN Usuarios u ON u.Id = @UsuarioId
    WHERE p.Id = @PersonaId;

    SELECT 1;
END
GO

-- DesvincularPersonaUsuario: retorna @@ROWCOUNT
IF OBJECT_ID(N'dbo.DesvincularPersonaUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.DesvincularPersonaUsuario;
GO
CREATE PROCEDURE [dbo].[DesvincularPersonaUsuario]
(
    @PersonaId BIGINT,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END

    UPDATE Usuarios SET PersonaId = NULL WHERE PersonaId = @PersonaId AND EmpresaId = @EmpresaId;
    SELECT @@ROWCOUNT;
END
GO

-- ObtenerPersonaIdPorUsuario: scalar (NULL si no hay vínculo)
IF OBJECT_ID(N'dbo.ObtenerPersonaIdPorUsuario', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonaIdPorUsuario;
GO
CREATE PROCEDURE [dbo].[ObtenerPersonaIdPorUsuario]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT PersonaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
END
GO

-- ObtenerAsignacionPorToken: fila única para la página anónima (sin @Usuario)
IF OBJECT_ID(N'dbo.ObtenerAsignacionPorToken', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerAsignacionPorToken;
GO
CREATE PROCEDURE [dbo].[ObtenerAsignacionPorToken]
(
    @TokenConfirmacion UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        pa.Id            AS Id,
        pa.PersonaId,
        pa.FechaInicio,
        pa.FechaConfirmacion,
        pa.FechaFin,
        a.Nombre         AS ActivoNombre,
        a.Serial         AS ActivoSerial,
        ta.Nombre        AS TipoActivoNombre,
        m.Nombre         AS MarcaNombre,
        mo.Nombre        AS ModeloNombre,
        p.Nombre         AS PersonaNombre,
        p.Apellido       AS PersonaApellido,
        ISNULL(u.Nombre, N'') + N' ' + ISNULL(u.Apellido, N'') AS AsignadorNombre
    FROM PersonaActivo pa
    INNER JOIN Activo a ON pa.ActivoId = a.Id
    LEFT JOIN TipoActivo ta ON a.TipoActivoID = ta.Id
    LEFT JOIN Marca m ON a.MarcaID = m.Id
    LEFT JOIN Modelo mo ON a.ModeloID = mo.Id
    INNER JOIN Persona p ON pa.PersonaId = p.Id
    LEFT JOIN Usuarios u ON pa.CreadoPor = u.NombreUsuario
    WHERE pa.TokenConfirmacion = @TokenConfirmacion;
END
GO

-- ObtenerPersonaActivoPorId: read por id (usado por IniciarDesvinculacion)
IF OBJECT_ID(N'dbo.ObtenerPersonaActivoPorId', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonaActivoPorId;
GO
CREATE PROCEDURE [dbo].[ObtenerPersonaActivoPorId]
(
    @Id BIGINT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT pa.*
    FROM PersonaActivo pa
    WHERE pa.Id = @Id;
END
GO

-- ============================================================
-- D1.4 — Reescribir ConfirmarRecepcionActivo (autenticado + titularidad)
--        Retornos: 0 desconocido · 1 confirmado ahora · 2 ya confirmado · 3 no autorizado
-- ============================================================
IF OBJECT_ID(N'dbo.ConfirmarRecepcionActivo', N'P') IS NOT NULL DROP PROCEDURE dbo.ConfirmarRecepcionActivo;
GO
CREATE PROCEDURE [dbo].[ConfirmarRecepcionActivo]
(
    @TokenConfirmacion UNIQUEIDENTIFIER,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id BIGINT, @PersonaId BIGINT, @PersonaIdUsuario BIGINT;

    SELECT @Id = Id, @PersonaId = PersonaId FROM PersonaActivo WHERE TokenConfirmacion = @TokenConfirmacion;
    IF @Id IS NULL BEGIN SELECT 0; RETURN; END                              -- token desconocido

    IF EXISTS(SELECT 1 FROM PersonaActivo WHERE Id = @Id AND FechaConfirmacion IS NOT NULL)
        BEGIN SELECT 2; RETURN; END                                         -- ya confirmado (idempotente)

    SELECT @PersonaIdUsuario = PersonaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @PersonaIdUsuario IS NULL OR @PersonaIdUsuario <> @PersonaId
        BEGIN SELECT 3; RETURN; END                                         -- 3 = no autorizado

    UPDATE PersonaActivo SET FechaConfirmacion = GETDATE() WHERE Id = @Id AND FechaConfirmacion IS NULL;
    SELECT 1;
END
GO

-- ============================================================
-- D1.5 — DesvincularActivoPersonaConfirmacion (autenticado + titularidad)
--        Retornos: 0 desconocido/ya desvinculado · 1 ok · 3 no autorizado
-- ============================================================
IF OBJECT_ID(N'dbo.DesvincularActivoPersonaConfirmacion', N'P') IS NOT NULL DROP PROCEDURE dbo.DesvincularActivoPersonaConfirmacion;
GO
CREATE PROCEDURE [dbo].[DesvincularActivoPersonaConfirmacion]
(
    @TokenConfirmacion UNIQUEIDENTIFIER,
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id BIGINT, @PersonaId BIGINT, @PersonaIdUsuario BIGINT;

    SELECT @Id = Id, @PersonaId = PersonaId FROM PersonaActivo WHERE TokenConfirmacion = @TokenConfirmacion;
    IF @Id IS NULL BEGIN SELECT 0; RETURN; END                              -- token desconocido

    IF EXISTS(SELECT 1 FROM PersonaActivo WHERE Id = @Id AND FechaFin IS NOT NULL)
        BEGIN SELECT 0; RETURN; END                                         -- ya desvinculado

    SELECT @PersonaIdUsuario = PersonaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @PersonaIdUsuario IS NULL OR @PersonaIdUsuario <> @PersonaId
        BEGIN SELECT 3; RETURN; END                                         -- 3 = no autorizado

    UPDATE PersonaActivo
    SET FechaFin = GETDATE(), ModificadoPor = @Usuario, FechaModificacion = GETDATE()
    WHERE Id = @Id AND FechaFin IS NULL;

    SELECT 1;
END
GO

-- ============================================================
-- D1.6 — Enriquecer ObtenerActivosPorPersona (aditivo, DROP/CREATE)
-- ============================================================
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
           pa.FechaConfirmacion, pa.TokenConfirmacion,
           a.Nombre AS ActivoNombre, a.Serial AS ActivoSerial,
           ta.Nombre AS TipoActivoNombre,
           m.Nombre AS MarcaNombre,
           mo.Nombre AS ModeloNombre,
           p.Nombre AS PersonaNombre, p.Apellido AS PersonaApellido,
           pa.CreadoPor AS AsignadoPor
    FROM PersonaActivo pa
    INNER JOIN Activo a ON pa.ActivoId = a.Id
    LEFT JOIN TipoActivo ta ON a.TipoActivoID = ta.Id
    LEFT JOIN Marca m ON a.MarcaID = m.Id
    LEFT JOIN Modelo mo ON a.ModeloID = mo.Id
    INNER JOIN Persona p ON pa.PersonaId = p.Id
    WHERE pa.PersonaId = @PersonaId AND pa.FechaFin IS NULL AND pa.Estatus = 1 AND pa.EmpresaId = @EmpresaId
    ORDER BY pa.FechaInicio DESC;
END
GO

-- ============================================================
-- D1.7 — Enriquecer ObtenerPersonas (DROP/CREATE)
--        Añade UsuarioId / NombreUsuarioVinculado vía LEFT JOIN Usuarios
-- ============================================================
IF OBJECT_ID(N'dbo.ObtenerPersonas', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerPersonas;
GO
CREATE PROCEDURE [dbo].[ObtenerPersonas]
(
    @Usuario NVARCHAR(25)
)
AS
BEGIN
    SELECT p.*,
           pu.Nombre AS PuestoNombre,
           u.Id AS UsuarioId,
           u.NombreUsuario AS NombreUsuarioVinculado
    FROM Persona p
    INNER JOIN Puesto pu ON p.PuestoId = pu.Id
    LEFT JOIN Usuarios u ON u.PersonaId = p.Id AND u.Estatus = 1
    WHERE p.Estatus = 1
        AND p.EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY p.Nombre, p.Apellido
END
GO

-- ============================================================
-- D1.8 — Pagina "Mis Activos" + RolPaginaAccion (PuedeLeer=1) para roles "Usuario"
-- ⚠️ FLAG: la estructura real de Pagina/RolPaginaAccion en la BD hosted puede
-- diferir del dump (columnas Estatus/CreadoPor/FechaCreacion; NombreVisible y
-- EmpresaId no reflejados en el dump). VERIFICAR columnas con un SELECT de solo
-- lectura antes de ejecutar contra la BD hosted (T47, manual).
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'MisActivos')
BEGIN
    INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
    VALUES (N'MisActivos', N'Mis Activos', N'Activos asignados al usuario', N'Menu', N'/Home/MisActivos', NULL, N'fas fa-laptop', 99, 1);
END
GO

INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
SELECT r.Id, p.Id, 1, 0, 0, 0, 0, N'migracion', GETDATE(), 1
FROM Rol r
CROSS JOIN Pagina p
WHERE p.Nombre = N'MisActivos'
  AND r.Nombre = N'Usuario'
  AND r.Estatus = 1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
GO
