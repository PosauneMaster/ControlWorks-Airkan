using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Impl;
using ControlWorks.Services.PVI.Models;
using ControlWorks.Services.PVI.Panel;
using ControlWorks.Services.PVI.Task;
using ControlWorks.Services.PVI.Variables;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

using VariableDetails = ControlWorks.Services.PVI.Variables.VariableDetails;

namespace ControlWorks.Services.PVI.Pvi
{
    public interface IPviApplication
    {
        void Connect();
        void Disconnect();
        ServiceDetail ServiceDetails();
        bool GetIsConnected();
        bool GetHasError();
        void AddCpu(CpuInfo info);
        CpuDetailResponse GetCpuByName(string name);
        CpuDetailResponse GetCpuByIp(string ip);
        void DeleteCpuByName(string name);
        void DeleteCpuByIp(string ip);
        CpuDetailResponse[] GetCpuData();
        void AddVariableByCpuName(string cpuName, string taskName, string variableName);
        void AddVariableByIp(string ip, string taskName, string variableName);
        void DeleteVariable(string ipAddress, string taskName, string variableName);
        VariableResponse ReadVariables(string cpuName, IList<string> variableNames);
        VariableResponse ReadAllVariablesByCpu(string cpuName);
        VariableResponse ReadAllVariablesByIp(string cpuName);
        VariableResponse ReadActiveVariables(string cpuName);
        List<VariableDetails> GetVariableDetails(string cpuName);
        List<string> GetTaskNames(string cpuName);
        List<string> GetTaskNamesByIp(string ipAddress);
        CpuDetails GetCpuDetailsByName(string cpuName);
        CpuDetails GetCpuDetailsByIp(string cpuName);
        CpuDetails GetTaskDetailsByName(string cpuName, string taskName);
        CpuDetails GetTaskDetailsByIp(string ip, string taskName);
        IEventNotifier GetEventNotifier();
        CommandStatus SendCommand(string cpuName, string commandName, string commandData);
        List<VariableMapping> FindVariable(string name);
        List<VariableConfiguration> GetVariableConfiguration(string cpuName);
        List<AirkanVariable> GetAirkanVariables(string cpuName);


    }
    public class PviAplication : IPviApplication
    {

        private PviContext _pviContext;
        private ICpuManager _cpuManager;
        private IVariableManager _variableManager;
        private readonly TaskLoader _taskLoader;
        private readonly CpuDataService _cpuDataService;
        private readonly AirkanVariableService _airkanVariableService;


        private readonly IEventNotifier _eventNotifier;
        private readonly IServiceWrapper _serviceWrapper;

        public PviAplication()
        {
            Trace.TraceInformation("Initializing PviAplication");
            _eventNotifier = new EventNotifier();
            _serviceWrapper = new ServiceWrapper(_eventNotifier);

            _cpuDataService = new CpuDataService();
            _taskLoader = new TaskLoader(_cpuDataService, _eventNotifier);
            _airkanVariableService = new AirkanVariableService(_eventNotifier);


        }

        #region public interface

        public void Connect()
        {
            Initialize();

            _eventNotifier.PviServiceConnected += _eventNotifier_PviServiceConnected;
            _eventNotifier.PviServiceDisconnected += _eventNotifier_PviServiceDisconnected;
            _eventNotifier.PviServiceError += _eventNotifier_PviServiceError;
            _eventNotifier.CpuConnected += _eventNotifier_CpuConnected;
            _eventNotifier.CpuDisconnected += _eventNotifier_CpuDisconnected;
            _eventNotifier.CpuError += _eventNotifier_CpuError;
            _eventNotifier.VariableConnected += _eventNotifier_VariableConnected;
            _eventNotifier.VariableError += _eventNotifier_VariableError;
            _eventNotifier.VariableValueChanged += _eventNotifier_VariableValueChanged;
            _eventNotifier.CpuManangerInitialized += _eventNotifier_CpuManangerInitialized;
            _eventNotifier.TasksLoaded += _eventNotifier_TasksLoaded;


            _pviContext = new PviContext(_serviceWrapper);
            Application.Run(_pviContext);
        }

        private void Initialize()
        {
            if (!Directory.Exists(ConfigurationProvider.BaseDirectory))
            {
                Directory.CreateDirectory(ConfigurationProvider.BaseDirectory);
            }

            var dbPath = Path.Combine(ConfigurationProvider.BaseDirectory, "Database");
            if(!Directory.Exists(dbPath))
            {
                Directory.CreateDirectory(dbPath);
            }
        }

        public ServiceDetail ServiceDetails()
        {
            return _serviceWrapper.ServiceDetails();
        }

        public bool GetIsConnected()
        {
            return _serviceWrapper.IsConnected;
        }

