using ControlWorks.Services.PVI.Pvi;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Processors
{
    public class AirkanProcessor
    {
        const string cpuName = "Airkan";
        private IPviApplication _pviApplication;
        private PVI.IEventNotifier _eventNotifier;

        public AirkanProcessor(IPviApplication pviApplication)
        {
            _pviApplication = pviApplication;
        }

        public async Task SendVariableAsync<T>(string command, T data)
        {
            var list = new List<T>() { data };
            var jsonData = JsonConvert.SerializeObject(list);
            await Task.Run(() => _pviApplication.SendCommand(cpuName, command, jsonData));
        }

        public async Task SendVariableListAsync<T>(string command, IEnumerable<T> data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            await Task.Run(() => _pviApplication.SendCommand(cpuName, command, jsonData));
        }

    }
}
