using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class BinItemActive
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Specification { get; set; }
        public int QuantityScanned { get; set; }

        public BinItemActive()
        {
        }

        public BinItemActive(string id, int quantity, string specification)
        {
            Id = id;
            Quantity = quantity;
            Specification = specification;
        }

        public override string ToString()
        {
            return $"Id={Id}, Quantity={Quantity}, Specification={Specification}, QuantityScanned={QuantityScanned}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is BinItemActive bi))
            {
                return false;
            }

            return ToString().Equals(bi.ToString());
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
