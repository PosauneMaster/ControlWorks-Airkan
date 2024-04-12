using ControlWorks.Common;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ControlWorks.Services.PVI.Database
{
    public static class AirkanOee
    {
        public static List<LF2024_ORDERS> Select_LF2024_ORDERS()
        {
            var list = new List<LF2024_ORDERS>();
            SqlDataReader reader = null;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("SELECT [CustomerOrder],[Status],[DateTime],[Misc1],[Misc2],[Misc3],[Misc4],[Misc5] ");
                sb.AppendLine("FROM [dbo].[LF2024_ORDERS]");

                using (var connection = new SqlConnection(ConfigurationProvider.AirkanConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sb.ToString();
                        command.CommandType = CommandType.Text;
                        reader = command.ExecuteReader();
                        var customerOrderOrdinal = reader.GetOrdinal("CustomerOrder");
                        var statusOrdinal = reader.GetOrdinal("Status");
                        var dateTimeOrdinal = reader.GetOrdinal("DateTime");
                        var misc1Ordinal = reader.GetOrdinal("Misc1");
                        var misc2Ordinal = reader.GetOrdinal("Misc2");
                        var misc3Ordinal = reader.GetOrdinal("Misc3");
                        var misc4Ordinal = reader.GetOrdinal("Misc4");
                        var misc5Ordinal = reader.GetOrdinal("Misc5");
                        while (reader.Read())
                        {
                            list.Add(
                                new LF2024_ORDERS
                                {
                                    CustomerOrder = reader.GetString(customerOrderOrdinal),
                                    Status = reader.GetString(statusOrdinal),
                                    DateTime = reader.GetString(dateTimeOrdinal),
                                    Misc1 = reader.GetString(misc1Ordinal),
                                    Misc2 = reader.GetString(misc2Ordinal),
                                    Misc3 = reader.GetString(misc3Ordinal),
                                    Misc4 = reader.GetString(misc4Ordinal),
                                    Misc5 = reader.GetString(misc5Ordinal)
                                });
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(
                    $"AirkanOee.{MethodBase.GetCurrentMethod()?.Name}: Error selecting LF2024_ORDERS\r\n{e.Message}",
                    e);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return list;

        }

        public static List<LF2024_PRODUCTION> Select_LF2024_PRODUCTION()
        {
            var list = new List<LF2024_PRODUCTION>();
            SqlDataReader reader = null;
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine(
                    "SELECT [DateTime],[CustomerOrder],[ShiptoName],[BilltoName],[YourReference],[EntryNo],[JobName] ");
                sb.AppendLine(",[Qty],[SizeA],[SizeB],[CoilNumber],[CoilGauge],[CoilWidth],[Misc1],[Misc2],[Misc3] ");
                sb.AppendLine("FROM [dbo].[LF2024_PRODUCTION]");

                using (var connection = new SqlConnection(ConfigurationProvider.AirkanConnectionString))
                {
                    connection.Open();
                    using (SqlCommand command = connection.CreateCommand())
                    {
                        command.CommandText = sb.ToString();
                        command.CommandType = CommandType.Text;
                        reader = command.ExecuteReader();
                        var dateTimeOrdinal = reader.GetOrdinal("DateTime");
                        var customerOrderOrdinal = reader.GetOrdinal("CustomerOrder");
                        var shiptoNameOrdinal = reader.GetOrdinal("ShiptoName");
                        var billtoNameOrdinal = reader.GetOrdinal("BilltoName");
                        var yourReferenceOrdinal = reader.GetOrdinal("YourReference");
                        var entryNoOrdinal = reader.GetOrdinal("EntryNo");
                        var jobNameOrdinal = reader.GetOrdinal("JobName");
                        var qtyOrdinal = reader.GetOrdinal("Qty");
                        var sizeAOrdinal = reader.GetOrdinal("SizeA");
                        var sizeBOrdinal = reader.GetOrdinal("SizeB");
                        var coilNumberOrdinal = reader.GetOrdinal("CoilNumber");
                        var coilGaugeOrdinal = reader.GetOrdinal("CoilGauge");
                        var coilWidthOrdinal = reader.GetOrdinal("CoilWidth");
                        var misc1Ordinal = reader.GetOrdinal("Misc1");
                        var misc2Ordinal = reader.GetOrdinal("Misc2");
                        var misc3Ordinal = reader.GetOrdinal("Misc3");

                        while (reader.Read())
                        {
                            list.Add(
                                new LF2024_PRODUCTION
                                {
                                    DateTime = reader.GetString(dateTimeOrdinal),
                                    CustomerOrder = reader.GetString(customerOrderOrdinal),
                                    ShiptoName = reader.GetString(shiptoNameOrdinal),
                                    BilltoName = reader.GetString(billtoNameOrdinal),
                                    YourReference = reader.GetString(yourReferenceOrdinal),
                                    EntryNo = reader.GetString(entryNoOrdinal),
                                    JobName = reader.GetString(jobNameOrdinal),
                                    Qty = reader.GetString(qtyOrdinal),
                                    SizeA = reader.GetString(sizeAOrdinal),
                                    SizeB = reader.GetString(sizeBOrdinal),
                                    CoilNumber = reader.GetString(coilNumberOrdinal),
                                    CoilGauge = reader.GetString(coilGaugeOrdinal),
                                    CoilWidth = reader.GetString(coilWidthOrdinal),
                                    Misc1 = reader.GetString(misc1Ordinal),
                                    Misc2 = reader.GetString(misc2Ordinal),
                                    Misc3 = reader.GetString(misc3Ordinal),
                                });
                        }

                        reader.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(
                    $"AirkanOee.{MethodBase.GetCurrentMethod()?.Name}: Error selecting LF2024_ORDERS\r\n{e.Message}",
                    e);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
            }

            return list;

        }
    }

    public class LF2024_PRODUCTION
    {
        public string DateTime { get; set; }
        public string CustomerOrder { get; set; }
        public string ShiptoName { get; set; }
        public string BilltoName { get; set; }
        public string YourReference { get; set; }
        public string EntryNo { get; set; }
        public string JobName { get; set; }
        public string Qty { get; set; }
        public string SizeA { get; set; }
        public string SizeB { get; set; }
        public string CoilNumber { get; set; }
        public string CoilGauge { get; set; }
        public string CoilWidth { get; set; }
        public string Misc1 { get; set; }
        public string Misc2 { get; set; }
        public string Misc3 { get; set; }

        public LF2024_PRODUCTION()
        {
        }
    }

    public class LF2024_ORDERS
    {
        public string CustomerOrder { get; set; }
        public string Status { get; set; }
        public string DateTime { get; set; }
        public string Misc1 { get; set; }
        public string Misc2 { get; set; }
        public string Misc3 { get; set; }
        public string Misc4 { get; set; }
        public string Misc5 { get; set; }

        public LF2024_ORDERS()
        {
        }

        public LF2024_ORDERS(string customerOrder, string status, string dateTime, string misc1, string misc2,
            string misc3, string misc4, string misc5)
        {
            CustomerOrder = customerOrder;
            Status = status;
            DateTime = dateTime;
            Misc1 = misc1;
            Misc2 = misc2;
            Misc3 = misc3;
            Misc4 = misc4;
            Misc5 = misc5;
        }
    }
}
