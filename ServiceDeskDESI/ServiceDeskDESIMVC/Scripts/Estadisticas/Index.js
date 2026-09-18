var miEstatusChart = null;
var miEvolucionChart = null;

function formatDecimal(v) {
    if (v === null || v === undefined || isNaN(v)) {
        return '0';
    }
    return parseFloat(Number(v).toFixed(1));
}

function formatFechaCorta(f) {
    if (!f) {
        return '';
    }
    var d = new Date(f);
    if (isNaN(d)) {
        return f;
    }
    var dd = ('0' + d.getDate()).slice(-2);
    var mm = ('0' + (d.getMonth() + 1)).slice(-2);
    return dd + '/' + mm;
}

function renderResumen(res) {
    var r = (res && res.IsSuccess && res.Response) ? res.Response : null;
    $('#kpiTotal').text(r ? (r.Total || 0) : 0);
    $('#kpiNuevos').text(r ? (r.Nuevos || 0) : 0);
    $('#kpiEnProgreso').text(r ? (r.EnProgreso || 0) : 0);
    $('#kpiResueltos').text(r ? (r.Resueltos || 0) : 0);
    $('#kpiCerrados').text(r ? (r.Cerrados || 0) : 0);
    $('#kpiRechazados').text(r ? (r.Rechazados || 0) : 0);
    $('#kpiEficiencia').text((r ? formatDecimal(r.Eficiencia) : 0) + '%');
    $('#kpiTiempoPromedio').text((r ? formatDecimal(r.HorasPromedioResolucion) : 0) + ' h');
}

function renderEstatus(data) {
    var total = 0;
    if (data && data.length) {
        data.forEach(function (d) { total += (d.Cantidad || 0); });
    }

    if (!data || !data.length || total === 0) {
        $('#estatusChartWrapper').hide();
        $('#estatusVacio').show();
        return;
    }

    $('#estatusVacio').hide();
    $('#estatusChartWrapper').show();

    var labels = data.map(function (d) { return d.Nombre || ''; });
    var valores = data.map(function (d) { return d.Cantidad || 0; });
    var colores = data.map(function (d) { return d.Color || '#4e73df'; });

    if (typeof Chart !== 'undefined') {
        if (miEstatusChart) { miEstatusChart.destroy(); }
        var ctx = document.getElementById('estatusChart').getContext('2d');
        miEstatusChart = new Chart(ctx, {
            type: 'pie',
            data: {
                labels: labels,
                datasets: [{ data: valores, backgroundColor: colores }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                var label = context.label || '';
                                return label + ': ' + context.parsed;
                            }
                        }
                    }
                }
            }
        });
    }
}

function renderEvolucion(data) {
    var hayActividad = false;
    if (data && data.length) {
        data.forEach(function (d) {
            if ((d.Creados || 0) > 0 || (d.Resueltos || 0) > 0) {
                hayActividad = true;
            }
        });
    }

    if (!data || !data.length || !hayActividad) {
        $('#evolucionChartWrapper').hide();
        $('#evolucionVacio').show();
        return;
    }

    $('#evolucionVacio').hide();
    $('#evolucionChartWrapper').show();

    var labels = data.map(function (d) { return formatFechaCorta(d.Fecha); });
    var creados = data.map(function (d) { return d.Creados || 0; });
    var resueltos = data.map(function (d) { return d.Resueltos || 0; });

    if (typeof Chart !== 'undefined') {
        if (miEvolucionChart) { miEvolucionChart.destroy(); }
        var ctx = document.getElementById('evolucionChart').getContext('2d');
        miEvolucionChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Creados',
                        data: creados,
                        borderColor: '#4e73df',
                        backgroundColor: 'rgba(78,115,223,0.1)',
                        fill: true,
                        tension: 0.2
                    },
                    {
                        label: 'Resueltos',
                        data: resueltos,
                        borderColor: '#1cc88a',
                        backgroundColor: 'rgba(28,200,138,0.1)',
                        fill: true,
                        tension: 0.2
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: { beginAtZero: true }
                }
            }
        });
    }
}

function renderRankingAreas(data) {
    var tbody = $('#rankingAreasBody');
    tbody.empty();

    if (!data || !data.length) {
        $('#rankingAreasTabla').hide();
        $('#rankingAreasVacio').show();
        return;
    }

    $('#rankingAreasVacio').hide();
    $('#rankingAreasTabla').show();

    data.forEach(function (a) {
        tbody.append(
            '<tr>' +
            '<td>' + (a.AreaNombre || '') + '</td>' +
            '<td class="text-center">' + (a.Total || 0) + '</td>' +
            '<td class="text-center">' + formatDecimal(a.PromUrgencia) + '</td>' +
            '<td class="text-center">' + (a.Cerrados || 0) + '</td>' +
            '<td class="text-center">' + (a.Rechazados || 0) + '</td>' +
            '</tr>');
    });
}

