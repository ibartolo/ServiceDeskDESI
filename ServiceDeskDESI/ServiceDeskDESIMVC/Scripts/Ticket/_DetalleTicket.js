var historialTable;

// Ajustar columnas del DataTable cuando el modal ya es visible (evita desalineación al inicializarse oculto)
$("#modalDetalleTicket").on("shown.bs.modal", function () {
    if (historialTable) {
        historialTable.columns.adjust().draw();
    }
});

function CargarHistorial(ticketId, callback) {
    GetMVC('/Ticket/ObtenerTicketAsignaciones?ticketId=' + ticketId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        var data = (result.IsSuccess && result.Response) ? result.Response : [];

        if (!historialTable) {
            historialTable = $('#tblHistorial').DataTable({
                data: data,
                columns: [
                    {
                        data: 'FechaCreacion',
                        render: function (d) {
                            return d ? new Date(d).toLocaleString() : '---';
                        }
                    },
                    { data: 'TipoMovimiento', defaultContent: '---' },
                    {
                        data: 'AgenteNombre',
                        render: function (d, type, row) {
                            return row.AgenteNombre ? row.AgenteNombre + ' ' + (row.AgenteApellido || '') : '---';
                        }
                    },
                    { data: 'Comentario', defaultContent: '---' },
                    {
                        data: 'EstatusNombre',
                        render: function (d, type, row) {
                            if (row.EstatusColor) {
                                return '<span class="badge" style="background-color: ' + row.EstatusColor + '; color: white;">' + row.EstatusNombre + '</span>';
                            }
                            return d || '---';
                        }
                    }
                ],
                language: {
                    url: "/Content/datatables/i18n/es-ES.json"
                }
            });
        } else {
            historialTable.clear().rows.add(data).draw();
        }

        if (typeof callback === 'function') { callback(); }
    });
}

$("#inputEvidenciasModal").change(function () {
    if (!ValidarArchivosEvidencia(this.files)) {
        $(this).val('');
    }
});

function CargarEvidencias(ticketId, callback) {
    GetMVC('/Ticket/ObtenerEvidenciasPorTicket?ticketId=' + ticketId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        var data = (result.IsSuccess && result.Response) ? result.Response : [];

        var html = '';

        if (!data || data.length === 0) {
            html = '<p class="text-muted mb-0">Sin evidencias.</p>';
        } else {
            html = '<ul class="list-group">';
            data.forEach(function (e) {
                var fecha = e.FechaSubida ? new Date(e.FechaSubida).toLocaleString() : '';
                html += '<li class="list-group-item d-flex justify-content-between align-items-center">' +
                    '<span><i class="fas fa-file me-2"></i>' + e.NombreArchivo +
                    (fecha ? ' <small class="text-muted">(' + fecha + ')</small>' : '') +
                    '</span>' +
                    '<a class="btn btn-sm btn-outline-primary" href="/Ticket/DescargarEvidencia?id=' + e.Id + '">' +
                    '<i class="fas fa-download me-1"></i>Descargar</a>' +
                    '</li>';
            });
            html += '</ul>';
        }

        $("#listaEvidencias").html(html);

        if (typeof callback === 'function') { callback(); }
    });
}

var ticketIdSubirEvidencia = 0;

function AbrirSubirEvidencia(ticketId) {
    ticketIdSubirEvidencia = ticketId;
    var input = document.getElementById('inputEvidenciasModal');
    if (input) input.value = '';
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalSubirEvidencia')).show();
}

function SubirEvidenciaDesdeModal() {
    var input = document.getElementById('inputEvidenciasModal');

    if (!input || !input.files || input.files.length === 0) {
        Swal.fire({ title: 'Error', text: 'Debe seleccionar al menos un archivo.', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }

    if (!ticketIdSubirEvidencia || ticketIdSubirEvidencia <= 0) {
        Swal.fire({ title: 'Error', text: 'No se ha seleccionado un ticket.', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }

    var files = Array.prototype.slice.call(input.files);

    if (!ValidarArchivosEvidencia(files)) {
        input.value = '';
        return;
    }

    SubirEvidencias(ticketIdSubirEvidencia, files, function () {
        input.value = '';
        bootstrap.Modal.getInstance(document.getElementById('modalSubirEvidencia')).hide();
        CargarEvidencias(ticketIdSubirEvidencia);
        ToastExito('Evidencias subidas correctamente.');
    });
}
