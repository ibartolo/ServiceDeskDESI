$(function () {
    // Cargar el horario actual
    CargarHorario();

    // Deshabilitar controles si no puede editar
    if (!puedeEditar) {
        $('#tablaHorario input, #tablaHorario select').prop('disabled', true);
    }

    // Cambio de checkbox "labora" -> habilitar/deshabilitar horas
    $('#tablaHorario').on('change', '.chkLabora', function () {
        var $tr = $(this).closest('tr');
        AplicarEstadoFila($tr, $(this).is(':checked'));
    });

    // Guardar horario
    $('#btnGuardarHorario').on('click', function () {
        GuardarHorario();
    });

    // Vista previa del logotipo antes de guardar
    $('#fileLogo').on('change', function (e) {
        var file = e.target.files[0];
        if (file) {
            var reader = new FileReader();
            reader.onload = function (ev) {
                $('#imgLogoPreview').attr('src', ev.target.result).show();
            };
            reader.readAsDataURL(file);
        }
    });

    // Subir logotipo
    $('#btnSubirLogo').on('click', function () {
        SubirLogo();
    });

    // Quitar logotipo
    $('#btnQuitarLogo').on('click', function () {
        QuitarLogo();
    });
});

function AplicarEstadoFila($tr, labora) {
    $tr.find('.hora-group select').prop('disabled', !labora);
}

function pad2(n) {
    return (n < 10 ? '0' : '') + n;
}

// Convierte "HH:mm" (24h) al valor de los selects (12h + AM/PM).
function setHora($group, hhmm) {
    if (!hhmm) { return; }
    var parts = hhmm.split(':');
    var h = parseInt(parts[0], 10);
    var m = parts[1] || '00';
    var ampm = h >= 12 ? 'PM' : 'AM';
    var h12 = h % 12;
    if (h12 === 0) { h12 = 12; }
    $group.find('.sel-hora').val(h12);
    $group.find('.sel-minuto').val(m);
    $group.find('.sel-ampm').val(ampm);
}

// Lee los selects (12h + AM/PM) y devuelve "HH:mm" (24h).
function getHora($group) {
    var h12 = parseInt($group.find('.sel-hora').val(), 10);
    var m = $group.find('.sel-minuto').val();
    var ampm = $group.find('.sel-ampm').val();
    var h24 = h12;
    if (ampm === 'AM') {
        if (h12 === 12) { h24 = 0; }
    } else {
        if (h12 !== 12) { h24 = h12 + 12; }
    }
    return pad2(h24) + ':' + m;
}

function CargarHorario() {
    GetMVC('/ConfiguracionEmpresa/ObtenerHorario', function (r) {
        if (r && r.IsSuccess && r.Response) {
            $.each(r.Response, function (i, item) {
                var $tr = $('#tablaHorario tbody tr[data-dia="' + item.DiaSemana + '"]');
                if ($tr.length) {
                    $tr.find('.chkLabora').prop('checked', item.EsLaboral);
                    if (item.EsLaboral) {
                        setHora($tr.find('.hora-inicio'), item.HoraInicio);
                        setHora($tr.find('.hora-fin'), item.HoraFin);
                    } else {
                        setHora($tr.find('.hora-inicio'), '09:00');
                        setHora($tr.find('.hora-fin'), '17:00');
                    }
                    if (!puedeEditar) {
                        $tr.find('.chkLabora, .hora-group select').prop('disabled', true);
                    } else {
                        AplicarEstadoFila($tr, item.EsLaboral);
                    }
                }
            });
        }
    });
}

function ObtenerHorarioGuardar() {
    var lista = [];
    $('#tablaHorario tbody tr').each(function () {
        var $tr = $(this);
        var dia = parseInt($tr.data('dia'));
        var labora = $tr.find('.chkLabora').is(':checked');
        var inicio = labora ? getHora($tr.find('.hora-inicio')) : '';
        var fin = labora ? getHora($tr.find('.hora-fin')) : '';

        lista.push({ DiaSemana: dia, EsLaboral: labora, HoraInicio: inicio, HoraFin: fin });
    });
    return lista;
}

