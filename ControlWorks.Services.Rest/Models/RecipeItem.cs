using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class RecipeItem
    {
        public string Id { get; set; }
        public int Quantity { get; set; }
        public string Specification { get; set; }

        public override string ToString()
        {
            return $"Id={Id}, Quantity={Quantity}, Specification={Specification}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is RecipeItem r))
            {
                return false;
            }

            return ToString().Equals(r.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();

        }
    }

}
