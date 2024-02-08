using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BR.AN.PviServices;
using ControlWorks.Services.PVI.Variables.Models;

namespace ControlWorks.Services.PVI
{
    public class AddOrderService
    {
        private readonly ConcurrentDictionary<string, VerizonOrderInfo> _orderCache;
        private const string _orderInfoName = "OrderInfo";

        public AddOrderService()
        {
            _orderCache = new ConcurrentDictionary<string, VerizonOrderInfo>();
        }
        public void AddOrder(Cpu cpu, VerizonOrderInfo orderInfo)
        {
            _orderCache.TryAdd(orderInfo.OrderNumber, orderInfo);



            if (cpu.Variables.ContainsKey(_orderInfoName))
            {
                Variable orderInfoVariable = cpu.Variables[_orderInfoName];
                orderInfoVariable.Members["Description"].Value = orderInfo.Description;
                orderInfoVariable.Members["RecordQuantity"].Value = orderInfo.RecordQuanity;

            }
        }
    }
}
