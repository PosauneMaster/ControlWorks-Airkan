using ControlWorks.Services.PVI.Impl;

using System.Collections.Generic;

namespace ControlWorks.Services.PVI.Variables
{
    public interface IVariableManager
    {
        void ConnectVariables(IList<string> cpuNames);
        void ConnectVariable(string cpuName);
        VariableResponse GetAllVariables(string cpuName);
        VariableResponse GetVariables(string cpuName, IList<string> variableNames);
        VariableResponse GetActiveVariables(string cpuName);
        void AddVariable(string cpuName, string taskName, string variableName);
        void DeleteVariable(string cpuName, string taskName, string variableName);
        List<VariableDetails> GetVariableDetails(string cpuName);


    }
    public class VariableManager : IVariableManager
    {
        private readonly IVariableWrapper _variableWrapper;
        private readonly IVariableInfoCollection _variableInfoCollection;


        public VariableManager(IVariableWrapper variableWrapper, IVariableInfoCollection variableInfoCollection)
        {
            _variableWrapper = variableWrapper;
            _variableInfoCollection = variableInfoCollection;
        }

        public VariableResponse GetAllVariables(string cpuName)
        {
            var info = _variableInfoCollection.FindByCpu(cpuName);
            return _variableWrapper.ReadVariables(cpuName, info);
        }

        public VariableResponse GetActiveVariables(string cpuName)
        {
            var infoList = _variableInfoCollection.FindByCpu(cpuName);
            return _variableWrapper.ReadActiveVariables(infoList, cpuName);
        }

        public VariableResponse GetVariables(string cpuName, IList<string> variableNames)
        {
            var info = _variableInfoCollection.FindByCpu(cpuName);
            return null;
        }

        public List<VariableDetails> GetVariableDetails(string cpuName)
        {
            var info = _variableInfoCollection.FindByCpu(cpuName);
            return _variableWrapper.GetVariableDetails(info);
        }

        public void ConnectVariables(IList<string> cpuNames)
        {
            foreach (var cpuName in cpuNames)
            {
                ConnectVariable(cpuName);
            }
        }

        public void ConnectVariable(string cpuName)
        {
            var info = _variableInfoCollection.FindByCpu(cpuName);
            if (info != null)
            {
                _variableWrapper.ConnectVariables(cpuName, info);
            }
        }

        public void AddVariable(string cpuName, string taskName,  string variableName)
        {
            _variableInfoCollection.Add(cpuName, taskName, variableName);

            var variableInfo = new VariableInfo(cpuName, taskName);
            variableInfo.TaskInfo.Variables.Add(variableName);

            _variableWrapper.ConnectVariables(cpuName, new [] { variableInfo });
        }

        public void DeleteVariable(string cpuName, string taskName, string variableName)
        {
            _variableInfoCollection.DeleteVariable(cpuName, taskName, variableName);
        }
    }
}
