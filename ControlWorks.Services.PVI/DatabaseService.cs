using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

using BR.AN.PviServices;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;

namespace ControlWorks.Services.PVI
{
    public interface IDatabaseService
    {
        bool WriteToOrderData(Variable orderVariable);
        bool WriteToOrderDataDatabase(Orders orderData);


    }
    public class DatabaseService
    {

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
    }
}
