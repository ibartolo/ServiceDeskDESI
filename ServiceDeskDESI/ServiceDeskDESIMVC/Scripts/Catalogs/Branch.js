var tablaSucursal;

$(document).ready(function () {
    InicializarDataTable();
    ConsultarTodasSucursales();
});

$(function () {
    $("#frmSucursal").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 250
            },
            "Descripcion": {
                required: true,
                maxlength: 500
            },
            "Calle": {
                required: true,
                maxlength: 100
            },
            "Ciudad": {
                required: true,
                maxlength: 100
            },
            "Colonia": {
                required: true,
                maxlength: 100
            },
            "CodigoPostal": {
                required: true,
                maxlength: 10
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 250 caracteres."
            },
            "Descripcion": {
                required: "El campo 'Descripción' es requerido.",
                maxlength: "La descripción no puede superar los 500 caracteres."
            },
            "Calle": {
                required: "El campo 'Calle' es requerido.",
                maxlength: "La calle no puede superar los 100 caracteres."
            },
            "Ciudad": {
                required: "El campo 'Ciudad' es requerido.",
                maxlength: "La ciudad no puede superar los 100 caracteres."
            },
            "Colonia": {
                required: "El campo 'Colonia' es requerido.",
                maxlength: "La colonia no puede superar los 100 caracteres."
            },
            "CodigoPostal": {
                required: "El campo 'Código Postal' es requerido.",
                maxlength: "El código postal no puede superar los 10 caracteres."
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
    tablaSucursal = $('#tblSucursal').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            { data: 'Calle', title: 'Calle' },
            { data: 'Ciudad', title: 'Ciudad' },
            { data: 'Colonia', title: 'Colonia' },
            { data: 'CodigoPostal', title: 'Código Postal' },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarSucursal(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarSucursal(' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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

function ConsultarTodasSucursales() {
    GetMVC("/Catalogs/ConsultarTodasLasSucursales", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            MapingPropertiesDataTable("tblSucursal", result.Response);
        } else {
            MapingPropertiesDataTable("tblSucursal", []);
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

function GuardarActualizarSucursal() {
    if ($("#frmSucursal").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var sucursal = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Descripcion: $("#Descripcion").val(),
            Calle: $("#Calle").val(),
            Ciudad: $("#Ciudad").val(),
            Colonia: $("#Colonia").val(),
            CodigoPostal: $("#CodigoPostal").val(),
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: $("#FechaCreacion").val(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };

        PostMVC('/Catalogs/GuardarActualizarSucursales', sucursal, function (response) {
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
                    window.location.href = '/Catalogs/Branch';
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

function EditarSucursal(id) {
    window.location.href = '/Catalogs/Branch/' + id;
}

function EliminarSucursal(id) {
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

            var sucursal = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarSucurales', sucursal, function (response) {
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
                        ConsultarTodasSucursales();
                        $("#frmSucursal")[0].reset();
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