        public bool GetHasError()
        {
            return _serviceWrapper.HasError;
        }

        private void _eventNotifier_CpuManangerInitialized(object sender, System.EventArgs e)
        {
            _variableManager.ConnectVariables(_cpuManager.GetCpuNames());
        }

        public void Disconnect()
        {
            if (_serviceWrapper != null)
            {
                _serviceWrapper.DisconnectPviService();
            }
            if (_pviContext != null)
            {
                _pviContext.Dispose();
            }
        }

        public void AddCpu(CpuInfo info)
        {
            Trace.TraceInformation($"Adding cpu {info.IpAddress}");
            if (_cpuManager == null)
            {
                Trace.TraceError("PviApplication. AddCpu:155 __cpuManager is null");
            }
            else
            {
                
                Trace.TraceInformation("__cpuManager is not null, adding");
            }

            _cpuManager.Add(info);
        }

        public CpuDetailResponse GetCpuByName(string name)
        {
            return _cpuManager.FindCpuByName(name);
        }

        public CpuDetailResponse GetCpuByIp(string ip)
        {
            return _cpuManager.FindCpuByIp(ip);
        }

        public void DeleteCpuByName(string name)
        {
            _cpuManager.DisconnectCpuByName(name);
        }

        public void DeleteCpuByIp(string ip)
        {
            _cpuManager.DisconnectCpuByIp(ip);
        }

        public CpuDetailResponse[] GetCpuData()
        {
            return _cpuManager.GetCpus();
        }

        public List<string> GetTaskNames(string cpuName)
        {
            return _cpuDataService.GetTaskNames(cpuName);
        }

        public List<string> GetTaskNamesByIp(string ipAddress)
        {
            var cpu = _cpuManager.FindCpuByIp(ipAddress);

            if (cpu == null)
            {
                return null;
            }

            return _cpuDataService.GetTaskNames(cpu.Name);
        }

        public CpuDetails GetCpuDetailsByName(string cpuName)
        {
            var cpu = _cpuManager.FindCpuByName(cpuName);

            if (cpu == null)
            {
                return null;
            }
            return _cpuDataService.GetCpuDetails(cpu.Name, cpu.IpAddress);
        }

        public CpuDetails GetCpuDetailsByIp(string ipAddress)
        {
            var cpu = _cpuManager.FindCpuByIp(ipAddress);

            if (cpu == null)
            {
                return null;
            }
            return _cpuDataService.GetCpuDetails(cpu.Name, cpu.IpAddress);
        }

        public CpuDetails GetTaskDetailsByName(string cpuName, string taskName)
        {
            var cpu = _cpuManager.FindCpuByName(cpuName);

            if (cpu == null)
            {
                return null;
            }
            return _cpuDataService.GetTaskDetails(cpu.Name, cpu.IpAddress, taskName);
        }

        public CpuDetails GetTaskDetailsByIp(string ipAddress, string taskName)
        {
            var cpu = _cpuManager.FindCpuByIp(ipAddress);

            if (cpu == null)
            {
                return null;
            }
            return _cpuDataService.GetTaskDetails(cpu.Name, cpu.IpAddress, taskName);
        }

        public VariableResponse ReadVariables(string cpuName, IList<string> variableNames)
        {
            return _variableManager.GetVariables(cpuName, variableNames);
        }

        public VariableResponse ReadAllVariablesByCpu(string cpuName)
        {
            return _variableManager.GetAllVariables(cpuName);
        }

        public VariableResponse ReadAllVariablesByIp(string ip)
        {
            var cpu = _cpuManager.FindCpuByIp(ip);

            if (cpu == null)
            {
                return null;
            }

            return ReadAllVariablesByCpu(cpu.Name);
        }

        public List<VariableDetails> GetVariableDetails(string cpuName)
        {
            return _variableManager.GetVariableDetails(cpuName);
        }

        public VariableResponse ReadActiveVariables(string cpuName)
        {
            return _variableManager.GetActiveVariables(cpuName);
        }

        public void AddVariableByCpuName(string cpuName, string taskName, string variableName)
        {
            _variableManager.AddVariable(cpuName, taskName, variableName);
        }

        public void AddVariableByIp(string ip, string taskName, string variableName)
        {
            var cpu = _cpuManager.FindCpuByIp(ip);

            if (cpu != null)
            {
                _variableManager.AddVariable(cpu.Name, taskName, variableName);
            }
        }

        public void DeleteVariable(string ipAddress, string taskName, string variableName)
        {
            var cpu = _cpuManager.FindCpuByIp(ipAddress);

            _variableManager.DeleteVariable(cpu.Name, taskName, variableName);
        }

        #endregion


        private void _eventNotifier_PviServiceError(object sender, PviApplicationEventArgs e)
        {
        }

