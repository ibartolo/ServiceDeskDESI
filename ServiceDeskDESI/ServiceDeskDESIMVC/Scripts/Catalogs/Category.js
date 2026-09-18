var tablaCategoria;

$(document).ready(function () {
    // Cargar categorías padres al cambiar el área
    $("#ddlArea").change(function () {
        CargarCategoriasPadre();
    });

    // Inicializar DataTable
    tablaCategoria = $('#tblCategoria').DataTable({
        columns: [
            { data: 'Id', title: 'ID', visible: false },
            { data: 'Nombre', title: 'Nombre' },
            { data: 'Descripcion', title: 'Descripción' },
            { data: 'CategoriaPadreNombre', title: 'Categoría Padre', defaultContent: '---' },
            { data: 'AreaNombre', title: 'Área' },
            { data: 'Orden', title: 'Orden', visible: false },
            {
                data: 'Id', title: 'Acciones', render: function (data) {
                    var html = '<div class="d-flex gap-3">';

                    // Botón Editar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEditar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-primary" onclick="EditarCategoria(' + data + ')"><i class="fas fa-edit"></i></button>';
                    }

                    // Botón Eliminar - solo si tiene permiso
                    if (permisosGlobal && permisosGlobal.PuedeEliminar) {
                        html += '<button type="button" class="btn btn-sm btn-outline-danger" onclick="EliminarCategoria(' + data + ')"><i class="fas fa-trash-alt"></i></button>';
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

    ConsultarCategorias();
    CargarCategoriasPadre();
});

$(function () {
    $("#frmCategoria").validate({
        rules: {
            "AreaId": {
                required: true,
                min: 1
            },
            "Nombre": {
                required: true,
                maxlength: 250
            }
        },
        messages: {
            "AreaId": {
                required: "El campo 'Área' es requerido.",
                min: "Seleccione un área válida."
            },
            "Nombre": {
                required: "El campo 'Nombre' es requerido.",
                maxlength: "El nombre no puede superar los 250 caracteres."
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

function CargarCategoriasPadre() {
    var areaId = $("#ddlArea").val();
    if (areaId) {
        GetMVC('/Catalogs/ConsultarTodasCategorias', function (response) {
            var result = typeof response === 'string' ? JSON.parse(response) : response;
            if (result.IsSuccess && result.Response) {
                var todasCategorias = result.Response;
                var ddl = $("#ddlCategoriaPadre");
                ddl.empty();
                ddl.append('<option value="">Sin categoría padre</option>');

                // Filtrar por área y solo categorías principales (sin padre)
                var categoriasFiltradas = todasCategorias.filter(function (c) {
                    return c.AreaId === parseInt(areaId) && c.CategoriaPadreId === null;
                });

                // Ordenar por Orden
                categoriasFiltradas.sort(function (a, b) {
                    return a.Orden - b.Orden;
                });

                categoriasFiltradas.forEach(function (c) {
                    ddl.append('<option value="' + c.Id + '">' + c.Nombre + '</option>');
                });

                if ($("#CategoriaPadreId").val() != '') {
                    ddl.val($("#CategoriaPadreId").val());
                }
            }
        });
    } else {
        $("#ddlCategoriaPadre").empty().append('<option value="">Sin categoría padre</option>');
    }
}

function ConsultarCategorias() {
    GetMVC("/Catalogs/ConsultarTodasCategorias", function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;

        if (result.IsSuccess && result.Response) {
            MapingPropertiesDataTable("tblCategoria", result.Response);
        } else {
            MapingPropertiesDataTable("tblCategoria", []);
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

function GuardarActualizarCategoria() {
    if ($("#frmCategoria").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var categoria = {
            Id: $("#Id").val() || 0,
            Nombre: $("#Nombre").val(),
            Descripcion: $("#Descripcion").val(),
            CategoriaPadreId: $("#ddlCategoriaPadre").val() ? parseInt($("#ddlCategoriaPadre").val()) : null,
            AreaId: parseInt($("#ddlArea").val()),
            Orden: parseInt($("#Orden").val()) || 0,
            CreadoPor: $("#CreadoPor").val(),
            FechaCreacion: $("#FechaCreacion").val(),
            ModificadoPor: $("#ModificadoPor").val() || null,
            FechaModificacion: null,
            Estatus: true
        };

        PostMVC('/Catalogs/GuardarOActualizarCategoria', categoria, function (response) {
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
                    window.location.href = '/Catalogs/Category';
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

function EditarCategoria(id) {
    window.location.href = '/Catalogs/Category/' + id;
}

function EliminarCategoria(id) {
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

            var categoria = {
                Id: id,
                ModificadoPor: $("#ModificadoPor").val() || "system",
                FechaModificacion: new Date().toISOString()
            };

            PostMVC('/Catalogs/EliminarCategoria', categoria, function (response) {
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
                        ConsultarCategorias();
                        $("#frmCategoria")[0].reset();
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
}
