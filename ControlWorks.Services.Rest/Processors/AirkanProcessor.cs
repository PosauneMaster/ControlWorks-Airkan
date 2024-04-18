using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.PVI.Variables;

using Newtonsoft.Json;

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ControlWorks.Services.PVI.Models;

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

        public async Task<List<AirkanJob>> GetAirkanVariablesAsync()
        {
            return await Task.Run(() => _pviApplication.GetAirkanVariables(cpuName));
        }

        public async Task<List<AirkanInputFileInfo>> GetAirkanInputFilesAsync()
        {
            return await Task.Run(() => _pviApplication.GetAirkanInputFiles());
        }

        public async Task ProcessFileAsync(string filename)
        {
            await Task.Run(() => _pviApplication.SendCommand(cpuName, "ProcessFile", filename));
        }

        public async Task ProcessFileByIndexAsync(string index)
        {
            await Task.Run(() => _pviApplication.SendCommand(cpuName, "ProcessInputFileByIndex", index));
        }

        public async Task ProcessSetFileTransferLocation(string location)
        {
            await Task.Run(() => _pviApplication.SendCommand(cpuName, "SetFileTransferLocation", location));
        }

        public async Task ProcessBarCode()
        {
            await Task.Run(() => _pviApplication.SendCommand(cpuName, "ProcessBarCode", ""));
        }

    }
}
