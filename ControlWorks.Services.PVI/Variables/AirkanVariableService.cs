using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;

using Newtonsoft.Json;

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
            lock (SyncLock)
            {
                var list = new List<string>();

                try
                {
                    if (_cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].Value.ToBoolean(null))
                    {
                        Trace.TraceInformation("Switching file location to USB");
                        DriveInfo[] allDrives = DriveInfo.GetDrives();

                        foreach (DriveInfo d in allDrives)
                        {
                            if (d.DriveType == DriveType.Removable && d.IsReady)
                            {
                                var files = d.RootDirectory.GetFiles();
                                {
                                    foreach (var fi in files)
                                    {
                                        if (fi.Extension == ".txt" || fi.Extension == ".csv" || fi.Extension == ".dat")
                                        {
                                            list.Add(fi.FullName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var fileNames = Directory
                            .EnumerateFiles(ConfigurationProvider.AirkanNetworkFolder)
                            .Where(f => f.EndsWith(".csv") || f.EndsWith(".txt") || f.EndsWith(".dat"));

                        Trace.TraceInformation("Switching file location to Network");
                        list.AddRange(fileNames);
                    }
                }
                catch (System.Exception e)
                {
                    Trace.TraceError(
                        $"{GetType().Name}.{MethodBase.GetCurrentMethod()?.Name}: Error loading files\r\n{e.Message}",
                        e);
                }

                return list;
            }
        }

        private void SetFiles()
        {
            _inputFiles.Clear();
            var fileNamesVariable = _cpu.Tasks["DataTrans1"].Variables["FileNames"];

            if (fileNamesVariable.IsConnected)
            {
                for (int i = 0; i < fileNamesVariable.Value.ArrayLength; i++)
                {
                    fileNamesVariable.Value[i].Assign(String.Empty);
                }

                var files = GetFiles();
                for (int i = 0; i < files.Count; i++)
                {
                    _inputFiles.TryAdd(i, files[i]);
                    fileNamesVariable.Value[i].Assign(files[i]);
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

                if (variable.Name == "ProductFinished")
                {
                    new BarTenderFileService().ProcessBarCode(_cpu, variable);
                }

                if (variable.Name == "FileNameToLoad")
                {
                    FileNameToLoad();
                }

            }
        }

        public void FileNameToLoad()
        {
            var sendToDisplayVariable = _cpu.Tasks["DataTrans1"].Variables["FileNameToLoad"];
            var index = sendToDisplayVariable.Value.ToInt32(null);
            if (_inputFiles.TryGetValue(index, out var path))
            {
                Trace.TraceInformation($"Processing Index {index} {path}");
                ProcessInputFile(path, _cpu);
            }

        }

        public CommandStatus ProcessCommand(Cpu cpu, string commandName, string commandData)
        {
            var dataTransferCompletedParts = _cpu.Tasks["DataTrans1"].Variables["DataTransferCompletedParts"];

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
                    case "ProcessInputFileByIndex":
                        ProcessInputFileByIndex(commandData, cpu);
                        break;
                    case "SetFileTransferLocation":
                        SetFileTransferLocation(commandData, cpu);
                        break;
                    case "ProcessBarCodeFromFile":
                        ProcessBarcodeFromFile(commandData, cpu);
                        break;

                    default:
                        break;
                }

                return new CommandStatus(0, String.Empty);
            }
        }

        private void ProcessBarcodeFromFile(string filePath, Cpu cpu)
        {
            _cpu.Tasks["DataTrans1"].Variables["ProductFinished"].Value.Assign(false);
            _cpu.Tasks["DataTrans1"].Variables["ProductFinished"].WriteValue();

            var json = File.ReadAllText(filePath);
            var bartender = JsonConvert.DeserializeObject<BarTenderFileService>(json);


            var dataTransferCompletedParts = _cpu.Tasks["DataTrans1"].Variables["DataTransferCompletedParts"];

            dataTransferCompletedParts["FileName"].AssignChecked(bartender.BtwFileName);
            dataTransferCompletedParts["NeoPrintData.CustomerOrderERP"].AssignChecked(bartender.Ordernummer);
            dataTransferCompletedParts["NeoPrintData.DeliveryYardERP"].AssignChecked(bartender.Werf);
            dataTransferCompletedParts["CustomerInfo.CustomerName"].AssignChecked(bartender.Klantreferentie);
            dataTransferCompletedParts["NeoPrintData.BarCode"].AssignChecked(bartender.Barcode);
            dataTransferCompletedParts["DuctJob.ConnTypeR"].AssignChecked(bartender.Kader1);
            dataTransferCompletedParts["DuctJob.ConnTypeL"].AssignChecked(bartender.Kader2);
            dataTransferCompletedParts["DuctJob.Length_1"].AssignChecked(bartender.Maat1);
            dataTransferCompletedParts["DuctJob.Length_2"].AssignChecked(bartender.Maat2);
            dataTransferCompletedParts["NeoPrintData.PieceNumberERP"].AssignChecked(bartender.Stuknr);
            dataTransferCompletedParts["DuctJob.Type"].AssignChecked(bartender.Type);
            dataTransferCompletedParts["Basic.CoilWidth"].AssignChecked(bartender.Lengte);
            dataTransferCompletedParts["Basic.CoilGauge"].AssignChecked(bartender.Dikte);
            dataTransferCompletedParts.WriteValue();

            _cpu.Tasks["DataTrans1"].Variables["ProductFinished"].AssignChecked(true);
            _cpu.Tasks["DataTrans1"].Variables["ProductFinished"].WriteValue();
        }

        public void SetFileTransferLocation(string command, Cpu cpu)
        {
            var fileTransferLocation = _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"];

            if (command.Equals("USB", StringComparison.OrdinalIgnoreCase))
            {
                _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].Value.Assign(true);
            }
            else
            {
                {
                    _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].Value.Assign(false);
                }
            }

            _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].WriteValue();

        }

        public List<AirkanInputFileInfo> GetAirkanInputFiles()
        {
            var fileTransferLocation = _cpu.Tasks["DataTrans1"].Variables["FileTransferLocation"].Value.ToBoolean(null);
            var list = new List<AirkanInputFileInfo>();

            var fileNamesVariable = _cpu.Tasks["DataTrans1"].Variables["FileNames"];
            if (fileNamesVariable.IsConnected)
            {

                for (int i = 0; i < fileNamesVariable.Value.ArrayLength; i++)
                {
                    if (!String.IsNullOrEmpty(fileNamesVariable.Value[i]))
                    {
                        list.Add(new AirkanInputFileInfo(i, fileTransferLocation,fileNamesVariable.Value[i]));
                    }
                }

                fileNamesVariable.WriteValue();
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
                    if (dataTransfer.Members[i]["RunQuantity"].Value.ToString(CultureInfo.InvariantCulture) == "0")
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


                    list.Add(new AirkanVariable()
                        { Name = "ReferenceERP", Value = dataTransfer.Members[i]["NeoPrintData"]["ReferenceERP"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "DeliveryYardERP", Value = dataTransfer.Members[i]["NeoPrintData"]["DeliveryYardERP"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "CustomerOrderERP", Value = dataTransfer.Members[i]["NeoPrintData"]["CustomerOrderERP"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "BarCode", Value = dataTransfer.Members[i]["NeoPrintData"]["BarCode"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "PieceNumberERP", Value = dataTransfer.Members[i]["NeoPrintData"]["PieceNumberERP"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "CustomerAddress", Value = dataTransfer.Members[i]["CustomerInfo"]["CustomerAddress"].Value });
                    list.Add(new AirkanVariable()
                        { Name = "CustomerAddress", Value = dataTransfer.Members[i]["CustomerInfo"]["CustomerName"].Value });

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

        public void ProcessInputFileByIndex(string index, Cpu cpu)
        {
            if (Int32.TryParse(index, out var i))
            {
                if (_inputFiles.TryGetValue(i, out var file))
                {
                    ProcessInputFile(file, cpu);
                }
            }
        }

        private void ClearDataTransfer()
        {
            Variable dataTransfer = _cpu.Variables[dataTransferVariable];

            foreach (Variable member in dataTransfer.Members)
            {
                member["RunQuantity"].Value = 0;
                member["DuctJob.Length_1"].Value = 0;
                member["DuctJob.Length_2"].Value = 0;
                member["DuctJob.Type"].Value = 0;
                member["DuctJob.Bead"].Value = 0;
                member["DuctJob.Damper"].Value = 0;
                member["DuctJob.ConnTypeR"].Value = 0;
                member["DuctJob.ConnTypeL"].Value = 0;
                member["DuctJob.CleatMode"].Value = 0;
                member["DuctJob.CleatType"].Value = 0;
                member["DuctJob.LockType"].Value = 0;
                member["DuctJob.SealantUsed"].Value = 0;
                member["DuctJob.Brake"].Value = 0;
                member["DuctJob.Insulation"].Value = 0;
                member["DuctJob.PinSpacing"].Value = 0;
                member["DuctJob.TieRodType_Leg_1"].Value = 0;
                member["DuctJob.TieRodType_Leg_2"].Value = 0; 
                member["DuctJob.TieRodHoles_Leg_1"].Value = 0;
                member["DuctJob.TieRodHoles_Leg_2"].Value = 0;
                member["DuctJob.HoleSize"].Value = 0;
                member["Basic.CoilGauge"].Value = 0;
                member["Basic.CoilWidth"].Value = 0;
                member["Basic.TotalLength"].Value = 0;
                member["Basic.TotalLength"].Value = 0;
                member["NeoPrintData"]["ReferenceERP"].Value = "";
                member["NeoPrintData"]["DeliveryYardERP"].Value = "";
                member["NeoPrintData"]["CustomerOrderERP"].Value = "";
                member["NeoPrintData"]["BarCode"].Value = "";
                member["NeoPrintData"]["PieceNumberERP"].Value = "";
                member["CustomerInfo"]["CustomerAddress"].Value = "";
                member["CustomerInfo"]["CustomerName"].Value = "";

            }
        }

        public void ProcessInputFile(string filePath, Cpu cpu)
        {
            ClearDataTransfer();

            if (!File.Exists(filePath))
            {
                Trace.TraceError($"File {filePath} not found.");
                throw new System.Exception($"File {filePath} not found.");
            }
            var list = new List<bool>();
            var lines = File.ReadAllLines(filePath);
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            for (int i = 0; i < lines.Length; i++)
            {
                var split = lines[i].Split(',');
                if (split.Length > 22)
                {
                    list.Add(dataTransfer.Members[i]["RunQuantity"].AssignChecked(split[0]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Length_1"].AssignChecked(split[1]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Length_2"].AssignChecked(split[2]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Type"].AssignChecked(split[3]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Bead"].AssignChecked(split[4]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Damper"].AssignChecked(split[5]));
                    list.Add(dataTransfer.Members[i]["DuctJob.ConnTypeR"].AssignChecked(split[6]));
                    list.Add(dataTransfer.Members[i]["DuctJob.ConnTypeL"].AssignChecked(split[7]));
                    list.Add(dataTransfer.Members[i]["DuctJob.CleatMode"].AssignChecked(split[8]));
                    list.Add(dataTransfer.Members[i]["DuctJob.CleatType"].AssignChecked(split[9]));
                    list.Add(dataTransfer.Members[i]["DuctJob.LockType"].AssignChecked(split[10]));
                    list.Add(dataTransfer.Members[i]["DuctJob.SealantUsed"].AssignChecked(split[11]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Brake"].AssignChecked(split[12]));
                    list.Add(dataTransfer.Members[i]["DuctJob.Insulation"].AssignChecked(split[13]));
                    list.Add(dataTransfer.Members[i]["DuctJob.PinSpacing"].AssignChecked(split[14]));
                    list.Add(dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].AssignChecked(split[15]));
                    list.Add(dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].AssignChecked(split[16]));
                    list.Add(dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].AssignChecked(split[17]));
                    list.Add(dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].AssignChecked(split[18]));
                    list.Add(dataTransfer.Members[i]["DuctJob.HoleSize"].AssignChecked(split[19]));
                    list.Add(dataTransfer.Members[i]["Basic.CoilNumber"].AssignChecked(split[20]));
                    list.Add(dataTransfer.Members[i]["Basic.CoilGauge"].AssignChecked(split[21]));
                    list.Add(dataTransfer.Members[i]["Basic.CoilWidth"].AssignChecked(split[22]));
                    list.Add(dataTransfer.Members[i]["Basic.TotalLength"].AssignChecked(split[23]));
                    list.Add(dataTransfer.Members[i]["NeoPrintData"]["ReferenceERP"].AssignChecked(split[24]));

                    if (split.Length > 25)
                    {
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["DeliveryYardERP"].AssignChecked(split[25]));
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["CustomerOrderERP"].AssignChecked(split[26]));
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["BarCode"].AssignChecked(split[27]));
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["PieceNumberERP"].AssignChecked(split[28]));
                        list.Add(dataTransfer.Members[i]["CustomerInfo"]["CustomerAddress"].AssignChecked(split[29]));
                        list.Add(dataTransfer.Members[i]["CustomerInfo"]["CustomerName"].AssignChecked(split[30]));
                    }
                }
            }

            if (list.TrueForAll(v => v))
            {
                dataTransfer.WriteValue();
                Trace.TraceInformation($"File {filePath} processed without errors");
            }
            else
            {
                Trace.TraceError($"File {filePath} not processed.  Invalid data detected");
            }
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