using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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
                    ProcessBarCode(variable);
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

        public void ProcessBarCode(Variable variable = null)
        {
            if (variable != null)
            {
                if (variable.Value != 1)
                {
                    return;
                }
            }

            var dataTransferCompletedParts = _cpu.Tasks["DataTrans1"].Variables["DataTransferCompletedParts"];

            var btwFileName = dataTransferCompletedParts["FileName"].Value.ToString(CultureInfo.InvariantCulture);
            var maat1 = dataTransferCompletedParts["DuctJob.Length_1"].Value.ToString(CultureInfo.InvariantCulture);
            var maat2 = dataTransferCompletedParts["DuctJob.Length_2"].Value.ToString(CultureInfo.InvariantCulture);
            var type = dataTransferCompletedParts["DuctJob.Type"].Value.ToString(CultureInfo.InvariantCulture);
            var kader1 = dataTransferCompletedParts["DuctJob.ConnTypeR"].Value.ToString(CultureInfo.InvariantCulture);
            var kader2 = dataTransferCompletedParts["DuctJob.ConnTypeL"].Value.ToString(CultureInfo.InvariantCulture);
            var dikte = dataTransferCompletedParts["Basic.CoilGauge"].Value.ToString(CultureInfo.InvariantCulture);
            var lengte = dataTransferCompletedParts["Basic.CoilWidth"].Value.ToString(CultureInfo.InvariantCulture);
            var werf = dataTransferCompletedParts["NeoPrintData.DeliveryYardERP"].Value.ToString(CultureInfo.InvariantCulture);
            var ordernummer = dataTransferCompletedParts["NeoPrintData.CustomerOrderERP"].Value.ToString(CultureInfo.InvariantCulture);
            var barcode = dataTransferCompletedParts["NeoPrintData.BarCode"].Value.ToString(CultureInfo.InvariantCulture);
            var stuknr = dataTransferCompletedParts["NeoPrintData.PieceNumberERP"].Value.ToString(CultureInfo.InvariantCulture);
            var klantreferentie = dataTransferCompletedParts["CustomerInfo.CustomerName"].Value.ToString(CultureInfo.InvariantCulture);

            BarTenderFileService bartenderService = new BarTenderFileService();
            var fileDeDetails = bartenderService.FileDetails(btwFileName, ordernummer, werf, klantreferentie, barcode, kader1, kader2, maat1,
                maat2, stuknr, type, lengte, dikte);


            
            
            
            
            


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
                    case "ProcessBarCode":
                        ProcessBarCode();
                        break;

                    default:
                        break;
                }

                return new CommandStatus(0, String.Empty);
            }
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

            var lines = File.ReadAllLines(filePath);
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            for (int i = 0; i < lines.Length; i++)
            {
                var split = lines[i].Split(',');
                if (split.Length > 22)
                {
                    dataTransfer.Members[i]["RunQuantity"].Value.Assign(split[0]);
                    dataTransfer.Members[i]["DuctJob.Length_1"].Value.Assign(split[1]);
                    dataTransfer.Members[i]["DuctJob.Length_2"].Value.Assign(split[2]);
                    dataTransfer.Members[i]["DuctJob.Type"].Value.Assign(split[3]);
                    dataTransfer.Members[i]["DuctJob.Bead"].Value.Assign(split[4]);
                    dataTransfer.Members[i]["DuctJob.Damper"].Value.Assign(split[5]);
                    dataTransfer.Members[i]["DuctJob.ConnTypeR"].Value.Assign(split[6]);
                    dataTransfer.Members[i]["DuctJob.ConnTypeL"].Value.Assign(split[7]);
                    dataTransfer.Members[i]["DuctJob.CleatMode"].Value.Assign(split[8]);
                    dataTransfer.Members[i]["DuctJob.CleatType"].Value.Assign(split[9]);
                    dataTransfer.Members[i]["DuctJob.LockType"].Value.Assign(split[10]);
                    dataTransfer.Members[i]["DuctJob.SealantUsed"].Value.Assign(split[11]);
                    dataTransfer.Members[i]["DuctJob.Brake"].Value.Assign(split[12]);
                    dataTransfer.Members[i]["DuctJob.Insulation"].Value.Assign(split[13]);
                    dataTransfer.Members[i]["DuctJob.PinSpacing"].Value.Assign(split[14]);
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_1"].Value.Assign(split[15]);
                    dataTransfer.Members[i]["DuctJob.TieRodType_Leg_2"].Value.Assign(split[16]);
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_1"].Value.Assign(split[17]);
                    dataTransfer.Members[i]["DuctJob.TieRodHoles_Leg_2"].Value.Assign(split[18]);
                    dataTransfer.Members[i]["DuctJob.HoleSize"].Value.Assign(split[19]);
                    dataTransfer.Members[i]["Basic.CoilNumber"].Value.Assign(split[20]);
                    dataTransfer.Members[i]["Basic.CoilGauge"].Value.Assign(split[21]);
                    dataTransfer.Members[i]["Basic.CoilWidth"].Value.Assign(split[22]);
                    dataTransfer.Members[i]["Basic.TotalLength"].Value.Assign(split[23]);
                    dataTransfer.Members[i]["NeoPrintData"]["ReferenceERP"].Value.Assign(split[24]);

                    if (split.Length > 25)
                    {
                        dataTransfer.Members[i]["NeoPrintData"]["DeliveryYardERP"].Value.Assign(split[25]);
                        dataTransfer.Members[i]["NeoPrintData"]["CustomerOrderERP"].Value.Assign(split[26]);
                        dataTransfer.Members[i]["NeoPrintData"]["BarCode"].Value.Assign(split[27]);
                        dataTransfer.Members[i]["NeoPrintData"]["PieceNumberERP"].Value.Assign(split[28]);
                        dataTransfer.Members[i]["CustomerInfo"]["CustomerAddress"].Value.Assign(split[29]);
                        dataTransfer.Members[i]["CustomerInfo"]["CustomerName"].Value.Assign(split[30]);
                    }

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