using System;
using System.Collections.Generic;
using System.Diagnostics;

using AutoMapper;

using ControlWorks.Common;
using ControlWorks.Services.PVI.Models;
using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.Rest.Models;

using LiteDB;

namespace ControlWorks.Services.Rest.Processors
{
    public interface IItemProcessor
    {
        //List<UnmatchedRecipeItem> GetUnmatchedItems();
        List<UnmatchedItem> GetUnmatchedItems();

        RecipeActionResult DeleteAllUnmatchedItems();
        RecipeActionResult DeleteUnmatchedItem(string itemId, string reference);
        RecipeActionResult AddUnmatchedItems(string itemId, string reference, string specification);

    }

    public class ItemProcessor : IItemProcessor
    {
        private readonly string _unmatchedItems = "unmatchedItems";

        private readonly IMapper _mapper;

        private IPviApplication _pviApplication;

        public ItemProcessor(IMapper mapper, IPviApplication pviApplication)
        {
            _pviApplication = pviApplication;
            _mapper = mapper;
        }

        //public List<UnmatchedRecipeItem> GetUnmatchedItems()
            public List<UnmatchedItem> GetUnmatchedItems()

        {
            try
            {

                return _pviApplication.GetUnmatchedItems();
                //var list = new List<UnmatchedRecipeItem>();
                //using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
                //{
                //    var recipesActiveCol = db.GetCollection<UnmatchedRecipeItemDto>(_unmatchedItems);

                //    var items = recipesActiveCol.FindAll();

                //    if (items != null)
                //    {
                //        foreach (var item in items)
                //        {
                //            var unmatched = _mapper.Map<UnmatchedRecipeItem>(item);
                //            list.Add(unmatched);
                //        }
                //    }
                //}

                return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"RecipeProcessor.GetUnmatchedItems. {ex.Message}\r\n", ex);
                throw;
            }
        }

        public RecipeActionResult AddUnmatchedItems(string itemId, string reference, string specification)
        {
            try
            {
                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
                {
                    var recipesActiveCol = db.GetCollection<UnmatchedRecipeItemDto>(_unmatchedItems);

                    var item = new UnmatchedRecipeItemDto(itemId, reference, specification);
                    recipesActiveCol.Insert(item);

                    return new RecipeActionResult(true, String.Empty);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"RecipeProcessor.AddUnmatchedItems. {ex.Message}\r\n", ex);
                throw;
            }

        }

        public RecipeActionResult DeleteUnmatchedItem(string itemId, string reference)
        {
            try
            {
                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
                {
                    var recipesActiveCol = db.GetCollection<UnmatchedRecipeItemDto>(_unmatchedItems);

                    var items = recipesActiveCol.Find(i => i.ItemId == itemId && i.Reference == reference);

                    foreach (var item in items)
                    {
                        recipesActiveCol.Delete(item.Id);
                    }

                    return new RecipeActionResult(true, $"Deleted itemId {itemId}, reference {reference}");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"RecipeProcessor.DeleteUnmatchedItems. {ex.Message}\r\n", ex);
                throw;
            }
        }

        public RecipeActionResult DeleteAllUnmatchedItems()
        {
            try
            {
                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
                {
                    var recipesActiveCol = db.GetCollection<UnmatchedRecipeItemDto>(_unmatchedItems);
                    recipesActiveCol.DeleteAll();

                    return new RecipeActionResult(true, "Deleted all unmatched items");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"RecipeProcessor.DeleteUnmatchedItems. {ex.Message}\r\n", ex);
                throw;
            }
        }
    }
}
