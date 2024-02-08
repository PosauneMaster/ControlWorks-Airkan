using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class UnmatchedRecipeItem
    {
        [JsonProperty(PropertyName = "id")]
        public string ItemId { get; set; }
        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }
        [JsonProperty(PropertyName = "specification")]
        public string Specification { get; set; }

        public UnmatchedRecipeItem()
        {
        }

        public UnmatchedRecipeItem(string itemId, string reference, string specification)
        {
            ItemId = itemId;
            Reference = reference;
            Specification = specification;
        }

        public override string ToString()
        {
            return $"Id={ItemId}, Reference={Reference}, Specification={Specification}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UnmatchedRecipeItem u))
            {
                return false;
            }

            return ToString().Equals(u.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();

        }
    }
}
