$(function () {
    $("#frmCompania").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 250
            },
            "Acronimo": {
                required: true,
                maxlength: 50
            },
            "RFC": {
                required: true,
                maxlength: 50
            },
            "Direccion": {
                required: true,
                maxlength: 250
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 250 caracteres."
            },
            "Acronimo": {
                required: "El campo 'Acrónimo' es requerido.",
                maxlength: "El acrónimo no puede superar los 50 caracteres."
            },
            "RFC": {
                required: "El campo 'RFC' es requerido.",
                maxlength: "El RFC no puede superar los 50 caracteres."
            },
            "Direccion": {
                required: "El campo 'Dirección' es requerido.",
                maxlength: "La dirección no puede superar los 250 caracteres."
            }
        },
        errorElement: "span",
        errorClass: "text-danger",
        highlight: function (element) {
            $(element).addClass("is-invalid").removeClass("is-valid");
        },
        unhighlight: function (element) {
            $(element).removeClass("is-invalid").addClass("is-valid");
        },
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        }
    });
});

$(document).ready(function () {
    var tabla = new DataTable('#tblCompania', {
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Acronimo', title: 'Acrónimo' },
            { data: 'Direccion', title: 'Dirección' },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarCompania(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarCompania(event, ' + data + ')"><i class="fas fa-trash-alt"></i></button>';
                    }

                    html += '</div>';
                    return html;
                }
            },
            { data: 'Estatus', title: 'Estatus', visible: false }
        ],
        language: {
            url: "/Content/datatables/i18n/es-ES.json"
        }
    });
    ConsultarTodasCompanias();
});

function GuardarActualizarCompania() {
    if ($("#frmCompania").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var compania = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Acronimo: $("#Acronimo").val(),
            RFC: $("#RFC").val(),
            Direccion: $("#Direccion").val(),
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: new Date().toISOString(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };

        PostMVC('/Catalogs/GuardarOActualizarCompanias', compania, function (response) {
            Swal.close();

            if (response.IsSuccess) {
                Swal.fire({
                    title: '¡Éxito!',
                    text: response.Message,
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false,
                    background: 'white',
                    iconColor: '#4e73df'
                }).then(() => {
                    window.location.href = '/Catalogs/Company';
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
}

function ConsultarTodasCompanias() {
    GetMVC("/Catalogs/ConsultarTodasLasCompanias", function (r) {
        var result = typeof r == 'string' ? JSON.parse(r) : r;

        if (result.IsSuccess) {
            MapingPropertiesDataTable("tblCompania", result.Response);
        } else {
            Swal.fire({
                title: 'Error',
                text: result.Message,
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function EditarCompania(id) {
    window.location.href = '/Catalogs/Company/' + id;
}

function EliminarCompania(event, id) {
    event.preventDefault();
    event.stopPropagation();

    Swal.fire({
        title: '¿Está seguro?',
        text: 'Esta acción eliminará lógicamente el registro',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#e74a3b',
        cancelButtonColor: '#858796'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Eliminando...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            var compania = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarCompanias', compania, function (response) {
                Swal.close();

                if (response.IsSuccess) {
                    Swal.fire({
                        title: '¡Eliminado!',
                        text: response.Message,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false,
                        background: 'white',
                        iconColor: '#4e73df'
                    }).then(() => {
                        ConsultarTodasCompanias();
                        $("#frmCompania")[0].reset();
                        $("#Id").val(0);
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
    });

    return false;
}
