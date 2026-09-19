function CargarActivosDisponibles() {
    GetMVC('/Catalogs/ObtenerActivosDisponibles', function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        var ddl = $("#ddlActivoDisponible");
        ddl.empty().append('<option value="">Seleccione un activo</option>');
        if (result && result.IsSuccess && result.Response) {
            result.Response.forEach(function (a) {
                if (a.Serial) {
                    ddl.append('<option value="' + a.Id + '">' + a.Nombre + ' — ' + a.Serial + '</option>');
                } else {
                    ddl.append('<option value="' + a.Id + '">' + a.Nombre + '</option>');
                }
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: (result && result.Message) || 'No se pudieron cargar los activos disponibles',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function CargarActivosPersona() {
    var personaId = $("#personaIdActivo").val();
    if (!personaId) return;
    GetMVC('/Catalogs/ObtenerActivosPorPersona?personaId=' + personaId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        var tbody = $("#tblActivosPersona tbody");
        tbody.empty();
        if (result && result.IsSuccess && result.Response && result.Response.length > 0) {
            result.Response.forEach(function (pa) {
                var fecha = pa.FechaInicio ? new Date(pa.FechaInicio).toLocaleDateString() : '---';
                var row = '<tr>';
                row += '<td>' + (pa.ActivoNombre || '---') + '</td>';
                row += '<td>' + (pa.ActivoSerial || '---') + '</td>';
                row += '<td>' + fecha + '</td>';
                row += '<td><button type="button" class="btn btn-sm btn-outline-danger" title="Desvincular" onclick="DesvincularActivo(this, ' + pa.Id + ')"><i class="fas fa-unlink"></i></button></td>';
                row += '</tr>';
                tbody.append(row);
            });
        } else {
            tbody.append('<tr><td colspan="4" class="text-center">Sin activos asignados</td></tr>');
        }
    });
}

function AsignarActivo() {
    var activoId = $("#ddlActivoDisponible").val();
    if (!activoId) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar un activo',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }
    var personaId = $("#personaIdActivo").val();
    var btn = $("#btnAsignarActivo");
    btn.prop("disabled", true);
    PostMVC('/Catalogs/AsignarActivoPersona', { personaId: personaId, activoId: activoId }, function (response) {
        btn.prop("disabled", false);
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result && result.IsSuccess) {
            Swal.fire({
                title: '¡Éxito!',
                text: result.Message || 'Activo asignado correctamente',
                icon: 'success',
                timer: 1500,
                showConfirmButton: false,
                background: 'white',
                iconColor: '#4e73df'
            });
            CargarActivosDisponibles();
            CargarActivosPersona();
        } else {
            Swal.fire({
                title: 'Error',
                text: (result && result.Message) || 'No se pudo asignar el activo',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function DesvincularActivo(btn, personaActivoId) {
    Swal.fire({
        title: '¿Iniciar desvinculación de este activo?',
        text: 'Se enviará un correo al usuario vinculado para que confirme la desvinculación.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, enviar correo',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#4e73df',
        cancelButtonColor: '#858796',
        background: 'white'
    }).then(function (result) {
        if (result.isConfirmed) {
            $(btn).prop("disabled", true);
            PostMVC('/Catalogs/IniciarDesvinculacion', { personaActivoId: personaActivoId }, function (response) {
                $(btn).prop("disabled", false);
                var r = typeof response === 'string' ? JSON.parse(response) : response;
                if (r && r.IsSuccess) {
                    Swal.fire({
                        title: '¡Éxito!',
                        text: r.Message || 'Se envió el correo de desvinculación al usuario',
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false,
                        background: 'white',
                        iconColor: '#4e73df'
                    });
                    CargarActivosDisponibles();
                    CargarActivosPersona();
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: (r && r.Message) || 'No se pudo iniciar la desvinculación',
                        icon: 'error',
                        confirmButtonText: 'Aceptar',
                        background: 'white',
                        confirmButtonColor: '#4e73df'
                    });
                }
            });
        }
    });
}
