var tablaRoles;
var rolSeleccionadoId = null;
var paginasByRol = {
    disponibles: [],
    asignadas: []
};
var conteoByRol = {};

$(document).ready(function () {
    InicializarTablaRoles();
    CargarRoles();

    // Evento: cambio de rol
    $("#ddlRol").change(function () {
        var rolId = $(this).val();
        if (rolId) {
            rolSeleccionadoId = parseInt(rolId);
            CargarPermisos(rolSeleccionadoId);
            $("#chooserContainer").show();
            $("#btnGuardarPermisos").show();  // Mostrar siempre al seleccionar un rol
        } else {
            $("#chooserContainer").hide();
            $("#btnGuardarPermisos").hide();  // Ocultar al no tener rol seleccionado
            rolSeleccionadoId = null;
            paginasByRol.disponibles = [];
            paginasByRol.asignadas = [];
            RenderizarChooser();
            ActualizarBadges();
            $('#filtroPaginas').val('');
        }
    });

    // Evento: guardar permisos
    $("#btnGuardarPermisos").click(function () {
        GuardarPermisos();
    });

    // Event delegation para los botones dinámicos
    $(document).on('click', '.btn-move', function () {
        var paginaId = $(this).data('id');
        if (paginaId) {
            AsignarPagina(paginaId);
        }
    });

    $(document).on('click', '.btn-move-danger', function () {
        var paginaId = $(this).data('id');
        if (paginaId) {
            QuitarPagina(paginaId);
        }
    });

    // Cambios en checkboxes - guardar en cache automáticamente
    $(document).on('change', '.permiso-check', function () {
        if (!rolSeleccionadoId) return;

        var paginaId = parseInt($(this).data('paginaid'));
        var accion = $(this).data('accion');
        var valor = $(this).is(':checked');

        // Buscar la página asignada y actualizar su permiso
        var asignada = paginasByRol.asignadas.find(p => p.Id === paginaId);
        if (asignada) {
            switch (accion) {
                case 'Leer': asignada.PuedeLeer = valor; break;
                case 'Crear': asignada.PuedeCrear = valor; break;
                case 'Editar': asignada.PuedeEditar = valor; break;
                case 'Eliminar': asignada.PuedeEliminar = valor; break;
                case 'Exportar': asignada.PuedeExportar = valor; break;
            }
        }
    });

    // Filtro de búsqueda para páginas
    $('#filtroPaginas').on('keyup', function () {
        var filtro = $(this).val().toLowerCase();
        AplicarFiltro(filtro);
    });

    $('#btnLimpiarFiltro').click(function () {
        $('#filtroPaginas').val('');
        AplicarFiltro('');
    });
});

function InicializarTablaRoles() {
    tablaRoles = $('#tblRoles').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            {
                data: 'Id', title: 'Páginas Asignadas', render: function () {
                    return '<span class="badge-paginas">0</span>';
                }
            },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    return '<button class="btn btn-sm btn-outline-primary" onclick="SeleccionarRol(' + data + ')"><i class="fas fa-edit"></i> Editar</button>';
                }
            }
        ],
        language: {
            url: "/Content/datatables/i18n/es-ES.json"
        }
    });
}

function CargarRoles() {
    GetMVC("/Security/ConsultarTodosLosRoles", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            var roles = result.Response;
            tablaRoles.clear();
            roles.forEach(function (rol) {
                tablaRoles.row.add({
                    Id: rol.Id,
                    Nombre: rol.Nombre,
                    Descripcion: rol.Descripcion
                });
            });
            tablaRoles.draw();
            CargarConteoPaginas();
        }
    });
}

function CargarConteoPaginas() {
    GetMVC("/Security/ConsultarConteoPaginasPorRol", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            result.Response.forEach(function (c) {
                conteoByRol[c.RolId] = c.TotalPaginas;
            });
        }
        ActualizarBadges();
    });
}

