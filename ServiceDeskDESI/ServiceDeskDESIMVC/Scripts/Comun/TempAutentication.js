document.addEventListener('DOMContentLoaded', function () {
    // =========================================
    // MOSTRAR/OCULTAR CONTRASEÑA
    // =========================================
    const togglePassword = document.getElementById('togglePassword');
    const passwordInput = document.getElementById('password');

    togglePassword.addEventListener('click', function () {
        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
        passwordInput.setAttribute('type', type);

        // Cambiar el icono
        const icon = this.querySelector('i');
        icon.classList.toggle('fa-eye');
        icon.classList.toggle('fa-eye-slash');
    });

    // =========================================
    // MANEJAR EL ENVÍO DEL FORMULARIO
    // =========================================
    const loginForm = document.getElementById('loginForm');
    const alertMessage = document.getElementById('alertMessage');
    const alertText = document.getElementById('alertText');

    loginForm.addEventListener('submit', function (e) {
        e.preventDefault();

        const username = document.getElementById('username').value;
        const password = document.getElementById('password').value;
        const remember = document.getElementById('remember').checked;

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
                        alertMessage.style.display = 'flex';
                        alertText.textContent = result.Message;

                        setTimeout(() => {
                            alertMessage.style.display = 'none';
                        }, 3000);
                    }
                },
                error: function (xhr, status, error) {
                    Swal.close();
                    alertMessage.style.display = 'flex';
                    alertText.textContent = 'Error de conexión con el servidor';

                    setTimeout(() => {
                        alertMessage.style.display = 'none';
                    }, 3000);
                }
            });
        } else {
            alertMessage.style.display = 'flex';
            alertText.textContent = 'Por favor, ingrese usuario y contraseña';

            setTimeout(() => {
                alertMessage.style.display = 'none';
            }, 3000);
        }
    });

    // =========================================
    // MANEJAR "OLVIDÉ MI CONTRASEÑA"
    // =========================================
    const forgotPassword = document.getElementById('forgotPassword');

    forgotPassword.addEventListener('click', function (e) {
        e.preventDefault();

        Swal.fire({
            title: 'Recuperar Contraseña',
            html: `
            <p>Ingresa tu nombre de usuario para recibir instrucciones:</p>
            <input type="text" id="resetEmail" class="form-control mt-3" placeholder="juan.perez">
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
                    NombreUsuario: nombreUsuario
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
    const registerLink = document.getElementById('registerLink');

    registerLink.addEventListener('click', function (e) {
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
    // Aplicar una pequeña animación al cargar
    document.querySelector('.login-container').style.opacity = '0';
    setTimeout(() => {
        document.querySelector('.login-container').style.opacity = '1';
    }, 100);
});