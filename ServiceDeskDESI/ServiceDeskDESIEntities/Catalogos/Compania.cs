using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
  public  class Compania : BaseObject
    {
        public string Nombre { get; set; }
        public string Acronimo { get; set; }
        public string RFC { get; set; }
        public string Direccion { get; set; }

    }
}
