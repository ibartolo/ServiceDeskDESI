$(function () {
    $("#frmTipoActivo").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 250
            },
            "Descripcion": {
                required: true,
                maxlength: 250
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 250 caracteres."
            },
            "Descripcion": {
                required: "El campo 'Descripción' es requerido.",
                maxlength: "La descripción no puede superar los 250 caracteres."
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
    var tabla = new DataTable('#tblTipoActivo', {
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarTipoActivo(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarTipoActivo(event, ' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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
    ConsultarTodosTipoActivos();
});

function GuardarActualizarTipoActivo() {
    if ($("#frmTipoActivo").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var tipoactivo = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Descripcion: $("#Descripcion").val(),
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: new Date().toISOString(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };
        PostMVC('/Catalogs/GuardarOActualizarTipoActivo', tipoactivo, function (response) {
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
                    window.location.href = '/Catalogs/TypeActive';
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

function ConsultarTodosTipoActivos() {
    GetMVC("/Catalogs/ConsultarTodosLosTipoActivos", function (r) {
        var result = typeof r == 'string' ? JSON.parse(r) : r;

        if (result.IsSuccess) {
            MapingPropertiesDataTable("tblTipoActivo", result.Response);
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

function EditarTipoActivo(id) {
    window.location.href = '/Catalogs/TypeActive/' + id;
}

function EliminarTipoActivo(event, id) {
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
            var tipoactivo = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };
            PostMVC('/Catalogs/EliminarTipoActivo', tipoactivo, function (response) {
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
                        ConsultarTodosTipoActivos();
                        $("#frmTipoActivo")[0].reset();
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
