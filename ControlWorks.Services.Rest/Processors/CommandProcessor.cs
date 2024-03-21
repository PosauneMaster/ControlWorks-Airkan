using ControlWorks.Services.PVI.Pvi;

using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Processors
{
    public class CommandProcessor
    {
        private IPviApplication _pviApplication;
        private IVariableProcessor _variableProcessor;
        private PVI.IEventNotifier _eventNotifier;

        public CommandProcessor(IPviApplication pviApplication)
        {
            _pviApplication = pviApplication;
        }

        public CommandProcessor(IPviApplication pviApplication, IVariableProcessor variableProcessor)
        {
            _pviApplication = pviApplication;
            _variableProcessor = variableProcessor;
        }

        public void Initialize()
        {
            _eventNotifier = _pviApplication.GetEventNotifier();

            var ipAddress = "10.1.12.20";
            var taskName = "API_Handsh";
            var variableName = "apiCmd";

            _variableProcessor.AddVariableByIpAddress(ipAddress, taskName, variableName);
            variableName = "apiResponse";
            _variableProcessor.AddVariableByIpAddress(ipAddress, taskName, variableName);

            _eventNotifier.VariableValueChanged += _eventNotifier_VariableValueChanged;
        }

        private void _eventNotifier_VariableValueChanged(object sender, PVI.PviApplicationEventArgs e)
        {
            VariableData data = VariableData.FromJson(e.Message);
        }

        private void SendCommand<T>(string cpuName, string commandName, T data)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            _pviApplication.SendCommand(cpuName, commandName, jsonData);
        }


        private void ProcessVariableChange(string variableName)
        {
            switch (variableName)
            {
                case "apiCmd":
                    break;
                case "apiResponse":
                    break;
                default:
                    break;

            }
        }


    }

    public class VariableData
    {
        public string VariableName { get; set; }
        public string CpuName { get; set; }
        public string TaskName { get; set; }
        public string IpAddress { get; set; }
        public string DataType { get; set; }
        public string Value { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static VariableData FromJson(string data)
        {
            return JsonConvert.DeserializeObject<VariableData>(data);
        }
    }



    //var data = new VariableData()
    //{
    //    CpuName = cpu.Name,
    //    IpAddress = cpu.Connection.TcpIp.DestinationIpAddress,
    //    DataType = Enum.GetName(typeof(IECDataTypes), variable.Value.IECDataType),
    //    VariableName = e.Name,
    //    Value = ConvertVariableValue(variable.Value)
    //};

}