function llenarTablaReasignaciones(selector, lista) {
    var tbody = $(selector);
    tbody.empty();

    lista.forEach(function (a) {
        var nombre = ((a.Nombre || '') + ' ' + (a.Apellido || '')).trim();
        tbody.append(
            '<tr>' +
            '<td>' + nombre + '</td>' +
            '<td>' + (a.NombreUsuario || '') + '</td>' +
            '<td class="text-center">' + (a.Cantidad || 0) + '</td>' +
            '</tr>');
    });
}

function renderRankingReasignaciones(data) {
    var reciben = data ? data.filter(function (d) { return d.Tipo === 'Reciben'; }) : [];
    var quitan = data ? data.filter(function (d) { return d.Tipo === 'Quitan'; }) : [];

    if (!reciben.length && !quitan.length) {
        $('#reasignacionesContenido').hide();
        $('#reasignacionesVacio').show();
        return;
    }

    $('#reasignacionesVacio').hide();
    $('#reasignacionesContenido').show();

    $('#bloqueReciben').toggle(reciben.length > 0);
    $('#bloqueQuitan').toggle(quitan.length > 0);

    llenarTablaReasignaciones('#tablaRecibenBody', reciben);
    llenarTablaReasignaciones('#tablaQuitanBody', quitan);
}

function cargarEstadisticas(inicio, fin) {
    var params = { fechaInicio: inicio, fechaFin: fin };

    // 1. Resumen (tarjetas KPI)
    $('#spinnerResumen').show();
    $.get('/Estadisticas/ObtenerResumen', params, function (res) {
        renderResumen(res);
    }, 'json')
        .fail(function () {
            renderResumen(null);
        })
        .always(function () {
            $('#spinnerResumen').hide();
        });

    // 2. Distribución por estatus (pie)
    $('#spinnerEstatus').show();
    $.get('/Estadisticas/ObtenerDistribucionEstatus', params, function (res) {
        renderEstatus(res && res.IsSuccess ? res.Response : null);
    }, 'json')
        .fail(function () {
            renderEstatus(null);
        })
        .always(function () {
            $('#spinnerEstatus').hide();
        });

    // 3. Evolución diaria (línea)
    $('#spinnerEvolucion').show();
    $.get('/Estadisticas/ObtenerEvolucionDiaria', params, function (res) {
        renderEvolucion(res && res.IsSuccess ? res.Response : null);
    }, 'json')
        .fail(function () {
            renderEvolucion(null);
        })
        .always(function () {
            $('#spinnerEvolucion').hide();
        });

    // 4. Ranking de áreas
    $('#spinnerAreas').show();
    $.get('/Estadisticas/ObtenerRankingAreas', params, function (res) {
        renderRankingAreas(res && res.IsSuccess ? res.Response : null);
    }, 'json')
        .fail(function () {
            renderRankingAreas(null);
        })
        .always(function () {
            $('#spinnerAreas').hide();
        });

    // 5. Ranking de reasignaciones
    $('#spinnerReasignaciones').show();
    $.get('/Estadisticas/ObtenerRankingReasignaciones', params, function (res) {
        renderRankingReasignaciones(res && res.IsSuccess ? res.Response : null);
    }, 'json')
        .fail(function () {
            renderRankingReasignaciones(null);
        })
        .always(function () {
            $('#spinnerReasignaciones').hide();
        });
}

$(document).ready(function () {
    $('#btnAplicar').on('click', function () {
        var inicio = $('#fechaInicio').val();
        var fin = $('#fechaFin').val();

        if (!inicio || !fin) {
            Swal.fire({
                icon: 'warning',
                title: 'Fechas incompletas',
                text: 'Seleccione la fecha de inicio y la fecha de fin.'
            });
            return;
        }

        if (inicio > fin) {
            Swal.fire({
                icon: 'error',
                title: 'Rango inválido',
                text: 'La fecha de inicio no puede ser mayor que la fecha de fin.'
            });
            return;
        }

        cargarEstadisticas(inicio, fin);
    });

    // Carga inicial con los defaults del servidor
    cargarEstadisticas(fechaInicioGlobal, fechaFinGlobal);
});
