using ServiceDeskDESIEntities.Catalogos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ServiceDeskDESIMVC.Models
{
    public class CategoriaResponsableViewModel
    {
        public long CategoriaId { get; set; }
        public CategoriaDTO Categoria { get; set; }
        public List<CategoriaResponsable> Responsables { get; set; }
    }
}