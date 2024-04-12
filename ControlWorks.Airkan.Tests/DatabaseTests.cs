using System.Configuration;
using ControlWorks.Services.PVI.Database;

using NUnit.Framework;

namespace ControlWorks.Airkan.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        [Test]
        public void Select_LF2024_ORDERS_Test()
        {
            var config = ConfigurationManager.ConnectionStrings;
            var results = AirkanOee.Select_LF2024_ORDERS();
        }

        [Test]
        public void Select_LF2024_PRODUCTION_Test()
        {
            var config = ConfigurationManager.ConnectionStrings;
            var results = AirkanOee.Select_LF2024_PRODUCTION();
        }

    }
}
