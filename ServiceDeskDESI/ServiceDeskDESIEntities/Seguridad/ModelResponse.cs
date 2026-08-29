using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDeskDESIEntities.Seguridad
{
    public class ModelResponse
    {
        public ModelResponse()
        {
            IsSuccess = false;
        }

        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Response { get; set; }
    }

    public class ModelResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public T Response { get; set; }
    }
}
