using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Models
{
    public class AlarmInfo
    {
        public int AlarmId { get; set; }
        public string Description { get; set; }

        public AlarmInfo()
        {
        }
        public AlarmInfo(int alarmId) : this(alarmId, String.Empty)
        {
        }

        public AlarmInfo(int alarmId, string description)
        {
            AlarmId = alarmId;
            Description = description;
        }
    }
}
