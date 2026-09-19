var personaSincronizadoUsuarioId = 0;   // usuario elegido en el modal para una persona nueva (aún no guardada)
var usuariosDisponibles = [];           // caché de usuarios listados en el modal de sincronización

$(function () {
    $("#frmPersona").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 150
            },
            "Apellido": {
                required: true,
                maxlength: 250
            },
            "Correo": {
                email: true,
                maxlength: 250
            },
            "Telefono": {
                maxlength: 50
            },
            "PuestoId": {
                required: true,
                min: 1
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 150 caracteres."
            },
            "Apellido": {
                required: "El campo 'Apellido' es requerido.",
                maxlength: "El apellido no puede superar los 250 caracteres."
            },
            "Correo": {
                email: "Ingrese un correo electrónico válido.",
                maxlength: "El correo no puede superar los 250 caracteres."
            },
            "Telefono": {
                maxlength: "El teléfono no puede superar los 50 caracteres."
            },
            "PuestoId": {
                required: "El campo 'Puesto' es requerido.",
                min: "Seleccione un puesto válido."
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
});

$(document).ready(function () {
    var tabla = new DataTable('#tblPersona', {
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Apellido', title: 'Apellido' },
            { data: 'Correo', title: 'Correo' },
            { data: 'Telefono', title: 'Teléfono' },
            { data: 'PuestoNombre', title: 'Puesto' },
            { data: 'NombreUsuarioVinculado', title: 'Usuario', defaultContent: '---' },
            {
                data: 'Id', title: 'Acciones', render: function (data, type, row) {
                    var html = '<div class="d-flex gap-3">';
                    // Botón "vincular activo" solo si la persona tiene UsuarioId
                    if (row.UsuarioId != null && row.UsuarioId > 0) {
                        html += '<button type="button" class="btn btn-sm btn-outline-info" title="Activos" onclick="AbrirActivosPersona(' + data + ')"><i class="fas fa-laptop"></i></button>';
                    }
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarPersona(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarPersona(event, ' + data + ')"><i class="fas fa-trash-alt"></i></button>';
                    }
                    html += '</div>';
                    return html;
                }
            },
            { data: 'Estatus', title: 'Estatus', visible: false },
            { data: 'UsuarioId', title: 'UsuarioId', visible: false }
        ],
        language: {
            url: "/Content/datatables/i18n/es-ES.json"
        }
    });
    ConsultarTodasPersonas();
    AplicarBloqueoSincronizado();
});

function GuardarActualizarPersona() {
    if ($("#frmPersona").valid()) {
        if (personaUsuarioId > 0 || personaSincronizadoUsuarioId > 0) {
            Swal.fire({
                title: 'Advertencia',
                text: 'Al guardar, la persona quedará vinculada al usuario seleccionado. Los datos (Nombre, Apellido, Correo y Teléfono) se sincronizarán con los del usuario y no podrá modificarlos. El puesto se conserva.',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Continuar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#4e73df',
                cancelButtonColor: '#858796',
                background: 'white'
            }).then(function (res) {
                if (res.isConfirmed) {
                    GuardarPersona();
                }
            });
        } else {
            GuardarPersona();
        }
    }
}

