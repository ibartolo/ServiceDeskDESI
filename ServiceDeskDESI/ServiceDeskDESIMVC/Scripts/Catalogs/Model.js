var tablaModelo;

$(document).ready(function () {
    InicializarDataTable();
    ConsultarTodosLosModelos();

    // Remover estilos de error cuando cambia el dropdown
    $("#ddlMarca").change(function () {
        $(this).removeClass("is-invalid-dropdown");
    });
});

$(function () {
    $("#frmModelo").validate({
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

function InicializarDataTable() {
    tablaModelo = $('#tblModelo').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            { data: 'MarcaNombre', title: 'Marca' },
            {
                data: 'FechaCreacion',
                title: 'Fecha Creación',
                render: function (data) {
                    if (data) {
                        var fecha = new Date(data);
                        return fecha.toLocaleDateString();
                    }
                    return '---';
                }
            },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarModelo(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarModelo(' + data + ')"><i class="fas fa-trash-alt"></i></button>';
                    }

                    html += '</div>';
                    return html;
                }
            }
        ],
        language: {
            url: "/Content/datatables/i18n/es-ES.json"
        }
    });
}

function ConsultarTodosLosModelos() {
    GetMVC("/Catalogs/ConsultarTodosLosModelos", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            MapingPropertiesDataTable("tblModelo", result.Response);
        } else {
            MapingPropertiesDataTable("tblModelo", []);
            if (!result.IsSuccess) {
                Swal.fire({
                    title: 'Error',
                    text: result.Message,
                    icon: 'error',
                    confirmButtonText: 'Aceptar',
                    background: 'white',
                    confirmButtonColor: '#4e73df'
                });
            }
        }
    });
}

function GuardarActualizarModelo() {
    if ($("#frmModelo").valid()) {
        if (!$("#ddlMarca").val()) {
            Swal.fire({
                title: 'Error',
                text: 'Debe seleccionar una marca',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
            return;
        }

        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var modelo = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Descripcion: $("#Descripcion").val(),
            MarcaId: parseInt($("#ddlMarca").val()),
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: $("#FechaCreacion").val(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };

        PostMVC('/Catalogs/GuardarOActualizarModelos', modelo, function (response) {
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
                    window.location.href = '/Catalogs/Model';
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

function EditarModelo(id) {
    window.location.href = '/Catalogs/Model/' + id;
}

function EliminarModelo(id) {
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

            var modelo = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarModelos', modelo, function (response) {
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
                        ConsultarTodosLosModelos();
                        $("#frmModelo")[0].reset();
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
}
