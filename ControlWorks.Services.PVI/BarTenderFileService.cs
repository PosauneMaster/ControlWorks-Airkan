using System.Diagnostics;
using System.Globalization;
using System.IO;
using System;
using System.Text;

using BR.AN.PviServices;

using ControlWorks.Common;

namespace ControlWorks.Services.PVI
{
    public class BarTenderFileService
    {

        public string BtwFileName { get; set; }
        public string Ordernummer { get; set; }
        public string Werf { get; set; }
        public string Klantreferentie { get; set; }
        public string Barcode { get; set; }
        public string Kader1 { get; set; }
        public string Kader2 { get; set; }
        public string Maat1 { get; set; }
        public string Maat2 { get; set; }
        public string Stuknr { get; set; }
        public string Type { get; set; }
        public string Lengte { get; set; }
        public string Dikte { get; set; }

        //%BTW% /AF="\\srvsql1\d$\ccs\navision\bartender Productie rechthoekig afmelden.btw" /PRN="KIOSK" /D="<Trigger File Name>" /DbTextHeader=3 /R=3 /P 
        //%END%
        //ordernummer;werf;klantreferentie;barcode; kaderl;kader2;maat1;maat2;stuknr;type;lengte;dikte
        //VO/23/012662; Société ERHYG; IO/H23/01384;2301266252093; E20;E20L; 100;200;8; BUIS;1600;0,75
        public BarTenderFileService() { }

        public string FileDetails(
            string btwFileName,
            string ordernummer,
            string werf,
            string klantreferentie,
            string barcode,
            string kader1,
            string kader2,
            string maat1,
            string maat2,
            string stuknr,
            string type,
            string lengte,
            string dikte)
        {
            BtwFileName = btwFileName;
            Ordernummer = ordernummer;
            Werf = werf;
            Klantreferentie = klantreferentie;
            Barcode = barcode;
            Kader1 = kader1;
            Kader2 = kader2;
            Maat1 = maat1;
            Maat2 = maat2;
            Stuknr = stuknr;
            Type = type;
            Lengte = lengte;
            Dikte = dikte;

            var sb = new StringBuilder();
            sb.AppendLine($@"%BTW% /AF=""\\srvsql1\d$\ccs\navision\bartender {btwFileName}"" /PRN=""KIOSK"" /D=""<Trigger File Name>"" /DbTextHeader=3 /R=3 /P");
            sb.AppendLine("%END%");
            sb.AppendLine("ordernummer;werf;klantreferentie;barcode; kaderl;kader2;maat1;maat2;stuknr;type;lengte;dikte");
            sb.AppendLine($"{ordernummer};{werf};{klantreferentie};{barcode};{kader1};{kader2};{maat1};{maat2};{stuknr};{type};{lengte};{dikte}");
            return sb.ToString();
        }
        public void ProcessBarCode(Cpu cpu, Variable variable = null)
        {
            if (variable != null)
            {
                if (!variable.Value)
                {
                    return;
                }
            }

            var dataTransferCompletedParts = cpu.Tasks["DataTrans1"].Variables["DataTransferCompletedParts"];

            if (!dataTransferCompletedParts.IsConnected)
            {
                return;
            }

            var btwFileName = dataTransferCompletedParts["FileName"].Value.ToString(CultureInfo.InvariantCulture);
            var ordernummer = dataTransferCompletedParts["NeoPrintData.CustomerOrderERP"].Value.ToString(CultureInfo.InvariantCulture);
            var werf = dataTransferCompletedParts["NeoPrintData.DeliveryYardERP"].Value.ToString(CultureInfo.InvariantCulture);
            var klantreferentie = dataTransferCompletedParts["CustomerInfo.CustomerName"].Value.ToString(CultureInfo.InvariantCulture);
            var barcode = dataTransferCompletedParts["NeoPrintData.BarCode"].Value.ToString(CultureInfo.InvariantCulture);
            var kader1 = dataTransferCompletedParts["DuctJob.ConnTypeR"].Value.ToString(CultureInfo.InvariantCulture);
            var kader2 = dataTransferCompletedParts["DuctJob.ConnTypeL"].Value.ToString(CultureInfo.InvariantCulture);
            var maat1 = dataTransferCompletedParts["DuctJob.Length_1"].Value.ToString(CultureInfo.InvariantCulture);
            var maat2 = dataTransferCompletedParts["DuctJob.Length_2"].Value.ToString(CultureInfo.InvariantCulture);
            var stuknr = dataTransferCompletedParts["NeoPrintData.PieceNumberERP"].Value.ToString(CultureInfo.InvariantCulture);
            var type = dataTransferCompletedParts["DuctJob.Type"].Value.ToString(CultureInfo.InvariantCulture);
            var lengte = dataTransferCompletedParts["Basic.CoilWidth"].Value.ToString(CultureInfo.InvariantCulture);
            var dikte = dataTransferCompletedParts["Basic.CoilGauge"].Value.ToString(CultureInfo.InvariantCulture);

            var fileDeDetails = FileDetails(btwFileName, ordernummer, werf, klantreferentie, barcode, kader1, kader2, maat1,
                maat2, stuknr, type, lengte, dikte);

            if (!String.IsNullOrEmpty(fileDeDetails))
            {
                if (Directory.Exists(ConfigurationProvider.AirkanBartenderFolder))
                {
                    var fi = new FileInfo(btwFileName);
                    var path = Path.Combine(ConfigurationProvider.AirkanBartenderFolder, fi.Name);
                    File.WriteAllText(path, fileDeDetails);
                    Trace.TraceInformation($"Created BarTender file {path}");
                }
                else
                {
                    Trace.TraceError($"Cannot file folder {ConfigurationProvider.AirkanBartenderFolder}.  Check setting for AirkanBartenderFolder");
                }
            }
            cpu.Tasks["DataTrans1"].Variables["ProductFinished"].Value.Assign(false);
            cpu.Tasks["DataTrans1"].Variables["ProductFinished"].WriteValue();

        }

    }
}
