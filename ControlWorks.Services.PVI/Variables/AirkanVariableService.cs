using BR.AN.PviServices;

using ControlWorks.Services.PVI.Models;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ControlWorks.Services.PVI.Variables
{
    public class AirkanVariableService
    {
        const string dataTransferVariable = "DataTransfer";

        private static object _syncLock = new object();
        private readonly IEventNotifier _eventNotifier;
        private List<string> _variableNames;

        public AirkanVariableService(IEventNotifier eventNotifier)
        {
            _eventNotifier = eventNotifier;
            _eventNotifier.VariableValueChanged += _eventNotifier_VariableValueChanged;

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

        private void _eventNotifier_VariableValueChanged(object sender, PviApplicationEventArgs e)
        {
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

        public List<AirkanVariable> GetAirkanVariables(Cpu cpu)
        {
            var list = new List<AirkanVariable>();
            Variable dataTransfer = cpu.Variables[dataTransferVariable];

            foreach (var variableName in _variableNames)
            {
                var variable = dataTransfer[variableName];
                list.Add(new AirkanVariable(
                    variableName,
                    "GLOBAL",
                    variable.Address,
                    variable.Value,
                    variable.IECDataType.ToString()));
            }
            return list;
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
            foreach (Variable job  in jobQueue.Members)
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