function CargarPermisos(rolId) {
    GetMVC("/Security/ObtenerPermisosPorRol?rolId=" + rolId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        // Inicializar estructuras
        paginasByRol.disponibles = [];
        paginasByRol.asignadas = [];

        if (result.IsSuccess && result.Response) {
            var permisos = result.Response;

            // Crear diccionario de permisos por paginaId
            var permisosDict = {};
            permisos.forEach(function (p) {
                permisosDict[p.PaginaId] = p;
            });

            // Separar páginas disponibles y asignadas
            todasLasPaginas.forEach(function (pagina) {
                if (permisosDict[pagina.Id]) {
                    paginasByRol.asignadas.push({
                        Id: pagina.Id,
                        Nombre: pagina.Nombre,
                        Direccion: pagina.Direccion,
                        PuedeLeer: permisosDict[pagina.Id].PuedeLeer || false,
                        PuedeCrear: permisosDict[pagina.Id].PuedeCrear || false,
                        PuedeEditar: permisosDict[pagina.Id].PuedeEditar || false,
                        PuedeEliminar: permisosDict[pagina.Id].PuedeEliminar || false,
                        PuedeExportar: permisosDict[pagina.Id].PuedeExportar || false
                    });
                } else {
                    paginasByRol.disponibles.push({
                        Id: pagina.Id,
                        Nombre: pagina.Nombre,
                        Direccion: pagina.Direccion
                    });
                }
            });
        } else {
            // Si no hay permisos, todas las páginas están disponibles
            todasLasPaginas.forEach(function (pagina) {
                paginasByRol.disponibles.push({
                    Id: pagina.Id,
                    Nombre: pagina.Nombre,
                    Direccion: pagina.Direccion
                });
            });
        }

        RenderizarChooser();
        ActualizarBadges();
    });
}

