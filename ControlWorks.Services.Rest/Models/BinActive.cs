using ControlWorks.Services.PVI.Variables.Models;

using Newtonsoft.Json;

using System.Collections.Generic;

namespace ControlWorks.Services.Rest.Models
{
    public class BinMapper
    {
        public BinCollection Map(IEnumerable<VerizonOrderInfo> verizonOrderCollection)
        {
            var binCollection = new BinCollection();
            foreach (var orderInfo in verizonOrderCollection)
            {
                var bin = new Bin(orderInfo.SortLocation, orderInfo.OrderNumber, orderInfo.Description);
                foreach (var orderItem in orderInfo.Items)
                {
                    var binItem = new BinItem(orderItem.SKUnumber, orderItem.RequiredQuanity, orderItem.Description,
                        orderItem.ActualQty);
                    bin.AddItem(binItem);

                }

                binCollection.AddBin(bin);
            }

            return binCollection;
        }
    }


    public class BinCollection
    {
        private readonly List<Bin> _bins;

        public BinCollection()
        {
            _bins = new List<Bin>();
        }

        [JsonProperty(PropertyName = "bins")] public Bin[] Bins => _bins.ToArray();

        public void AddBin(Bin bin)
        {
            _bins.Add(bin);
        }
    }

    public class Bin
    {
        private List<BinItem> _items;

        public Bin()
        {

        }

        public Bin(int sortingBin, string reference, string description)
        {
            _items = new List<BinItem>();

            SortingBin = sortingBin;
            Reference = reference;
            Description = description;
        }

        [JsonProperty(PropertyName = "sortingBin")]
        public int SortingBin { get; set; }

        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "items")] public BinItem[] Items => _items.ToArray();

        public void AddItem(BinItem item)
        {
            if (_items == null)
            {
                _items = new List<BinItem>();
            }

            _items.Add(item);
        }

    }

    public class BinItem
    {
        public BinItem()
        {
        }

        public BinItem(string id, int quantity, string specification, int quantityScanned)
        {
            Id = id;
            Quantity = quantity;
            Specification = specification;
            QuantityScanned = quantityScanned;
        }

        [JsonProperty(PropertyName = "id")] public string Id { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "specification")]
        public string Specification { get; set; }

        [JsonProperty(PropertyName = "quantity-scanned")]
        public int QuantityScanned { get; set; }
    }
}



