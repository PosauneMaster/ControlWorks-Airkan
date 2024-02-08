using System.Collections.Generic;

using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class BinCompleteCollection
    {
        private readonly List<BinComplete> _bins;

        public BinCompleteCollection()
        {
            _bins = new List<BinComplete>();
        }

        [JsonProperty(PropertyName = "bins")] 
        public BinComplete[] Bins => _bins.ToArray();

        public void AddBin(BinComplete bin)
        {
            _bins.Add(bin);
        }
    }

    public class BinComplete
    {
        private List<BinItemComplete> _items;

        public BinComplete()
        {

        }

        public BinComplete(int sortingBin, string reference, string description)
        {
            _items = new List<BinItemComplete>();

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

        [JsonProperty(PropertyName = "items")] public BinItemComplete[] Items => _items.ToArray();

        public void AddItem(BinItemComplete item)
        {
            if (_items == null)
            {
                _items = new List<BinItemComplete>();
            }

            _items.Add(item);
        }
    }
}
