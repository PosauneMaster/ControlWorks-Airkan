using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Batch;
using ControlWorks.Services.Rest.Models;
using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Mocks
{
    public class RecipeProcessorMock : IRecipeProcessor
    {
        public Task<RecipeActionResult> AddRecipe(Recipe recipe)
        {
            throw new NotImplementedException();
        }

        public Task<RecipeActionResult> Delete(string reference)
        {
            throw new NotImplementedException();
        }

        public Task<RecipeActionResult> DeleteAll()
        {
            throw new NotImplementedException();
        }

        public async Task<BinCollection> GetActiveRecipes()
        {
            return await Task.Run(GetActive);
        }

        private BinCollection GetActive()
        {
            var binCollection = new BinCollection();

            var bin1 = new Bin(1, "bin-1 reference", "bin-1 description");
            bin1.AddItem(new BinItem("item1", 10, "specification-1", 1));
            bin1.AddItem(new BinItem("item2", 11, "specification-2", 2));
            bin1.AddItem(new BinItem("item3", 12, "specification-3", 3));
            bin1.AddItem(new BinItem("item4", 13, "specification-4", 4));
            binCollection.AddBin(bin1);

            var bin2 = new Bin(2, "bin-2 reference", "bin-2 description");
            bin2.AddItem(new BinItem("item5", 20, "specification-5", 5));
            bin2.AddItem(new BinItem("item6", 21, "specification-6", 6));
            bin2.AddItem(new BinItem("item7", 22, "specification-7", 7));
            bin2.AddItem(new BinItem("item8", 23, "specification-8", 8));
            binCollection.AddBin(bin2);

            var bin3 = new Bin(3, "bin-3 reference", "bin-3 description");
            bin3.AddItem(new BinItem("item9", 30, "specification-9", 9));
            bin3.AddItem(new BinItem("item10", 31, "specification-10", 10));
            bin3.AddItem(new BinItem("item11", 32, "specification-11", 11));
            bin3.AddItem(new BinItem("item12", 33, "specification-12", 12));
            binCollection.AddBin(bin3);

            return binCollection;
        }

        public Task<BinCollection> GetCompleteRecipes()
        {
            throw new NotImplementedException();
        }

        public void IncrementItem(string recipeReference, string itemId)
        {
            throw new NotImplementedException();
        }
    }
}
