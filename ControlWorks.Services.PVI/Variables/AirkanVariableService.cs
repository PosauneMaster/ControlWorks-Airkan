using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        const string dataTransferVariable = "DataTransfer";

        private static readonly object SyncLock = new object();
        private readonly IEventNotifier _eventNotifier;
        private readonly FileWatcher _fileWatcher;
        private readonly Cpu _cpu;
        private readonly ConcurrentDictionary<int, string> _inputFiles = new ConcurrentDictionary<int, string>();


        public AirkanVariableService(IEventNotifier eventNotifier, Cpu cpu)
        {
            _cpu = cpu;
            _fileWatcher = new FileWatcher();
            _fileWatcher.FilesChanged += _fileWatcher_FilesChanged;

            _eventNotifier = eventNotifier;
            _eventNotifier.VariableValueChanged += _eventNotifier_VariableValueChanged;
            _eventNotifier.VariableConnected += _eventNotifier_VariableConnected;

       }

        private void _eventNotifier_VariableConnected(object sender, PviApplicationEventArgs e)
        {
            if (sender is Variable v)
            {
                if (v.Name == "FileNames")
                {
                    System.Threading.Tasks.Task.Run(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(5000);
                        SetFiles();
                    });
                }
            }
        }

        private void _fileWatcher_FilesChanged(object sender, FileWatchEventArgs e)
        {
            SetFiles();
        }

        private List<string> GetFiles()
        {
            var list = new List<string>();

            try
            {
                if (_cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].Value.ToBoolean(null))
                {
                    DriveInfo[] allDrives = DriveInfo.GetDrives();

                    foreach (DriveInfo d in allDrives)
                    {
                        if (d.DriveType == DriveType.Removable && d.IsReady)
                        {
                            var fileInfo = d.RootDirectory.GetFiles();
                            foreach (var info in fileInfo)
                            {
                                list.Add(info.FullName);
                            }
                        }
                    }
                }
                else
                {
                    list.AddRange(Directory.GetFiles(ConfigurationProvider.AirkanNetworkFolder));
                }
            }
            catch (System.Exception e)
            {
                Trace.TraceError($"{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}: Error loading files\r\n{e.Message}", e);
            }

            return list;
        }

        private void SetFiles()
        {
            var fileNamesVariable = _cpu.Tasks["DataTrans1"].Variables["FileNames"];
            if (fileNamesVariable.IsConnected)
            {
                var files = GetFiles();
                for (int i = 0; i < files.Count; i++)
                {
                    _inputFiles.TryAdd(i + 1, files[i]);
                    fileNamesVariable.Value[i] = files[i];
                }

                fileNamesVariable.WriteValue();
            }
        }

        private void _eventNotifier_VariableValueChanged(object sender, PviApplicationEventArgs e)
        {
            if (sender is Variable variable)
            {
                if (variable.Name == "FileTransferLocation")
                {
                    SetFiles();
                }

                if (variable.Name == "FileSendToDisplay")
                {
                    FileSendToDisplay();
                }
            }
        }

        public void FileSendToDisplay()
        {
            var sendToDisplayVariable = _cpu.Tasks["DataTrans1"].Variables["FileSendToDisplay"];
            var index = sendToDisplayVariable.Value.ToInt32(null);
            if (_inputFiles.TryGetValue(index, out var path))
            {
                ProcessInputFile(path, _cpu);
            }

        }

        public CommandStatus ProcessCommand(Cpu cpu, string commandName, string commandData)
        {
            lock (SyncLock)
            {
                switch (commandName)
                {
                    case "SetVariable":
                        Send(cpu, commandData);
                        break;
                    case "ProcessFile":
                        ProcessInputFile(commandData, cpu);
                        break;
                    default:
                        break;
                }

                return new CommandStatus(0, String.Empty);
            }
        }

        public List<AirkanInputFileInfo> GetAirkanInputFiles()
        {
            var list = new List<AirkanInputFileInfo>();
            foreach (var kvp in _inputFiles)
            {
                if (!String.IsNullOrEmpty(kvp.Value))
                {
                    list.Add(new AirkanInputFileInfo(kvp.Key, kvp.Value));
                }
            }

            return list;
        }

        public List<AirkanJob> GetAirkanVariables(Cpu cpu)
        {
            var airkanJobList = new List<AirkanJob>();
            try
            {
                Variable dataTransfer = cpu.Variables[dataTransferVariable];

                for (int i = 0; i < dataTransfer.Members.Count; i++)
                {
                    if (dataTransfer.Members[i]["RunQuantity"].Value <= 0)
                    {
                        continue;
                    }

                    var airkanJob = new AirkanJob();
                    var list = new List<AirkanVariable>();

                    list.Add(new AirkanVariable()
                    { Name = "RunQuantity", Value = dataTransfer.Members[i]["RunQuantity"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Length_1", Value = dataTransfer.Members[i]["DuctJob.Length_1"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Length_2", Value = dataTransfer.Members[i]["DuctJob.Length_2"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Type", Value = dataTransfer.Members[i]["DuctJob.Type"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Bead", Value = dataTransfer.Members[i]["DuctJob.Bead"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Damper", Value = dataTransfer.Members[i]["DuctJob.Damper"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.ConnTypeR", Value = dataTransfer.Members[i]["DuctJob.ConnTypeR"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.ConnTypeL", Value = dataTransfer.Members[i]["DuctJob.ConnTypeL"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.CleatMode", Value = dataTransfer.Members[i]["DuctJob.CleatMode"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.CleatType", Value = dataTransfer.Members[i]["DuctJob.CleatType"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.LockType", Value = dataTransfer.Members[i]["DuctJob.LockType"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.SealantUsed", Value = dataTransfer.Members[i]["DuctJob.SealantUsed"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Brake", Value = dataTransfer.Members[i]["DuctJob.Brake"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.Insulation", Value = dataTransfer.Members[i]["DuctJob.Insulation"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.PinSpacing", Value = dataTransfer.Members[i]["DuctJob.PinSpacing"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.TieRodType_Leg_1", Value = dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.TieRodType_Leg_2", Value = dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.TieRodHoles_Leg_1", Value = dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.TieRodHoles_Leg_2", Value = dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "DuctJob.HoleSize", Value = dataTransfer.Members[i]["DuctJob.HoleSize"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "Basic.CoilNumber", Value = dataTransfer.Members[i]["Basic.CoilNumber"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "Basic.CoilGauge", Value = dataTransfer.Members[i]["Basic.CoilGauge"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "Basic.CoilWidth", Value = dataTransfer.Members[i]["Basic.CoilWidth"].Value });
                    list.Add(new AirkanVariable()
                    { Name = "Basic.TotalLength", Value = dataTransfer.Members[i]["Basic.TotalLength"].Value });

                    airkanJob.LineNumber = i;
                    airkanJob.Variables = new List<AirkanVariable>(list);
                    airkanJobList.Add(airkanJob);

                }
            }
            catch (System.Exception e)
            {
                Trace.TraceError($"{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}: Error setting variables\r\n{e.Message}", e);
            }

            return airkanJobList;
        }

        public void Send(Cpu cpu, string commandData)
        {

        }

        public void ProcessInputFile(string filePath, Cpu cpu)
        {
            var v1 = _cpu.Variables["DataTransfer"];
            var v2 = _cpu.Tasks["DataTrans1"].Variables["FileNames"];
            var v3 = _cpu.Tasks["DataTrans1"].Variables["FileNameToLoad"]; //int
            var v4 = _cpu.Tasks["DataTrans1"].Variables["FileSendToDisplay"]; //bool
            var v5 = _cpu.Tasks["DataTrans1"].Variables["FileLocationJobs"]; //string
            var v6 = _cpu.Tasks["DataTrans1"].Variables["FileLocationPrinter"]; //string
            var v7 = _cpu.Tasks["DataTrans1"].Variables["FileLocationDataReturn"]; //string
            var v8 = _cpu.Tasks["DataTrans1"].Variables["ProductFinished"]; //bool
            var v9 = _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"]; //bool


            if (!File.Exists(filePath))
            {
                Trace.TraceError($"File {filePath} not found.");
                throw new System.Exception($"File {filePath} not found.");
            }

            var lines = File.ReadAllLines(filePath);
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            for (int i = 0; i < lines.Length; i++)
            {
                var split = lines[i].Split(',');
                if (split.Length > 22)
                {
                    dataTransfer.Members[i]["RunQuantity"].Value = split[0];
                    dataTransfer.Members[i]["DuctJob.Length_1"].Value = split[1];
                    dataTransfer.Members[i]["DuctJob.Length_2"].Value = split[2];
                    dataTransfer.Members[i]["DuctJob.Type"].Value = split[3];
                    dataTransfer.Members[i]["DuctJob.Bead"].Value = split[4];
                    dataTransfer.Members[i]["DuctJob.Damper"].Value = split[5];
                    dataTransfer.Members[i]["DuctJob.ConnTypeR"].Value = split[6];
                    dataTransfer.Members[i]["DuctJob.ConnTypeL"].Value = split[7];
                    dataTransfer.Members[i]["DuctJob.CleatMode"].Value = split[8];
                    dataTransfer.Members[i]["DuctJob.CleatType"].Value = split[9];
                    dataTransfer.Members[i]["DuctJob.LockType"].Value = split[10];
                    dataTransfer.Members[i]["DuctJob.SealantUsed"].Value = split[11];
                    dataTransfer.Members[i]["DuctJob.Brake"].Value = split[12];
                    dataTransfer.Members[i]["DuctJob.Insulation"].Value = split[13];
                    dataTransfer.Members[i]["DuctJob.PinSpacing"].Value = split[14];
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].Value = split[15];
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].Value = split[16];
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].Value = split[17];
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].Value = split[18];
                    dataTransfer.Members[i]["DuctJob.HoleSize"].Value = split[19];
                    dataTransfer.Members[i]["Basic.CoilNumber"].Value = split[20];
                    dataTransfer.Members[i]["Basic.CoilGauge"].Value = split[21];
                    dataTransfer.Members[i]["Basic.CoilWidth"].Value = split[22];
                    dataTransfer.Members[i]["Basic.TotalLength"].Value = split[23];
                    dataTransfer.Members[i]["Basic.TotalLength"].Value = split[23];

                }
            }
            dataTransfer.WriteValue();
        }
    }

    public class AirkanJob
    {
        public string Line { get; set; }
        public int LineNumber { get; set; }
        public List<AirkanVariable> Variables { get; set; }
    }


    public class AirkanVariable
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }

        public AirkanVariable() { }
        public AirkanVariable(string name, string taskName, string address, string value, string dataType) 
        {
            Name = name;
            TaskName = taskName;
            Address = address;
            Value = value;
            DataType = dataType;

        }
    }

}