function GuardarPersona() {
    Swal.fire({
        title: 'Guardando...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    var persona = {
        Id: $("#Id").val() || 0,
        Nombre: $("#Nombre").val(),
        Apellido: $("#Apellido").val(),
        Correo: $("#Correo").val(),
        Telefono: $("#Telefono").val(),
        PuestoId: $("#PuestoId").val(),
        CreadoPor: $("#CreadoPor").val(),
        FechaCreacion: new Date().toISOString(),
        ModificadoPor: $("#ModificadoPor").val() || null,
        FechaModificacion: null,
        Estatus: true
    };
    PostMVC('/Catalogs/GuardarOActualizarPersona', persona, function (response) {
        Swal.close();

        if (response.IsSuccess) {
            var nuevoId = response.Response ? response.Response.Id : 0;

            if (personaSincronizadoUsuarioId > 0 && nuevoId > 0) {
                // Vínculo pendiente (persona nueva o en edición): persistir al guardar
                PostMVC('/Catalogs/VincularPersonaUsuario', { personaId: nuevoId, usuarioId: personaSincronizadoUsuarioId }, function (respVincular) {
                    Swal.close();
                    var res = typeof respVincular === 'string' ? JSON.parse(respVincular) : respVincular;
                    if (res && res.IsSuccess) {
                        Swal.fire({
                            title: '¡Éxito!',
                            text: res.Message,
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false,
                            background: 'white',
                            iconColor: '#4e73df'
                        }).then(() => {
                            window.location.href = '/Catalogs/Persona';
                        });
                    } else {
                        Swal.fire({
                            title: 'Error',
                            text: (res && res.Message) || 'La persona se guardó, pero no se pudo vincular al usuario.',
                            icon: 'error',
                            confirmButtonText: 'Aceptar',
                            background: 'white',
                            confirmButtonColor: '#4e73df'
                        });
                    }
                });
            } else {
                Swal.fire({
                    title: '¡Éxito!',
                    text: response.Message,
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false,
                    background: 'white',
                    iconColor: '#4e73df'
                }).then(() => {
                    window.location.href = '/Catalogs/Persona';
                });
            }
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

function ConsultarTodasPersonas() {
    GetMVC("/Catalogs/ConsultarTodasLasPersonas", function (r) {
        var result = typeof r == 'string' ? JSON.parse(r) : r;

        if (result.IsSuccess) {
            MapingPropertiesDataTable("tblPersona", result.Response);
        } else {
            Swal.fire({
                title: 'Error',
                text: result.Message,
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}

function EditarPersona(id) {
    window.location.href = '/Catalogs/Persona/' + id;
}

function EliminarPersona(event, id) {
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
            var persona = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };
            PostMVC('/Catalogs/EliminarPersona', persona, function (response) {
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
                        ConsultarTodasPersonas();
                        $("#frmPersona")[0].reset();
                        $("#Id").val(0);
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

function AbrirActivosPersona(id) {
    var row = $('#tblPersona').DataTable().rows().data().toArray().find(function (r) { return r.Id === id; });
    if (row) {
        $("#personaNombreActivo").text(row.Nombre + ' ' + row.Apellido);
    } else {
        $("#personaNombreActivo").text('---');
    }
    $("#personaIdActivo").val(id);
    $("#tblActivosPersona tbody").empty();
    $("#ddlActivoDisponible").empty().append('<option value="">Seleccione un activo</option>');
    CargarActivosDisponibles();
    CargarActivosPersona();
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalAsignarActivo')).show();
}

// ===== Sincronizar Persona <-> Usuario =====

function AbrirSincronizarUsuario() {
    CargarUsuariosSincronizar();
    bootstrap.Modal.getOrCreateInstance(document.getElementById('modalSincronizarUsuario')).show();
}

function CargarUsuariosSincronizar() {
    var tbody = $("#tblUsuariosSincronizar tbody");
    tbody.empty();
    GetMVC("/User/ConsultarTodosLosUsuarios", function (r) {
        var result = typeof r === 'string' ? JSON.parse(r) : r;
        if (result && result.IsSuccess && result.Response) {
            usuariosDisponibles = result.Response.filter(function (u) {
                return u.PersonaId == null || u.PersonaId === 0;
            });
            if (usuariosDisponibles.length === 0) {
                tbody.append('<tr><td colspan="5" class="text-center">No hay usuarios disponibles para sincronizar</td></tr>');
                return;
            }
            usuariosDisponibles.forEach(function (u) {
                var row = '<tr>';
                row += '<td>' + (u.NombreUsuario || '') + '</td>';
                row += '<td>' + (u.Nombre || '') + '</td>';
                row += '<td>' + (u.Apellido || '') + '</td>';
                row += '<td>' + (u.Correo || '') + '</td>';
                row += '<td><button type="button" class="btn btn-sm btn-outline-primary" onclick="ConfirmarSincronizar(' + u.Id + ')">Sincronizar</button></td>';
                row += '</tr>';
                tbody.append(row);
            });
        } else {
            tbody.append('<tr><td colspan="5" class="text-center">No hay usuarios disponibles</td></tr>');
        }
    });
}

function ConfirmarSincronizar(usuarioId) {
    var usuario = null;
    for (var i = 0; i < usuariosDisponibles.length; i++) {
        if (usuariosDisponibles[i].Id === usuarioId) { usuario = usuariosDisponibles[i]; break; }
    }

    Swal.fire({
        title: '¿Sincronizar con este usuario?',
        text: 'Se copiarán los datos del usuario (Nombre, Apellido, Correo y Teléfono) a la persona y se bloquearán los campos. Al guardar, la persona quedará vinculada a este usuario. Si cancela o sale sin guardar, no se vinculará nada.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, sincronizar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#4e73df',
        cancelButtonColor: '#858796',
        background: 'white'
    }).then(function (result) {
        if (!result.isConfirmed) { return; }

        // Copiar datos y bloquear inputs. El vínculo NO se persiste aquí: se aplica al guardar.
        if (usuario) {
            $("#Nombre").val(usuario.Nombre || '');
            $("#Apellido").val(usuario.Apellido || '');
            $("#Correo").val(usuario.Correo || '');
            $("#Telefono").val(usuario.Celular || '');
            $("#usuarioVinculado").val(usuario.NombreUsuario || '');
        }
        personaSincronizadoUsuarioId = usuarioId;
        AplicarBloqueoSincronizado();
        bootstrap.Modal.getOrCreateInstance(document.getElementById('modalSincronizarUsuario')).hide();

        Swal.fire({
            title: 'Usuario seleccionado',
            text: 'Datos copiados y campos bloqueados. Guarde la persona para completar la vinculación.',
            icon: 'success',
            timer: 2500,
            showConfirmButton: false,
            background: 'white',
            iconColor: '#4e73df'
        });
    });
}

function AplicarBloqueoSincronizado() {
    if (personaUsuarioId > 0 || personaSincronizadoUsuarioId > 0 || (personaIdEdicion > 0 && !permisosGlobal.PuedeEditar)) {
        $("#Nombre").prop("disabled", true);
        $("#Apellido").prop("disabled", true);
        $("#Correo").prop("disabled", true);
        $("#Telefono").prop("disabled", true);
    }
    if (personaNombreUsuarioVinculado && personaNombreUsuarioVinculado !== "") {
        $("#usuarioVinculado").val(personaNombreUsuarioVinculado);
    }
}
