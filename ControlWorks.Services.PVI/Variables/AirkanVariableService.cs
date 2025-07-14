using ControlWorks.Common;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;

using Newtonsoft.Json;

using System.Text.RegularExpressions;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        const string dataTransferVariable = "DataTransfer";

        private static readonly object SyncLock = new object();
        private readonly IEventNotifier _eventNotifier;
        private FileWatcher _fileWatcher;
        private readonly Cpu _cpu;
        private readonly ConcurrentDictionary<int, string> _inputFiles = new ConcurrentDictionary<int, string>();
        private readonly UsbMonitor _monitor;
        private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

        public AirkanVariableService(IEventNotifier eventNotifier, Cpu cpu)
        {
            _cpu = cpu;

            _eventNotifier = eventNotifier;
            _eventNotifier.VariableValueChanged += _eventNotifier_VariableValueChanged;
            _eventNotifier.VariableConnected += _eventNotifier_VariableConnected;

            _monitor = new UsbMonitor();
            _monitor.DriveChanged += (sender, args) =>
            {
                SetFiles();
            };
            System.Threading.Tasks.Task.Run(() => _monitor.Run());
        }

        private void _eventNotifier_VariableConnected(object sender, PviApplicationEventArgs e)
        {
            if (sender is Variable v)
            {
                if (v.Name == dataTransferVariable)
                {
                    _autoResetEvent.Set();
                }
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

        private void WaitForDataTransfer()
        {
            var counter = 0;
            Variable dataTransfer = _cpu.Variables[dataTransferVariable];
            while (dataTransfer.IsConnected == false)
            {
                System.Threading.Tasks.Task.Delay(500);
                counter++;
            }
        }

        public class NetworkConnection
        {
            public static void MapShare(string networkPath, string username, string password)
            {
                var psi = new ProcessStartInfo("net", $"use \"{networkPath}\" /user:{username} {password}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if ((process.ExitCode != 0) && (process.ExitCode != 1219))
                    {
                        Trace.TraceError($"Failed to map network drive: {error}");
                    }
                }
            }
        }

        private List<string> GetFiles()
        {
            var list = new List<string>();

            try
            {
                var connected = _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].IsConnected;
                var fileTransferLocation =
                    _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.ToBoolean(null);
                Trace.TraceInformation($"FileTransferLocation={_cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.ToBoolean(null)}");
                // true = USB false = Network
                string directoryPath;
                string fileServerUsername = @ConfigurationProvider.FileServerDomain + "\\" + @ConfigurationProvider.FileServerUserName;

                string printerPath = GetFileLocationPrinter(_cpu);

                NetworkConnection.MapShare(printerPath, fileServerUsername, ConfigurationProvider.FileServerPassword);

                if (_cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.ToBoolean(null))
                {
                    Trace.TraceInformation("Switching file location to USB");
                    DriveInfo[] allDrives = DriveInfo.GetDrives();

                    foreach (DriveInfo d in allDrives)
                    {
                        if (d.DriveType == DriveType.Removable && d.IsReady)
                        {
                            Trace.TraceInformation($"Reading from USB Drive {d.Name}");
                            var allowedExtensions = new[] { ".txt", ".csv", ".dat" };
                            directoryPath = d.RootDirectory.FullName;
                            var files = d.RootDirectory.GetFiles()
                                .Where(f => allowedExtensions.Contains(f.Extension, StringComparer.OrdinalIgnoreCase))
                                .OrderBy(f => f.Name)
                                .Take(20);

                            foreach (var fi in files)
                            {
                                list.Add(fi.Name);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    directoryPath = GetFileLocationJobs(_cpu);
                    NetworkConnection.MapShare(directoryPath, fileServerUsername, ConfigurationProvider.FileServerPassword);

                    if (!String.IsNullOrEmpty(directoryPath) && Directory.Exists(directoryPath))
                    {
                        var allowedExtensions = new[] { ".txt", ".csv", ".dat" };
                        var di = new DirectoryInfo(directoryPath);
                        var fileNames = Directory
                            .GetFiles(directoryPath)
                            .Where(f => allowedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
                            .Select(f => new FileInfo(f))
                            .OrderBy(f => f.Name)
                            .Take(20)
                            .Select(f => f.FullName)  // convert back to string paths
                            .ToArray();

                        Trace.TraceInformation("Switching file location to Network");

                        foreach (var fileName in fileNames)
                        {
                            var fi = new FileInfo(fileName);
                            var name = fi.Name;
                            list.Add(name);
                        }
                    }
                    else
                    {
                        Trace.TraceError($"The directory path {directoryPath} is not found");
                    }
                    SetFileWatcher(directoryPath);
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

        private void SetFileWatcher(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                Trace.TraceError("SetFileWatcher: The directory path is empty");
                return;
            }

            if (!new DirectoryInfo(path).Exists)
            {
                Trace.TraceError("SetFileWatcher: The directory path is not found");
                return;
            }

            if (_fileWatcher != null && _fileWatcher.DirectoryPath.Equals(path))
            {
                return;
            }

            _fileWatcher = new FileWatcher(path);
            _fileWatcher.FilesChanged += (sender, args) =>
            {
                SetFiles();
            };
        }

        private string GetFileLocationJobs(Cpu cpu)
        {
            if (cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.ToBoolean(null)) 
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();

                foreach (DriveInfo d in allDrives)
                {
                    if (d.DriveType == DriveType.Removable && d.IsReady)
                    {
                        return d.RootDirectory.FullName;
                    }
                }
            }
            else
            {
                if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("FileLocationJobs"))
                {
                    return _cpu.Tasks["DataTransf"].Variables["FileLocationJobs"].Value.ToString(CultureInfo.InvariantCulture);
                }
            }

            return String.Empty;

        }

        private string GetFileLocationPrinter(Cpu cpu)
        {
            if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("FileLocationPrinter"))
            {
                return _cpu.Tasks["DataTransf"].Variables["FileLocationPrinter"].Value.ToString(CultureInfo.InvariantCulture);
            }

            return String.Empty;
        }

        private void SetFiles()
        {
            try
            {
                lock (SyncLock)
                {
                    _inputFiles.Clear();
                    var fileNamesVariable = _cpu.Tasks["DataTransf"].Variables["FileNames"];

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
            }
            catch (System.Exception e)
            {
                Trace.TraceError(e.ToString());
            }
        }

        private StringBuilder GetBartenderBtwHeader(StringBuilder sb)
        {
            var bartenderHeader = ConfigurationManager.AppSettings["BartenderFileHeader"];
            if (String.IsNullOrEmpty(bartenderHeader))
            {
                Trace.TraceError("Configuration BartenderFileHeader not found");
                return null;
            }
            
            var split = bartenderHeader.Split('|');

            foreach (var line in split)
            {
                sb.AppendLine(line);
            }

            return sb;
        }

        private bool PrintData(Variable printDataVariable)
        {
            var headerList = new List<string>();
            var headers = _cpu.Tasks["DataTransf"].Variables["PrinterHeaders"];

            if (headers.Members == null)
            {
                Trace.TraceError("Unable to locate PrinterHeaders variable");
                return false;
            }

            foreach (var member in headers.Members)
            {
                var memberVariable = member as Variable;
                var value = memberVariable.Value;
                if (string.IsNullOrEmpty(value))
                {
                    headerList.Add("");
                }
                else
                {
                    headerList.Add(value);
                }

            }
            var header = String.Join(";", headerList);

            var list = new List<string>();

            if (printDataVariable.Members == null)
            {
                Trace.TraceError("Unable to locate PrintData variable");
                return false;
            }

            foreach (var member in printDataVariable.Members)
            {
                var memberVariable = member as Variable;
                var value = memberVariable.Value;
                if (string.IsNullOrEmpty(value))
                {
                    list.Add("");
                }
                else
                {
                    list.Add(value);
                }
            }

            var csv = String.Join(";", list);

            var sb = new StringBuilder();
            sb = GetBartenderBtwHeader(sb);
            if (sb == null)
            {
                Trace.TraceError("Unable to process PrintData");
                return false;
            }

            sb.AppendLine(header);
            sb.AppendLine(csv);

            try
            {
                if (printDataVariable == null || printDataVariable.Value == null)
                {
                    Trace.TraceError("PrintData Variable is null");
                    return false;
                }

                string customerOrderErp = "PrintFile";
                if (_cpu.Tasks["DataTransf"].Variables["PrintData"].Members["CustomerOrderERP"] != null)
                {
                    customerOrderErp = _cpu.Tasks["DataTransf"].Variables["PrintData"].Members["CustomerOrderERP"].Value.ToString(CultureInfo.InvariantCulture);
                }

                if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("FileLocationPrinter"))
                {
                    var cleanName = Regex.Replace(customerOrderErp, @"[<>:""/\\|?*]", "_");
                    var filename = $"{cleanName}_{DateTime.Now:yyyyMMddHHmmss}.txt";
                    var location = _cpu.Tasks["DataTransf"].Variables["FileLocationPrinter"].Value.ToString(CultureInfo.InvariantCulture);

                    if (!Directory.Exists(location))
                    {
                        Directory.CreateDirectory(location);
                    }
                    
                    if (Directory.Exists(location))
                    {
                        var path = Path.Combine(location, filename);
                        File.WriteAllText(path, sb.ToString());
                        Trace.TraceInformation($"Print file sent to {path}");
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
        }

        private void _eventNotifier_VariableValueChanged(object sender, PviApplicationEventArgs e)
        {
            if (sender is Variable variable)
            {
                if (variable.Name == "SendToPrinter")
                {
                    if (variable.Value.ToBoolean(null) == true)
                    {
                        var printDataVariable = _cpu.Tasks["DataTransf"].Variables["PrintData"];
                        
                        var result = PrintData(printDataVariable);
                        if (result)
                        {
                            variable.Value.Assign(false);
                            variable.WriteValue();
                        }
                    }
                }

                if (variable.Name == "FileLocationJobs")
                {
                    SetFiles();
                }
                if (variable.Name == "FileTransferLocation")
                {
                    SetFiles();
                }

                if (variable.Name == "ProductFinished")
                {
                    if (variable.Value == true)
                    {
                        Trace.TraceInformation("Product Finished received as true");

                        var result = WriteToProductionDataDatabase();
                        if (result)
                        {
                            variable.Value.Assign(false);
                            variable.WriteValue();
                        }
                    }
                }
                if (variable.Name == "SendOrderData")
                {
                    if (variable.Value == true)
                    {
                        var result = WriteToOrderDataDatabase();
                        if (result)
                        {
                            variable.Value.Assign(false);
                            variable.WriteValue();
                        }
                    }
                }


                if (variable.Name == "FileNameToLoad")
                {
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        _autoResetEvent.WaitOne(TimeSpan.FromMilliseconds(2000));
                        FileNameToLoad();
                    });

                }

                if (variable.Name == "DeleteFile")
                {
                    if (variable.Value == true)
                    {
                        var result = FileNameToDelete();
                        {
                            variable.Value.Assign(false);
                            variable.WriteValue();
                        }
                    }
                }
            }
        }

        private bool WriteToOrderDataDatabase()
        {
            if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("OrderData"))
            {
                var orderDataVariable = _cpu.Tasks["DataTransf"].Variables["OrderData"];
                if (!orderDataVariable.IsConnected)
                {
                    return true;
                }

                Trace.TraceInformation("Write Order Data to Database");
                var service = new DatabaseService();
                return service.WriteToOrderData(orderDataVariable);

            }

            return true;

        }


        private bool WriteToProductionDataDatabase()
        {

            if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("ProductionData"))
            {
                var productionData = _cpu.Tasks["DataTransf"].Variables["ProductionData"];
                if (!productionData.IsConnected)
                {
                    Trace.TraceInformation("ProductionData variable is not connected");
                    return true;
                }

                Trace.TraceInformation("Write Production Data to database");
                var service = new DatabaseService();
                return service.WriteToOrderData(productionData);
                // return service.WriteToProductionData(productionData);
            }

            Trace.TraceInformation("ProductionData variable is not found");

            return true;
        }

        public void FileNameToLoad()
        {
            if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("FileNameToLoad"))
            {
                var sendToDisplayVariable = _cpu.Tasks["DataTransf"].Variables["FileNameToLoad"];
                var index = sendToDisplayVariable.Value.ToInt32(null);
                if (_inputFiles.TryGetValue(index, out var path))
                {
                    Trace.TraceInformation($"Processing Index {index} {path}");
                    ProcessInputFile(path, _cpu);
                }
            }
        }

        public bool FileNameToDelete()
        {
            if (_cpu.Tasks["DataTransf"].Variables.ContainsKey("FileNameToLoad"))
            {
                var sendToDisplayVariable = _cpu.Tasks["DataTransf"].Variables["FileNameToLoad"];
                var index = sendToDisplayVariable.Value.ToInt32(null);
                if (_inputFiles.TryGetValue(index, out var path))
                {
                    Trace.TraceInformation($"Deleting Index {index} {path}");
                    return DeleteInputFile(path, _cpu);
                }

                return false;
            }

            return false;
        }

        public CommandStatus ProcessCommand(Cpu cpu, string commandName, string commandData)
        {
            var dataTransferCompletedParts = _cpu.Tasks["DataTransf"].Variables["DataTransferCompletedParts"];

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
            _cpu.Tasks["DataTransf"].Variables["ProductFinished"].Value.Assign(false);
            _cpu.Tasks["DataTransf"].Variables["ProductFinished"].WriteValue();

            var json = File.ReadAllText(filePath);
            var bartender = JsonConvert.DeserializeObject<BarTenderFileService>(json);


            var dataTransferCompletedParts = _cpu.Tasks["DataTransf"].Variables["DataTransferCompletedParts"];

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

            _cpu.Tasks["DataTransf"].Variables["ProductFinished"].AssignChecked(true);
            _cpu.Tasks["DataTransf"].Variables["ProductFinished"].WriteValue();
        }

        public void SetFileTransferLocation(string command, Cpu cpu)
        {
            var fileTransferLocation = _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"];

            if (command.Equals("USB", StringComparison.OrdinalIgnoreCase))
            {
                _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.Assign(true);
            }
            else
            {
                {
                    _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.Assign(false);
                }
            }

            _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].WriteValue();

        }

        public List<AirkanInputFileInfo> GetAirkanInputFiles()
        {
            var fileTransferLocation = _cpu.Tasks["DataTransf"].Variables["FileTransferLocation"].Value.ToBoolean(null);
            var list = new List<AirkanInputFileInfo>();

            var fileNamesVariable = _cpu.Tasks["DataTransf"].Variables["FileNames"];
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

        private string ToCsv(Variable printData)
        {
            var list = new List<string>();
            foreach (Variable member in printData.Members)
            {
                list.Add(member["RunQuantity"].Value.ToString());
                list.Add(member["DuctJob.Length_1"].Value.ToString());
                list.Add(member["DuctJob.Length_2"].Value.ToString());
                list.Add(member["DuctJob.Type"].Value.ToString());
                list.Add(member["DuctJob.Bead"].Value.ToString());
                list.Add(member["DuctJob.Damper"].Value.ToString());
                list.Add(member["DuctJob.ConnTypeR"].Value.ToString());
                list.Add(member["DuctJob.ConnTypeL"].Value.ToString());
                list.Add(member["DuctJob.CleatMode"].Value.ToString());
                list.Add(member["DuctJob.CleatType"].Value.ToString());
                list.Add(member["DuctJob.LockType"].Value.ToString());
                list.Add(member["DuctJob.SealantUsed"].Value.ToString());
                list.Add(member["DuctJob.Brake"].Value.ToString());
                list.Add(member["DuctJob.Insulation"].Value.ToString());
                list.Add(member["DuctJob.PinSpacing"].Value.ToString());
                list.Add(member["DuctJob.TieRodType_Leg_1"].Value.ToString());
                list.Add(member["DuctJob.TieRodType_Leg_2"].Value.ToString());
                list.Add(member["DuctJob.TieRodHoles_Leg_1"].Value.ToString());
                list.Add(member["DuctJob.TieRodHoles_Leg_2"].Value.ToString());
                list.Add(member["DuctJob.HoleSize"].Value.ToString());
                list.Add(member["Basic.CoilGauge"].Value.ToString());
                list.Add(member["Basic.CoilWidth"].Value.ToString());
                list.Add(member["Basic.TotalLength"].Value.ToString());
                list.Add(member["Basic.TotalLength"].Value.ToString());
                list.Add(member["NeoPrintData"]["ReferenceERP"].Value.ToString());
                list.Add(member["NeoPrintData"]["DeliveryYardERP"].Value.ToString());
                list.Add(member["NeoPrintData"]["CustomerOrderERP"].Value.ToString());
                list.Add(member["NeoPrintData"]["BarCode"].Value.ToString());
                list.Add(member["NeoPrintData"]["PieceNumberERP"].Value.ToString());
                list.Add(member["CustomerInfo"]["CustomerAddress"].Value.ToString());
                list.Add(member["CustomerInfo"]["CustomerName"].Value.ToString());

            }

            var csv = String.Join(",", list);

            return csv;
        }


        private void ClearDataTransfer()
        {
            Variable dataTransfer = _cpu.Variables[dataTransferVariable];

            if (dataTransfer.Members == null)
            {
                return;
            }

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
            var directoryPath = GetFileLocationJobs(cpu);
            var fi = new FileInfo(filePath);
            var fullPath = Path.Combine(directoryPath, fi.Name);

            if (!File.Exists(fullPath))
            {
                Trace.TraceError($"File {fullPath} not found.");
                return;
            }
            var list = new List<bool>();
            var lines = File.ReadAllLines(fullPath);
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            if (dataTransfer.Members == null)
            {
                return;
            }

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
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["ConnTypeR"].AssignChecked(split[31]));
                        list.Add(dataTransfer.Members[i]["NeoPrintData"]["ConnTypeL"].AssignChecked(split[32]));
                    }
                }
            }

            if (list.TrueForAll(v => v))
            {
                dataTransfer.WriteValue();
                Trace.TraceInformation($"File {filePath} processed without errors!");
            }
            else
            {
                Trace.TraceError($"File {filePath} not processed.  Invalid data detected");
            }
        }

        public bool DeleteInputFile(string filePath, Cpu cpu)
        {
            var directoryPath = GetFileLocationJobs(cpu);
            var fi = new FileInfo(filePath);
            var fullPath = Path.Combine(directoryPath, fi.Name);

            if (!File.Exists(fullPath))
            {
                Trace.TraceError($"File {fullPath} not found.");
                return false;
            }

            File.Delete(fullPath);
            return true;
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