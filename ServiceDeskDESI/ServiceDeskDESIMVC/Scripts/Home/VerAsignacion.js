function abrirModalLogin() {
    $("#loginError").addClass("d-none");
    $("#loginUser").val("");
    $("#loginPass").val("");
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalLoginAsignacion')).show();
}

function iniciarSesion() {
    var user = $("#loginUser").val();
    var pass = $("#loginPass").val();

    if (!user || !pass) {
        $("#loginError").removeClass("d-none");
        return;
    }

    PostMVC('/Home/LogIn', { user: user, pass: pass }, function (resp) {
        var res = typeof resp === 'string' ? JSON.parse(resp) : resp;
        if (res && res.IsSuccess) {
            // Sesión creada (FormsAuth + TokenCookie). Ahora ejecutar la acción.
            if (esDesvinculacion) {
                PostMVC('/Home/DesvincularAsignacion', { token: tokenAsignacion }, function (r2) {
                    var acc = typeof r2 === 'string' ? JSON.parse(r2) : r2;
                    if (acc && acc.IsSuccess) {
                        Swal.fire({
                            title: '¡Desvinculado!',
                            text: acc.Message,
                            icon: 'success',
                            timer: 2000,
                            showConfirmButton: false,
                            background: 'white',
                            iconColor: '#4e73df'
                        }).then(function () {
                            window.location.href = '/Home/Autentication';
                        });
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: (acc && acc.Message) || 'No se pudo desvincular el activo.',
                            icon: 'error',
                            confirmButtonText: 'Aceptar',
                            background: 'white',
                            confirmButtonColor: '#4e73df'
                        });
                    }
                });
            } else {
                PostMVC('/Home/AceptarAsignacion', { token: tokenAsignacion }, function (r2) {
                    var acc = typeof r2 === 'string' ? JSON.parse(r2) : r2;
                    if (acc && acc.IsSuccess) {
                        Swal.fire({
                            title: '¡Aceptado!',
                            text: acc.Message,
                            icon: 'success',
                            timer: 2000,
                            showConfirmButton: false,
                            background: 'white',
                            iconColor: '#4e73df'
                        }).then(function () {
                            window.location.href = '/Home/MisActivos';
                        });
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: (acc && acc.Message) || 'No se pudo confirmar la asignación.',
                            icon: 'error',
                            confirmButtonText: 'Aceptar',
                            background: 'white',
                            confirmButtonColor: '#4e73df'
                        });
                    }
                });
            }
        } else {
            // Credenciales incorrectas: error sin cambio de estado (Status 1 intacto)
            $("#loginError").removeClass("d-none");
        }
    });
}
