using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Models
{
    public class CommandStatus
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public CommandStatus()
        {
        }

        public CommandStatus(int statusCode, string message)
        {
            StatusCode = statusCode;
            Message = message;
        }
    }
}
