$(document).ready(function () {
    var $cards = $('.seccion-card');
    var $msg = $('#resultadoMsg');

    function normalizar(texto) {
        // minúsculas + sin acentos para comparar mejor
        return texto.toLowerCase()
            .normalize('NFD')
            .replace(/[\u0300-\u036f]/g, '')
            .replace(/[^a-z0-9\s]/g, ' ');
    }

    function filtrar() {
        var q = normalizar($('#buscadorAyuda').val());
        if (!q) {
            $cards.show();
            $msg.text('');
            return;
        }
        var palabras = q.split(/\s+/).filter(function (w) { return w.length > 0; });
        var visibles = 0;
        $cards.each(function () {
            var texto = normalizar($(this).attr('data-tags') + ' ' + $(this).text());
            var ok = palabras.every(function (p) { return texto.indexOf(p) !== -1; });
            $(this).toggle(ok);
            if (ok) { visibles++; }
        });

        if (visibles === 0) {
            $msg.html('No encontré secciones con "<strong>' + $('#buscadorAyuda').val() +
                '</strong>". Prueba con: ticket, pausar, materiales, terceros, activo, tipo de activo, marca, modelo, asignar, correo, contraseña, permiso, rol, catálogo, estadísticas, horario, empresa, logo.');
        } else {
            $msg.text(visibles + ' sección(es) encontrada(s).');
        }
    }

    $('#buscadorAyuda').on('input', filtrar);

    // Índice rápido: desplazarse a la sección elegida
    $('.chip[data-ir]').on('click', function (e) {
        e.preventDefault();
        var destino = $(this).attr('data-ir');
        $('html, body').animate({ scrollTop: $(destino).offset().top - 90 }, 400);
        $(destino).css('box-shadow', '0 0 0 3px #4e73df');
        setTimeout(function () { $(destino).css('box-shadow', ''); }, 2500);
    });

    // Volver arriba
    $('#btnArriba').on('click', function (e) {
        e.preventDefault();
        $('html, body').animate({ scrollTop: 0 }, 300);
    });

    // Soporte: enlaces internos tipo #seccion que puedan venir de otra vista
    if (window.location.hash) {
        try {
            var $dest = $(window.location.hash);
            if ($dest.length) {
                setTimeout(function () {
                    $('html, body').animate({ scrollTop: $dest.offset().top - 90 }, 400);
                    $dest.css('box-shadow', '0 0 0 3px #4e73df');
                    setTimeout(function () { $dest.css('box-shadow', ''); }, 2500);
                }, 300);
            }
        } catch (err) { }
    }
});