function RenderizarChooser() {
    // Renderizar disponibles
    var htmlDisponibles = '';
    if (paginasByRol.disponibles.length === 0) {
        htmlDisponibles = '<div class="empty-message">No hay páginas disponibles</div>';
    } else {
        paginasByRol.disponibles.forEach(function (pagina) {
            htmlDisponibles += `
                <div class="chooser-item disponible" data-id="${pagina.Id}">
                    <div class="item-info">
                        <span class="item-nombre">${pagina.Nombre}</span>
                    </div>
                    <div class="item-actions">
                        <button class="btn-move" data-id="${pagina.Id}" title="Asignar página">
                            <i class="fas fa-arrow-right"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }
    $('#paginasDisponibles').html(htmlDisponibles);

    // Renderizar asignadas
    var htmlAsignadas = '';
    if (paginasByRol.asignadas.length === 0) {
        htmlAsignadas = '<div class="empty-message">No hay páginas asignadas a este rol</div>';
    } else {
        paginasByRol.asignadas.forEach(function (item) {
            htmlAsignadas += `
                <div class="chooser-item asignada" data-id="${item.Id}">
                    <div class="item-info">
                        <span class="item-nombre">${item.Nombre}</span>
                        <span class="text-muted-small">${item.Direccion || ''}</span>
                    </div>
                    <div class="item-actions">
                        <div class="permisos-checkboxes">
                            <label><input type="checkbox" class="permiso-check" data-paginaid="${item.Id}" data-accion="Leer" ${item.PuedeLeer ? 'checked' : ''}> Leer</label>
                            <label><input type="checkbox" class="permiso-check" data-paginaid="${item.Id}" data-accion="Crear" ${item.PuedeCrear ? 'checked' : ''}> Crear</label>
                            <label><input type="checkbox" class="permiso-check" data-paginaid="${item.Id}" data-accion="Editar" ${item.PuedeEditar ? 'checked' : ''}> Editar</label>
                            <label><input type="checkbox" class="permiso-check" data-paginaid="${item.Id}" data-accion="Eliminar" ${item.PuedeEliminar ? 'checked' : ''}> Eliminar</label>
                            <label><input type="checkbox" class="permiso-check" data-paginaid="${item.Id}" data-accion="Exportar" ${item.PuedeExportar ? 'checked' : ''}> Exportar</label>
                        </div>
                        <button class="btn-move btn-move-danger" data-id="${item.Id}" title="Quitar página">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }
    $('#paginasAsignadas').html(htmlAsignadas);
}

function AsignarPagina(paginaId) {
    if (!rolSeleccionadoId) return;

    // Verificar que la página no esté ya asignada
    var existe = paginasByRol.asignadas.some(p => p.Id === paginaId);
    if (existe) return;

    // Buscar la página en disponibles
    var index = paginasByRol.disponibles.findIndex(p => p.Id === paginaId);
    if (index === -1) return;

    // Mover de disponibles a asignadas con permisos por defecto
    var pagina = paginasByRol.disponibles.splice(index, 1)[0];
    paginasByRol.asignadas.push({
        Id: pagina.Id,
        Nombre: pagina.Nombre,
        Direccion: pagina.Direccion,
        PuedeLeer: true,
        PuedeCrear: false,
        PuedeEditar: false,
        PuedeEliminar: false,
        PuedeExportar: false
    });

    RenderizarChooser();
    ActualizarBadges();
}

function QuitarPagina(paginaId) {
    if (!rolSeleccionadoId) return;

    // Buscar la página en asignadas
    var index = paginasByRol.asignadas.findIndex(p => p.Id === paginaId);
    if (index === -1) return;

    // Mover de asignadas a disponibles
    var pagina = paginasByRol.asignadas.splice(index, 1)[0];
    paginasByRol.disponibles.push({
        Id: pagina.Id,
        Nombre: pagina.Nombre,
        Direccion: pagina.Direccion
    });

    RenderizarChooser();
    ActualizarBadges();
}

function SeleccionarRol(rolId) {
    $("#ddlRol").val(rolId).trigger('change');
}

function ActualizarBadges() {
    // Actualizar el contador de páginas en la tabla de roles
    tablaRoles.rows().every(function (rowIdx, tableLoop, rowLoop) {
        var data = this.data();
        var rolId = data.Id;

        var count = 0;
        // Si el rol seleccionado es el que está en la tabla, usar el conteo actual
        if (rolId === rolSeleccionadoId) {
            count = paginasByRol.asignadas.length;
        } else {
            // Para otros roles, usar el conteo total devuelto por el SP (sin N+1)
            count = conteoByRol[rolId] || 0;
        }

        var cell = this.node().querySelector('.badge-paginas');
        if (cell) {
            cell.textContent = count;
        }
    });
}

function AplicarFiltro(filtro) {
    // Filtrar páginas disponibles
    $('.chooser-item.disponible').each(function () {
        var nombre = $(this).find('.item-nombre').text().toLowerCase();
        if (filtro === '' || nombre.includes(filtro)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });

    // Filtrar páginas asignadas
    $('.chooser-item.asignada').each(function () {
        var nombre = $(this).find('.item-nombre').text().toLowerCase();
        if (filtro === '' || nombre.includes(filtro)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function GuardarPermisos() {
    if (!rolSeleccionadoId) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar un rol',
            icon: 'error',
            confirmButtonText: 'Aceptar'
        });
        return;
    }

    if (paginasByRol.asignadas.length === 0) {
        Swal.fire({
            title: 'Advertencia',
            text: 'No hay permisos asignados para guardar',
            icon: 'warning',
            confirmButtonText: 'Entendido'
        });
        return;
    }

    // Construir la lista de permisos
    var permisos = paginasByRol.asignadas.map(function (item) {
        return {
            PaginaId: item.Id,
            PuedeLeer: item.PuedeLeer || false,
            PuedeCrear: item.PuedeCrear || false,
            PuedeEditar: item.PuedeEditar || false,
            PuedeEliminar: item.PuedeEliminar || false,
            PuedeExportar: item.PuedeExportar || false
        };
    });

    // Construir el request
    var request = {
        RolId: rolSeleccionadoId,
        Permisos: permisos
    };

    // Mostrar confirmación antes de guardar
    Swal.fire({
        title: '¿Está seguro?',
        text: 'Se guardarán los permisos asignados para este rol',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#4e73df',
        cancelButtonColor: '#858796'
    }).then((result) => {
        if (result.isConfirmed) {
            // Mostrar loading
            Swal.fire({
                title: 'Guardando...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Una sola llamada al nuevo endpoint masivo
            PostMVC('/Security/GuardarPermisosRolMasivo', request, function (response) {
                Swal.close();

                if (response.IsSuccess) {
                    Swal.fire({
                        title: '¡Éxito!',
                        text: response.Message,
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.reload();
                    });
                } else {
                    Swal.fire({
                        title: 'Error',
                        text: response.Message,
                        icon: 'error',
                        confirmButtonText: 'Aceptar'
                    });
                }
            });
        }
    });
}
