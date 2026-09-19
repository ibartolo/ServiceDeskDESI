function ConfirmarReasignar() {
    var ticketId = $("#ticketIdReasignar").val();
    var nuevoUsuarioId = $("#ddlUsuarioReasignar").val();
    var comentario = $("#comentarioReasignar").val();

    if (!nuevoUsuarioId) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar un usuario',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    if (!comentario || !comentario.trim()) {
        Swal.fire({
            title: 'Error',
            text: 'Debe escribir un comentario para reasignar el ticket.',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    if (comentario.length > 300) {
        Swal.fire({
            title: 'Error',
            text: 'El comentario no puede superar los 300 caracteres.',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    Swal.fire({
        title: 'Reasignando...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    PostMVC('/Ticket/ReasignarTicket', { ticketId: ticketId, nuevoUsuarioId: parseInt(nuevoUsuarioId), comentario: comentario }, function (response) {
        Swal.close();

        if (response.IsSuccess) {
            bootstrap.Modal.getInstance(document.getElementById('modalReasignarTicket')).hide();

            if (typeof RefrescarTabla === 'function') {
                RefrescarTabla();
            }

            Swal.fire({
                title: '¡Éxito!',
                text: response.Message,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false,
                background: 'white',
                iconColor: '#4e73df'
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: response.Message,
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}
