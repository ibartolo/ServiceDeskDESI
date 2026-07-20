//using Microsoft.Analytics.Interfaces;
//using Microsoft.Analytics.Types.Sql;
using Serilog;
using ServiceDeskDESIEntities.Seguridad;
using ServiceDeskDESIWebApi.DAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;

namespace ServiceDeskDESIWebApi.Services
{
    public  class CompaniaService
    {
        private readonly DbWrapper _dbWrapper;
        public CompaniaService()
        {
            _dbWrapper = new DbWrapper();
        }
      
    }
}