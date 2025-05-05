using System.Configuration;
using ControlWorks.Common;
using ControlWorks.Services.PVI;
using ControlWorks.Services.PVI.Database;
using ControlWorks.Services.PVI.Models;
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

        [Test]
        public void WriteToOrderData_Test()
        {
            var orderData = new Orders
            {
                CustomerOrder = "CustomerOrder",
                Status = "Status",
                DateTime = "DateTime",
                Misc1 = "Misc1",
                Misc2 = "Misc2",
                Misc3 = "Misc3",
                Misc4 = "Misc4",
                Misc5 = "Misc5"
            };

            var dbService = new DatabaseService();
            dbService.WriteToOrderDataDatabase(orderData);
        }

        [Test]
        public void WriteToProductionData_Test()
        {
            var productionData = new Production
            {
                DateTime = "DateTime",
                CustomerOrder = "CustomerOrder",
                ShiptoName = "ShipToName",
                BilltoName = "BilltoName",
                YourReference = "YourReference",
                EntryNo = "EntryNo",
                JobName = "JobName",
                Qty = "Qty",
                SizeA = "SizeA",
                SizeB = "SizeB",
                CoilNumber = "CoilNumber",
                CoilGauge = "CoilGauge",
                CoilWidth = "CoilWidth",
                Misc1 = "Misc1",
                Misc2 = "Misc2",
                Misc3 = "Misc3",
            };

            var dbService = new DatabaseService();
            dbService.WriteProductionToDatabase(productionData);
        }

        [Test]
        public void OrdersConnectionString_IntegratedSecurity_True_Test()
        {
            var sqlConnectionService = new SqlConnectionService();
            var ordersConnectionString = sqlConnectionService.OrdersConnectionString();
        }

        [Test]
        public void OrdersConnectionString_IntegratedSecurity_False_Test()
        {
            var sqlConnectionService = new SqlConnectionService();
            var ordersConnectionString = sqlConnectionService.OrdersConnectionString();
        }

    }
}
