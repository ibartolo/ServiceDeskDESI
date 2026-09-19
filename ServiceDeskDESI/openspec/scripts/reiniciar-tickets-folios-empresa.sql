/* ============================================================================
   ServiceDeskDESI — Script de PRUEBAS: borrar tickets y reiniciar folios
   ----------------------------------------------------------------------------
   Uso: cambia @EmpresaId y ejecuta.
   - Borra las evidencias y asignaciones de los tickets de la empresa.
   - Borra los tickets de la empresa.
   - Reinicia el foliador de tickets (Consecutivo = 0) de la empresa.
   ============================================================================ */

SET NOCOUNT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

DECLARE @EmpresaId BIGINT = 27;   -- <<<<< CAMBIA AQUÍ EL ID DE LA EMPRESA

BEGIN TRY
    BEGIN TRAN;

    DELETE FROM TicketEvidencia  WHERE EmpresaId = @EmpresaId;
    DELETE FROM TicketAsignacion WHERE EmpresaId = @EmpresaId;
    DELETE FROM Ticket           WHERE EmpresaId = @EmpresaId;

    -- Reiniciar el foliador de tickets (el siguiente será 1 -> T-00001)
    UPDATE Foliador
    SET Consecutivo = 0,
        FechaActualizacion = GETDATE()
    WHERE EmpresaId = @EmpresaId
      AND Nombre = 'Ticket';

    COMMIT;

    PRINT 'Tickets borrados y folios reiniciados para la empresa ' + CAST(@EmpresaId AS VARCHAR(20)) + '.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK;
    PRINT 'ERROR: ' + ERROR_MESSAGE();
    THROW;
END CATCH
GO
