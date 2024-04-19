using ControlWorks.Services.PVI;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ControlWorks.Airkan.Tests
{
    [TestFixture]
    public class BarTenderFileServiceTests
    {
        [Test]
        public void FileTemplateTest()
        {
            string btwFileName = "Productie rechthoekig afmelden.btw";
            string ordernummer = "VO/23/012662";
            string werf = "Société ERHYG";
            string klantreferentie = "IO/H23/01384";
            string barcode = "2301266252093";
            string kaderl = "E20";
            string kader2 = "E20L";
            string maat1 = "100";
            string maat2 = "200";
            string stuknr = "8";
            string type = "BUIS";
            string lengte = "1600";
            string dikte = "0,75";

            var bartenderService = new BarTenderFileService();
            var fileDetails = bartenderService.FileDetails(btwFileName,ordernummer, werf, klantreferentie, barcode, kaderl, kader2, maat1,
                maat2, stuknr, type, lengte, dikte);

            var json = JsonConvert.SerializeObject(bartenderService);






        }
    }
}
