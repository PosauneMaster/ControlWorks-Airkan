using System.Collections.Generic;

using AutoMapper;

using ControlWorks.Services.Rest.Models;

namespace ControlWorks.Services.Rest
{

    public interface IBinService
    {
        BinCollection CreateActive(IEnumerable<RecipeActiveDto> recipes);
        BinCompleteCollection CreateComplete(IEnumerable<RecipeComplete> recipes);

    }

    public class BinService : IBinService
    {
        private readonly IMapper _mapper;

        public BinService()
        {

        }

        public BinService(IMapper mapper)
        {
            _mapper = mapper;
        }

        public BinCompleteCollection CreateComplete(IEnumerable<RecipeComplete> recipes)
        {
            var binCollection = new BinCompleteCollection();

            foreach (var recipe in recipes)
            {
                var bin = new BinComplete(recipe.BinId, recipe.Reference, recipe.Description);
                foreach (var item in recipe.Items)
                {
                    var binItem = _mapper.Map<BinItemComplete>(item);
                    bin.AddItem(binItem);
                }

                binCollection.AddBin(bin);
            }

            return binCollection;
        }



        public BinCollection CreateActive(IEnumerable<RecipeActiveDto> recipes)
        {
            var binCollection = new BinCollection();

            foreach (var recipe in recipes)
            {
                var bin = new Bin(recipe.BinId, recipe.Reference, recipe.Description);
                foreach (var item in recipe.Items)
                {
                    var binItem = _mapper.Map<BinItem>(item);
                    bin.AddItem(binItem);
                }

                binCollection.AddBin(bin);
            }

            return binCollection;
        }
    }
}
