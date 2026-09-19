$(document).ready(function () {
    // Cascada Área -> Categoría
    $("#ddlArea").change(function () {
        var areaId = $(this).val();

        // Limpiar categorías y subcategorías
        $("#ddlCategoria").empty().append('<option value="">Seleccione una categoría</option>');
        $("#ddlSubcategoria").empty().append('<option value="">Seleccione una subcategoría</option>');

        if (areaId) {
            GetMVC('/Ticket/ObtenerCategoriasPorArea?areaId=' + areaId, function (response) {
                var result = typeof response === 'string' ? JSON.parse(response) : response;
                if (result.IsSuccess && result.Response) {
                    var todasCategorias = result.Response;
                    var ddlCategoria = $("#ddlCategoria");

                    // Filtrar solo categorías padre (CategoriaPadreId == null)
                    var categoriasPadre = todasCategorias.filter(function (c) {
                        return c.CategoriaPadreId == null;
                    });

                    categoriasPadre.forEach(function (c) {
                        ddlCategoria.append('<option value="' + c.Id + '">' + c.Nombre + '</option>');
                    });
                }
            });
        }
    });

    // Cascada Categoría -> Subcategoría
    $("#ddlCategoria").change(function () {
        var categoriaId = $(this).val();

        // Limpiar subcategorías
        $("#ddlSubcategoria").empty().append('<option value="">Seleccione una subcategoría</option>');

        if (categoriaId) {
            GetMVC('/Ticket/ObtenerSubcategoriasPorCategoria?categoriaId=' + categoriaId, function (result) {
                if (result.IsSuccess && result.Response) {
                    var subcategorias = result.Response;
                    var ddlSubcategoria = $("#ddlSubcategoria");

                    subcategorias.forEach(function (s) {
                        ddlSubcategoria.append('<option value="' + s.Id + '">' + s.Nombre + '</option>');
                    });
                }
            });
        }
    });

    // Reset al abrir el modal
    $("#modalCapturarTicket").on("shown.bs.modal", function () {
        LimpiarCaptura();
        CargarFolioPreview();
    });

    // Validación de evidencias al seleccionar archivos
    $("#inputEvidenciasCaptura").change(function () {
        if (!ValidarArchivosEvidencia(this.files)) {
            $(this).val('');
        }
    });
});

function LimpiarCaptura() {
    if ($("#frmCapturaTicket")[0]) {
        $("#frmCapturaTicket")[0].reset();
    }
    $("#ddlCategoria").empty().append('<option value="">Seleccione una categoría</option>');
    $("#ddlSubcategoria").empty().append('<option value="">Seleccione una subcategoría</option>');
}

function CargarFolioPreview() {
    GetMVC('/Ticket/ConsultarFoliador', function (response) {
        var result = typeof response === 'string' ? JSON.parse(response) : response;
        if (result && result.IsSuccess && result.Response && result.Response.FolioSiguiente) {
            $("#Folio").val(result.Response.FolioSiguiente);
        }
    });
}

function GuardarTicket() {
    var areaId = $("#ddlArea").val();
    var categoriaId = $("#ddlCategoria").val();
    var subcategoriaId = $("#ddlSubcategoria").val();
    var urgencia = $("#ddlUrgencia").val();
    var titulo = $("#Titulo").val();
    var descripcion = $("#Descripcion").val();

    if (!areaId) {
        Swal.fire({ title: 'Error', text: 'Debe seleccionar un área', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }
    if (!categoriaId) {
        Swal.fire({ title: 'Error', text: 'Debe seleccionar una categoría', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }
    if (!subcategoriaId) {
        Swal.fire({ title: 'Error', text: 'Debe seleccionar una subcategoría', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }
    if (!urgencia) {
        Swal.fire({ title: 'Error', text: 'Debe seleccionar un nivel de urgencia', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }
    if (!titulo || !titulo.trim()) {
        Swal.fire({ title: 'Error', text: 'Debe ingresar un título', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }
    if (!descripcion || !descripcion.trim()) {
        Swal.fire({ title: 'Error', text: 'Debe ingresar una descripción', icon: 'error', confirmButtonText: 'Aceptar', background: 'white', confirmButtonColor: '#4e73df' });
        return;
    }

    // Capturar archivos seleccionados ANTES de limpiar el formulario
    var archivos = [];
    var inputEvidencias = document.getElementById('inputEvidenciasCaptura');
    if (inputEvidencias && inputEvidencias.files && inputEvidencias.files.length > 0) {
        archivos = Array.prototype.slice.call(inputEvidencias.files);
    }

    // Validación client-side de evidencias (tope, peso, extensión)
    if (!ValidarArchivosEvidencia(archivos)) {
        return;
    }

    Swal.fire({
        title: 'Guardando...',
        text: 'Por favor espere',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Un solo request: ticket + evidencias juntos (transaccional).
    var parameters = [
        { Name: 'Id', Value: 0 },
        { Name: 'AreaId', Value: areaId },
        { Name: 'CategoriaId', Value: categoriaId },
        { Name: 'SubcategoriaId', Value: subcategoriaId },
        { Name: 'Urgencia', Value: urgencia },
        { Name: 'Titulo', Value: titulo },
        { Name: 'Descripcion', Value: descripcion },
        { Name: 'TicketEstatusId', Value: 1 }
    ];

    for (var i = 0; i < archivos.length; i++) {
        parameters.push({ Name: 'archivos', Value: archivos[i] });
    }

    PostFileMVC('/Ticket/GuardarTicketConEvidencias', parameters, function (response) {
        Swal.close();

        if (response && response.IsSuccess) {
            bootstrap.Modal.getInstance(document.getElementById('modalCapturarTicket')).hide();
            LimpiarCaptura();

            if (typeof RefrescarTabla === 'function') {
                RefrescarTabla();
            } else {
                window.location.reload();
            }

            Swal.fire({
                title: '¡Éxito!',
                text: response.Message,
                icon: 'success',
                timer: 1500,
                showConfirmButton: false,
                background: 'white',
                iconColor: '#4e73df'
            });
        } else {
            Swal.fire({
                title: 'Error',
                text: (response && response.Message) ? response.Message : 'No se pudo guardar el ticket.',
                icon: 'error',
                confirmButtonText: 'Aceptar',
                background: 'white',
                confirmButtonColor: '#4e73df'
            });
        }
    });
}
