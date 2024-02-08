namespace ControlWorks.Services.PVI.Variables
{
    public class VariableDetails
    {
        public string Name { get; set; }
        public string CpuName { get; set; }
        public string TaskName { get; set; }
        public string Value { get; set; }
        public bool IsConnected { get; set; }
        public bool HasError { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorText { get; set; }
    }
}
