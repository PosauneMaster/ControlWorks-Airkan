using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class LogItem
    {
        private List<string> _logFiles = new List<string>();
        public string LogDirectory { get; set; }
        public string[] LogFiles => _logFiles.ToArray();
        
        public void AddLogFile(string file)
        {
            _logFiles.Add(file);
        }
    }
}
