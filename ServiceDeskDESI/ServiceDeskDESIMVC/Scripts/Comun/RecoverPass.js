// Validar token al cargar la página
$(document).ready(function () {
    GetMVC('/Home/ValidarToken/' + token, function (response) {
        if (response.IsSuccess && response.Response) {
            $('#spanNombreUsuario').text(response.Response.NombreUsuario);
            $('#spanNombre').text(response.Response.Nombre);
            $('#spanApellido').text(response.Response.Apellido);
            $('#spanCorreo').text(response.Response.Correo);
            $('#infoUsuario').show();
        } else {
            Swal.fire({
                title: 'Enlace inválido',
                text: response.Message || 'El enlace de recuperación no es válido o ha expirado.',
                icon: 'error',
                confirmButtonText: 'Ir al inicio'
            }).then(() => {
                window.location.href = '/Home/Autentication';
            });
        }
    });

    // Validar contraseña en tiempo real
    $('#nuevaContrasena').on('keyup', function () {
        validarContrasena($(this).val());
    });

    // Mostrar/ocultar contraseña
    $('#togglePassword').click(function () {
        const input = $('#nuevaContrasena');
        const icon = $(this).find('i');
        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });
});

// Validaciones de contraseña
function validarContrasena(contrasena) {
    let isValid = true;

    const tieneLongitud = contrasena.length >= 6;
    const tieneMayuscula = /[A-Z]/.test(contrasena);
    const tieneMinuscula = /[a-z]/.test(contrasena);
    const tieneNumero = /[0-9]/.test(contrasena);
    const tieneEspecial = /[!@#$%^&*]/.test(contrasena);

    // Actualizar UI
    actualizarRequisito('#reqLongitud', tieneLongitud);
    actualizarRequisito('#reqMayuscula', tieneMayuscula);
    actualizarRequisito('#reqMinuscula', tieneMinuscula);
    actualizarRequisito('#reqNumero', tieneNumero);
    actualizarRequisito('#reqEspecial', tieneEspecial);

    return tieneLongitud && tieneMayuscula && tieneMinuscula && tieneNumero && tieneEspecial;
}

function actualizarRequisito(elemento, isValid) {
    const $el = $(elemento);
    if (isValid) {
        $el.removeClass('invalid').addClass('valid');
        $el.html('<i class="fas fa-check-circle"></i> ' + $el.text().substring(1));
    } else {
        $el.removeClass('valid').addClass('invalid');
        $el.html('<i class="fas fa-circle"></i> ' + $el.text().substring(1));
    }
}

// Enviar formulario
function GuardarNuevaContrasena() {
    const nuevaContrasena = $('#nuevaContrasena').val();
    const confirmarContrasena = $('#confirmarContrasena').val();

    if (!validarContrasena(nuevaContrasena)) {
        Swal.fire({
            title: 'Contraseña inválida',
            text: 'La contraseña no cumple con los requisitos de seguridad.',
            icon: 'warning',
            confirmButtonText: 'Entendido'
        });
        return false;
    }

    if (nuevaContrasena !== confirmarContrasena) {
        Swal.fire({
            title: 'Las contraseñas no coinciden',
            text: 'Por favor verifica que ambas contraseñas sean iguales.',
            icon: 'warning',
            confirmButtonText: 'Entendido'
        });
        return false;
    }

    Swal.fire({
        title: 'Guardando...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    const data = {
        Token: token,
        NuevaContrasena: nuevaContrasena
    };

    PostMVC('/Home/RestablecerContrasenia', data, function (response) {
        Swal.close();

        if (response.IsSuccess) {
            Swal.fire({
                title: '¡Contraseña actualizada!',
                text: 'Tu contraseña ha sido actualizada correctamente. Ahora puedes iniciar sesión.',
                icon: 'success',
                confirmButtonText: 'Ir al inicio'
            }).then(() => {
                window.location.href = '/Home/Autentication';
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: response.Message,
                icon: 'error',
                confirmButtonText: 'Entendido'
            });
        }
    });
}