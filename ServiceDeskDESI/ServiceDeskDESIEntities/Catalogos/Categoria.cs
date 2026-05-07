using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    public class Categoria : BaseObject
    {
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public Categoria CategoriaPadre { get; set; }
        public Area Area { get; set; }
        public int Orden { get; set; }
    }
}
