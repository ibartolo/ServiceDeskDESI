var tablaActivo;

$(document).ready(function () {
    InicializarDataTable();
    ConsultarTodosLosActivos();

    // Evento: al cambiar la marca, cargar modelos
    $("#ddlMarca").change(function () {
        var marcaId = $(this).val();

        // Limpiar el DDL de modelos
        $("#ddlModelo").empty().append('<option value="">Seleccione un modelo</option>');
        $("#ModeloId").val('');

        // Remover estilo de error del DDL de modelo
        $("#ddlModelo").removeClass("is-invalid-dropdown");

        if (marcaId) {
            // Mostrar spinner de carga en el DDL de modelos
            $("#ddlModelo").html('<option value="">Cargando modelos...</option>');

            GetMVC("/Catalogs/ConsultarModelosPorMarca?marcaId=" + marcaId, function (response) {
                var result = typeof response === 'string' ? JSON.parse(response) : response;

                // Limpiar y restaurar el DDL de modelos
                $("#ddlModelo").empty().append('<option value="">Seleccione un modelo</option>');

                if (result.IsSuccess && result.Response) {
                    var modelos = result.Response;
                    if (modelos.length > 0) {
                        modelos.forEach(function (m) {
                            $("#ddlModelo").append('<option value="' + m.Id + '">' + m.Nombre + '</option>');
                        });

                        // Seleccionar el modelo si estamos en modo edición
                        if (modeloSelect > 0) {
                            $("#ddlModelo").val(modeloSelect);
                            $("#ModeloId").val(modeloSelect);
                        }
                    } else {
                        $("#ddlModelo").append('<option value="">No hay modelos disponibles</option>');
                    }
                } else {
                    $("#ddlModelo").append('<option value="">Error al cargar modelos</option>');
                    Swal.fire({
                        title: 'Error',
                        text: result.Message || 'No se pudieron cargar los modelos',
                        icon: 'error',
                        confirmButtonText: 'Aceptar',
                        background: 'white',
                        confirmButtonColor: '#4e73df'
                    });
                }
            });
        }
    });

    // Remover estilos de error cuando cambian los dropdowns
    $("#ddlTipoActivo, #ddlMarca, #ddlModelo").change(function () {
        $(this).removeClass("is-invalid-dropdown");
    });

    // Modo edición: preseleccionar valores
    if (activoId != 0) {
        // Seleccionar tipo de activo
        if (tipoActivoSelect > 0) {
            $("#ddlTipoActivo").val(tipoActivoSelect);
        }

        // Seleccionar marca y disparar evento para cargar modelos
        if (marcaSelect > 0) {
            $("#ddlMarca").val(marcaSelect);
            $("#ddlMarca").trigger('change');
        }
    }
});

$(function () {
    $("#frmActivo").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 250
            },
            "Descripcion": {
                required: true
            },
            "FechaCompra": {
                required: true
            },
            "Notas": {
                maxlength: 250
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 250 caracteres."
            },
            "Descripcion": {
                required: "El campo 'Descripción' es requerido."
            },
            "FechaCompra": {
                required: "El campo 'Fecha de Compra' es requerido."
            },
            "Notas": {
                maxlength: "Las notas no pueden superar los 250 caracteres."
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

function InicializarDataTable() {
    tablaActivo = $('#tblActivo').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            { data: 'Serial', title: 'Serial', defaultContent: '---' },
            { data: 'MarcaNombre', title: 'Marca', defaultContent: '---' },
            { data: 'ModeloNombre', title: 'Modelo', defaultContent: '---' },
            { data: 'TipoActivoNombre', title: 'Tipo', defaultContent: '---' },
            { data: 'PersonaNombre', title: 'Asignado a', defaultContent: '---', render: function (data, type, row) { return row.PersonaNombre ? row.PersonaNombre + ' ' + (row.PersonaApellido || '') : '---'; } },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Mantenimientos - solo si tiene permiso de lectura (reutiliza "Activos")
                    if (permisosGlobal && permisosGlobal.PuedeLeer) {
                        html += '<button type="button" class="btn btn-sm btn-outline-info" title="Mantenimientos" onclick="AbrirMantenimientos(' + data + ')"><i class="fas fa-tools"></i></button>';
                    }

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarActivo(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarActivo(' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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

function ConsultarTodosLosActivos() {
    GetMVC("/Catalogs/ConsultarTodosLosActivo", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            MapingPropertiesDataTable("tblActivo", result.Response);
        } else {
            MapingPropertiesDataTable("tblActivo", []);
            if (!result.IsSuccess) {
                Swal.fire({
                    title: 'Error',
                    text: result.Message,
                    icon: 'error',
                    confirmButtonText: 'Aceptar',
                    background: 'white',
                    confirmButtonColor: '#4e73df'
                });
            }
        }
    });
}

function GuardarActualizarActivo() {
    // Limpiar estilos de error previos
    $("#ddlTipoActivo, #ddlMarca, #ddlModelo").removeClass("is-invalid-dropdown");

    var isValidDropdown = true;
    var errorMessage = "";

    if (!$("#ddlTipoActivo").val()) {
        $("#ddlTipoActivo").addClass("is-invalid-dropdown");
        isValidDropdown = false;
        errorMessage = "Debe seleccionar un tipo de activo";
    }

    if (!$("#ddlMarca").val()) {
        $("#ddlMarca").addClass("is-invalid-dropdown");
        isValidDropdown = false;
        if (errorMessage === "") errorMessage = "Debe seleccionar una marca";
    }

    if (!$("#ddlModelo").val()) {
        $("#ddlModelo").addClass("is-invalid-dropdown");
        isValidDropdown = false;
        if (errorMessage === "") errorMessage = "Debe seleccionar un modelo";
    }

    if (!isValidDropdown) {
        Swal.fire({
            title: 'Error',
            text: errorMessage,
            icon: 'error',
            confirmButtonText: 'Aceptar',
            background: 'white',
            confirmButtonColor: '#4e73df'
        });
        return;
    }

    if ($("#frmActivo").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var activo = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Descripcion: $("#Descripcion").val(),
            Serial: $("#Serial").val(),
            SerieLocal: $("#SerieLocal").val(),
            Notas: $("#Notas").val(),
            FechaCompra: $("#FechaCompra").val(),
            TipoActivoId: parseInt($("#ddlTipoActivo").val()),
            MarcaId: parseInt($("#ddlMarca").val()),
            ModeloId: parseInt($("#ddlModelo").val()),
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: $("#FechaCreacion").val(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };

        PostMVC('/Catalogs/GuardarOActualizarActivos', activo, function (response) {
            Swal.close();

            if (response.IsSuccess) {
                Swal.fire({
                    title: '¡Éxito!',
                    text: response.Message,
                    icon: 'success',
                    timer: 1500,
                    showConfirmButton: false,
                    background: 'white',
                    iconColor: '#4e73df'
                }).then(() => {
                    window.location.href = '/Catalogs/Active';
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
}

function EditarActivo(id) {
    window.location.href = '/Catalogs/Active/' + id;
}

function EliminarActivo(id) {
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

            var activo = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarActivos', activo, function (response) {
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
                        ConsultarTodosLosActivos();
                        $("#frmActivo")[0].reset();
                        $("#Id").val(0);
                        $("#ddlModelo").empty().append('<option value="">Seleccione un modelo</option>');
                        // Limpiar estilos de error
                        $("#ddlTipoActivo, #ddlMarca, #ddlModelo").removeClass("is-invalid-dropdown");
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
}
