using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;
using ControlWorks.Services.PVI.Variables.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        private static object _syncLock = new object();
        private readonly Dictionary<string, AirkanVariable> _dataTransferDict;

        public AirkanVariableService()
        {
            _dataTransferDict.Add("CutLength", new AirkanVariable("GLOBAL", "Basic", "CutLength", ""));
            _dataTransferDict.Add("CoilGauge", new AirkanVariable("GLOBAL", "Basic", "CoilGauge", ""));
            _dataTransferDict.Add("CoilWidthMeasured", new AirkanVariable("GLOBAL", "Basic", "CoilWidthMeasured", ""));
            _dataTransferDict.Add("CoilWidth", new AirkanVariable("GLOBAL", "Basic", "CoilWidth", ""));
            _dataTransferDict.Add("PartScrapLength", new AirkanVariable("GLOBAL", "Basic", "PartScrapLength", ""));
            _dataTransferDict.Add("JobScrapLength", new AirkanVariable("GLOBAL", "Basic", "JobScrapLength", ""));
            _dataTransferDict.Add("TotalLength", new AirkanVariable("GLOBAL", "Basic", "TotalLength", ""));

            _dataTransferDict.Add("Bead", new AirkanVariable("GLOBAL", "DuctJob", "Bead", ""));
            _dataTransferDict.Add("Brake", new AirkanVariable("GLOBAL", "DuctJob", "Brake", ""));
            _dataTransferDict.Add("CleatMode", new AirkanVariable("GLOBAL", "DuctJob", "CleatMode", ""));
            _dataTransferDict.Add("CleatType", new AirkanVariable("GLOBAL", "DuctJob", "CleatType", ""));
            _dataTransferDict.Add("Damper", new AirkanVariable("GLOBAL", "DuctJob", "Damper", ""));
            _dataTransferDict.Add("HoleSize", new AirkanVariable("GLOBAL", "DuctJob", "HoleSize", ""));
            _dataTransferDict.Add("Insulation", new AirkanVariable("GLOBAL", "DuctJob", "Insulation", ""));
            _dataTransferDict.Add("Length_1", new AirkanVariable("GLOBAL", "DuctJob", "Length_1", ""));
            _dataTransferDict.Add("Length_2", new AirkanVariable("GLOBAL", "DuctJob", "Length_2", ""));
            _dataTransferDict.Add("LockType", new AirkanVariable("GLOBAL", "DuctJob", "LockType", ""));
            _dataTransferDict.Add("ConnTypeL", new AirkanVariable("GLOBAL", "DuctJob", "ConnTypeL", ""));
            _dataTransferDict.Add("ConnTypeR", new AirkanVariable("GLOBAL", "DuctJob", "ConnTypeR", ""));
            _dataTransferDict.Add("Cutouts", new AirkanVariable("GLOBAL", "DuctJob", "Cutouts", ""));
            _dataTransferDict.Add("PinSpacing", new AirkanVariable("GLOBAL", "DuctJob", "PinSpacing", ""));
            _dataTransferDict.Add("SealantUsed", new AirkanVariable("GLOBAL", "DuctJob", "SealantUsed", ""));
            _dataTransferDict.Add("Sides", new AirkanVariable("GLOBAL", "DuctJob", "Sides", ""));
            _dataTransferDict.Add("TieRodType_Leg_1", new AirkanVariable("GLOBAL", "DuctJob", "TieRodType_Leg_1", ""));
            _dataTransferDict.Add("TieRodType_Leg_2", new AirkanVariable("GLOBAL", "DuctJob", "TieRodType_Leg_2", ""));
            _dataTransferDict.Add("TieRodHoles_Leg_1", new AirkanVariable("GLOBAL", "DuctJob", "TieRodHoles_Leg_1", ""));
            _dataTransferDict.Add("TieRodHoles_Leg_2", new AirkanVariable("GLOBAL", "DuctJob", "TieRodHoles_Leg_2", ""));
            _dataTransferDict.Add("Type", new AirkanVariable("GLOBAL", "DuctJob", "Type", ""));

            _dataTransferDict.Add("0", new AirkanVariable("GLOBAL", "Cutouts", "0", ""));
            _dataTransferDict.Add("1", new AirkanVariable("GLOBAL", "Cutouts", "1", ""));
            _dataTransferDict.Add("2", new AirkanVariable("GLOBAL", "Cutouts", "2", ""));
            _dataTransferDict.Add("3", new AirkanVariable("GLOBAL", "Cutouts", "3", ""));
            _dataTransferDict.Add("4", new AirkanVariable("GLOBAL", "Cutouts", "4", ""));
            _dataTransferDict.Add("5", new AirkanVariable("GLOBAL", "Cutouts", "5", ""));
            _dataTransferDict.Add("6", new AirkanVariable("GLOBAL", "Cutouts", "6", ""));
            _dataTransferDict.Add("7", new AirkanVariable("GLOBAL", "Cutouts", "7", ""));

            _dataTransferDict.Add("JobName", new AirkanVariable("GLOBAL", "JobName", "", ""));
            _dataTransferDict.Add("DownloadNumber", new AirkanVariable("GLOBAL", "DownloadNumber", "", ""));
            _dataTransferDict.Add("JobNum", new AirkanVariable("GLOBAL", "JobNum", "", ""));

            _dataTransferDict.Add("ReferenceERP", new AirkanVariable("GLOBAL", "NeoPrintData", "ReferenceERP", ""));
            _dataTransferDict.Add("DeliveryYardERP", new AirkanVariable("GLOBAL", "NeoPrintData", "DeliveryYardERP", ""));
            _dataTransferDict.Add("CustomerOrderERP", new AirkanVariable("GLOBAL", "NeoPrintData", "CustomerOrderERP", ""));
            _dataTransferDict.Add("BarCode", new AirkanVariable("GLOBAL", "NeoPrintData", "BarCode", ""));
            _dataTransferDict.Add("PieceNumberERP", new AirkanVariable("GLOBAL", "NeoPrintData", "PieceNumberERP", ""));

            _dataTransferDict.Add("CustomerOrderNumber", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerOrderNumber", ""));
            _dataTransferDict.Add("CustomerAddress", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerAddress", ""));
            _dataTransferDict.Add("CustomerName", new AirkanVariable("GLOBAL", "CustomerInfo", "CustomerName", ""));

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
                    case "AddOrderInfo":
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
            var data = JsonConvert.DeserializeObject<List<AirkanVariable>>(commandData);
            if (data != null)
            {
                foreach (AirkanVariable airkanVariable in data)
                {
                    if (String.IsNullOrEmpty(airkanVariable.Member))
                    {
                    }
                }
            }
        }

    }

    public class AirkanVariable
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public string VariableName { get; set; }
        public string Member { get; set; }
        public string Value { get; set; }

        public AirkanVariable() { }
        public AirkanVariable(string taskName, string variableName, string member, string value ) 
        {
            TaskName = taskName;
            Member = member;
            VariableName = variableName;
            Value = value;
            if (String.IsNullOrEmpty(member))
            {
                Name = variableName;
            }
            else
            {
                Name = member;
            }

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


