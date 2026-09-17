var tablaResponsables;
var categoriaSeleccionadaId = null;
var todosLosUsuarios = [];
var responsablesByCategoria = {
    disponibles: [],
    asignados: []
};
// Registros originales de la BD (CategoriaResponsable.Id + UsuarioId) para sincronizar al guardar
var responsablesOriginales = [];

$(document).ready(function () {
    InicializarTabla();
    CargarResponsables();

    // Obtener todos los usuarios que pueden atender tickets
    CargarTodosLosUsuarios();

    // Evento: cambio de categoría
    $("#ddlCategoria").change(function () {
        var categoriaId = $(this).val();
        if (categoriaId) {
            categoriaSeleccionadaId = parseInt(categoriaId);
            CargarResponsablesPorCategoria(categoriaSeleccionadaId);
            $("#chooserContainer").show();
            if (permisosGlobal && permisosGlobal.PuedeCrear) {
                $("#btnGuardarResponsables").show();
            }
        } else {
            $("#chooserContainer").hide();
            $("#btnGuardarResponsables").hide();
            categoriaSeleccionadaId = null;
            responsablesByCategoria.disponibles = [];
            responsablesByCategoria.asignados = [];
            RenderizarChooser();
            ActualizarTabla();
            $('#filtroUsuarios').val('');
        }
    });

    // Evento: guardar responsables
    $("#btnGuardarResponsables").click(function () {
        GuardarResponsables();
    });

    // Event delegation para los botones dinámicos
    $(document).on('click', '.btn-move', function () {
        var usuarioId = $(this).data('id');
        if (usuarioId) {
            AsignarResponsable(usuarioId);
        }
    });

    $(document).on('click', '.btn-move-danger', function () {
        var usuarioId = $(this).data('id');
        if (usuarioId) {
            QuitarResponsable(usuarioId);
        }
    });

    // Filtro de búsqueda
    $('#filtroUsuarios').on('keyup', function () {
        var filtro = $(this).val().toLowerCase();
        AplicarFiltro(filtro);
    });

    $('#btnLimpiarFiltro').click(function () {
        $('#filtroUsuarios').val('');
        AplicarFiltro('');
    });

    // Event delegation para marcar como principal
    $(document).on('change', '.principal-check', function () {
        var usuarioId = parseInt($(this).data('usuario-id'));
        var esPrincipal = $(this).is(':checked');

        // Desmarcar todos los demás
        $('.principal-check').not(this).prop('checked', false);

        // Actualizar en la lista de asignados
        var asignado = responsablesByCategoria.asignados.find(u => u.Id === usuarioId);
        if (asignado) {
            asignado.EsPrincipal = esPrincipal;
        }
    });
});

function InicializarTabla() {
    tablaResponsables = $('#tblResponsables').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'CategoriaNombre', title: 'Categoría' },
            { data: 'AreaNombre', title: 'Área' },
            { data: 'UsuarioNombre', title: 'Responsable' },
            {
                data: 'EsPrincipal', title: 'Principal', render: function (data) {
                    return data ? '<span class="badge-principal"><i class="fas fa-star"></i> Principal</span>' : '---';
                }
            },
            {
                data: 'Id', title: 'Acciones', render: function (data, type, row) {
                    var html = '<div class="d-flex gap-3">';

                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarResponsable(' + row.CategoriaId + ')"><i class="fas fa-edit"></i> Editar</button>';
                    }

                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarResponsable(event, ' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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
}

function CargarTodosLosUsuarios() {
    GetMVC("/Catalogs/ConsultarUsuariosQuePuedenAtender", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result.IsSuccess && result.Response) {
            todosLosUsuarios = result.Response;
        }
    });
}

function CargarResponsables() {
    GetMVC("/Catalogs/ConsultarTodosLosResponsables", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result.IsSuccess && result.Response) {
            var data = result.Response;
            tablaResponsables.clear();
            data.forEach(function (item) {
                tablaResponsables.row.add({
                    Id: item.Id,
                    CategoriaId: item.CategoriaId,
                    CategoriaNombre: item.CategoriaNombre || 'N/A',
                    AreaNombre: item.AreaNombre || 'N/A',
                    UsuarioNombre: item.Nombre ? item.Nombre + ' ' + (item.Apellido || '') : 'N/A',
                    EsPrincipal: item.EsPrincipal
                });
            });
            tablaResponsables.draw();
            ActualizarTabla();
        }
    });
}

