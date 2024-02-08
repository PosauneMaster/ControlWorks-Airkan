using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlWorks.Services.Rest.Models;

namespace ControlWorks.Services.Rest.Data
{
    public class BinCollection
    {
        public Bin[] Bins { get; set; }
    }

    public class Bin
    {
        public int SortingBin { get; set; }
        public string Reference { get; set; }
        public string Description { get; set; }
        public Item[] Items { get; set; }
    }

    public class Item
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Specification { get; set; }
        public int Quantityscanned { get; set; }
    }
}
