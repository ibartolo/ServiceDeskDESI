using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Catalogos
{
    // Catálogo simple de 4 campos (Nombre, Acronimo, RFC, Direccion).
    // Es una entidad distinta de `Empresa` (tenant, con vigencia/trial): solo comparten RFC/Direccion.
    // NO es residuo: tiene CRUD completo (tabla + 4 SPs, CompaniaController/Service/DbWrapper.Compania.cs y UI MVC).
    // No eliminar ni mergear con `Empresa` sin decisión de negocio previa.
    public  class Compania : BaseObject
    {
        public string Nombre { get; set; }
        public string Acronimo { get; set; }
        public string RFC { get; set; }
        public string Direccion { get; set; }

    }
}
