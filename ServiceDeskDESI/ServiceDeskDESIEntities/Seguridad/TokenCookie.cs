using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class TokenCookie
    {
        public Token Token { get; set; }
        public long UserID { get; set; }
        public long EmpresaID { get; set; }
        public string UserName { get; set; }
        public string ProfileImage { get; set; }
        public string UserAvatar { get; set; }
    }
}
