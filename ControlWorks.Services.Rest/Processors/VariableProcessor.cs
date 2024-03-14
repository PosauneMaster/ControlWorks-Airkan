using ControlWorks.Services.PVI;
using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.PVI.Task;
using ControlWorks.Services.PVI.Variables;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using VariableDetails = ControlWorks.Services.PVI.Variables.VariableDetails;

namespace ControlWorks.Services.Rest.Processors
{
    public interface IVariableProcessor
    {
        Task<List<VariableDetailRespose>> GetAll();
        Task<VariableResponse> FindByCpu(string name);
        Task<VariableResponse> FindByIp(string ip);
        Task<VariableResponse> FindActiveByCpuName(string name);
        Task AddVariableByCpuName(string cpuName, string taskName, string variableName);
        Task AddVariableByIpAddress(string ip, string taskName, string variableName);
        Task DeleteVariable(string cpuName, string taskName, string variableName);
        Task<VariableDetailRespose> Copy(string source, string destination);
        Task AddMaster(string[] variables);
        Task AddCpuRange(string[] cpus);
        Task RemoveCpuRange(string[] cpus);
        Task<List<VariableDetails>> GetVariableDetails(string cpuName);
        Task<List<VariableConfiguration>> GetVariableConfiguration(string cpuName);

    }


    public class VariableProcessor : IVariableProcessor
    {
        private IPviApplication _application;

        public VariableProcessor() { }

        public VariableProcessor(IPviApplication application)
        {
            _application = application;
        }

        public async Task<List<VariableConfiguration>> GetVariableConfiguration(string cpuName)
        {
            var result = await Task.Run(() => _application.GetVariableConfiguration(cpuName));
            return result;
        }

        public async Task<List<VariableDetails>> GetVariableDetails(string cpuName)
        {
            var result = await Task.Run(() => _application.GetVariableDetails(cpuName));
            return result;
        }

        public async Task<List<VariableMapping>> FindVariable(string name)
        {
            return await Task.Run(() => _application.FindVariable(name));
        }


        public async Task AddVariableByCpuName(string cpuName, string taskName, string variableName)
        {
            await Task.Run(() =>
            {
                _application.AddVariableByCpuName(cpuName, taskName, variableName);
            });
           
        }

        public async Task AddVariableByIpAddress(string ip, string taskName, string variableName)
        {
            await Task.Run(() =>
            {
                _application.AddVariableByIp(ip, taskName, variableName);
            });
        }

        public Task AddCpuRange(string[] cpus)
        {
            throw new NotImplementedException();
        }

        public Task AddMaster(string[] variables)
        {
            throw new NotImplementedException();
        }

        public Task<VariableDetailRespose> Copy(string source, string destination)
        {
            throw new NotImplementedException();
        }

        public Task<VariableResponse> FindByCpu(string name)
        {
            var result = Task.Run(() => _application.ReadAllVariablesByCpu(name));
            return result;
        }

        public Task<VariableResponse> FindByIp(string ip)
        {
            var result = Task.Run(() => _application.ReadAllVariablesByIp(ip));
            return result;
        }

        public Task<VariableResponse> FindActiveByCpuName(string name)
        {
            return Task.Run(() => _application.ReadActiveVariables(name));
        }

        public Task<List<VariableDetailRespose>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task DeleteVariable(string ipAddress, string taskName, string variableName)
        {
            return Task.Run(() => _application.DeleteVariable(ipAddress, taskName, variableName));
        }

        public Task RemoveCpuRange(string[] cpus)
        {
            throw new NotImplementedException();
        }
    }
}
