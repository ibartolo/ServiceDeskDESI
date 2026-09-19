$(document).ready(function () {
    var tabla = new DataTable('#tblMisActivos', {
        columns: [
            { data: 'ActivoNombre', title: 'Activo' },
            { data: 'ActivoSerial', title: 'Serial', defaultContent: '---' },
            { data: 'TipoActivoNombre', title: 'Tipo', defaultContent: '---' },
            { data: 'MarcaNombre', title: 'Marca', defaultContent: '---' },
            { data: 'ModeloNombre', title: 'Modelo', defaultContent: '---' },
            {
                data: 'FechaInicio', title: 'Fecha inicio', render: function (d) {
                    return d ? new Date(d).toLocaleDateString() : '---';
                }
            },
            { data: 'AsignadoPor', title: 'Asignado por', defaultContent: '---' },
            {
                data: 'FechaConfirmacion', title: 'Estado', render: function (d) {
                    return d
                        ? '<span class="badge bg-success">Vigente</span>'
                        : '<span class="badge bg-warning text-dark">Por aceptar</span>';
                }
            },
            {
                data: 'TokenConfirmacion', title: 'Acciones', render: function (d, type, row) {
                    if (d && !row.FechaConfirmacion) {
                        return '<button type="button" class="btn btn-sm btn-success" onclick="AceptarActivo(\'' + d + '\')"><i class="fas fa-check me-1"></i>Aceptar</button>';
                    }
                    return '';
                }
            }
        ],
        language: {
            url: "/Content/datatables/i18n/es-ES.json"
        }
    });
    CargarMisActivos();
});

function CargarMisActivos() {
    GetMVC('/Home/ObtenerMisActivos', function (r) {
        var result = typeof r === 'string' ? JSON.parse(r) : r;
        if (result && result.IsSuccess) {
            MapingPropertiesDataTable("tblMisActivos", result.Response);
        } else {
            MapingPropertiesDataTable("tblMisActivos", []);
        }
    });
}

function AceptarActivo(token) {
    Swal.fire({
        title: '¿Aceptar este activo?',
        text: 'Confirmará la recepción del activo.',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, aceptar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#1cc88a',
        cancelButtonColor: '#858796',
        background: 'white'
    }).then(function (res) {
        if (res.isConfirmed) {
            PostMVC('/Home/AceptarAsignacion', { token: token }, function (r2) {
                var acc = typeof r2 === 'string' ? JSON.parse(r2) : r2;
                if (acc && acc.IsSuccess) {
                    Swal.fire({
                        title: '¡Aceptado!',
                        text: acc.Message,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false,
                        background: 'white',
                        iconColor: '#4e73df'
                    }).then(function () {
                        CargarMisActivos();
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: (acc && acc.Message) || 'No se pudo aceptar el activo.',
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
