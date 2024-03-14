namespace ControlWorks.Services.PVI.Variables
{
    public class VariableConfiguration
    {
        public string VariableName { get; set; }
        public string TaskName { get; set; }
        public string CpuName { get; set; }

        public VariableConfiguration() { }
        public VariableConfiguration(string variableName, string taskName, string cpuName)
        {
            VariableName = variableName;
            TaskName = taskName;
            CpuName = cpuName;
        }
    }
}
