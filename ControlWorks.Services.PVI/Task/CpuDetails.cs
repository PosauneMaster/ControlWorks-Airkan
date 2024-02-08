using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace ControlWorks.Services.PVI.Task
{
    public class CpuDetails
    {
        private readonly List<VariableDetails> _variableDetails;

        public string Name { get; set; }
        public string IpAddress { get; set; }
        public VariableDetails[] VariableDetails => _variableDetails.ToArray();

        public CpuDetails()
        {
            _variableDetails = new List<VariableDetails>();
        }

        public void AddVariableDetail(string name, string task, string type)
        {
            _variableDetails.Add(new VariableDetails(name, task, type));
        }

        public string ToJson()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }
    }

    public class VariableDetails
    {
        public string Name { get; set; }
        public string Task { get; set; }
        public String Type { get; set; }

        public VariableDetails()
        {
        }

        public VariableDetails(string name, string task, string type)
        {
            Name = name;
            Task = task;
            Type = type;
        }
    }
}
