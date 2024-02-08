using Newtonsoft.Json;

using System.Collections.Generic;

namespace ControlWorks.Services.PVI.Variables.Models
{
    public class VerizonOrderInfo
    {


        //SortLocation
        //ConveyorNumber
        //OrderNumber
        //RecordQuanity
        //TotalItems
        //Description
        //SortItems

        public int SortLocation { get; set; }
        public int ConveyorNumber { get; set; }
        public string OrderNumber { get; set; }
        public int RecordQuanity { get; set; }
        public int TotalItems { get; set; }
        public string Description { get; set; }
        public List<SortItem> Items { get; set; }

        public VerizonOrderInfo()
        {
            Items = new List<SortItem>();
        }

        public void AddItem(string skuNumber, string description, int quantity)
        {
            Items.Add(new SortItem(skuNumber, description, quantity));
        }

        public void AddItem(SortItem sortItem)
        {
            Items.Add(sortItem);
        }

        public void AddItemRange(IEnumerable<SortItem> sortItemList)
        {
            Items.AddRange(sortItemList);
        }



        public string ToJson()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            return json;
        }

        public static VerizonOrderInfo FromJson(string data)
        {
            return JsonConvert.DeserializeObject<VerizonOrderInfo>(data);
        }
    }

    public class SortItem
    {

        public string SKUnumberAddendum { get; set; }
        public string SKUnumber { get; set; }
        public string Description { get; set; }
        public int RequiredQuanity { get; set; }
        public int SortLocation { get; set; }
        public int ActualQty { get; set; }
        public SortItem()
        {
        }
        public SortItem(string skuNumber, string description, int quantity)
        {
            SKUnumber = skuNumber;
            Description = description;
            ActualQty = quantity;
        }
    }
}
