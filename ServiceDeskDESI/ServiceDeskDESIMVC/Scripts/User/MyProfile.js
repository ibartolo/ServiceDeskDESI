$(function () {
    $("#frmPerfil").validate({
        rules: {
            "Nombre": {
                required: true,
                maxlength: 150
            },
            "Apellido": {
                required: true,
                maxlength: 250
            },
            "Celular": {
                required: true,
                maxlength: 50
            },
            "RFC": {
                required: true,
                maxlength: 50
            }
        },
        messages: {
            "Nombre": {
                required: "El campo 'Nombre(s)' es requerido.",
                maxlength: "El nombre no puede superar los 150 caracteres."
            },
            "Apellido": {
                required: "El campo 'Apellidos' es requerido.",
                maxlength: "Los apellidos no pueden superar los 250 caracteres."
            },
            "Celular": {
                required: "El campo 'Teléfono celular' es requerido.",
                maxlength: "El teléfono no puede superar los 50 caracteres."
            },
            "RFC": {
                required: "El campo 'RFC' es requerido.",
                maxlength: "El RFC no puede superar los 50 caracteres."
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

function GuardarPerfil() {
    if ($("#frmPerfil").valid()) {
        Swal.fire({
            title: 'Guardando...',
            text: 'Por favor espere',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        var parameters = new Array();
        parameters.push({ Name: "Id", Value: $("#Id").val() });
        parameters.push({ Name: "Nombre", Value: $("#Nombre").val() });
        parameters.push({ Name: "Apellido", Value: $("#Apellido").val() });
        parameters.push({ Name: "Celular", Value: $("#Celular").val() });
        parameters.push({ Name: "RFC", Value: $("#RFC").val() });
        parameters.push({ Name: "SucursalId", Value: $("#SucursalId").val() });
        parameters.push({ Name: "AreaId", Value: $("#AreaId").val() });
        parameters.push({ Name: "CreadoPor", Value: $("#CreadoPor").val() });
        parameters.push({ Name: "FechaCreacion", Value: $("#FechaCreacion").val() });
        parameters.push({ Name: "ModificadoPor", Value: $("#ModificadoPor").val() });
        parameters.push({ Name: "FechaModificacion", Value: new Date().toISOString() });
        parameters.push({ Name: "Estatus", Value: $("#Estatus").val() });
        parameters.push({ Name: "Contrasena", Value: $("#Contrasena").val() });
        parameters.push({ Name: "NombreUsuario", Value: $("#NombreUsuario").val() });
        parameters.push({ Name: "Correo", Value: $("#Correo").val() });
        parameters.push({ Name: "EmpresaId", Value: $("#EmpresaId").val() });
        parameters.push({ Name: "ImagenPerfil", Value: perfilTemp });



        var file = document.getElementById("ImagenPerfil").files[0];
        if (file) {
            parameters.push({ Name: "file", Value: file });
        }

        PostFileMVC('/User/ActualizarPerfilUsuario', parameters, function (response) {
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
                    window.location.href = '/User/MyProfile';
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

$('#ImagenPerfil').on('change', function (e) {
    const file = e.target.files[0];
    if (file) {
        const reader = new FileReader();
        reader.onload = function (e) {
            if ($('#imgPerfil').length) {
                $('#imgPerfil').attr('src', e.target.result);
            } else {
                $('#avatarIniciales').replaceWith('<img src="' + e.target.result + '" alt="Foto de perfil" class="rounded-circle" id="imgPerfil" style="width: 80px; height: 80px; object-fit: cover;" />');
            }
        }
        reader.readAsDataURL(file);
    }
});
