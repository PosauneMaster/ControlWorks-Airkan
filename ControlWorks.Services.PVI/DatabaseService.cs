using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;
using Exception = BR.AN.PviServices.Exception;

namespace ControlWorks.Services.PVI
{
    public interface IDatabaseService
    {
        bool WriteToProductionData(Variable productionVariable);
        bool WriteToOrderData(Variable orderVariable);
        bool WriteToOrderDataDatabase(Orders orderData);
        bool WriteProductionToDatabase(Production productionData);


    }
    public class DatabaseService
    {
        public bool WriteToProductionData(Variable productionVariable)
        {
            var productionData = new Production
            {
                DateTime = productionVariable.Members["DateTime"].Value.ToString(CultureInfo.InvariantCulture),
                CustomerOrder = productionVariable.Members["CustomerOrder"].Value.ToString(CultureInfo.InvariantCulture),
                ShiptoName = productionVariable.Members["ShiptoName"].Value.ToString(CultureInfo.InvariantCulture),
                BilltoName = productionVariable.Members["BilltoName"].Value.ToString(CultureInfo.InvariantCulture),
                YourReference = productionVariable.Members["YourReference"].Value.ToString(CultureInfo.InvariantCulture),
                EntryNo = productionVariable.Members["EntryNo"].Value.ToString(CultureInfo.InvariantCulture),
                JobName = productionVariable.Members["JobName"].Value.ToString(CultureInfo.InvariantCulture),
                Qty = productionVariable.Members["Qty"].Value.ToString(CultureInfo.InvariantCulture),
                SizeA = productionVariable.Members["SizeA"].Value.ToString(CultureInfo.InvariantCulture),
                SizeB = productionVariable.Members["SizeB"].Value.ToString(CultureInfo.InvariantCulture),
                CoilNumber = productionVariable.Members["CoilNumber"].Value.ToString(CultureInfo.InvariantCulture),
                CoilGauge = productionVariable.Members["CoilGauge"].Value.ToString(CultureInfo.InvariantCulture),
                CoilWidth = productionVariable.Members["CoilWidth"].Value.ToString(CultureInfo.InvariantCulture),
                Misc1 = productionVariable.Members["Misc1"].Value.ToString(CultureInfo.InvariantCulture),
                Misc2 = productionVariable.Members["Misc2"].Value.ToString(CultureInfo.InvariantCulture),
                Misc3 = productionVariable.Members["Misc3"].Value.ToString(CultureInfo.InvariantCulture)
            };

            return WriteProductionToDatabase(productionData);
        }

        public bool WriteProductionToDatabase(Production productionData)
        {
            try
            {
                var sbSql = new StringBuilder();
                sbSql.AppendLine("INSERT INTO [dbo].[LF2024_PRODUCTION]");
                sbSql.AppendLine("([DateTime],[CustomerOrder],[ShiptoName],[BilltoName],[YourReference],[EntryNo],[JobName],");
                sbSql.AppendLine("[Qty],[SizeA],[SizeB],[CoilNumber],[CoilGauge],[CoilWidth],[Misc1],[Misc2],[Misc3])");
                sbSql.AppendLine("VALUES");
                sbSql.AppendLine("(@DateTime,@CustomerOrder,@ShiptoName,@BilltoName,@YourReference,@EntryNo,@JobName,@Qty,@SizeA,@SizeB,");
                sbSql.AppendLine("@CoilNumber,@CoilGauge,@CoilWidth,@Misc1,@Misc2,@Misc3)");


                using (var connection = new SqlConnection(ConfigurationProvider.AirkanConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.Parameters.AddWithValue("DateTime", productionData.DateTime);
                        command.Parameters.AddWithValue("CustomerOrder", productionData.CustomerOrder);
                        command.Parameters.AddWithValue("ShiptoName", productionData.ShiptoName);
                        command.Parameters.AddWithValue("BilltoName", productionData.BilltoName);
                        command.Parameters.AddWithValue("YourReference", productionData.YourReference);
                        command.Parameters.AddWithValue("EntryNo", productionData.EntryNo);
                        command.Parameters.AddWithValue("JobName", productionData.JobName);
                        command.Parameters.AddWithValue("Qty", productionData.Qty);
                        command.Parameters.AddWithValue("SizeA", productionData.SizeA);
                        command.Parameters.AddWithValue("SizeB", productionData.SizeB);
                        command.Parameters.AddWithValue("CoilNumber", productionData.CoilGauge);
                        command.Parameters.AddWithValue("CoilGauge", productionData.CoilGauge);
                        command.Parameters.AddWithValue("CoilWidth", productionData.CoilWidth);
                        command.Parameters.AddWithValue("Misc1", productionData.Misc1);
                        command.Parameters.AddWithValue("Misc2", productionData.Misc2);
                        command.Parameters.AddWithValue("Misc3", productionData.Misc3);

                        command.CommandText = sbSql.ToString();
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError($"DatabaseService.WriteToOrderDataDatabase: {ex.Message}\r\n{ex}");
                return false;
            }
        }

        public bool WriteToOrderData(Variable orderVariable)
        {

            var orderData = new Orders
            {
                CustomerOrder = orderVariable.Members["CustomerOrder"].Value.ToString(CultureInfo.InvariantCulture),
                Status = orderVariable.Members["Status"].Value.ToString(CultureInfo.InvariantCulture),
                DateTime = orderVariable.Members["DateTime"].Value.ToString(CultureInfo.InvariantCulture),
                Misc1 = orderVariable.Members["Misc1"].Value.ToString(CultureInfo.InvariantCulture),
                Misc2 = orderVariable.Members["Misc2"].Value.ToString(CultureInfo.InvariantCulture),
                Misc3 = orderVariable.Members["Misc3"].Value.ToString(CultureInfo.InvariantCulture),
                Misc4 = orderVariable.Members["Misc4"].Value.ToString(CultureInfo.InvariantCulture),
                Misc5 = orderVariable.Members["Misc5"].Value.ToString(CultureInfo.InvariantCulture)
            };

            return WriteToOrderDataDatabase(orderData);
        }

        public bool WriteToOrderDataDatabase(Orders orderData)
        {
            try
            {
                var sbSql = new StringBuilder();
                sbSql.AppendLine("INSERT INTO [dbo].[LF2024_ORDERS]");
                sbSql.AppendLine("([CustomerOrder],[Status],[DateTime],[Misc1],[Misc2],[Misc3],[Misc4],[Misc5])");
                sbSql.AppendLine("VALUES");
                sbSql.AppendLine("(@CustomerOrder, @Status, @DateTime, @Misc1, @Misc2, @Misc3, @Misc4, @Misc5)");

                using (var connection = new SqlConnection(ConfigurationProvider.AirkanConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.Parameters.AddWithValue("CustomerOrder", orderData.CustomerOrder);
                        command.Parameters.AddWithValue("Status", orderData.Status);
                        command.Parameters.AddWithValue("DateTime", orderData.DateTime);
                        command.Parameters.AddWithValue("Misc1", orderData.Misc1);
                        command.Parameters.AddWithValue("Misc2", orderData.Misc2);
                        command.Parameters.AddWithValue("Misc3", orderData.Misc3);
                        command.Parameters.AddWithValue("Misc4", orderData.Misc4);
                        command.Parameters.AddWithValue("Misc5", orderData.Misc5);

                        command.CommandText = sbSql.ToString();
                        command.CommandType = CommandType.Text;
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }

                return true;
            }
            catch (System.Exception ex)
            {
                Trace.TraceError($"DatabaseService.WriteToOrderDataDatabase: {ex.Message}\r\n{ex}");
                return false;
            }
        }
    }
}
