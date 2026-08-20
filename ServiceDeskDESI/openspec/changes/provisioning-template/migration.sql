/* ============================================================================
   ServiceDeskDESI — Migración: Provisioning con template de roles (D4)
   ----------------------------------------------------------------------------
   Cambio: provisioning-template (ref D4)
   Fecha:  2026-08-18
   Base de datos: db_9c7990_servicedeskdesi
============================================================================ */

USE [db_9c7990_servicedeskdesi];
GO

IF OBJECT_ID('dbo.PlantillaRol', 'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PlantillaRol](
        [Id] [bigint] IDENTITY(1,1) NOT NULL,
        [Nombre] [nvarchar](50) NOT NULL,
        [Descripcion] [nvarchar](250) NULL,
        [PuedeAtenderTickets] [bit] NOT NULL,
        [Orden] [int] NOT NULL,
        CONSTRAINT [PK_PlantillaRol] PRIMARY KEY CLUSTERED ([Id] ASC)
    ) ON [PRIMARY]
END
GO

-- Seed (idempotente: solo inserta si no existen registros)
IF NOT EXISTS (SELECT 1 FROM PlantillaRol)
BEGIN
    INSERT INTO [dbo].[PlantillaRol] ([Nombre], [Descripcion], [PuedeAtenderTickets], [Orden])
    VALUES
    (N'Administrador', N'Control total del sistema', 1, 1),
    (N'Supervisor', N'Gestión de tickets y usuarios', 1, 2),
    (N'Agente', N'Atención de tickets', 1, 3),
    (N'Usuario', N'Creación de tickets', 0, 4)
END
GO

CREATE OR ALTER PROCEDURE [dbo].[ObtenerPlantillaRoles]
AS
BEGIN
    SELECT Id, Nombre, Descripcion, PuedeAtenderTickets, Orden
    FROM PlantillaRol
    ORDER BY Orden
END
GO
