using System;

namespace ServiceDeskDESIEntities.Autenticacion
{
    public class UsuarioDTO : Usuario
    {
        public string SucursalNombre { get; set; }
        public string AreaNombre { get; set; }
        // ObtenerUsuarios/ObtenerUsuarioPorId devuelven "EmpresaNombre"
        public string EmpresaNombre { get; set; }
        // AutenticarUsuario devuelve "EmpresaNombreComercial" + datos de vigencia/trial
        public string EmpresaNombreComercial { get; set; }
        public string EmpresaRazonSocial { get; set; }
        public string EmpresaRFC { get; set; }
        public string EmpresaResponsable { get; set; }
        public string EmpresaDireccion { get; set; }
        public string EmpresaCiudad { get; set; }
        public string EmpresaEstado { get; set; }
        public string EmpresaCodigoPostal { get; set; }
        public string EmpresaTelefono { get; set; }
        public string EmpresaCorreoContacto { get; set; }
        public DateTime? FechaVigenciaInicio { get; set; }
        public DateTime? FechaVigenciaFin { get; set; }
        public bool? EsPeriodoPrueba { get; set; }
    }
}
