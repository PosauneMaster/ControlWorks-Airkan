using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Models
{
    public class StatusInfo
    {
        public int UnmatchedQuanitity { get; set; }
        public int ActiveOrders { get; set; }
        public int ActiveAlarms { get; set; }
        public string ConveyorUptime { get; set; }
        public int IdleBinCount { get; set; }
        public int BusyBinCount { get; set; }
        public bool Heartbeat { get; set; }
        public string SystemStatus { get; set; }

    }
}
