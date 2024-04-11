using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlWorks.Common;

namespace ControlWorks.Services.PVI.Database
{
    public class AirkanOee
    {
        public void Select_LF2024_ORDERS()
        {
            var sb = new StringBuilder();
            sb.AppendLine("SELECT [CustomerOrder],[Status],[DateTime],[Misc1],[Misc2],[Misc3],[Misc4],[Misc5]");
            sb.AppendLine("FROM [dbo].[LF2024_ORDERS]");
        }

        //public void Insert_LF2024_ORDERS()
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