        private void _eventNotifier_PviServiceDisconnected(object sender, PviApplicationEventArgs e)
        {
            System.Threading.Thread.Sleep(TimeSpan.FromSeconds(10));
            _serviceWrapper.ReConnectPviService();
        }

        private void _eventNotifier_PviServiceConnected(object sender, PviApplicationEventArgs e)
        {

            _cpuManager = e.CpuManager;

            _variableManager = e.VariableManager;

            Trace.TraceInformation(e.Message);

            if (_cpuManager == null)
            {
                Trace.TraceError($"_eventNotifier_PviServiceConnected CpuManager is null");
            }
            else
            {
                _cpuManager.LoadCpus();
            }
        }

        private void _eventNotifier_VariableValueChanged(object sender, PviApplicationEventArgs e)
        {
            var variable = sender as Variable;

            Cpu cpu = null;

            if (variable.Parent is Cpu parent)
            {
                cpu = parent;
            }
            if (variable.Parent is BR.AN.PviServices.Task task)
            {
                cpu = task.Parent as Cpu;
            }

            if (ConfigurationProvider.VerboseVariableLogging)
            {
                Trace.TraceInformation(e.Message);
            }
        }

        private void _eventNotifier_VariableError(object sender, PviApplicationEventArgs e)
        {
            Trace.TraceInformation(e.Message);

        }

        private void _eventNotifier_VariableConnected(object sender, PviApplicationEventArgs e)
        {
            if (sender is Variable variable)
            {
                if (variable.Name == "Status")
                {
                    if (variable.Parent is Cpu cpu)
                    {
                        var heartBeatService = new HeartBeatService(cpu);
                        System.Threading.Tasks.Task.Run(() => heartBeatService.HeartBeat());
                    }
                }
            }

            Trace.TraceInformation(e.Message);

        }

        private void _eventNotifier_CpuError(object sender, PviApplicationEventArgs e)
        {
            Trace.TraceInformation(e.Message);
        }

        private void _eventNotifier_CpuDisconnected(object sender, PviApplicationEventArgs e)
        {
        }

        private void _eventNotifier_CpuConnected(object sender, CpuConnectionArgs e)
        {
            if (e.Cpu != null && e.Cpu.IsConnected && !e.Cpu.HasError)
            {
                Trace.TraceInformation(e.Message);

                System.Threading.Tasks.Task.Run(() => _taskLoader.LoadTasks(e.Cpu));
            }
        }

        private void _eventNotifier_TasksLoaded(object sender, TaskLoaderEventArgs e)
        {
            _variableManager.ConnectVariable(e.CpuName);
        }

        public IEventNotifier GetEventNotifier()
        {
            return _eventNotifier;
        }

        private Cpu FindCpu(string cpuName)
        {
            var service = _serviceWrapper.GetService();
            var name = cpuName;

            if (String.IsNullOrEmpty(cpuName))
            {
                foreach (var key in service.Cpus.Keys)
                {
                    name = key.ToString();
                }
            }

            return service?.Cpus?[name];
        }

        public CommandStatus SendCommand(string cpuName, string commandName, string commandData)
        {
            var cpu = FindCpu(cpuName);

            if (cpu != null)
            {
                return _airkanVariableService.ProcessCommand(cpu, commandName, commandData);
            }

            return new CommandStatus(1, $"Unable to locate Cpu {cpuName}");
        }

        public List<AirkanVariable> GetAirkanVariables(string cpuName)
        {
            var cpu = FindCpu(cpuName);

            if (cpu != null)
            {
                return _airkanVariableService.GetAirkanVariables(cpu);
            }

            Trace.TraceError($"PVIApplication.GetAirkanVariables. Unable to locate Cpu {cpuName}");
            return null;
        }

        public List<VariableMapping>FindVariable(string name)
        {
            var cpu = FindCpu("Airkan");
            var variables = cpu.Variables;
            var dataTransfer = variables["DataTransfer"];
            var connected = dataTransfer.IsConnected;

            var sb = new StringBuilder();
            foreach ( Variable member in dataTransfer.Members )
            {
                if (member.Members == null)
                {
                    sb.AppendLine($"{member.Name}");
                }
                else
                {
                    foreach (Variable submember in member.Members)
                    {
                        sb.AppendLine($"{member.Name}.{submember.Name}");
                    }
                }

                sb.AppendLine();

            }

            File.WriteAllText(@"C:\ControlWorks\Airkan\VariableNames.txt", sb.ToString());


            return _cpuDataService.FindVariable(name);
        }

        public List<VariableConfiguration> GetVariableConfiguration(string cpuName)
        {
            return null;
            //return _variableManager.GetVariableConfiguration(cpuName);
        }
    }
}
