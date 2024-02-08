using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;
using ControlWorks.Services.PVI.Variables.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace ControlWorks.Services.PVI.Variables
{
    public class VerizonVariableService
    {
        private static object _syncLock = new object();
        private readonly Dictionary<string, string> _apiCmdDict;
        private readonly Dictionary<string, string> _apiResponseDict;

        private bool _newOrderRequestRejected = false;
        private bool _newOrderAccepted = false;

        private static AutoResetEvent _addOrderEvent;

        private const string OrderInfoName = "TempOrderInfo";

        public VerizonVariableService()
        {
            _apiCmdDict = new Dictionary<string, string>();
            _apiCmdDict.Add("addOrder", "False");
            _apiCmdDict.Add("deleteAlarmAll", "False");
            _apiCmdDict.Add("deleteAlarm", "False");
            _apiCmdDict.Add("AlarmID", "0");
            _apiCmdDict.Add("deleteUnmatched", "False");
            _apiCmdDict.Add("deleteUnmatchedAll", "False");
            _apiCmdDict.Add("UnmatchedID", "");
            _apiCmdDict.Add("deleteAllOrders", "False");
            _apiCmdDict.Add("deleteOrder", "False");
            _apiCmdDict.Add("deleteOrderID", "");

            _apiResponseDict = new Dictionary<string, string>();
            _apiResponseDict.Add("newOrderRequestRejected", "False");
            _apiResponseDict.Add("newOrderAccepted", "False");
            
            _newOrderRequestRejected = false;
            _newOrderAccepted = false;

            _addOrderEvent = new AutoResetEvent(true);
        }

        public List<AlarmInfo> GetAlarms(Cpu cpu)
        {
            var list = new List<AlarmInfo>();
            if (cpu.Variables.ContainsKey("gMachineAlarmArray"))
            {
                Variable gMachineAlarmArray = cpu.Variables["gMachineAlarmArray"];

                for (int i = 0; i < gMachineAlarmArray.Value.ArrayLength; i++)
                {
                    if (gMachineAlarmArray.Value[i].ToString(CultureInfo.InvariantCulture).ToUpper() == "TRUE")
                    {
                        list.Add(new AlarmInfo(i));
                    }
                }
            }

            return list;
        }

        public CommandStatus ProcessCommand(Cpu cpu, string commandName, string commandData)
        {
            lock (_syncLock)
            {

                switch (commandName)
                {
                    case ("AddOrderInfo"):
                        return SendOrderInfo(cpu, commandData);
                    case ("apiCmd"):
                        ProcessApiCommand(cpu);
                        break;
                    case ("apiResponse"):
                        ProcessApiResponse(cpu, commandData);
                        break;
                    case ("DeleteRecipe"):
                        DeleteOrder(cpu, commandData);
                        break;
                    case ("DeleteAllRecipes"):
                        DeleteAllOrders(cpu);
                        break;
                    case "DeleteAlarm":
                        DeleteAlarm(cpu, commandData);
                        break;
                    case "DeleteAllAlarms":
                        DeleteAllAlarms(cpu);
                        break;

                    default:
                        break;
                }

                return new CommandStatus(0, String.Empty);
            }
        }

        private void ProcessApiCommand(Cpu cpu)
        {
            Variable variable = FindVariable(cpu, "GLOBAL", "apiCmd");

            if (variable != null)
            {
                foreach (Variable member in variable.Members.Values)
                {
                    _apiCmdDict[member.Name] = member.Value.ToString();
                }
            }

        }

        public void DeleteAlarm(Cpu cpu, string alarmId)
        {
            _apiCmdDict["deleteAlarm"] = "True";
            _apiCmdDict["AlarmID"] = alarmId;

            SendData(cpu, "GLOBAL", "apiCmd", _apiCmdDict);
        }
        public void DeleteAllAlarms(Cpu cpu)
        {
            _apiCmdDict["deleteAlarmAll"] = "True";

            SendData(cpu, "GLOBAL", "apiCmd", _apiCmdDict);
        }

        public List<VerizonOrderInfo> GetAllOrders(Cpu cpu)
        {
            var list = new List<VerizonOrderInfo>();
            if (cpu.Variables.ContainsKey("Order_Data"))
            {
                Variable orderData = cpu.Variables["Order_Data"];
                foreach (Variable data in orderData.Members.Values)
                {
                    VerizonOrderInfo orderInfo = new VerizonOrderInfo();

                    orderInfo.SortLocation = data.Members["SortLocation"].Value;
                    orderInfo.ConveyorNumber = data.Members["ConveyorNumber"].Value;
                    orderInfo.OrderNumber = data.Members["OrderNumber"].Value;
                    orderInfo.RecordQuanity = data.Members["RecordQuanity"].Value;
                    orderInfo.TotalItems = data.Members["TotalItems"].Value;
                    orderInfo.Description = data.Members["Description"].Value;

                    Variable sortItems = data.Members["SortItems"];
                    foreach(Variable item in sortItems.Members.Values)
                    {
                        var sortItem = new SortItem();
                        sortItem.SKUnumberAddendum = item.Members["SKUnumberAddendum"].Value;
                        sortItem.SKUnumber = item.Members["SKUnumber"].Value;
                        sortItem.Description = item.Members["Description"].Value;
                        sortItem.RequiredQuanity = item.Members["RequiredQuanity"].Value;
                        sortItem.SortLocation = item.Members["SortLocation"].Value;
                        sortItem.ActualQty = item.Members["ActualQty"].Value;


                        orderInfo.AddItem(sortItem);
                    }

                    list.Add(orderInfo);
                }
            }
            return list;
        }

        private void ProcessApiResponse(Cpu cpu, string commandData)
        {
            try
            {
                var newOrderRequestRejected = false;
                var newOrderAccepted = false;

                if (cpu.Variables.ContainsKey("apiResponse"))
                {
                    Variable response = cpu.Variables["apiResponse"];

                    if (Boolean.TryParse(response.Members["newOrderRequestRejected"].Value, out var isReject))
                    {
                        newOrderRequestRejected = isReject;
                    }

                    if (Boolean.TryParse(response.Members["newOrderAccepted"].Value, out var isAccept))
                    {
                        newOrderAccepted = isAccept;
                    }

                    if (newOrderRequestRejected || newOrderAccepted)
                    {

                        _newOrderRequestRejected = newOrderRequestRejected;
                        _newOrderAccepted = newOrderAccepted;

                        response.Members["newOrderRequestRejected"].Value = "False";
                        response.Members["newOrderAccepted"].Value = "False";
                        response.WriteValue();
                    }


                }
            }
            catch (System.Exception e)
            {
                Trace.TraceError($"VerizonVariableService.ProcessApiResponse. {e.Message}");
                throw;
            }
            finally
            {
                _addOrderEvent.Set();
            }
        }

        public void DeleteOrder(Cpu cpu, string reference)
        {
            _apiCmdDict["deleteOrderID"] = reference;
            _apiCmdDict["deleteOrder"] = "True";

            SendData(cpu, "GLOBAL", "apiCmd", _apiCmdDict);
           
        }

        public void DeleteAllOrders(Cpu cpu)
        {
            _apiCmdDict["deleteAllOrders"] = "True";

            SendData(cpu, "GLOBAL", "apiCmd", _apiCmdDict);

        }

        public List<UnmatchedItem> GetUnmatchedItems(Cpu cpu)
        {
            var list = new List<UnmatchedItem>();
            if (cpu.Variables.ContainsKey("Unmatched"))
            {
                Variable unmatched = cpu.Variables["Unmatched"];
                foreach(Variable v in unmatched.Members.Values)
                {
                    var unmatchedItem = new UnmatchedItem();
                    unmatchedItem.Reference = v.Members["reference"].Value;
                    unmatchedItem.Specification = v.Members["specification"].Value;
                    unmatchedItem.Sku_ID = v.Members["Sku_ID"].Value;

                    list.Add(unmatchedItem);

                }
            }

            return list;
        }

        public StatusInfo GetStatusInfo(Cpu cpu)
        {
            var statusInfo = new StatusInfo();
            try
            {
                if (cpu.Variables.ContainsKey("Status"))
                {
                    Variable status = cpu.Variables["Status"];
                    statusInfo.ActiveAlarms = status.Members["ActiveAlarms"].Value;
                    statusInfo.ActiveOrders = status.Members["ActiveOrders"].Value;
                    statusInfo.BusyBinCount = status.Members["BusyBinCount"].Value;
                    statusInfo.ConveyorUptime = status.Members["ConveyorUptime"].Value;
                    statusInfo.Heartbeat = status.Members["Heartbeat"].Value;
                    statusInfo.IdleBinCount = status.Members["IdleBinCount"].Value;
                    statusInfo.SystemStatus = status.Members["SystemStatus"].Value;
                }
                return statusInfo;

            }
            catch (System.Exception e)
            {
                Trace.TraceError($"VerizonVariableService.GetStatusInfo. {e.Message}");
                throw;
            }
        }

        public CommandStatus SendOrderInfo(Cpu cpu, string orderInfoData)
        {

            if (cpu.Variables.ContainsKey("Heartbeat"))
            {
                Variable orderData = cpu.Variables["Heartbeat"];
            }

            try
            {
                var orderInfo = VerizonOrderInfo.FromJson(orderInfoData);

                if (cpu.Variables.ContainsKey(OrderInfoName))
                {
                    Variable orderInfoVariable = cpu.Variables[OrderInfoName];

                    var sb = new StringBuilder();
                    foreach (Variable info in orderInfoVariable.Members.Values)
                    {
                        sb.AppendLine(info.Name);
                    }

                    orderInfoVariable.Members["OrderNumber"].Value = orderInfo.OrderNumber;
                    orderInfoVariable.Members["Description"].Value = orderInfo.Description;
                    orderInfoVariable.Members["RecordQuanity"].Value = orderInfo.RecordQuanity;
                    Variable sortItems = orderInfoVariable.Members["SortItems"];

                    for (int i = 0; i < orderInfo.Items.Count; i++)
                    {
                        Variable sortItem = sortItems.Members[i];
                        sortItem.Members["SKUnumber"].Value = orderInfo.Items[i].SKUnumber;
                        sortItem.Members["Description"].Value = orderInfo.Items[i].Description;
                        sortItem.Members["RequiredQuanity"].Value = orderInfo.Items[i].ActualQty;
                    }

                    orderInfoVariable.WriteValue();

                    _apiCmdDict["addOrder"] = "True";

                    _newOrderRequestRejected = false;
                    _newOrderAccepted = false;


                    SendData(cpu, "GLOBAL", "apiCmd", _apiCmdDict);

                    _addOrderEvent.WaitOne();
                }

                var status = new CommandStatus(0, String.Empty);

                if (_newOrderRequestRejected)
                {
                    status.StatusCode = 0;
                    status.Message = "Order not accepted";
                }

                _newOrderRequestRejected = false;
                _newOrderAccepted = false;


                return status;

            }
            catch (System.Exception e)
            {
                Trace.TraceError($"VerizonVariableService.SendOrderInfo. {e.Message}");
                throw;
            }
        }


        public void SendData(Cpu cpu, string taskName, string variableName, Dictionary<string, string> dict)
        {
            Variable variable = FindVariable(cpu, taskName, variableName);

            if (variable != null)
            {
                foreach (KeyValuePair<string, string> kvp in dict)
                {
                    variable.Members[kvp.Key].Value = kvp.Value;
                }
                variable.WriteValue(true);
            }
        }

        private Variable FindVariable(Cpu cpu, string taskName, string variableName)
        {
            Variable variable = null;

            if (taskName.Equals("Global", StringComparison.OrdinalIgnoreCase))
            {
                if (cpu.Variables.ContainsKey(variableName))
                {
                    variable = cpu.Variables[variableName];
                }
            }
            else
            {
                if (cpu.Tasks.ContainsKey(taskName))
                {
                    var task = cpu.Tasks[taskName];
                    if (task.Variables.ContainsKey(variableName))
                    {
                        variable = task.Variables[variableName];
                    }
                }
            }

            return variable;
        }

        public void SetVariable(string message, Cpu cpu)
        {
            VariableData variableData = JsonConvert.DeserializeObject<VariableData>(message);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(variableData.Value);

            var variable = FindVariable(cpu, variableData.TaskName, variableData.VariableName);

            if (variable.Name == "apiCmd")
            {
                if (IsChanged(_apiCmdDict, dict))
                {
                    try
                    {
                        SendData(cpu, variableData.TaskName, variableData.VariableName, _apiCmdDict);
                    }
                    catch (System.Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }

            if (variable.Name == "apiResponse")
            {
                if (IsChanged(_apiResponseDict, dict))
                {
                    try
                    {
                        SendData(cpu, variableData.TaskName, variableData.VariableName, _apiResponseDict);

                        variable.Members["newOrderRequestRejected"].Value = false;
                        variable.Members["newOrderAccepted"].Value = false;

                        variable.WriteValue(true);
                    }
                    catch (System.Exception ex)
                    {
                        Trace.TraceError(ex.Message);
                    }
                }
            }
        }

        private bool IsChanged(Dictionary<string, string> dictBase, Dictionary<string, string> dictCompare)
        {
            foreach (KeyValuePair<string, string> kvp in dictCompare)
            {
                if (dictBase.ContainsKey(kvp.Key))
                {
                    if (dictBase[kvp.Key] != kvp.Value)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
