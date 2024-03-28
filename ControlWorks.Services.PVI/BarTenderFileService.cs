using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.PVI
{
    public class BarTenderFileService
    {
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
            string kaderl,
            string kader2,
            string maat1,
            string maat2,
            string stuknr,
            string type,
            string lengte,
            string dikte)
        {
            var sb = new StringBuilder();
            sb.AppendLine($@"%BTW% /AF=""\\srvsql1\d$\ccs\navision\bartender {btwFileName}"" /PRN=""KIOSK"" /D=""<Trigger File Name>"" /DbTextHeader=3 /R=3 /P");
            sb.AppendLine("%END%");
            sb.AppendLine("ordernummer;werf;klantreferentie;barcode; kaderl;kader2;maat1;maat2;stuknr;type;lengte;dikte");
            sb.AppendLine($"{ordernummer};{werf};{klantreferentie};{barcode};{kaderl};{kader2};{maat1};{maat2};{stuknr};{type};{lengte};{dikte}");
            return sb.ToString();
        }
    }
}
