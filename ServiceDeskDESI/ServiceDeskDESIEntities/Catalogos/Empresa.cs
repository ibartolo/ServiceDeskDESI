using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Empresa:BaseObject
    {
        public string NombreComercial { get; set; }
        public string RazonSocial { get; set; }
        public string RFC { get; set; }
        public string Responsable { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Estado { get; set; }
        public string CodigoPostal { get; set; }
        public string Telefono { get; set; }
        public string CorreoContacto { get; set; }
        public DateTime FechaVigenciaInicio { get; set; }
        public DateTime FechaVigenciaFin { get; set; }
        public bool EsPeriodoPrueba { get; set; }
    }
}
