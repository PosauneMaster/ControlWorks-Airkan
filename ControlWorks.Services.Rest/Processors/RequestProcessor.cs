using ControlWorks.Common;
using ControlWorks.Services.PVI.Panel;
using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.PVI.Task;

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Processors
{
    public interface IRequestProcessor
    {
        Task<List<CpuDetailResponse>> GetCpuDetails();
        Task<CpuDetailResponse> GetCpuByName(string name);
        Task<CpuDetailResponse> GetCpuByIp(string ip);
        Task Add(CpuInfoRequest request);
        Task DeleteCpuByName(string name);
        Task DeleteCpuByIp(string ip);
        Task<List<string>> GetTaskNames(string cpuName);
        Task<List<string>> GetTaskNamesByIp(string ipAddress);
        Task<CpuDetails> GetCpuDetailsByName(string cpuName);
        Task<CpuDetails> GetCpuDetailsByIp(string ipAddress);
        Task<CpuDetails> GetTaskDetailsByName(string cpuName, string taskName);
        Task<CpuDetails> GetTaskDetailsByIp(string ipAddress, string taskName);

    }
    public class RequestProcessor : BaseProcessor, IRequestProcessor
    {
        private readonly IPviApplication _application;

        public RequestProcessor() { }

        public RequestProcessor(IPviApplication application)
        {
            _application = application;
        }

        public async Task<List<CpuDetailResponse>> GetCpuDetails()
        {
            var result = await Task.Run(() => _application.GetCpuData().ToList());

            return result;
        }

        public async Task<CpuDetailResponse> GetCpuByName(string name)
        {
            var result = await Task.Run(() => _application.GetCpuByName(name));

            return result;
        }

        public async Task<CpuDetailResponse> GetCpuByIp(string ip)
        {
            var result = await Task.Run(() => _application.GetCpuByIp(ip));

            return result;
        }

        public async Task Add(CpuInfoRequest request)
        {
            var info = new CpuInfo()
            {
                Name = request.Name,
                Description = request.Description,
                IpAddress = request.IpAddress
            };

            Trace.TraceInformation($"RequestProcessor Operation=Add request={ToJson(request)}");

            await Task.Run(() => _application.AddCpu(info));
        }

        public async Task DeleteCpuByName(string name)
        {
            Trace.TraceInformation($"RequestProcessor Operation=DeleteCpuByName name={name}");

            await Task.Run(() => _application.DeleteCpuByName(name));
        }

        public async Task DeleteCpuByIp(string ip)
        {
            Trace.TraceInformation($"RequestProcessor Operation=DeleteCpuByIp name={ip}");

            await Task.Run(() => _application.DeleteCpuByIp(ip));
        }

        public async Task<List<string>> GetTaskNames(string cpuName)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetTaskNames name={cpuName}");

            var result = await Task.Run(() => _application.GetTaskNames(cpuName));

            return result;
        }

        public async Task<List<string>> GetTaskNamesByIp(string ipAddress)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetTaskNames ipAddress={ipAddress}");

            var result = await Task.Run(() => _application.GetTaskNamesByIp(ipAddress));

            return result;
        }

        public async Task<CpuDetails> GetCpuDetailsByName(string cpuName)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetCpuDetailsByName name={cpuName}");

            var result = await Task.Run(() => _application.GetCpuDetailsByName(cpuName));

            return result;
        }

        public async Task<CpuDetails> GetCpuDetailsByIp(string ipAddress)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetCpuDetailsByIp name={ipAddress}");

            var result = await Task.Run(() => _application.GetCpuDetailsByIp(ipAddress));

            return result;
        }

        public async Task<CpuDetails> GetTaskDetailsByName(string cpuName, string taskName)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetTaskDetailsByName name={cpuName}, taskName={taskName}");

            var result = await Task.Run(() => _application.GetTaskDetailsByName(cpuName, taskName));

            return result;
        }

        public async Task<CpuDetails> GetTaskDetailsByIp(string ipAddress, string taskName)
        {
            Trace.TraceInformation($"RequestProcessor Operation=GetTaskDetailsByIp ip={ipAddress}, taskName={taskName}");

            var result = await Task.Run(() => _application.GetTaskDetailsByIp(ipAddress, taskName));

            return result;
        }
    }
}
