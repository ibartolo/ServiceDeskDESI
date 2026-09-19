/* ============================================================================
   ServiceDeskDESI — Script de PRUEBAS: borrar TODOS los datos de una empresa
   ----------------------------------------------------------------------------
   Uso:   1) Cambia @EmpresaId por el Id de la empresa a borrar.
          2) Ejecuta el script completo en la base de pruebas.
   NO es un stored procedure (script suelto para pruebas).
   Borra en orden de dependencias (hijos -> padre) dentro de una transacción.
   ============================================================================ */

SET NOCOUNT ON;
-- Los índices filtrados (UX_Usuarios_NombreUsuario_Empresa) requieren estas opciones para DML.
-- SqlClient (la app) las trae ON por defecto; sqlcmd las trae OFF, por eso se fuerzan aquí.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @EmpresaId BIGINT = 26;   -- <<<<< CAMBIA AQUÍ EL ID DE LA EMPRESA

IF NOT EXISTS (SELECT 1 FROM Empresa WHERE Id = @EmpresaId)
BEGIN
    PRINT 'La empresa con Id ' + CAST(@EmpresaId AS VARCHAR(20)) + ' no existe.';
    RETURN;
END

BEGIN TRY
    BEGIN TRAN;

    -- Ids auxiliares (usuarios y roles de la empresa)
    DECLARE @Usuarios TABLE (Id BIGINT);
    DECLARE @Roles    TABLE (Id BIGINT);
    INSERT INTO @Usuarios SELECT Id FROM Usuarios WHERE EmpresaId = @EmpresaId;
    INSERT INTO @Roles    SELECT Id FROM Rol      WHERE EmpresaId = @EmpresaId;

    /* --- 1) Hijos que dependen de Usuarios / Roles (sin EmpresaId) --- */
    DELETE FROM TokenRecuperacion WHERE UsuarioId IN (SELECT Id FROM @Usuarios);
    DELETE FROM UsuarioPagina     WHERE UsuarioID IN (SELECT Id FROM @Usuarios);
    DELETE FROM UsuarioRol        WHERE UsuarioId IN (SELECT Id FROM @Usuarios)
                                     OR RolId     IN (SELECT Id FROM @Roles);
    DELETE FROM RolPaginaAccion   WHERE RolId     IN (SELECT Id FROM @Roles);

    /* --- 2) Bitácora de correo ligada a asignaciones de activos de la empresa --- */
    DELETE FROM BitacoraCorreo
    WHERE ReferenciaId IN (SELECT Id FROM PersonaActivo WHERE EmpresaId = @EmpresaId);

    /* --- 3) Hijos de Activo / Ticket / Categoría --- */
    DELETE FROM Mantenimiento    WHERE EmpresaId = @EmpresaId;
    DELETE FROM PersonaActivo    WHERE EmpresaId = @EmpresaId;
    DELETE FROM TicketEvidencia  WHERE EmpresaId = @EmpresaId;
    DELETE FROM TicketAsignacion WHERE EmpresaId = @EmpresaId;
    DELETE FROM Ticket           WHERE EmpresaId = @EmpresaId;
    DELETE FROM CategoriaResponsable WHERE EmpresaId = @EmpresaId;

    /* --- 4) Catálogos de activos --- */
    DELETE FROM Activo     WHERE EmpresaId = @EmpresaId;
    DELETE FROM Modelo     WHERE EmpresaId = @EmpresaId;
    DELETE FROM Marca      WHERE EmpresaId = @EmpresaId;
    DELETE FROM TipoActivo WHERE EmpresaId = @EmpresaId;

    /* --- 5) Categorías (auto-referencia: borrar hojas primero) --- */
    WHILE EXISTS (SELECT 1 FROM Categoria c
                  WHERE c.EmpresaId = @EmpresaId
                    AND NOT EXISTS (SELECT 1 FROM Categoria ch WHERE ch.CategoriaPadreId = c.Id))
    BEGIN
        DELETE FROM Categoria
        WHERE EmpresaId = @EmpresaId
          AND Id IN (SELECT c.Id FROM Categoria c
                     WHERE c.EmpresaId = @EmpresaId
                       AND NOT EXISTS (SELECT 1 FROM Categoria ch WHERE ch.CategoriaPadreId = c.Id));
    END

    /* --- 6) Usuarios y su estructura (Usuarios antes de Persona/Sucursal/Area) --- */
    DELETE FROM Usuarios WHERE EmpresaId = @EmpresaId;
    DELETE FROM Persona  WHERE EmpresaId = @EmpresaId;
    DELETE FROM Puesto   WHERE EmpresaId = @EmpresaId;
    DELETE FROM Area     WHERE EmpresaId = @EmpresaId;
    DELETE FROM Sucursal WHERE EmpresaId = @EmpresaId;
    DELETE FROM Rol      WHERE EmpresaId = @EmpresaId;

    /* --- 7) Configuración / foliador de la empresa --- */
    DELETE FROM Foliador               WHERE EmpresaId = @EmpresaId;
    DELETE FROM EmpresaHorarioLaboral  WHERE EmpresaId = @EmpresaId;

    /* --- 8) La empresa --- */
    DELETE FROM Empresa WHERE Id = @EmpresaId;

    COMMIT;
    PRINT 'Empresa ' + CAST(@EmpresaId AS VARCHAR(20)) + ' y todos sus datos fueron eliminados.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT 'ERROR al borrar la empresa: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO
