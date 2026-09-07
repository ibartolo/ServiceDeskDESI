-- ============================================================
-- Migration: configuracion-empresa
-- Módulo "Configuración de Empresa": horario laboral (7 días) + logotipo.
-- Fecha: 2026-09-07
-- Idempotente: guards IF OBJECT_ID / IF NOT EXISTS (sys.columns / NOT EXISTS).
-- NO ejecutar contra la BD desde sdd-apply; la aplica el usuario manualmente.
-- ============================================================

-- 1. Tabla EmpresaHorarioLaboral
IF OBJECT_ID(N'dbo.EmpresaHorarioLaboral', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[EmpresaHorarioLaboral](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [EmpresaId] [bigint] NOT NULL,
        [DiaSemana] [tinyint] NOT NULL,            -- 1=Lun .. 7=Dom
        [HoraInicio] [datetime] NULL,              -- ancla 1900-01-01
        [HoraFin] [datetime] NULL,
        [Estatus] [bit] NOT NULL CONSTRAINT [DF_EmpresaHorarioLaboral_Estatus] DEFAULT ((1)),
        [CreadoPor] [nvarchar](25) NOT NULL,
        [FechaCreacion] [datetime] NOT NULL,
        [ModificadoPor] [nvarchar](25) NULL,
        [FechaModificacion] [datetime] NULL,
        CONSTRAINT [PK_EmpresaHorarioLaboral] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_EmpresaHorarioLaboral_EmpresaDia] UNIQUE ([EmpresaId], [DiaSemana]),
        CONSTRAINT [FK_EmpresaHorarioLaboral_Empresa] FOREIGN KEY ([EmpresaId]) REFERENCES [dbo].[Empresa]([Id])
    );
END
GO

-- 2. Columna Empresa.LogoUrl (URL parcial del logotipo)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.Empresa') AND name = N'LogoUrl')
    ALTER TABLE [dbo].[Empresa] ADD [LogoUrl] [nvarchar](500) NULL;
GO

-- 3. SP ObtenerHorarioLaboral (lectura; tenant desde @Usuario)
IF OBJECT_ID(N'dbo.ObtenerHorarioLaboral', N'P') IS NOT NULL DROP PROCEDURE dbo.ObtenerHorarioLaboral;
GO
CREATE PROCEDURE [dbo].[ObtenerHorarioLaboral] @Usuario NVARCHAR(25)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT DiaSemana, HoraInicio, HoraFin, Estatus
    FROM [dbo].[EmpresaHorarioLaboral]
    WHERE EmpresaId = (SELECT EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1)
    ORDER BY DiaSemana;
END
GO

-- 4. SP GuardarHorarioLaboralDia (upsert idempotente por día; tenant desde @Usuario)
IF OBJECT_ID(N'dbo.GuardarHorarioLaboralDia', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarHorarioLaboralDia;
GO
CREATE PROCEDURE [dbo].[GuardarHorarioLaboralDia]
    @Usuario NVARCHAR(25), @DiaSemana TINYINT,
    @HoraInicio DATETIME, @HoraFin DATETIME, @EsLaboral BIT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END
    IF @EsLaboral = 0 BEGIN SET @HoraInicio = NULL; SET @HoraFin = NULL; END
    IF EXISTS (SELECT 1 FROM EmpresaHorarioLaboral WHERE EmpresaId = @EmpresaId AND DiaSemana = @DiaSemana)
        UPDATE EmpresaHorarioLaboral
           SET HoraInicio = @HoraInicio, HoraFin = @HoraFin, Estatus = @EsLaboral,
               ModificadoPor = @Usuario, FechaModificacion = GETDATE()
         WHERE EmpresaId = @EmpresaId AND DiaSemana = @DiaSemana;
    ELSE
        INSERT INTO EmpresaHorarioLaboral (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
        VALUES (@EmpresaId, @DiaSemana, @HoraInicio, @HoraFin, @EsLaboral, @Usuario, GETDATE());
    SELECT 1;
END
GO

-- 5. SP GuardarLogoEmpresa (persiste URL parcial; tenant desde @Usuario)
IF OBJECT_ID(N'dbo.GuardarLogoEmpresa', N'P') IS NOT NULL DROP PROCEDURE dbo.GuardarLogoEmpresa;
GO
CREATE PROCEDURE [dbo].[GuardarLogoEmpresa] @Usuario NVARCHAR(25), @LogoUrl NVARCHAR(500)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @EmpresaId BIGINT;
    SELECT @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @EmpresaId IS NULL BEGIN SELECT 0; RETURN; END
    UPDATE Empresa SET LogoUrl = @LogoUrl, ModificadoPor = @Usuario, FechaModificacion = GETDATE()
     WHERE Id = @EmpresaId AND Estatus = 1;
    SELECT 1;
END
GO

-- 6. Backfill idempotente (empresas existentes sin filas de horario)
--    Lun-Vie 09:00-17:00 laborables; Sáb/Dom no laborables (horas NULL).
INSERT INTO [dbo].[EmpresaHorarioLaboral] (EmpresaId, DiaSemana, HoraInicio, HoraFin, Estatus, CreadoPor, FechaCreacion)
SELECT e.Id, d.Dia, d.Inicio, d.Fin, CASE WHEN d.Dia BETWEEN 1 AND 5 THEN 1 ELSE 0 END, N'migracion', GETDATE()
FROM [dbo].[Empresa] e
CROSS JOIN (VALUES
    (1, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (2, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (3, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (4, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (5, CAST('09:00' AS datetime), CAST('17:00' AS datetime)),
    (6, NULL, NULL),
    (7, NULL, NULL)) d(Dia, Inicio, Fin)
WHERE NOT EXISTS (SELECT 1 FROM EmpresaHorarioLaboral h WHERE h.EmpresaId = e.Id);
GO

-- 7. Seed de página "ConfiguracionEmpresa" + RolPaginaAccion (solo rol Administrador)
-- ⚠️ Verificar columnas reales de Pagina/RolPaginaAccion en la BD hosted antes de ejecutar (drift conocido).
IF NOT EXISTS (SELECT 1 FROM Pagina WHERE Nombre = N'ConfiguracionEmpresa')
BEGIN
    INSERT INTO Pagina (Nombre, NombreVisible, Descripcion, Tipo, Direccion, PermisosPadreId, Logo, OrdenB, Estatus)
    VALUES (N'ConfiguracionEmpresa', N'Configuración de Empresa', N'Datos, horario laboral y logotipo de la empresa', N'Menu', N'/ConfiguracionEmpresa', NULL, N'fa-cog', 9, 1);
END
GO

INSERT INTO RolPaginaAccion (RolId, PaginaId, PuedeLeer, PuedeCrear, PuedeEditar, PuedeEliminar, PuedeExportar, CreadoPor, FechaCreacion, Estatus)
SELECT r.Id, p.Id, 1, 0, 1, 0, 0, N'migracion', GETDATE(), 1
FROM Rol r CROSS JOIN Pagina p
WHERE p.Nombre = N'ConfiguracionEmpresa' AND r.Nombre = N'Administrador' AND r.Estatus = 1
  AND NOT EXISTS (SELECT 1 FROM RolPaginaAccion rpa WHERE rpa.RolId = r.Id AND rpa.PaginaId = p.Id);
GO
