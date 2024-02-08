using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlWorks.Services.Rest.Models;

namespace ControlWorks.Services.Rest
{
    public interface IRecipeService
    {
        bool IsComplete(RecipeActiveDto recipe);
    }

    public class RecipeService : IRecipeService
    {
        public bool IsComplete(RecipeActiveDto recipe)
        {
            foreach (var item in recipe.Items)
            {
                if (item.QuantityScanned < item.Quantity)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
