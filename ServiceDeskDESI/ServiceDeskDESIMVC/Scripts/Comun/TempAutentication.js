$(document).ready(function () {
    // =========================================
    // MOSTRAR/OCULTAR CONTRASEÑA
    // =========================================
    $('#togglePassword').click(function () {
        const passwordInput = $('#password');
        const icon = $(this).find('i');

        if (passwordInput.attr('type') === 'password') {
            passwordInput.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            passwordInput.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // =========================================
    // MANEJAR EL ENVÍO DEL FORMULARIO
    // =========================================
    $('#loginForm').on('submit', function (e) {
        e.preventDefault();

        const username = $('#username').val();
        const password = $('#password').val();

        if (username && password) {
            // Mostrar loading
            Swal.fire({
                title: 'Iniciando sesión...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Llamada AJAX al Login
            $.ajax({
                url: '/Home/LogIn',
                type: 'POST',
                data: {
                    user: username,
                    pass: password
                },
                success: function (response) {
                    var result = JSON.parse(response);

                    if (result.IsSuccess) {
                        Swal.fire({
                            title: '¡Bienvenido!',
                            text: result.Message,
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false,
                            background: 'white',
                            iconColor: '#4e73df'
                        }).then(() => {
                            window.location.href = '/Home/Index';
                        });
                    } else {
                        Swal.close();
                        $('#alertMessage').css('display', 'flex');
                        $('#alertText').text(result.Message);

                        setTimeout(() => {
                            $('#alertMessage').css('display', 'none');
                        }, 3000);
                    }
                },
                error: function () {
                    Swal.close();
                    $('#alertMessage').css('display', 'flex');
                    $('#alertText').text('Error de conexión con el servidor');

                    setTimeout(() => {
                        $('#alertMessage').css('display', 'none');
                    }, 3000);
                }
            });
        } else {
            $('#alertMessage').css('display', 'flex');
            $('#alertText').text('Por favor, ingrese usuario y contraseña');

            setTimeout(() => {
                $('#alertMessage').css('display', 'none');
            }, 3000);
        }
    });

    // =========================================
    // MANEJAR "OLVIDÉ MI CONTRASEÑA"
    // =========================================
    $('#forgotPassword').on('click', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Recuperar Contraseña',
            html: `
                <p>Ingresa tu correo para recibir instrucciones:</p>
                <input type="text" id="resetEmail" class="form-control mt-3" placeholder="juan.perez@gmail.com">
            `,
            icon: 'info',
            confirmButtonText: 'Enviar',
            cancelButtonText: 'Cancelar',
            showCancelButton: true,
            confirmButtonColor: '#4e73df',
            cancelButtonColor: '#e74a3b',
            preConfirm: () => {
                const nombreUsuario = document.getElementById('resetEmail').value;
                if (!nombreUsuario) {
                    Swal.showValidationMessage('Por favor, ingrese su nombre de usuario');
                }
                return nombreUsuario;
            }
        }).then((result) => {
            if (result.isConfirmed) {
                const nombreUsuario = result.value;

                Swal.fire({
                    title: 'Enviando...',
                    text: 'Por favor espere',
                    allowOutsideClick: false,
                    didOpen: () => {
                        Swal.showLoading();
                    }
                });

                var usuario = {
                    Correo: nombreUsuario
                };

                PostMVC('/Home/ValidarRecetearContrasenia', usuario, function (response) {
                    Swal.close();

                    if (response.IsSuccess) {
                        Swal.fire({
                            title: 'Correo Enviado',
                            text: response.Message,
                            icon: 'success',
                            confirmButtonColor: '#4e73df'
                        });
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: response.Message,
                            icon: 'error',
                            confirmButtonColor: '#4e73df'
                        });
                    }
                });
            }
        });
    });

    // =========================================
    // MANEJAR SOLICITUD DE ACCESO
    // =========================================
    $('#registerLink').on('click', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Solicitar Acceso',
            html: `
                <form id="registerForm">
                    <div class="mb-3">
                        <input type="text" class="form-control" placeholder="Nombre completo" required>
                    </div>
                    <div class="mb-3">
                        <input type="email" class="form-control" placeholder="Correo electrónico" required>
                    </div>
                    <div class="mb-3">
                        <input type="text" class="form-control" placeholder="Empresa/Departamento" required>
                    </div>
                    <div class="mb-3">
                        <select class="form-select" required>
                            <option value="">Seleccione motivo</option>
                            <option value="nuevo">Nuevo empleado</option>
                            <option value="proyecto">Nuevo proyecto</option>
                            <option value="otro">Otro</option>
                        </select>
                    </div>
                </form>
            `,
            icon: 'question',
            confirmButtonText: 'Solicitar',
            cancelButtonText: 'Cancelar',
            showCancelButton: true,
            confirmButtonColor: '#4e73df',
            cancelButtonColor: '#e74a3b'
        }).then((result) => {
            if (result.isConfirmed) {
                Swal.fire({
                    title: 'Solicitud Enviada',
                    text: 'Su solicitud ha sido enviada. Recibirá una respuesta en su correo.',
                    icon: 'success',
                    confirmButtonColor: '#4e73df'
                });
            }
        });
    });

    // =========================================
    // ANIMACIÓN DE ENTRADA
    // =========================================
    $('.login-container').css('opacity', '0');
    setTimeout(() => {
        $('.login-container').css('opacity', '1');
    }, 100);
});