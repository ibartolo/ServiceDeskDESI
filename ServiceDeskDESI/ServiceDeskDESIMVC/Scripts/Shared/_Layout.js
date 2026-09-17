$(document).ready(function () {
    $("#sidebar").empty().load("/Home/MenusUser", function () {
        //console.log("jQuery cargado correctamente - versión: " + $.fn.jquery);

        // =========================================
        // FUNCIONALIDAD DEL SIDEBAR (COLLAPSE)
        // =========================================
        $('#sidebarCollapse').on('click', function () {
            $('#sidebar').toggleClass('active');
            $('#content').toggleClass('active');
            console.log('Sidebar toggled');
        });

        // =========================================
        // MANEJO DE SUBMENÚS DESPLEGABLES
        // =========================================
        $('.has-arrow').on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const $parent = $(this).parent();

            // Cerrar otros submenús abiertos (opcional - comentar si no se desea)
            $('.components li').not($parent).removeClass('active');

            // Toggle del submenú actual
            $parent.toggleClass('active');

            console.log('Submenú toggled:', $parent.find('.menu-text').text());
        });

        // =========================================
        // MARCAR ENLACES SIN REDIRECCIÓN COMO DESHABILITADOS
        // =========================================
        // Añade la clase `disabled-module` a todos los enlaces en .components con href="#".
        // Para enlaces que NO son padres (.has-arrow) se evita el click y se añade aria-disabled.
        $('.components a[href="#"]').each(function () {
            var $a = $(this);
            $a.addClass('disabled-module');

            if (!$a.hasClass('has-arrow')) {
                // Marcar como realmente no accionable (legend/tooltip accesible)
                $a.attr('aria-disabled', 'true');
                $a.on('click.disabled', function (ev) {
                    ev.preventDefault();
                    ev.stopPropagation();
                    // opcional: mostrar un tooltip o Swal indicando "Módulo en desarrollo"
                    // Swal.fire('Módulo en desarrollo', '', 'info');
                });
            }
        });

        // =========================================
        // CERRAR SUBMENÚS AL HACER CLIC FUERA
        // =========================================
        $(document).on('click', function (e) {
            if (!$(e.target).closest('.components li').length) {
                $('.components li').removeClass('active');
            }
        });

        // =========================================
        // MANEJO DE RESPONSIVE - CERRAR SIDEBAR EN MÓVIL AL SELECCIONAR OPCIÓN
        // =========================================
        $('.components a').on('click', function () {
            if ($(window).width() <= 992) {
                if (!$(this).hasClass('has-arrow')) {
                    $('#sidebar').removeClass('active');
                    $('#content').removeClass('active');
                }
            }
        });

        // =========================================
        // AJUSTE EN CAMBIO DE TAMAÑO DE VENTANA
        // =========================================
        $(window).on('resize', function () {
            if ($(window).width() > 992) {
                $('#sidebar, #content').removeClass('active');
            }
        });

        // =========================================
        // MARCAR ENLACES SIN REDIRECCIÓN COMO DESHABILITADOS
        // =========================================
        $('.components a[href="#"]').each(function () {
            var $a = $(this);

            // Si es un padre de submenú (tiene .has-arrow) no lo marcamos como "disabled"
            if ($a.hasClass('has-arrow')) {
                // Asegurar que no tenga la clase por algún motivo previo
                $a.removeClass('disabled-module');
                $a.removeAttr('aria-disabled');
                $a.off('click.disabled');
            } else {
                // Solo marcar enlaces leaf (sin submenú) que usan href="#"
                $a.addClass('disabled-module');
                $a.attr('aria-disabled', 'true');
                $a.on('click.disabled', function (ev) {
                    ev.preventDefault();
                    ev.stopPropagation();
                    // opcional: mostrar aviso: Swal.fire('Módulo en desarrollo', '', 'info');
                });
            }
        });

        // Verificar SweetAlert2 disponible
        if (typeof Swal !== 'undefined') {
            console.log('SweetAlert2 disponible');
        }
    });
});