function CargarResponsablesPorCategoria(categoriaId) {
    GetMVC("/Catalogs/ConsultarResponsablesPorCategoria?categoriaId=" + categoriaId, function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        // Inicializar estructuras
        responsablesByCategoria.disponibles = [];
        responsablesByCategoria.asignados = [];

        if (result.IsSuccess && result.Response) {
            var responsables = result.Response;

            // Crear diccionario de responsables por usuarioId (el DTO trae CategoriaResponsable.Id)
            var responsablesDict = {};
            responsablesOriginales = [];
            responsables.forEach(function (r) {
                responsablesDict[r.UsuarioId] = r;
                responsablesOriginales.push({ id: r.Id, usuarioId: r.UsuarioId });
            });

            // Separar usuarios disponibles y asignados
            todosLosUsuarios.forEach(function (usuario) {
                if (responsablesDict[usuario.Id]) {
                    responsablesByCategoria.asignados.push({
                        Id: usuario.Id,
                        responsableId: responsablesDict[usuario.Id].Id,
                        Nombre: usuario.Nombre,
                        Apellido: usuario.Apellido,
                        NombreUsuario: usuario.NombreUsuario,
                        Correo: usuario.Correo,
                        EsPrincipal: responsablesDict[usuario.Id].EsPrincipal || false
                    });
                } else {
                    responsablesByCategoria.disponibles.push({
                        Id: usuario.Id,
                        Nombre: usuario.Nombre,
                        Apellido: usuario.Apellido,
                        NombreUsuario: usuario.NombreUsuario,
                        Correo: usuario.Correo
                    });
                }
            });
        } else {
            // Si no hay responsables, todos los usuarios están disponibles
            responsablesOriginales = [];
            todosLosUsuarios.forEach(function (usuario) {
                responsablesByCategoria.disponibles.push({
                    Id: usuario.Id,
                    Nombre: usuario.Nombre,
                    Apellido: usuario.Apellido,
                    NombreUsuario: usuario.NombreUsuario,
                    Correo: usuario.Correo
                });
            });
        }

        RenderizarChooser();
        ActualizarTabla();
    });
}

