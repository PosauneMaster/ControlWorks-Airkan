using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        const string dataTransferVariable = "DataTransfer";

        private static readonly object SyncLock = new object();
        private readonly IEventNotifier _eventNotifier;
        private readonly List<string> _variableNames;
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

            _variableNames = new List<String>();
            _variableNames.Add("Basic.CutLength");
            _variableNames.Add("Basic.CoilGauge");
            _variableNames.Add("Basic.CoilWidthMeasured");
            _variableNames.Add("Basic.CoilWidth");
            _variableNames.Add("Basic.PartScrapLength");
            _variableNames.Add("Basic.JobScrapLength");
            _variableNames.Add("Basic.TotalLength");
            _variableNames.Add("DuctJob.Bead");
            _variableNames.Add("DuctJob.Brake");
            _variableNames.Add("DuctJob.CleatMode");
            _variableNames.Add("DuctJob.CleatType");
            _variableNames.Add("DuctJob.Damper");
            _variableNames.Add("DuctJob.HoleSize");
            _variableNames.Add("DuctJob.Insulation");
            _variableNames.Add("DuctJob.Length_1");
            _variableNames.Add("DuctJob.Length_2");
            _variableNames.Add("DuctJob.LockType");
            _variableNames.Add("DuctJob.ConnTypeL");
            _variableNames.Add("DuctJob.ConnTypeR");
            _variableNames.Add("DuctJob.Cutouts");
            _variableNames.Add("DuctJob.PinSpacing");
            _variableNames.Add("DuctJob.SealantUsed");
            _variableNames.Add("DuctJob.Sides");
            _variableNames.Add("DuctJob.TieRodType_Leg_1");
            _variableNames.Add("DuctJob.TieRodType_Leg_2");
            _variableNames.Add("DuctJob.TieRodHoles_Leg_1");
            _variableNames.Add("DuctJob.TieRodHoles_Leg_2");
            _variableNames.Add("DuctJob.Type");
            _variableNames.Add("Cutouts.[0]");
            _variableNames.Add("Cutouts.[1]");
            _variableNames.Add("Cutouts.[2]");
            _variableNames.Add("Cutouts.[3]");
            _variableNames.Add("Cutouts.[4]");
            _variableNames.Add("Cutouts.[5]");
            _variableNames.Add("Cutouts.[6]");
            _variableNames.Add("Cutouts.[7]");
            _variableNames.Add("JobName");
            _variableNames.Add("DownloadNumber");
            _variableNames.Add("JobNum");
            _variableNames.Add("NeoPrintData.ReferenceERP"); // Data for label
            _variableNames.Add("NeoPrintData.DeliveryYardERP"); //to be included in the Bartender file
            _variableNames.Add("NeoPrintData.CustomerOrderERP");
            _variableNames.Add("NeoPrintData.BarCode");
            _variableNames.Add("NeoPrintData.PieceNumberERP");
            _variableNames.Add("CustomerInfo.CustomerOrderNumber");
            _variableNames.Add("CustomerInfo.CustomerAddress");
            _variableNames.Add("CustomerInfo.CustomerName");
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
                        SetFiles(ConfigurationProvider.AirkanNetworkFolder);
                    });
                }
            }
        }

        private void _fileWatcher_FilesChanged(object sender, FileWatchEventArgs e)
        {
            SetFiles(ConfigurationProvider.AirkanNetworkFolder);
        }

        private void SetFiles(string directory)
        {
            Trace.TraceInformation($"Setting input files from {directory}");
            if (Directory.Exists(directory))
            {
                try
                {
                    var files = Directory.GetFiles(directory);
                    for (int i = 0; i < 20; i++)
                    {
                        _inputFiles.TryAdd(i + 1, files[i]);
                        _cpu.Tasks["DataTransf"].Variables["FileNames"].Value[i] = files[i];
                    }

                    _cpu.Tasks["DataTransf"].Variables["FileNames"].WriteValue();
                }
                catch (System.Exception e)
                {
                    Trace.TraceError($"AirkanVariableService.SetFiles. Error setting files\r\n{e.Message}");
                }
            }
            else
            {
                Trace.TraceError($"AirkanVariableService.SetFiles. Directory {directory} not found");
            }
        }

        private void _eventNotifier_VariableValueChanged(object sender, PviApplicationEventArgs e)
        {
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
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            for (int i = 0; i < dataTransfer.Members.Count; i++)
            {
                var airkanJob = new AirkanJob();
                var list = new List<AirkanVariable>();

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


            //var value1 = dataTransfer.Members[i]["DuctJob.Length_1"].Value;

            //dataTransfer.Members[i]["DuctJob.Length_1"].Value = split[1];
            //dataTransfer.Members[i]["DuctJob.Length_2"].Value = split[2];
            //dataTransfer.Members[i]["DuctJob.Type"].Value = split[3];
            //dataTransfer.Members[i]["DuctJob.Bead"].Value = split[4];
            //dataTransfer.Members[i]["DuctJob.Damper"].Value = split[5];
            //dataTransfer.Members[i]["DuctJob.ConnTypeR"].Value = split[6];
            //dataTransfer.Members[i]["DuctJob.ConnTypeL"].Value = split[7];
            //dataTransfer.Members[i]["DuctJob.CleatMode"].Value = split[8];
            //dataTransfer.Members[i]["DuctJob.CleatType"].Value = split[9];
            //dataTransfer.Members[i]["DuctJob.LockType"].Value = split[10];
            //dataTransfer.Members[i]["DuctJob.SealantUsed"].Value = split[1];
            //dataTransfer.Members[i]["DuctJob.Brake"].Value = split[11];
            //dataTransfer.Members[i]["DuctJob.Insulation"].Value = split[12];
            //dataTransfer.Members[i]["DuctJob.PinSpacing"].Value = split[13];
            //dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].Value = split[14];
            //dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].Value = split[15];
            //dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].Value = split[16];
            //dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].Value = split[17];
            //dataTransfer.Members[i]["DuctJob.HoleSize"].Value = split[18];
            //dataTransfer.Members[i]["Basic.CoilNumber"].Value = split[19];
            //dataTransfer.Members[i]["Basic.CoilGauge"].Value = split[20];
            //dataTransfer.Members[i]["Basic.CoilWidth"].Value = split[21];
            //dataTransfer.Members[i]["Basic.TotalLength"].Value = split[22];







            //Variable dataTransfer = cpu.Variables[dataTransferVariable];

            //foreach (var variableName in _variableNames)
            //{
            //    var variable = dataTransfer[variableName];
            //    list.Add(new AirkanVariable(
            //        variableName,
            //        "GLOBAL",
            //        variable.Address,
            //        variable.Value,
            //        variable.IECDataType.ToString()));
            //}
            return airkanJobList;
        }



        public void Send(Cpu cpu, string commandData)
        {

            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            var list = new List<string>();
            var sb = new StringBuilder();

            foreach (Variable variable in dataTransfer.Members)
            {
                list.Add(variable.Address);
                if (variable.Members != null)
                {
                    foreach (Variable member in variable.Members)
                    {
                        sb.AppendLine(member.Address);
                        list.Add(member.Address);
                    }
                }
            }

            var jobQueue = cpu.Variables["JobQueue"]["Jobs"].Members[1];

            var sbJobs = new StringBuilder();
            foreach (Variable job in jobQueue.Members)
            {
                sbJobs.AppendLine(job.Address);
            }

            File.WriteAllText(@"C:\ControlWorks\AirKan\JobQueueNames_Latest.txt", sbJobs.ToString());


            //File.WriteAllText(@"C:\ControlWorks\AirKan\VariableNames_Latest.txt", sb.ToString());

            var airkanVariableList = new List<AirkanVariable>();

            //foreach (var item in list)
            //{
            //    airkanVariableList.Add(new AirkanVariable("GLOBAL", item, ""));
            //}


            //foreach (AirkanVariable airkanVariable in airkanVariableList)
            //{
            //    Variable variable = dataTransferVariable[airkanVariable.Address];

            //    airkanVariable.Name = variable.Address;
            //    airkanVariable.DataType = variable.IECDataType.ToString();

            //}

            //var jsonData = JsonConvert.SerializeObject(airkanVariableList, Formatting.Indented);
            int counter = 1;
            var data = JsonConvert.DeserializeObject<List<AirkanVariable>>(commandData);
            if (data != null)
            {
                foreach (AirkanVariable airkanVariable in data)
                {
                    Variable variable = dataTransfer[airkanVariable.Address];

                    if (variable.IECDataType == IECDataTypes.STRING) //("string", StringComparison.OrdinalIgnoreCase))
                    {
                        variable.Value = $"Test_Value_{counter}";
                    }
                    else
                    {
                        variable.Value = counter;
                    }

                    counter += 1;

                }

                dataTransfer.WriteValue();
            }
        }

        public void ProcessInputFile(string filePath, Cpu cpu)
        {
            var v1 = _cpu.Variables["DataTransfer"];
            var v2 = _cpu.Tasks["DataTransf"].Variables["FileNames"];
            var v3 = _cpu.Tasks["DataTransf"].Variables["FileNameToLoad"]; //int
            var v4 = _cpu.Tasks["DataTransf"].Variables["FileSendToDisplay"]; //bool
            var v5 = _cpu.Tasks["DataTransf"].Variables["FileLocationJobs"]; //string
            var v6 = _cpu.Tasks["DataTransf"].Variables["FileLocationPrinter"]; //string
            var v7 = _cpu.Tasks["DataTransf"].Variables["FileLocationDataReturn"]; //string
            var v8 = _cpu.Tasks["DataTransf"].Variables["ProductFinished"]; //bool



            if (!File.Exists(filePath))
            {
                Trace.TraceError($"File {filePath} not found.");
                throw new System.Exception($"File {filePath} not found.");
            }

            var lines = File.ReadAllLines(filePath);
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            for (int i = 0; i < lines.Length; i++)
            //foreach (var line in lines)
            {
                var split = lines[i].Split(',');
                if (split.Length > 0)
                {
                    dataTransfer.Members["Qty"].Value = split[0];
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
                    dataTransfer.Members[i]["DuctJob.SealantUsed"].Value = split[1];
                    dataTransfer.Members[i]["DuctJob.Brake"].Value = split[11];
                    dataTransfer.Members[i]["DuctJob.Insulation"].Value = split[12];
                    dataTransfer.Members[i]["DuctJob.PinSpacing"].Value = split[13];
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].Value = split[14];
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].Value = split[15];
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].Value = split[16];
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].Value = split[17];
                    dataTransfer.Members[i]["DuctJob.HoleSize"].Value = split[18];
                    dataTransfer.Members[i]["Basic.CoilNumber"].Value = split[19];
                    dataTransfer.Members[i]["Basic.CoilGauge"].Value = split[20];
                    dataTransfer.Members[i]["Basic.CoilWidth"].Value = split[21];
                    dataTransfer.Members[i]["Basic.TotalLength"].Value = split[22];
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