function GuardarHorario() {
    var $btn = $('#btnGuardarHorario');
    $btn.prop('disabled', true);
    $('#spinnerHorario').removeClass('d-none');
    $('#txtGuardarHorario').text('Guardando…');

    $.ajax({
        url: '/ConfiguracionEmpresa/GuardarHorario',
        type: 'POST',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: JSON.stringify({ horario: ObtenerHorarioGuardar() }),
        success: function (response) {
            $btn.prop('disabled', false);
            $('#spinnerHorario').addClass('d-none');
            $('#txtGuardarHorario').text('Guardar');

            if (response && response.IsSuccess) {
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
                    text: response && response.Message ? response.Message : 'Ocurrió un error al guardar.',
                    icon: 'error',
                    confirmButtonText: 'Aceptar',
                    background: 'white',
                    confirmButtonColor: '#4e73df'
                });
            }
        },
        error: function () {
            $btn.prop('disabled', false);
            $('#spinnerHorario').addClass('d-none');
            $('#txtGuardarHorario').text('Guardar');

            Swal.fire({
                title: 'Error',
                text: 'Ocurrió un error al guardar.',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function SubirLogo() {
    var file = document.getElementById('fileLogo').files[0];
    if (!file) {
        Swal.fire({
            title: 'Aviso',
            text: 'Seleccione un archivo de logotipo.',
            icon: 'info',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    var $btn = $('#btnSubirLogo');
    $btn.prop('disabled', true);
    $('#spinnerLogo').removeClass('d-none');
    $('#txtSubirLogo').text('Guardando…');

    var parameters = [];
    parameters.push({ Name: "file", Value: file });

    PostFileMVC('/ConfiguracionEmpresa/SubirLogo', parameters, function (response) {
        $btn.prop('disabled', false);
        $('#spinnerLogo').addClass('d-none');
        $('#txtSubirLogo').text('Subir');

        if (response && response.IsSuccess) {
            Swal.fire({
                title: '¡Éxito!',
                text: response.Message,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false,
                background: 'white',
                iconColor: '#4e73df'
            }).then(function () {
                // Refrescar el sidebar para mostrar el nuevo logotipo.
                // La vista previa ya muestra la imagen seleccionada (FileReader).
                $("#sidebar").empty().load("/Home/MenusUser");
                // Ahora existe un logotipo: habilitar la opción "Quitar logo".
                $('#btnQuitarLogo').show();
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: response && response.Message ? response.Message : 'Ocurrió un error al subir el logotipo.',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function QuitarLogo() {
    Swal.fire({
        title: '¿Está seguro?',
        text: 'Se quitará el logotipo actual y se restaurará el logo de DESi.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, quitar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#e74a3b',
        cancelButtonColor: '#858796'
    }).then(function (result) {
        if (result.isConfirmed) {
            var $btn = $('#btnQuitarLogo');
            $btn.prop('disabled', true);
            $('#spinnerQuitarLogo').removeClass('d-none');
            $('#txtQuitarLogo').text('Quitando…');

            PostMVC('/ConfiguracionEmpresa/QuitarLogo', {}, function (response) {
                $btn.prop('disabled', false);
                $('#spinnerQuitarLogo').addClass('d-none');
                $('#txtQuitarLogo').text('Quitar logo');

                if (response && response.IsSuccess) {
                    // Ocultar la vista previa y refrescar el sidebar (fallback DESi).
                    $('#imgLogoPreview').attr('src', '').hide();
                    $('#btnQuitarLogo').hide();
                    $("#sidebar").empty().load("/Home/MenusUser");

                    Swal.fire({
                        title: '¡Éxito!',
                        text: response.Message || 'Logotipo quitado correctamente.',
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false,
                        background: 'white',
                        iconColor: '#4e73df'
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response && response.Message ? response.Message : 'Ocurrió un error al quitar el logotipo.',
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
