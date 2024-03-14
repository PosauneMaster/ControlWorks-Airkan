//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading.Tasks;

//using AutoMapper;

//using ControlWorks.Common;
//using ControlWorks.Services.PVI.Models;
//using ControlWorks.Services.PVI.Pvi;
//using ControlWorks.Services.Rest.Models;

//using LiteDB;

//namespace ControlWorks.Services.Rest.Processors
//{
//    public interface IItemProcessor
//    {
//        Task<List<UnmatchedItem>> GetUnmatchedItems();
//        Task DeleteAllUnmatchedItems();
//        Task DeleteUnmatchedItem(string itemId);
//        RecipeActionResult AddUnmatchedItems(string itemId, string reference, string specification);

//    }

//    public class ItemProcessor : IItemProcessor
//    {
//        private readonly string _unmatchedItems = "unmatchedItems";

//        private readonly IMapper _mapper;

//        private IPviApplication _pviApplication;

//        public ItemProcessor(IMapper mapper, IPviApplication pviApplication)
//        {
//            _pviApplication = pviApplication;
//            _mapper = mapper;
//        }
        
//        public async Task<List<UnmatchedItem>> GetUnmatchedItems()
//        {
//            try
//            {
//                return await Task.Run(() => _pviApplication.GetUnmatchedItems());

//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.GetUnmatchedItems. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public RecipeActionResult AddUnmatchedItems(string itemId, string reference, string specification)
//        {
//            try
//            {
//                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
//                {
//                    var recipesActiveCol = db.GetCollection<UnmatchedRecipeItemDto>(_unmatchedItems);

//                    var item = new UnmatchedRecipeItemDto(itemId, reference, specification);
//                    recipesActiveCol.Insert(item);

//                    return new RecipeActionResult(true, String.Empty);
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.AddUnmatchedItems. {ex.Message}\r\n", ex);
//                throw;
//            }

//        }

//        public async Task DeleteUnmatchedItem(string itemId)
//        {
//            try
//            {
//                var commandName = "DeleteUnmatched";
//                await Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, itemId.ToString()));
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"DeleteUnmatchedItem. itemId={itemId} {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task DeleteAllUnmatchedItems()
//        {
//            try
//            {
//                var commandName = "DeleteAllUnmatched";
//                await Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, String.Empty));
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"DeleteAllUnmatchedItems. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }
//    }
//}
