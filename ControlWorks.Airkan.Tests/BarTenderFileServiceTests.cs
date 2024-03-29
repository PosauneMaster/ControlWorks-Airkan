﻿using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Airkan.Tests
{
    [TestFixture]
    public class BarTenderFileServiceTests
    {
        public void FileTemplateTest()
        {
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
        }
    }
}
