using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        private static object _syncLock = new object();
        private readonly Dictionary<string, AirkanVariable> _dataTransferDict;

        public AirkanVariableService()
        {
            _dataTransferDict = new Dictionary<string, AirkanVariable>();
            //_dataTransferDict.Add("CutLength", new AirkanVariable("GLOBAL", "Basic", "CutLength", ""));
            //_dataTransferDict.Add("CoilGauge", new AirkanVariable("GLOBAL", "Basic", "CoilGauge", ""));
            //_dataTransferDict.Add("CoilWidthMeasured", new AirkanVariable("GLOBAL", "Basic", "CoilWidthMeasured", ""));
            //_dataTransferDict.Add("CoilWidth", new AirkanVariable("GLOBAL", "Basic", "CoilWidth", ""));
            //_dataTransferDict.Add("PartScrapLength", new AirkanVariable("GLOBAL", "Basic", "PartScrapLength", ""));
            //_dataTransferDict.Add("JobScrapLength", new AirkanVariable("GLOBAL", "Basic", "JobScrapLength", ""));
            //_dataTransferDict.Add("TotalLength", new AirkanVariable("GLOBAL", "Basic", "TotalLength", ""));

            //_dataTransferDict.Add("Bead", new AirkanVariable("GLOBAL", "DuctJob", "Bead", ""));
            //_dataTransferDict.Add("Brake", new AirkanVariable("GLOBAL", "DuctJob", "Brake", ""));
            //_dataTransferDict.Add("CleatMode", new AirkanVariable("GLOBAL", "DuctJob", "CleatMode", ""));
            //_dataTransferDict.Add("CleatType", new AirkanVariable("GLOBAL", "DuctJob", "CleatType", ""));
            //_dataTransferDict.Add("Damper", new AirkanVariable("GLOBAL", "DuctJob", "Damper", ""));
            //_dataTransferDict.Add("HoleSize", new AirkanVariable("GLOBAL", "DuctJob", "HoleSize", ""));
            //_dataTransferDict.Add("Insulation", new AirkanVariable("GLOBAL", "DuctJob", "Insulation", ""));
            //_dataTransferDict.Add("Length_1", new AirkanVariable("GLOBAL", "DuctJob", "Length_1", ""));
            //_dataTransferDict.Add("Length_2", new AirkanVariable("GLOBAL", "DuctJob", "Length_2", ""));
            //_dataTransferDict.Add("LockType", new AirkanVariable("GLOBAL", "DuctJob", "LockType", ""));
            //_dataTransferDict.Add("ConnTypeL", new AirkanVariable("GLOBAL", "DuctJob", "ConnTypeL", ""));
            //_dataTransferDict.Add("ConnTypeR", new AirkanVariable("GLOBAL", "DuctJob", "ConnTypeR", ""));
            //_dataTransferDict.Add("Cutouts", new AirkanVariable("GLOBAL", "DuctJob", "Cutouts", ""));
            //_dataTransferDict.Add("PinSpacing", new AirkanVariable("GLOBAL", "DuctJob", "PinSpacing", ""));
            //_dataTransferDict.Add("SealantUsed", new AirkanVariable("GLOBAL", "DuctJob", "SealantUsed", ""));
            //_dataTransferDict.Add("Sides", new AirkanVariable("GLOBAL", "DuctJob", "Sides", ""));
            //_dataTransferDict.Add("TieRodType_Leg_1", new AirkanVariable("GLOBAL", "DuctJob", "TieRodType_Leg_1", ""));
            //_dataTransferDict.Add("TieRodType_Leg_2", new AirkanVariable("GLOBAL", "DuctJob", "TieRodType_Leg_2", ""));
            //_dataTransferDict.Add("TieRodHoles_Leg_1", new AirkanVariable("GLOBAL", "DuctJob", "TieRodHoles_Leg_1", ""));
            //_dataTransferDict.Add("TieRodHoles_Leg_2", new AirkanVariable("GLOBAL", "DuctJob", "TieRodHoles_Leg_2", ""));
            //_dataTransferDict.Add("Type", new AirkanVariable("GLOBAL", "DuctJob", "Type", ""));

            //_dataTransferDict.Add("0", new AirkanVariable("GLOBAL", "Cutouts", "0", ""));
            //_dataTransferDict.Add("1", new AirkanVariable("GLOBAL", "Cutouts", "1", ""));
            //_dataTransferDict.Add("2", new AirkanVariable("GLOBAL", "Cutouts", "2", ""));
            //_dataTransferDict.Add("3", new AirkanVariable("GLOBAL", "Cutouts", "3", ""));
            //_dataTransferDict.Add("4", new AirkanVariable("GLOBAL", "Cutouts", "4", ""));
            //_dataTransferDict.Add("5", new AirkanVariable("GLOBAL", "Cutouts", "5", ""));
            //_dataTransferDict.Add("6", new AirkanVariable("GLOBAL", "Cutouts", "6", ""));
            //_dataTransferDict.Add("7", new AirkanVariable("GLOBAL", "Cutouts", "7", ""));

            //_dataTransferDict.Add("JobName", new AirkanVariable("GLOBAL", "JobName", "", ""));
            //_dataTransferDict.Add("DownloadNumber", new AirkanVariable("GLOBAL", "DownloadNumber", "", ""));
            //_dataTransferDict.Add("JobNum", new AirkanVariable("GLOBAL", "JobNum", "", ""));

            //_dataTransferDict.Add("ReferenceERP", new AirkanVariable("GLOBAL", "NeoPrintData", "ReferenceERP", ""));
            //_dataTransferDict.Add("DeliveryYardERP", new AirkanVariable("GLOBAL", "NeoPrintData", "DeliveryYardERP", ""));
            //_dataTransferDict.Add("CustomerOrderERP", new AirkanVariable("GLOBAL", "NeoPrintData", "CustomerOrderERP", ""));
            //_dataTransferDict.Add("BarCode", new AirkanVariable("GLOBAL", "NeoPrintData", "BarCode", ""));
            //_dataTransferDict.Add("PieceNumberERP", new AirkanVariable("GLOBAL", "NeoPrintData", "PieceNumberERP", ""));

            //_dataTransferDict.Add("CustomerOrderNumber", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerOrderNumber", ""));
            //_dataTransferDict.Add("CustomerAddress", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerAddress", ""));
            //_dataTransferDict.Add("CustomerName", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerName", ""));

            Reset();
        }

        private void Reset()
        {
            foreach (var key in _dataTransferDict.Keys)
            {
                _dataTransferDict[key].Value = null;
            }
        }

        public CommandStatus ProcessCommand(Cpu cpu, string commandName, string commandData)
        {
            lock (_syncLock)
            {
                switch (commandName)
                {
                    case "SetVariable":
                        Send(cpu, commandData);
                        break;

                    default:
                        break;
                }

                return new CommandStatus(0, String.Empty);
            }
        }

        public void Send(Cpu cpu, string commandData)
        {
            const string dataTrasferVariable = "DataTransfer";
            Variable dataTransferVariable = cpu.Variables[dataTrasferVariable];


            var list = new List<String>();
            list.Add("Basic.CutLength");
            list.Add("Basic.CoilGauge");
            list.Add("Basic.CoilWidthMeasured");
            list.Add("Basic.CoilWidth");
            list.Add("Basic.PartScrapLength");
            list.Add("Basic.JobScrapLength");
            list.Add("Basic.TotalLength");
            list.Add("DuctJob.Bead");
            list.Add("DuctJob.Brake");
            list.Add("DuctJob.CleatMode");
            list.Add("DuctJob.CleatType");
            list.Add("DuctJob.Damper");
            list.Add("DuctJob.HoleSize");
            list.Add("DuctJob.Insulation");
            list.Add("DuctJob.Length_1");
            list.Add("DuctJob.Length_2");
            list.Add("DuctJob.LockType");
            list.Add("DuctJob.ConnTypeL");
            list.Add("DuctJob.ConnTypeR");
            list.Add("DuctJob.Cutouts");
            list.Add("DuctJob.PinSpacing");
            list.Add("DuctJob.SealantUsed");
            list.Add("DuctJob.Sides");
            list.Add("DuctJob.TieRodType_Leg_1");
            list.Add("DuctJob.TieRodType_Leg_2");
            list.Add("DuctJob.TieRodHoles_Leg_1");
            list.Add("DuctJob.TieRodHoles_Leg_2");
            list.Add("DuctJob.Type");
            list.Add("Cutouts.[0]");
            list.Add("Cutouts.[1]");
            list.Add("Cutouts.[2]");
            list.Add("Cutouts.[3]");
            list.Add("Cutouts.[4]");
            list.Add("Cutouts.[5]");
            list.Add("Cutouts.[6]");
            list.Add("Cutouts.[7]");
            list.Add("JobName");
            list.Add("DownloadNumber");
            list.Add("JobNum");
            list.Add("NeoPrintData.ReferenceERP");
            list.Add("NeoPrintData.DeliveryYardERP");
            list.Add("NeoPrintData.CustomerOrderERP");
            list.Add("NeoPrintData.BarCode");
            list.Add("NeoPrintData.PieceNumberERP");
            list.Add("CustomerInfo.CustomerOrderNumber");
            list.Add("CustomerInfo.CustomerAddress");
            list.Add("CustomerInfo.CustomerName");

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
                    Variable variable = dataTransferVariable[airkanVariable.Address];

                    //if (airkanVariable.DataType.Equals("string", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    variable.Value = $"Test_Value_{counter}";
                    //}
                    //else
                    //{
                    //    variable.Value = counter;
                    //}

                    //counter += 1;

                }

                dataTransferVariable.WriteValue();
            }
        }

    }

    public class AirkanVariable
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public string Address { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }

        public AirkanVariable() { }
        public AirkanVariable(string taskName, string address, string value ) 
        {
            TaskName = taskName;
            Address = address;
            Value = value;

        }

    }
}

