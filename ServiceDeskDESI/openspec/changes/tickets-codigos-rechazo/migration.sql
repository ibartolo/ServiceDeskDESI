/* ============================================================================
   ServiceDeskDESI — Migración: códigos de rechazo en TransicionarTicket
   ----------------------------------------------------------------------------
   Cambio: tickets-codigos-rechazo
   Fecha:  2026-09-09
   Motivo: el SP devolvía SIEMPRE 0 ante cualquier rechazo -> el DAL mostraba un
           mensaje genérico ("Verifique que sea un agente, que el ticket sea de su
           área...") sin saber la causa real.
   Ahora devuelve un CÓDIGO NEGATIVO por cada motivo de rechazo.
   Además: se quita la validación de ÁREA en 'Tomar' y 'Retomar' (un agente puede
   tomar cualquier ticket de su empresa, consistente con la visibilidad global).

   CÓDIGOS DE RETORNO:
     > 0  Éxito (Id de la asignación creada)
     0    Error inesperado (catch)
    -1    Usuario no encontrado o inactivo
    -2    Ticket no encontrado / no pertenece a la empresa
    -3    El usuario no es agente (no puede atender tickets)
    -4    El estatus del ticket no permite ese movimiento
    -5    El ticket no está disponible (ya tiene una asignación activa)   [Tomar]
    -6    El usuario no es el agente asignado (dueño) del ticket          [Resolver/Pausas/Reanudar]
    -7    Comentario requerido o inválido (1..300)
    -8    El usuario no es responsable del área del ticket                [Reasignar]
    -9    El usuario destino no es un agente válido del área del ticket   [Reasignar]
    -10   El usuario no es el creador del ticket                          [Cerrar/Rechazar]

   Aplica a: db_9c7990_servicedeskdesi (dev) y db_9c7990_helpdeskdesi (prod).
   Idempotente: sí (CREATE OR ALTER).
   ============================================================================ */

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[TransicionarTicket]
(
    @TicketId        BIGINT,
    @TipoMovimiento  NVARCHAR(20),   -- Tomar|Resolver|Retomar|Cerrar|Rechazar|Reasignar|PendienteMateriales|EnEsperaTerceros|Reanudar
    @Comentario      NVARCHAR(300) = NULL,
    @NuevoUsuarioId  BIGINT = NULL,
    @FechaEstimada   DATE = NULL,
    @Usuario         NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UsuarioId BIGINT, @EmpresaId BIGINT, @EstatusActual INT, @EsAgente BIT = 0,
            @Resultado INT, @AgenteFinal BIGINT, @EsActiva BIT = 1, @AsignacionId BIGINT,
            @TicketAreaId BIGINT, @CreadoPorTicket NVARCHAR(25);

    -- -1) Usuario no encontrado / inactivo
    SELECT @UsuarioId = Id, @EmpresaId = EmpresaId FROM Usuarios WHERE NombreUsuario = @Usuario AND Estatus = 1;
    IF @UsuarioId IS NULL BEGIN SELECT -1; RETURN; END

    -- -2) Ticket no encontrado / no pertenece a la empresa
    SELECT @EstatusActual = TicketEstatusId, @TicketAreaId = AreaId, @CreadoPorTicket = CreadoPor
    FROM Ticket WHERE Id = @TicketId AND Estatus = 1 AND EmpresaId = @EmpresaId;
    IF @EstatusActual IS NULL BEGIN SELECT -2; RETURN; END

    IF EXISTS(SELECT 1 FROM UsuarioRol ur INNER JOIN Rol r ON ur.RolId = r.Id
              WHERE ur.UsuarioId = @UsuarioId AND r.PuedeAtenderTickets = 1 AND ur.Estatus = 1 AND r.Estatus = 1)
        SET @EsAgente = 1;

    -- ===================== TOMAR =====================
    IF @TipoMovimiento = 'Tomar'
    BEGIN
        IF @EsAgente <> 1 BEGIN SELECT -3; RETURN; END   -- no es agente
        IF @EstatusActual <> 1 BEGIN SELECT -4; RETURN; END   -- no está "Nuevo"
        IF EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1)
            BEGIN SELECT -5; RETURN; END   -- ya está asignado
        -- (Sin validación de área: un agente puede tomar cualquier ticket de su empresa)
    END

    -- ===================== RESOLVER =====================
    IF @TipoMovimiento = 'Resolver'
    BEGIN
        IF @EsAgente <> 1 BEGIN SELECT -3; RETURN; END
        IF @EstatusActual <> 2 BEGIN SELECT -4; RETURN; END
        IF NOT EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
            BEGIN SELECT -6; RETURN; END   -- no es el agente dueño
        IF @Comentario IS NULL OR LEN(LTRIM(RTRIM(@Comentario))) NOT BETWEEN 1 AND 300 BEGIN SELECT -7; RETURN; END
    END

    -- ===================== RETOMAR =====================
    IF @TipoMovimiento = 'Retomar'
    BEGIN
        IF @EsAgente <> 1 BEGIN SELECT -3; RETURN; END
        IF @EstatusActual <> 4 BEGIN SELECT -4; RETURN; END
        -- (Sin validación de área: consistente con 'Tomar')
    END

    -- ===================== CERRAR / RECHAZAR =====================
    IF @TipoMovimiento IN ('Cerrar','Rechazar')
    BEGIN
        IF @CreadoPorTicket <> @Usuario BEGIN SELECT -10; RETURN; END   -- no es el creador
        IF @EstatusActual <> 3 BEGIN SELECT -4; RETURN; END
        IF @Comentario IS NULL OR LEN(LTRIM(RTRIM(@Comentario))) NOT BETWEEN 1 AND 300 BEGIN SELECT -7; RETURN; END
    END

    -- ===================== REASIGNAR =====================
    IF @TipoMovimiento = 'Reasignar'
    BEGIN
        IF @NuevoUsuarioId IS NULL BEGIN SELECT -9; RETURN; END
        IF @EstatusActual NOT IN (2, 4) BEGIN SELECT -4; RETURN; END
        IF @Comentario IS NULL OR LEN(LTRIM(RTRIM(@Comentario))) NOT BETWEEN 1 AND 300 BEGIN SELECT -7; RETURN; END
        IF NOT EXISTS(SELECT 1 FROM Area WHERE Id = @TicketAreaId AND UsuarioResponsableId = @UsuarioId)
            BEGIN SELECT -8; RETURN; END   -- no es responsable del área
        IF NOT EXISTS(SELECT 1 FROM Usuarios u INNER JOIN UsuarioRol ur ON u.Id = ur.UsuarioId INNER JOIN Rol r ON ur.RolId = r.Id
                      WHERE u.Id = @NuevoUsuarioId AND u.EmpresaId = @EmpresaId AND u.Estatus = 1
                        AND ur.Estatus = 1 AND r.Estatus = 1 AND r.PuedeAtenderTickets = 1
                        AND u.AreaId = @TicketAreaId)
            BEGIN SELECT -9; RETURN; END   -- destino no es agente del área
    END

    -- ===================== PAUSAS =====================
    IF @TipoMovimiento IN ('PendienteMateriales','EnEsperaTerceros')
    BEGIN
        IF @EsAgente <> 1 BEGIN SELECT -3; RETURN; END
        IF @EstatusActual <> 2 BEGIN SELECT -4; RETURN; END
        IF NOT EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
            BEGIN SELECT -6; RETURN; END
        IF @Comentario IS NULL OR LEN(LTRIM(RTRIM(@Comentario))) NOT BETWEEN 1 AND 300 BEGIN SELECT -7; RETURN; END
    END

    -- ===================== REANUDAR =====================
    IF @TipoMovimiento = 'Reanudar'
    BEGIN
        IF @EsAgente <> 1 BEGIN SELECT -3; RETURN; END
        IF @EstatusActual NOT IN (6, 7) BEGIN SELECT -4; RETURN; END
        IF NOT EXISTS(SELECT 1 FROM TicketAsignacion WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1 AND UsuarioId = @UsuarioId)
            BEGIN SELECT -6; RETURN; END
    END

    SET @Resultado = CASE @TipoMovimiento
        WHEN 'Tomar' THEN 2 WHEN 'Resolver' THEN 3 WHEN 'Retomar' THEN 2
        WHEN 'Cerrar' THEN 5 WHEN 'Rechazar' THEN 4 WHEN 'Reasignar' THEN 2
        WHEN 'PendienteMateriales' THEN 6 WHEN 'EnEsperaTerceros' THEN 7 WHEN 'Reanudar' THEN 2 END;

    SET @AgenteFinal = CASE WHEN @TipoMovimiento = 'Reasignar' THEN @NuevoUsuarioId ELSE @UsuarioId END;
    SET @EsActiva = CASE WHEN @TipoMovimiento IN ('Cerrar','Rechazar') THEN 0 ELSE 1 END;

    IF @TipoMovimiento NOT IN ('PendienteMateriales','EnEsperaTerceros') SET @FechaEstimada = NULL;

    BEGIN TRY
        BEGIN TRAN;
        UPDATE TicketAsignacion SET EsActiva = 0, ModificadoPor = @Usuario, FechaModificacion = GETDATE()
            WHERE TicketId = @TicketId AND EsActiva = 1 AND Estatus = 1;
        INSERT INTO TicketAsignacion (TicketId, UsuarioId, Comentario, EsActiva, TipoMovimiento, TicketEstatusId, FechaEstimada, CreadoPor, FechaCreacion, Estatus, EmpresaId)
            VALUES (@TicketId, @AgenteFinal, @Comentario, @EsActiva, @TipoMovimiento, @Resultado, @FechaEstimada, @Usuario, GETDATE(), 1, @EmpresaId);
        SET @AsignacionId = SCOPE_IDENTITY();
        UPDATE Ticket SET TicketEstatusId = @Resultado, ModificadoPor = @Usuario, FechaModificacion = GETDATE() WHERE Id = @TicketId;
        COMMIT TRAN;
        SELECT @AsignacionId;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRAN;
        SELECT 0;   -- error inesperado
    END CATCH
END
GO