function RenderizarChooser() {
    // Renderizar disponibles
    var htmlDisponibles = '';
    if (responsablesByCategoria.disponibles.length === 0) {
        htmlDisponibles = '<div class="empty-message">No hay usuarios disponibles</div>';
    } else {
        responsablesByCategoria.disponibles.forEach(function (usuario) {
            htmlDisponibles += `
                <div class="chooser-item disponible" data-id="${usuario.Id}">
                    <div class="item-info">
                        <span class="item-nombre">${usuario.Nombre} ${usuario.Apellido}</span>
                        <span class="text-muted-small">${usuario.NombreUsuario} - ${usuario.Correo}</span>
                    </div>
                    <div class="item-actions">
                        <button class="btn-move" data-id="${usuario.Id}" title="Asignar responsable">
                            <i class="fas fa-arrow-right"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }
    $('#usuariosDisponibles').html(htmlDisponibles);

    // Renderizar asignados
    var htmlAsignados = '';
    if (responsablesByCategoria.asignados.length === 0) {
        htmlAsignados = '<div class="empty-message">No hay responsables asignados</div>';
    } else {
        responsablesByCategoria.asignados.forEach(function (usuario) {
            htmlAsignados += `
                <div class="chooser-item asignada" data-id="${usuario.Id}">
                    <div class="item-info">
                        <span class="item-nombre">${usuario.Nombre} ${usuario.Apellido}</span>
                        <span class="text-muted-small">${usuario.NombreUsuario} - ${usuario.Correo}</span>
                        <div class="mt-1">
                            <label class="text-muted-small">
                                <input type="checkbox" class="principal-check" data-usuario-id="${usuario.Id}" ${usuario.EsPrincipal ? 'checked' : ''}>
                                <i class="fas fa-star"></i> Responsable principal
                            </label>
                        </div>
                    </div>
                    <div class="item-actions">
                        <button class="btn-move btn-move-danger" data-id="${usuario.Id}" title="Quitar responsable">
                            <i class="fas fa-times"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }
    $('#responsablesAsignados').html(htmlAsignados);
}

function AsignarResponsable(usuarioId) {
    if (!categoriaSeleccionadaId) return;

    // Verificar que el usuario no esté ya asignado
    var existe = responsablesByCategoria.asignados.some(u => u.Id === usuarioId);
    if (existe) return;

    // Buscar el usuario en disponibles
    var index = responsablesByCategoria.disponibles.findIndex(u => u.Id === usuarioId);
    if (index === -1) return;

    // Mover de disponibles a asignados
    var usuario = responsablesByCategoria.disponibles.splice(index, 1)[0];
    responsablesByCategoria.asignados.push({
        Id: usuario.Id,
        Nombre: usuario.Nombre,
        Apellido: usuario.Apellido,
        NombreUsuario: usuario.NombreUsuario,
        Correo: usuario.Correo,
        EsPrincipal: false
    });

    RenderizarChooser();
    ActualizarTabla();
}

function QuitarResponsable(usuarioId) {
    if (!categoriaSeleccionadaId) return;

    // Buscar el usuario en asignados
    var index = responsablesByCategoria.asignados.findIndex(u => u.Id === usuarioId);
    if (index === -1) return;

    // Mover de asignados a disponibles
    var usuario = responsablesByCategoria.asignados.splice(index, 1)[0];
    responsablesByCategoria.disponibles.push({
        Id: usuario.Id,
        Nombre: usuario.Nombre,
        Apellido: usuario.Apellido,
        NombreUsuario: usuario.NombreUsuario,
        Correo: usuario.Correo
    });

    RenderizarChooser();
    ActualizarTabla();
}

function ActualizarTabla() {
    // Actualizar la tabla de resumen con los datos actuales
    // Esto se actualiza cuando se carga la página o se cambia de categoría
}

function AplicarFiltro(filtro) {
    // Filtrar usuarios disponibles
    $('.chooser-item.disponible').each(function () {
        var nombre = $(this).find('.item-nombre').text().toLowerCase();
        var username = $(this).find('.text-muted-small').text().toLowerCase();
        if (filtro === '' || nombre.includes(filtro) || username.includes(filtro)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });

    // Filtrar usuarios asignados
    $('.chooser-item.asignada').each(function () {
        var nombre = $(this).find('.item-nombre').text().toLowerCase();
        var username = $(this).find('.text-muted-small').text().toLowerCase();
        if (filtro === '' || nombre.includes(filtro) || username.includes(filtro)) {
            $(this).show();
        } else {
            $(this).hide();
        }
    });
}

function EditarResponsable(categoriaId) {
    window.location.href = '/Catalogs/CategoriaResponsable?categoriaId=' + categoriaId;
}

function EliminarResponsable(event, id) {
    event.preventDefault();
    event.stopPropagation();

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

            var responsable = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarCategoriaResponsable', responsable, function (response) {
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
                        CargarResponsables();
                        if (categoriaSeleccionadaId) {
                            CargarResponsablesPorCategoria(categoriaSeleccionadaId);
                        }
                        $("#ddlCategoria").val(categoriaSeleccionadaId || '');
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

    return false;
}

function GuardarResponsables() {
    if (!categoriaSeleccionadaId) {
        Swal.fire({
            title: 'Error',
            text: 'Debe seleccionar una categoría',
            icon: 'error',
            confirmButtonText: 'Aceptar'
        });
        return;
    }

    if (responsablesByCategoria.asignados.length === 0 && responsablesOriginales.length === 0) {
        Swal.fire({
            title: 'Advertencia',
            text: 'No hay responsables asignados para guardar',
            icon: 'warning',
            confirmButtonText: 'Entendido'
        });
        return;
    }

    // Mostrar confirmación antes de guardar
    Swal.fire({
        title: '¿Está seguro?',
        text: 'Se sincronizarán los responsables de esta categoría (se agregarán, actualizarán y eliminarán según corresponda)',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Sí, guardar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#4e73df',
        cancelButtonColor: '#858796'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire({
                title: 'Guardando...',
                text: 'Por favor espere',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            var asignados = responsablesByCategoria.asignados;
            var asignadosUsuarioIds = asignados.map(function (u) { return u.Id; });

            // Operaciones de guardar/actualizar: cada responsable asignado (Id=0 crea, Id>0 actualiza)
            var opsGuardar = asignados.map(function (usuario) {
                return {
                    Id: usuario.responsableId || 0,
                    CategoriaId: categoriaSeleccionadaId,
                    UsuarioId: usuario.Id,
                    EsPrincipal: usuario.EsPrincipal || false
                };
            });

            // Operaciones de eliminar: originales de BD que ya no están en asignados
            var opsEliminar = responsablesOriginales
                .filter(function (orig) { return asignadosUsuarioIds.indexOf(orig.usuarioId) === -1; })
                .map(function (orig) {
                    return { Id: orig.id };
                });

            var totalOps = opsGuardar.length + opsEliminar.length;
            var completados = 0;
            var errores = [];

            function finalizar() {
                Swal.close();
                if (errores.length > 0) {
                    Swal.fire({
                        title: 'Errores al guardar',
                        text: 'Algunos responsables no se pudieron procesar: ' + errores.join(', '),
                        icon: 'warning',
                        confirmButtonText: 'Aceptar'
                    });
                } else {
                    Swal.fire({
                        title: '¡Éxito!',
                        text: 'Responsables guardados correctamente',
                        icon: 'success',
                        timer: 1500,
                        showConfirmButton: false
                    }).then(() => {
                        window.location.reload();
                    });
                }
            }

            if (totalOps === 0) {
                finalizar();
                return;
            }

            opsGuardar.forEach(function (op) {
                PostMVC('/Catalogs/GuardarOActualizarCategoriaResponsable', op, function (response) {
                    completados++;
                    if (!response.IsSuccess) { errores.push(response.Message); }
                    if (completados === totalOps) { finalizar(); }
                });
            });

            opsEliminar.forEach(function (op) {
                PostMVC('/Catalogs/EliminarCategoriaResponsable', op, function (response) {
                    completados++;
                    if (!response.IsSuccess) { errores.push(response.Message); }
                    if (completados === totalOps) { finalizar(); }
                });
            });
        }
    });
}