//Basic.CutLength
//Basic.CoilGauge
//Basic.CoilWidthMeasured
//Basic.CoilWidth
//Basic.PartScrapLength
//Basic.JobScrapLength
//Basic.TotalLength

//DuctJob.Bead
//DuctJob.Brake
//DuctJob.CleatMode
//DuctJob.CleatType
//DuctJob.Damper
//DuctJob.HoleSize
//DuctJob.Insulation
//DuctJob.Length_1
//DuctJob.Length_2
//DuctJob.LockType
//DuctJob.ConnTypeL
//DuctJob.ConnTypeR
//DuctJob.Cutouts
//DuctJob.PinSpacing
//DuctJob.SealantUsed
//DuctJob.Sides
//DuctJob.TieRodType_Leg_1
//DuctJob.TieRodType_Leg_2
//DuctJob.TieRodHoles_Leg_1
//DuctJob.TieRodHoles_Leg_2
//DuctJob.Type

//Cutouts.[0]
//Cutouts.[1]
//Cutouts.[2]
//Cutouts.[3]
//Cutouts.[4]
//Cutouts.[5]
//Cutouts.[6]
//Cutouts.[7]

//JobName

//DownloadNumber

//JobNum

//NeoPrintData.ReferenceERP
//NeoPrintData.DeliveryYardERP
//NeoPrintData.CustomerOrderERP
//NeoPrintData.BarCode
//NeoPrintData.PieceNumberERP

//CustomerInfo.CustomerOrderNumber
//CustomerInfo.CustomerAddress
//CustomerInfo.CustomerName


