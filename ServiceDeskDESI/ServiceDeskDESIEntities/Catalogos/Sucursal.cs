using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Sucursal : BaseObject
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string Calle { get; set; }
        public string Ciudad { get; set; }
        public string Colonia { get; set; }
        public string CodigoPostal { get; set; }
    }
}
