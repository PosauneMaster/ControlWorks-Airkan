using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Task
{
    public enum VariableScope
    {
        Global,
        Task
    }

    public class CpuDataService
    {
        private readonly List<DataSet> _variableDataSetList;
        private readonly Dictionary<string, List<string>> _taskNames;

        public CpuDataService()
        {
            _variableDataSetList = new List<DataSet>();
            _taskNames = new Dictionary<string, List<string>>();
        }

        private DataSet GetDataSet(string dataSetName)
        {
            var ds = _variableDataSetList.FirstOrDefault(d => d.DataSetName == dataSetName);
            if (ds == null)
            {
                ds = new DataSet
                {
                    DataSetName = dataSetName
                };
                _variableDataSetList.Add(ds);
            }

            return ds;
        }

        private DataTable GetVariableDataTable(string cpuName, VariableScope variableScope, string taskName = null)
        {
            var ds = GetDataSet(cpuName);

            if (variableScope == VariableScope.Global)
            {
                if (!ds.Tables.Contains("Global"))
                {
                    var globalTable = CreateVariableTable("Global");
                    ds.Tables.Add(globalTable);
                }

                return ds.Tables["Global"];
            }

            if (String.IsNullOrEmpty(taskName))
            {
                throw new ArgumentException("taskName cannot be null");
            }


            if (!ds.Tables.Contains(taskName))
            {
                var taskTable = CreateTaskTable(taskName);
                ds.Tables.Add(taskTable);
            }

            return ds.Tables[taskName];
        }

        public void AddVariable(string cpuName, VariableScope variableScope, string variableName, string dataType, string taskName = null)
        {
            var dt = GetVariableDataTable(cpuName, variableScope, taskName);
            dt.Rows.Add(variableName, dataType);
        }

        public DataTable GetDataTable(string cpuName, VariableScope variableScope, string taskName = null)
        {
            var ds = GetDataSet(cpuName);
            if (variableScope == VariableScope.Global && ds.Tables.Contains("Global"))
            {
                return ds.Tables["Global"];
            }

            if (ds.Tables.Contains(taskName))
            {
                return ds.Tables[taskName];
            }

            return null;
        }

        public List<string> GetTaskNames(string cpuName)
        {
            if (_taskNames.ContainsKey(cpuName))
            {
                return _taskNames[cpuName];
            }

            var list = new List<string>();
            var ds = _variableDataSetList.FirstOrDefault(d => d.DataSetName == cpuName);

            if (ds != null)
            {
                foreach (DataTable table in ds.Tables)
                {
                    if (table.TableName != "Global")
                    {
                        list.Add(table.TableName);
                    }
                }
            }

            _taskNames.Add(cpuName, list);

            return list;
        }

        private DataTable CreateVariableTable(string tableName)
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("CpuVariableName", typeof(string));
            dt.Columns.Add("CpuVariableDataType", typeof(string));

            return dt;
        }

        private DataTable CreateTaskTable(string tableName)
        {
            var dt = new DataTable(tableName);
            dt.Columns.Add("TaskVariableName", typeof(string));
            dt.Columns.Add("TaskVariableDataType", typeof(string));

            return dt;

        }

        public CpuDetails GetCpuDetails(string cpuName, string ipAddress)
        {
            var cpuDetails = new CpuDetails();

            var taskNames = GetTaskNames(cpuName);
            var dtCpu = GetDataTable(cpuName, VariableScope.Global);
            if(dtCpu == null)
            {
                return null;
            }

            cpuDetails.Name = cpuName;
            cpuDetails.IpAddress = ipAddress;

            foreach (DataRow row in dtCpu.Rows)
            {
                var variableName = row.Field<string>("CpuVariableName");
                var dataTypeName = row.Field<string>("CpuVariableDataType");

                cpuDetails.AddVariableDetail(variableName, "Global", dataTypeName);
            }

            foreach (var taskName in taskNames)
            {
                var dtTask = GetDataTable(cpuName, VariableScope.Task, taskName);

                foreach (DataRow row in dtTask.Rows)
                {
                    var variableName = row.Field<string>("TaskVariableName");
                    var dataTypeName = row.Field<string>("TaskVariableDataType");

                    cpuDetails.AddVariableDetail(variableName, taskName, dataTypeName);
                }
            }

            return cpuDetails;
        }

        public List<VariableMapping> FindVariable(string name)
        {
            var list = new List<VariableMapping>();

            foreach (var dataSet in _variableDataSetList)
            {
                var cpuName = dataSet.DataSetName;
                foreach (DataTable table in dataSet.Tables)
                {
                    var taskName = table.TableName;

                    foreach (DataRow row in table.Rows)
                    {
                        if (row[0] != null && row[1] != null)
                        {
                            if (row[0].ToString() == name)
                            {
                                var vi = new VariableMapping(cpuName, taskName, row[0].ToString(), row[1].ToString());
                                list.Add(vi);
                            }
                        }
                    }

                }
            }

            return list;
        }

        public CpuDetails GetTaskDetails(string cpuName, string ipAddress, string taskName)
        {
            var cpuDetails = new CpuDetails();
            cpuDetails.Name = cpuName;
            cpuDetails.IpAddress = ipAddress;

            var variableScope = taskName.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase)
                ? VariableScope.Global
                : VariableScope.Task;

            var dt = GetDataTable(cpuName, variableScope, taskName);

            if (dt == null)
            {
                return cpuDetails;
            }

            if (variableScope == VariableScope.Global)
            {
                foreach (DataRow row in dt.Rows)
                {
                    var variableName = row.Field<string>("CpuVariableName");
                    var dataTypeName = row.Field<string>("CpuVariableDataType");

                    cpuDetails.AddVariableDetail(variableName, "Global", dataTypeName);
                }
            }
            else
            {
                foreach (DataRow row in dt.Rows)
                {
                    var variableName = row.Field<string>("TaskVariableName");
                    var dataTypeName = row.Field<string>("TaskVariableDataType");

                    cpuDetails.AddVariableDetail(variableName, taskName, dataTypeName);
                }
            }

            return cpuDetails;
        }

        public string GetCpuVariableDetails(string cpuName, string ipAddress)
        {
            return GetCpuDetails(cpuName, ipAddress).ToJson();
        }
    }

    public class VariableMapping
    {
        public string CpuName { get; set; }
        public string TaskName { get; set; }
        public string VariableName { get; set; }
        public string DataType { get; set; }

        public VariableMapping()
        {
        }

        public VariableMapping(string cpuName, string taskName, string variableName, string dataType)
        {
            CpuName = cpuName;
            TaskName = taskName;
            VariableName = variableName;
            DataType = dataType;
        }

    }

}

