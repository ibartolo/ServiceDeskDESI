function AbrirMantenimientos(id) {
    $("#activoIdMantenimiento").val(id);
    $("#mantenimientoComentario").val('');
    $("#tblMantenimientos tbody").empty();
    CargarMantenimientos(id);
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalMantenimientoActivo')).show();
}

function CargarMantenimientos(activoId) {
    var tbody = $("#tblMantenimientos tbody");
    tbody.empty();
    GetMVC('/Catalogs/ObtenerMantenimientosPorActivo?activoId=' + activoId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result && result.IsSuccess && result.Response && result.Response.length > 0) {
            result.Response.forEach(function (m) {
                var fecha = m.Fecha ? new Date(m.Fecha).toLocaleString() : '---';
                var row = '<tr>';
                row += '<td>' + fecha + '</td>';
                row += '<td>' + (m.Comentario || '---') + '</td>';
                row += '<td>' + (m.CreadoPor || '---') + '</td>';
                row += '</tr>';
                tbody.append(row);
            });
        } else {
            tbody.append('<tr><td colspan="3" class="text-center">Sin mantenimientos registrados</td></tr>');
        }
    });
}

function GuardarMantenimiento() {
    var activoId = $("#activoIdMantenimiento").val();
    var comentario = $("#mantenimientoComentario").val().trim();
    if (!comentario) {
        Swal.fire({
            title: 'Error',
            text: 'El comentario es requerido',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }
    var btn = $("#btnGuardarMantenimiento");
    btn.prop("disabled", true);
    PostMVC('/Catalogs/GuardarMantenimiento', { ActivoId: activoId, Comentario: comentario }, function (response) {
        btn.prop("disabled", false);
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result && result.IsSuccess) {
            Swal.fire({
                title: '¡Éxito!',
                text: result.Message || 'Mantenimiento guardado correctamente',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false,
                background: 'white',
                iconColor: '#4e73df'
            });
            $("#mantenimientoComentario").val('');
            CargarMantenimientos(activoId);
        } else {
            Swal.fire({
                title: 'Error',
                text: (result && result.Message) || 'No se pudo guardar el mantenimiento',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}
