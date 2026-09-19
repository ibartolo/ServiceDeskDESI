$(document).ready(function () {
    // Convertir RFC a mayúsculas
    $('#RFC').on('input', function () {
        $(this).val($(this).val().toUpperCase());
    });

    // Validaciones con jQuery Validate
    $("#frmEmpresa").validate({
        rules: {
            "NombreComercial": {
                required: true,
                maxlength: 250
            },
            "RazonSocial": {
                required: true,
                maxlength: 250
            },
            "RFC": {
                required: true,
                maxlength: 50,
                minlength: 12
            },
            "Responsable": {
                required: true,
                maxlength: 250
            },
            "Direccion": {
                required: true,
                maxlength: 500
            },
            "CorreoContacto": {
                required: true,
                email: true,
                maxlength: 250
            }
        },
        messages: {
            "NombreComercial": {
                required: "El campo 'Nombre Comercial' es requerido.",
                maxlength: "El nombre comercial no puede superar los 250 caracteres."
            },
            "RazonSocial": {
                required: "El campo 'Razón Social' es requerido.",
                maxlength: "La razón social no puede superar los 250 caracteres."
            },
            "RFC": {
                required: "El campo 'RFC' es requerido.",
                maxlength: "El RFC no puede superar los 50 caracteres.",
                minlength: "El RFC debe tener al menos 12 caracteres."
            },
            "Responsable": {
                required: "El campo 'Responsable' es requerido.",
                maxlength: "El responsable no puede superar los 250 caracteres."
            },
            "Direccion": {
                required: "El campo 'Dirección' es requerido.",
                maxlength: "La dirección no puede superar los 500 caracteres."
            },
            "CorreoContacto": {
                required: "El campo 'Correo de Contacto' es requerido.",
                email: "Ingrese un correo electrónico válido.",
                maxlength: "El correo no puede superar los 250 caracteres."
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

    // Botón Registrar
    $('#btnRegistrar').on('click', function () {
        if ($("#frmEmpresa").valid()) {
            Swal.fire({
                title: 'Registrando...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            var empresa = {
                NombreComercial: $('#NombreComercial').val().trim(),
                RazonSocial: $('#RazonSocial').val().trim(),
                RFC: $('#RFC').val().trim().toUpperCase(),
                Responsable: $('#Responsable').val().trim(),
                Direccion: $('#Direccion').val().trim(),
                Ciudad: $('#Ciudad').val().trim() || null,
                Estado: $('#Estado').val().trim() || null,
                CodigoPostal: $('#CodigoPostal').val().trim() || null,
                Telefono: $('#Telefono').val().trim() || null,
                CorreoContacto: $('#CorreoContacto').val().trim()
            };

            PostMVC('/Home/GuardarNuevaEmpresa', empresa, function (response) {
                Swal.close();

                if (response.IsSuccess) {
                    Swal.fire({
                        title: '¡Registro Exitoso!',
                        text: response.Message || 'Su empresa ha sido registrada correctamente.',
                        icon: 'success',
                        confirmButtonColor: '#4e73df',
                        confirmButtonText: 'Aceptar'
                    }).then(function () {
                        window.location.href = '/Home/Autentication';
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.Message || 'Error al registrar la empresa. Intente nuevamente.',
                        icon: 'error',
                        confirmButtonColor: '#4e73df',
                        confirmButtonText: 'Aceptar'
                    });
                }
            });
        }
    });

    // Botón Cancelar
    $('#btnCancelar').on('click', function () {
        Swal.fire({
            title: '¿Cancelar registro?',
            text: 'Los datos ingresados se perderán.',
            icon: 'question',
            showCancelButton: true,
            confirmButtonColor: '#e74a3b',
            cancelButtonColor: '#4e73df',
            confirmButtonText: 'Sí, cancelar',
            cancelButtonText: 'No, continuar'
        }).then((result) => {
            if (result.isConfirmed) {
                $('#frmEmpresa')[0].reset();
                Swal.fire('Formulario limpio', 'Puedes comenzar de nuevo.', 'info');
            }
        });
    });
});
