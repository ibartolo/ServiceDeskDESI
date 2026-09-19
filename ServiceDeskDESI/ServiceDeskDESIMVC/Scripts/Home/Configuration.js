function CambiarTema(tema) {
    // Aplicar el cambio de estilos de inmediato (para que no se sienta lento)
    $('body').toggleClass('dark-theme', tema === 'dark');

    // Guardar la preferencia en la cookie
    $.post('/Home/GuardarTema', { tema: tema }, function (response) {
        if (response && response.IsSuccess) {
            // Forzar recarga: el servidor vuelve a pintar la página leyendo la cookie
            // nueva. Sin esta recarga, el tema solo se aplicaba al abrir/borrar cookies.
            setTimeout(function () {
                window.location.reload();
            }, 200);
        } else {
            Swal.fire({
                title: 'Error',
                text: 'No se pudo guardar la preferencia de tema.',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    }).fail(function () {
        Swal.fire({
            title: 'Error',
            text: 'No se pudo guardar la preferencia de tema.',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
    });
}
