-- ============================================================
-- Migration: hotfix-quoted-identifier-usuarios
-- Corrige error SQL 1934 ("... 'QUOTED_IDENTIFIER'") al guardar/actualizar usuarios.
--
-- Causa raíz: el procedure se alteró con `SET QUOTED_IDENTIFIER OFF`.
-- SQL Server guarda ese valor en la metadata del proc (sys.sql_modules) y
-- lo usa AL EJECUTAR, ignorando la conexión. Como la tabla Usuarios tiene
-- el índice filtrado UX_Usuarios_PersonaId, todo INSERT/UPDATE/DELETE sobre
-- ella exige QUOTED_IDENTIFIER ON → error 1934.
--
-- Fix: ALTERAR el proc con `SET QUOTED_IDENTIFIER ON` (metadata ON) y,
-- además, setearlo DENTRO del body (runtime ON). Doble garantía.
-- Fecha: 2026-08-27
-- ============================================================

USE [db_9c7990_servicedeskdesi]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GuardarOActualizarUsuarioAdmin]
(
    @Id BIGINT,
    @NombreUsuario NVARCHAR(25),
    @Contrasena NVARCHAR(250) = NULL,
    @ImagenPerfil NVARCHAR(250) = NULL,
    @Correo NVARCHAR(250),
    @Nombre NVARCHAR(150),
    @Apellido NVARCHAR(250),
    @Celular NVARCHAR(50),
    @CreadoPor NVARCHAR(25),
    @FechaCreacion DATETIME,
    @ModificadoPor NVARCHAR(25) = NULL,
    @FechaModificacion DATETIME = NULL,
    @Estatus BIT,
    @SucursalId BIGINT,
    @Firma NVARCHAR(250) = NULL,
    @RFC NVARCHAR(250),
    @AreaId BIGINT,
    @EmpresaId BIGINT,
    @UsuarioAdmin NVARCHAR(25)
)
AS
BEGIN
    SET QUOTED_IDENTIFIER ON;
    SET ANSI_NULLS ON;
    SET NOCOUNT ON;

    IF NOT EXISTS(
        SELECT 1
        FROM Usuarios
        WHERE NombreUsuario = @UsuarioAdmin
            AND EmpresaId = @EmpresaId
            AND Estatus = 1
    )
    BEGIN
        SELECT 0
        RETURN
    END

    IF EXISTS(SELECT 1 FROM Usuarios WHERE Id = @Id)
    BEGIN
        IF NOT EXISTS(
            SELECT 1
            FROM Usuarios
            WHERE Id = @Id
                AND EmpresaId = @EmpresaId
                AND Estatus = 1
        )
        BEGIN
            SELECT 0
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND Id != @Id AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -1
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE Correo = @Correo AND Id != @Id AND EmpresaId = @EmpresaId AND Estatus = 1)
        BEGIN
            SELECT -2
            RETURN
        END

        -- UPDATE: actualiza la contraseña solo si viene con valor; si no, conserva la existente
        UPDATE Usuarios
        SET NombreUsuario = @NombreUsuario,
            Contrasena = CASE
                WHEN @Contrasena IS NOT NULL AND @Contrasena != '' THEN @Contrasena
                ELSE Contrasena
            END,
            ImagenPerfil = @ImagenPerfil,
            Correo = @Correo,
            Nombre = @Nombre,
            Apellido = @Apellido,
            Celular = @Celular,
            ModificadoPor = @ModificadoPor,
            FechaModificacion = @FechaModificacion,
            Estatus = @Estatus,
            SucursalId = @SucursalId,
            Firma = @Firma,
            RFC = @RFC,
            AreaId = @AreaId,
            EmpresaId = @EmpresaId
        WHERE Id = @Id

        SELECT @Id
    END
    ELSE
    BEGIN
        IF EXISTS(SELECT 1 FROM Usuarios WHERE NombreUsuario = @NombreUsuario AND EmpresaId = @EmpresaId)
        BEGIN
            SELECT -1
            RETURN
        END

        IF EXISTS(SELECT 1 FROM Usuarios WHERE Correo = @Correo AND EmpresaId = @EmpresaId)
        BEGIN
            SELECT -2
            RETURN
        END

        -- INSERT: la contraseña viene ya hasheada desde el service (obligatoria al crear)
        INSERT INTO Usuarios
        (NombreUsuario, Contrasena, ImagenPerfil, Correo, Nombre, Apellido,
         Celular, CreadoPor, FechaCreacion, Estatus, SucursalId, Firma, RFC, AreaId, EmpresaId)
        VALUES
        (@NombreUsuario, @Contrasena, @ImagenPerfil, @Correo, @Nombre, @Apellido,
         @Celular, @CreadoPor, @FechaCreacion, @Estatus, @SucursalId, @Firma, @RFC, @AreaId, @EmpresaId)

        SELECT SCOPE_IDENTITY()
    END
END
GO

-- ============================================================
-- IMPORTANTE: los siguientes procedures TAMBIÉN modifican la tabla Usuarios
-- y tendrán el mismo error 1934 si fueron alterados con QUOTED_IDENTIFIER OFF:
--   · GuardarOActualizarUsuario
--   · ActualizarPerfilUsuario
--   · EliminarUsuario
--   · ActualizarContrasena / RestablecerContrasena
--   · VincularPersonaUsuario / DesvincularPersonaUsuario
--
-- Para cada uno, aplicar el MISMO fix:
--   1) Asegurar `SET QUOTED_IDENTIFIER ON` antes del CREATE/ALTER.
--   2) Agregar `SET QUOTED_IDENTIFIER ON; SET ANSI_NULLS ON;` al inicio del body.
-- ============================================================
