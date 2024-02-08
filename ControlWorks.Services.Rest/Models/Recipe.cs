using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Models
{
    public class Recipe
    {
        public string Reference { get; set; }
        public string Description { get; set; }
        public RecipeItem[] Items { get; set; }

        public Recipe() { }
        public Recipe(string reference, string description, IEnumerable<RecipeItem> recipeItems)
        {
            Reference = reference;
            Description = description;
            Items = new List<RecipeItem>(recipeItems).ToArray();
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
}
