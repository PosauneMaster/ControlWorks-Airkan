﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ControlWorks.Common;
using ControlWorks.Services.PVI.Panel;
using LiteDB;
using Newtonsoft.Json;

namespace ControlWorks.Services.PVI.Variables
{
    public interface IVariableInfoCollection
    {
        List<VariableInfo> GetAll();
        List<VariableInfo> FindByCpu(string name);
        bool Add(string cpuName, string taskName, string variableName);
        void DeleteVariable(string cpuName, string taskName, string variableName);
    }

    public class VariableInfo
    {
        public int Id { get; set; }
        public string CpuName { get; set; }
        public TaskInfo TaskInfo { get; set; }

        public VariableInfo()
        {
        }

        public VariableInfo(string cpuName, string taskName)
        {
            CpuName = cpuName;
            TaskInfo = new TaskInfo(taskName);
        }
    }

    public class TaskInfo
    {
        public string TaskName { get; set; }
        public List<string> Variables { get; set; }
        public TaskInfo()
        {
            TaskName = "Global";
            Variables = new List<string>();
        }

        public TaskInfo(string taskName)
        {
            TaskName = taskName;
            Variables = new List<string>();
        }

        public bool VariableExists(string variableName)
        {
            return Variables.Exists(v => v == variableName);
        }

        public bool Add(string variableName)
        {
            if (!VariableExists(variableName))
            {
                Variables.Add(variableName);
                return true;
            }

            return false;
        }
        
        public void Remove(string variableName)
        {
            if (VariableExists(variableName))
            {
                Variables.Remove(variableName);
            }
        }
    }


    public class VariableInfoCollection : IVariableInfoCollection
    {
        private static readonly AutoResetEvent VariableInfoEvent = new AutoResetEvent(true);
        private readonly string _variableSettingsName = "VariableSettings";

        public VariableInfoCollection()
        {
        }

        public List<VariableInfo> GetAll()
        {
            VariableInfoEvent.WaitOne(1000);
            var list = new List<VariableInfo>();
            var settingsDb = ConfigurationProvider.ControlworksSettingsDbConnectionString;

            try
            {
                using (var db = new LiteDatabase(settingsDb))
                {
                    var variableInfoCol = db.GetCollection<VariableInfo>(_variableSettingsName);

                    if (variableInfoCol != null)
                    {
                        list.AddRange(variableInfoCol.FindAll());
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"VariableInfoCollection.GetAll. {e.Message} \r\n", e);
            }
            finally
            {
                VariableInfoEvent.Set();
            }

            return list;
        }


        public List<VariableInfo> FindByCpu(string name)
        {
            return GetAll()
                .Where(i => i.CpuName == name)
                .ToList();
        }

        public bool Add(string cpuName, string taskName, string variableName)
        {
            bool inserted = false;
            VariableInfoEvent.WaitOne(1000);

            try
            {
                var settingsDb = ConfigurationProvider.ControlworksSettingsDbConnectionString;
                using (var db = new LiteDatabase(settingsDb))
                {
                    var variableInfoCol = db.GetCollection<VariableInfo>(_variableSettingsName);
                    var currentVariable = variableInfoCol
                        .Find(v => v.CpuName == cpuName && v.TaskInfo.TaskName == taskName).FirstOrDefault();

                    VariableInfo info = currentVariable;

                    if (currentVariable == null)
                    {
                        info = new VariableInfo(cpuName, taskName);
                    }

                    inserted = info.TaskInfo.Add(variableName);
                    return variableInfoCol.Upsert(info);
                }

            }
            catch (Exception e)
            {
                Trace.TraceError($"VariableInfoCollection.Add. {e.Message} \r\n", e);
            }
            finally
            {
                VariableInfoEvent.Set();
            }

            return inserted;

        }

        public void DeleteVariable(string cpuName, string taskName, string variableName)
        {
            VariableInfoEvent.WaitOne(1000);

            try
            {
                var settingsDb = ConfigurationProvider.ControlworksSettingsDbConnectionString;
                using (var db = new LiteDatabase(settingsDb))
                {
                    var variableInfoCol = db.GetCollection<VariableInfo>(_variableSettingsName);
                    var currentVariable = variableInfoCol
                        .Find(v => v.CpuName == cpuName && v.TaskInfo.TaskName == taskName).FirstOrDefault();
                    if (currentVariable != null)
                    {
                        currentVariable.TaskInfo.Remove(variableName);
                        variableInfoCol.Update(currentVariable);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError($"VariableInfoCollection.Remove. {e.Message} \r\n", e);
            }
            finally
            {
                VariableInfoEvent.Set();
            }
        }
    }

}
