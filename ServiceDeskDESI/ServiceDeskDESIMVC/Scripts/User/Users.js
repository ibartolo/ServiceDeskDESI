var tablaUsuario;

$(document).ready(function () {
    ConsultarUsuarios();

    tablaUsuario = $('#tblUsuario').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            {
                data: 'Estatus', title: 'Estado', render: function (data) {
                    return data ? '<svg width="12" height="12"><circle cx="6" cy="6" r="6" fill="#28a745" /></svg>' : '<svg width="12" height="12"><circle cx="6" cy="6" r="6" fill="#dc3545" /></svg>';
                }
            },
            { data: 'NombreUsuario', title: 'Usuario' },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Correo', title: 'Correo' },
            { data: 'SucursalNombre', title: 'Sucursal', defaultContent: '---' },
            { data: 'AreaNombre', title: 'Área', defaultContent: '---' },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarUsuario(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarUsuario(' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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
});

$(function () {
    $("#frmUsuario").validate({
        rules: {
            "NombreUsuario": {
                required: true,
                maxlength: 25
            },
            "Correo": {
                required: true,
                email: true,
                maxlength: 250
            },
            "Nombre": {
                required: true,
                maxlength: 150
            },
            "Apellido": {
                required: true,
                maxlength: 250
            },
            "Celular": {
                required: true,
                maxlength: 50
            },
            "RFC": {
                maxlength: 50
            }
        },
        messages: {
            "NombreUsuario": {
                required: "El campo 'Nombre de usuario' es requerido.",
                maxlength: "El nombre de usuario no puede superar los 25 caracteres."
            },
            "Correo": {
                required: "El campo 'Correo' es requerido.",
                email: "Ingrese un correo electrónico válido.",
                maxlength: "El correo no puede superar los 250 caracteres."
            },
            "Nombre": {
                required: "El campo 'Nombre(s)' es requerido.",
                maxlength: "El nombre no puede superar los 150 caracteres."
            },
            "Apellido": {
                required: "El campo 'Apellidos' es requerido.",
                maxlength: "Los apellidos no pueden superar los 250 caracteres."
            },
            "Celular": {
                required: "El campo 'Teléfono celular' es requerido.",
                maxlength: "El teléfono no puede superar los 50 caracteres."
            },
            "RFC": {
                maxlength: "El RFC no puede superar los 50 caracteres."
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

function SugerirUsername() {
    var nombre = $("#txtNombre").val().trim();
    var apellido = $("#txtApellido").val().trim();

    if (nombre && apellido) {
        // Tomar inicial del apellido y convertir a minúscula
        var inicialApellido = apellido.charAt(0).toLowerCase();
        // Tomar el nombre completo en minúscula y sin espacios
        var nombreLower = nombre.toLowerCase().replace(/\s/g, '');
        // Generar username: inicial.apellido + . + nombre
        var usernameSugerido = inicialApellido + "." + nombreLower;

        // Eliminar caracteres especiales
        usernameSugerido = usernameSugerido.replace(/[^a-zA-Z0-9.]/g, '');

        // Si el campo de username está vacío, asignar la sugerencia
        if ($("#txtNombreUsuario").val().trim() === "") {
            $("#txtNombreUsuario").val(usernameSugerido);
        }
    }
}

function ValidarUsernameExistente() {
    var valor = $("#txtNombreUsuario").val().trim();

    // No validar si el campo está vacío
    if (valor === "") {
        $("#txtNombreUsuario").removeClass("is-invalid").removeClass("is-valid");
        return;
    }

    // No validar cuando se está editando un usuario existente (Id > 0)
    if (parseInt($("#Id").val() || 0, 10) > 0) {
        return;
    }

    GetMVC('/User/ExisteNombreUsuario?nombreUsuario=' + encodeURIComponent(valor), function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result && result.IsSuccess && result.Response === true) {
            $("#txtNombreUsuario").addClass("is-invalid").removeClass("is-valid");
            Swal.fire({
                title: 'Nombre de usuario no disponible',
                text: "El nombre de usuario '" + valor + "' ya existe. Elige otro.",
                icon: 'warning',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        } else if (result && result.IsSuccess) {
            $("#txtNombreUsuario").removeClass("is-invalid").addClass("is-valid");
        }
    });
}

function ConsultarUsuarios() {
    GetMVC("/User/ConsultarTodosLosUsuarios", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            MapingPropertiesDataTable("tblUsuario", result.Response);
        } else {
            MapingPropertiesDataTable("tblUsuario", []);
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

function GuardarActualizarUsuario() {
if ($("#frmUsuario").valid()) {

    // Validar que se haya seleccionado una sucursal
    if (!$("#ddlSucursal").val()) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar una sucursal',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    // Validar que se haya seleccionado un área
    if (!$("#ddlArea").val()) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar un área',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    // Validar que se haya seleccionado un rol
    if (!$("#ddlRol").val()) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar un rol',
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

    var usuario = {
        Id: $("#Id").val() || 0,
        NombreUsuario: $("#txtNombreUsuario").val(),
        Contrasena: $("#Contrasena").val(),
        Correo: $("#Correo").val(),
        Nombre: $("#txtNombre").val(),
        Apellido: $("#txtApellido").val(),
        Celular: $("#Celular").val(),
        RFC: $("#RFC").val(),
        SucursalId: parseInt($("#ddlSucursal").val()),
        AreaId: parseInt($("#ddlArea").val()),
        RolId: parseInt($("#ddlRol").val()),
        Firma: $("#Firma").val() || null,
        CreadoPor: $("#CreadoPor").val(),
        FechaCreacion: $("#FechaCreacion").val(),
        ModificadoPor: $("#ModificadoPor").val() || null,
        FechaModificacion: null,
        Estatus: true,
        EmpresaId: empresaId
    };

    PostMVC('/User/GuardarOActualizarUsuarioAdmin', usuario, function (response) {
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
                window.location.href = '/User/Users';
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

function EditarUsuario(id) {
    window.location.href = '/User/Users/' + id;
}

function EliminarUsuario(id) {
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

            var usuario = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/User/EliminarUsuarioAdmin', usuario, function (response) {
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
                        ConsultarUsuarios();
                        $("#frmUsuario")[0].reset();
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
