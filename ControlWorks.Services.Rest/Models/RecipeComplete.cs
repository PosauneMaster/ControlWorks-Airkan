using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Models
{
    public class RecipeComplete
    {
        [JsonProperty(PropertyName = "sortingBin")]
        public int BinId { get; set; }
        [JsonProperty(PropertyName = "reference")]
        public string Reference { get; set; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
        [JsonProperty(PropertyName = "items")]
        public RecipeCompleteItem[] Items { get; set; }

        public RecipeComplete() { }
        public RecipeComplete(string reference, string description, IEnumerable<RecipeCompleteItem> recipeItems)
        {
            Reference = reference;
            Description = description;
            Items = new List<RecipeCompleteItem>(recipeItems).ToArray();
        }

        public override string ToString()
        {
            var items = String.Join(",", Items.Select(i => i.ToString()));
            return $"Reference = {Reference}, Description={Description}, Items={items}";
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Recipe r))
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

    public class RecipeCompleteItem
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
        [JsonProperty(PropertyName = "specification")]
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
