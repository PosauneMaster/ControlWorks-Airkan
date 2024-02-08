using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class SystemStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }

        public SystemStatus()
        {
        }

        public SystemStatus(string status)
        {
            Status = status;
        }
    }
}
