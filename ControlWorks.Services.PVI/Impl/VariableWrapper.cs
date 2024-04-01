using BR.AN.PviServices;

using ControlWorks.Services.PVI.Variables;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace ControlWorks.Services.PVI.Impl
{
    public interface IVariableWrapper
    {
        void ConnectVariables(string cpuName, IEnumerable<VariableInfo> variables);
        void ConnectVariable(string cpuName, string name);
        VariableResponse ReadVariables(string cpuName, List<VariableInfo> info);
        VariableResponse ReadActiveVariables(List<VariableInfo> infoList, string cpuName);
        void DisconnectVariables(string cpuName, IEnumerable<string> variableNames);
        List<VariableDetails> GetVariableDetails(IEnumerable<VariableInfo> info);

    }

    public class VariableWrapper : IVariableWrapper
    {
        private readonly Service _service;
        private readonly IEventNotifier _eventNotifier;

        public VariableWrapper(Service service, IEventNotifier eventNotifier)
        {
            _service = service;
            _eventNotifier = eventNotifier;
        }

        public List<VariableDetails> GetVariableDetails(IEnumerable<VariableInfo> info)
        {
            var list = new List<VariableDetails>();

            foreach (var variableInfo in info)
            {
                var cpuName = variableInfo.CpuName;
                var taskName = variableInfo.TaskInfo.TaskName;
                var cpu = _service.Cpus[cpuName];

                foreach (var variableName in variableInfo.TaskInfo.Variables)
                {
                    if (taskName.Equals("Global", StringComparison.OrdinalIgnoreCase))
                    {
                        if (cpu.Variables.ContainsKey(variableName))
                        {
                            var value = ConvertVariableValue(cpu.Variables[variableName].Value);

                            var details = new VariableDetails();
                            details.Name = cpu.Variables[variableName].Name;
                            details.CpuName = cpuName;
                            details.TaskName = taskName;
                            details.Value = value;
                            details.IsConnected = cpu.Variables[variableName].IsConnected;
                            details.HasError = cpu.Variables[variableName].HasError;
                            details.ErrorCode = cpu.Variables[variableName].ErrorCode.ToString();
                            details.ErrorText = cpu.Variables[variableName].ErrorText;

                            list.Add(details);
                        }
                    }
                    else
                    {
                        if (cpu.Tasks[taskName].Variables.ContainsKey(variableName))
                        {
                            var value = ConvertVariableValue(cpu.Tasks[taskName].Variables[variableName].Value);

                            var details = new VariableDetails();
                            details.Name = cpu.Tasks[taskName].Variables[variableName].Name;
                            details.CpuName = cpuName;
                            details.TaskName = taskName;
                            details.Value = value;
                            details.IsConnected = cpu.Tasks[taskName].Variables[variableName].IsConnected;
                            details.HasError = cpu.Tasks[taskName].Variables[variableName].HasError;
                            details.ErrorCode = cpu.Tasks[taskName].Variables[variableName].ErrorCode.ToString();
                            details.ErrorText = cpu.Tasks[taskName].Variables[variableName].ErrorText;

                            list.Add(details);
                        }
                    }
                }

            }

            return list;

        }

        public VariableResponse ReadVariables(string cpuName, List<VariableInfo> variableInfoList)
        {
            var response = new VariableResponse(cpuName);

            if (_service.Cpus.ContainsKey(cpuName))
            {
                var cpu = _service.Cpus[cpuName];

                foreach(VariableInfo info in variableInfoList)
                {
                    var taskName = info.TaskInfo.TaskName;

                    foreach (var variableName in info.TaskInfo.Variables)
                    {
                        if (taskName.Equals("Global", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (cpu.Variables.ContainsKey(variableName))
                            {
                                var value = ConvertVariableValue(cpu.Variables[variableName].Value);
                                response.AddValue(variableName, taskName, value);
                            }
                        }
                        else
                        {
                            if (cpu.Tasks.ContainsKey(taskName))
                            {
                                if (cpu.Tasks[taskName].Variables.ContainsKey(variableName))
                                {
                                    var taskVariable = cpu.Tasks[taskName].Variables[variableName];
                                    var value = ConvertVariableValue(taskVariable.Value);
                                    response.AddValue(variableName, taskName, value);
                                }
                            }
                        }

                    }
                }
            }

            return response;
        }

        public VariableResponse ReadActiveVariables(List<VariableInfo> infoList, string cpuName)
        {
            var response = new VariableResponse(cpuName);

            if (_service.Cpus.ContainsKey(cpuName))
            {
                var cpu = _service.Cpus[cpuName];

                foreach (var info in infoList)
                {
                    foreach (var taskVariable in info.TaskInfo.Variables)
                    {
                        if (info.TaskInfo.TaskName.Equals("Global", StringComparison.OrdinalIgnoreCase))
                        {
                            if (cpu.Variables.ContainsKey(taskVariable))
                            {
                                if (cpu.Variables[taskVariable].IsConnected)
                                {
                                    var value = ConvertVariableValue(cpu.Variables[taskVariable].Value);
                                    response.AddValue(taskVariable, info.TaskInfo.TaskName, value);
                                }
                            }
                        }
                        else
                        {
                            {
                                if (cpu.Tasks.ContainsKey(info.TaskInfo.TaskName))
                                {
                                    if (cpu.Tasks[info.TaskInfo.TaskName].Variables.ContainsKey(taskVariable))
                                    {
                                        if (cpu.Tasks[info.TaskInfo.TaskName].Variables[taskVariable].IsConnected)
                                        {
                                            var value = ConvertVariableValue(cpu.Tasks[info.TaskInfo.TaskName].Variables[taskVariable].Value);
                                            response.AddValue(taskVariable, info.TaskInfo.TaskName, value);
                                        }
                                    }
                                }
                            }
                        }

                    }
                    
                }
            }

            return response;
        }

        public void ConnectVariables(string cpuName, IEnumerable<VariableInfo> variables)
        {
            if (!String.IsNullOrEmpty(cpuName) && variables != null)
            {
                foreach (var v in variables)
                {
                    foreach (var variableName in v.TaskInfo.Variables)
                    {
                        ConnectVariable(cpuName, v.TaskInfo.TaskName, variableName);
                    }
                }
            }
        }

        public void ConnectVariable(string cpuName, string taskName, string variableName)
        {
            if (_service.Cpus.ContainsKey(cpuName))
            {
                var cpu = _service.Cpus[cpuName];
                if (cpu.IsConnected)
                {
                    ConnectVariable(_service.Cpus[cpuName], taskName, variableName);
                }
            }
        }

        private void ConnectVariable(Cpu cpu, string taskName, string variableName)
        {
            if (taskName.Equals("Global", StringComparison.OrdinalIgnoreCase))
            {
                if (cpu.Variables.ContainsKey(variableName) && !cpu.Variables[variableName].IsConnected)
                {
                    var variable = cpu.Variables[variableName];
                    variable.Connected += Variable_Connected;
                    variable.Error += Variable_Error;
                    variable.ValueChanged += Variable_ValueChanged;
                    variable.Active = true;
                    variable.Connect();
                }


                if (!cpu.Variables.ContainsKey(variableName) && !String.IsNullOrEmpty(variableName))
                {
                    var variable = new Variable(cpu, variableName)
                    {
                        UserTag = variableName,
                        UserData = cpu.UserData
                    };
                    variable.Connected += Variable_Connected;
                    variable.Error += Variable_Error;
                    variable.ValueChanged += Variable_ValueChanged;
                    variable.Active = true;
                    variable.Connect();
                }
            }

            if (cpu.Tasks.ContainsKey(taskName))
            {
                if (cpu.Tasks[taskName].Variables.ContainsKey(variableName))
                {
                    var taskVariable = cpu.Tasks[taskName].Variables[variableName];

                    if (!taskVariable.IsConnected)
                    {

                        taskVariable.Connected += Variable_Connected;
                        taskVariable.Error += Variable_Error;
                        taskVariable.ValueChanged += Variable_ValueChanged;
                        taskVariable.Active = true;
                        taskVariable.Connect();
                    }
                }
            }

        }

        private string ConvertVariableValue(Value v)
        {
            string value = null;

            if (v == null)
            {
                return String.Empty;
            }

            try
            {
                var iceDataType = v.IECDataType;

                switch (v.IECDataType)
                {

                    case IECDataTypes.BOOL:
                        if (v.IsOfTypeArray && v.ArrayLength > 1)
                        {
                            value = v[0].ToBoolean(CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            value = v.ToBoolean(CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture);
                        }
                        break;

                    case IECDataTypes.REAL:
                        value = v.ToDouble(CultureInfo.CurrentCulture).ToString("G", CultureInfo.CurrentCulture);
                        break;

                    case IECDataTypes.DATE:
                    case IECDataTypes.DATE_AND_TIME:
                        value = v.ToDateTime(CultureInfo.CurrentCulture).ToString("o", CultureInfo.CurrentCulture);
                        break;
                    case IECDataTypes.STRUCT:

                        if (v.Parent.Members != null)
                        {
                            var dict = new Dictionary<string, string>();

                            foreach (var k in v.Parent.Members.Keys)
                            {
                                var index = (int)k;
                                var name = ((Variable)v.Parent.Members[k]).Name;
                                var val = ((Variable)v.Parent.Members[k]).Value.ToString();
                                dict.Add(name, val);
                            }
                            value = JsonConvert.SerializeObject(dict);
                        }
                        break;

                    default:
                        value = v.ToIECString();
                        break;
                }
            }
            catch (System.Exception e)
            {
                Trace.TraceError(e.Message, e);
                throw;
            }
            return value ?? String.Empty;
        }


        private void Variable_ValueChanged(object sender, VariableEventArgs e)
        {
            if (!(sender is Variable variable))
            {
                return;
            }

            if (variable.Parent is Cpu cpu)
            {
                if (variable.Value is null)
                {
                    return;
                }
                var data = new VariableData()
                {
                    CpuName = cpu.Name,
                    TaskName = "Global",
                    IpAddress = cpu.Connection.TcpIp.DestinationIpAddress,
                    DataType = Enum.GetName(typeof(IECDataTypes), variable.Value.IECDataType),
                    VariableName = e.Name,
                    Value = ConvertVariableValue(variable.Value)
                };
                _eventNotifier.OnVariableValueChanged(sender, new PviApplicationEventArgs() { Message = data.ToJson() });
            }

            if (variable.Parent is BR.AN.PviServices.Task task)
            {
                if (variable.Value is null)
                {
                    return;
                }
                var taskCpu = task.Parent as Cpu;

                var data = new VariableData()
                {
                    
                    CpuName = taskCpu.Name,
                    TaskName = task.Name,
                    IpAddress = taskCpu.Connection.TcpIp.DestinationIpAddress,
                    DataType = Enum.GetName(typeof(IECDataTypes), variable.Value.IECDataType),
                    VariableName = e.Name,
                    Value = ConvertVariableValue(variable.Value)
                };
                _eventNotifier.OnVariableValueChanged(sender, new PviApplicationEventArgs() { Message = data.ToJson() });
            }
        }

        private void Variable_Error(object sender, PviEventArgs e)
        {
            var cpuName = String.Empty;
            var cpuIp = String.Empty;

            var v = sender as Variable;
            if (v != null)
            {
                var c = v.Parent as Cpu;

                if (c != null)
                {
                    cpuName = c.Name;
                    cpuIp = c.Connection.TcpIp.DestinationIpAddress;
                }

            }

            var pviEventMsg = Utils.FormatPviEventMessage($"ServiceWrapper.Variable_Error Cpu Name={cpuName}, cpuIp={cpuIp} ", e);
            _eventNotifier.OnVariableError(sender, new PviApplicationEventArgs() { Message = pviEventMsg });
        }

        private void Variable_Connected(object sender, PviEventArgs e)
        {
            var cpuName = String.Empty;
            var cpuIp = String.Empty;

            var v = sender as Variable;
            if (v != null)
            {
                var c = v.Parent as Cpu;

                if (c != null)
                {
                    cpuName = c.Name;
                    cpuIp = c.Connection.TcpIp.DestinationIpAddress;
                }

            }

            var pviEventMsg = Utils.FormatPviEventMessage($"ServiceWrapper.Variable_Connected Cpu Name={cpuName}, cpuIp={cpuIp} ", e);
            _eventNotifier.OnVariableConnected(sender, new PviApplicationEventArgs() { Message = pviEventMsg });
        }

        public void DisconnectVariables(string cpuName, IEnumerable<string> variableNames)
        {
            foreach (var v in variableNames)
            {
                if (_service.Cpus.ContainsKey(cpuName))
                {
                    var variables = _service.Cpus[cpuName].Variables;
                    if (variables.ContainsKey(v))
                    {
                        variables[v].Disconnect();
                        variables.Remove(variables[v]);
                    }
                }
            }
        }

        public void ConnectVariable(string cpuName, string name)
        {
            throw new NotImplementedException();
        }
    }
}
