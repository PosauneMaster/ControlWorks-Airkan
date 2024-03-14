//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using AutoMapper;

//using ControlWorks.Common;
//using ControlWorks.Services.PVI.Pvi;
//using ControlWorks.Services.PVI.Variables.Models;
//using ControlWorks.Services.Rest.Models;

//using LiteDB;
//using Newtonsoft.Json;

//namespace ControlWorks.Services.Rest.Processors
//{
//    public interface IRecipeProcessor
//    {
//        Task<RecipeActionResult> AddRecipe(Recipe recipe);
//        Task<BinCollection> GetActiveRecipes();
//        Task<BinCollection> GetCompleteRecipes();
//        Task<RecipeActionResult> Delete(string reference);
//        Task<RecipeActionResult> DeleteAll();
//        void IncrementItem(string recipeReference, string itemId);
//    }

//    public class RecipeProcessor : IRecipeProcessor
//    {
//        private readonly string _activeRecipes = "activeRecipes";

//        private readonly AutoResetEvent _waitHandle;
//        private readonly IMapper _mapper;
//        private readonly IRecipeService _recipeService;
//        private readonly IBinService _binService;

//        private readonly BinMapper _binMapper;

//        private IPviApplication _pviApplication;

//        public RecipeProcessor() { }

//        public RecipeProcessor(IMapper mapper, IRecipeService recipeService, IBinService binService, IPviApplication pviApplication)
//        {
//            _mapper = mapper;
//            _recipeService = recipeService;
//            _binService = binService;
//            _waitHandle = new AutoResetEvent(false);
//            _pviApplication = pviApplication;

//            _binMapper = new BinMapper();

//        }

//        public async Task<RecipeActionResult> AddRecipe(Recipe recipe)
//        {
//            //limit to 50 items -- configurable
//            const string commandName = "AddOrderInfo";
//            try
//            {
//                VerizonOrderInfo orderInfo = new VerizonOrderInfo();
//                orderInfo.OrderNumber = recipe.Reference;
//                orderInfo.Description = recipe.Description;
//                foreach (var item in recipe.Items)
//                {
//                    orderInfo.AddItem(item.Id, item.Specification, item.Quantity);
//                }

//                var addResult = await
//                    Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, orderInfo.ToJson()));

//                if (addResult.StatusCode != 0)
//                {
//                    return new RecipeActionResult(false, $"Cannot add recipe: {addResult.Message}");
//                }

//                return new RecipeActionResult(true, $"request added");
//            }

//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.AddRecipe. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        private List<VerizonOrderInfo> GetVerizonOrderInfoTestData()
//        {
//            var jsonFilePath = @"C:\ControlWorks\Verizon\VerizonOrderInfo.json";
//            if (File.Exists(jsonFilePath))
//            {
//                var json = File.ReadAllText(jsonFilePath);
//                var verizonOrderInfo = JsonConvert.DeserializeObject<List<VerizonOrderInfo>>(json);
//                return verizonOrderInfo;
//            }

//            return null;
//        }

//        private List<VerizonOrderInfo> ResolveOrders(IEnumerable<VerizonOrderInfo> verizonOrderInfoList,
//            Func<VerizonOrderInfo, bool> selector)
//        {
//            var validOrders = verizonOrderInfoList.Where(o => !String.IsNullOrEmpty(o.OrderNumber));

//            var activeOrderList = new List<VerizonOrderInfo>();

//            foreach (var order in validOrders)
//            {
//                var validOrder = _mapper.Map<VerizonOrderInfo>(order);

//                var validItems = order.Items.Where(i => !String.IsNullOrEmpty(i.SKUnumber));
//                validOrder.AddItemRange(validItems);

//                if (selector(validOrder))
//                {
//                    activeOrderList.Add(validOrder);
//                }
//            }

//            return activeOrderList;
//        }

//        private List<VerizonOrderInfo> ResolveActiveOrders(IEnumerable<VerizonOrderInfo> verizonOrderInfoList)
//        {
//            bool Selector(VerizonOrderInfo order) => order.Items.Any(i => i.ActualQty != i.RequiredQuanity);
//            return ResolveOrders(verizonOrderInfoList, Selector);
//        }

//        private List<VerizonOrderInfo> ResolveCompleteOrders(IEnumerable<VerizonOrderInfo> verizonOrderInfoList)
//        {
//            bool Selector(VerizonOrderInfo order) => order.Items.All(i => i.ActualQty == i.RequiredQuanity);
//            return ResolveOrders(verizonOrderInfoList, Selector);
//        }


//        public async Task<BinCollection> GetActiveRecipes()
//        {
//            try
//            {
//                var orderInfoList = await Task.Run(() => _pviApplication.GetAllRecipies());

//                await Task.Delay(50);

//                //var orderInfoList = await Task.Run(GetVerizonOrderInfoTestData);
//                var activeOrders = ResolveActiveOrders(orderInfoList);

//                var bins = _binMapper.Map(activeOrders);
//                return bins;
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.GetActiveRecipes. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task<BinCollection> GetCompleteRecipes()
//        {
//            try
//            {
//                var orderInfoList = await Task.Run(() => _pviApplication.GetAllRecipies());

//                //var orderInfoList = await Task.Run(GetVerizonOrderInfoTestData);
//                var completeOrders = ResolveCompleteOrders(orderInfoList);

//                var bins = _binMapper.Map(completeOrders);
//                return bins;

//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.GetCompleteRecipes. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }


//        public void IncrementItem(string recipeReference, string itemId)
//        {
//            try
//            {
//                using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
//                {
//                    var recipesActiveCol = db.GetCollection<RecipeActiveDto>(_activeRecipes);
//                    var recipe = recipesActiveCol.Query()
//                        .Where(r => r.Reference == recipeReference)
//                        .FirstOrDefault();

//                    var item = recipe?.Items.FirstOrDefault(i => i.Id == itemId);

//                    if (item != null)
//                    {
//                        item.QuantityScanned += 1;
//                        recipesActiveCol.Update(recipe);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.IncrementItem. {ex.Message}\r\n", ex);
//                throw;
//            }
//        }

//        public async Task<RecipeActionResult> Delete(string reference)
//        {
//            try
//            {
//                var commandName = "DeleteRecipe";
//                var addResult = await
//                    Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, reference));

//                return new RecipeActionResult(false, $"Recipe {reference} deleted");
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.Delete. reference={reference}, {ex.Message}\r\n", ex);
//                return new RecipeActionResult(true, $"reference={reference}, ex.Message");
//            }
//        }

//        public async Task<RecipeActionResult> DeleteAll()
//        {
//            try
//            {
//                var commandName = "DeleteAllRecipes";
//                var addResult = await
//                    Task.Run(() => _pviApplication.SendCommand(String.Empty, commandName, String.Empty));

//                return new RecipeActionResult(false, $"All recipes deleted");


//                //using (var db = new LiteDatabase(ConfigurationProvider.SmartConveyorDbConnectionString))
//                //{
//                //    var recipesActiveCol = db.GetCollection<RecipeActiveDto>(_activeRecipes);

//                //    if (recipesActiveCol == null)
//                //    {
//                //        return new RecipeActionResult(false, "recipes not found");
//                //    }

//                //    recipesActiveCol.DeleteAll();
//                //    return new RecipeActionResult(true, String.Empty);
//                //}
//            }
//            catch (Exception ex)
//            {
//                Trace.TraceError($"RecipeProcessor.DeleteAll. {ex.Message}\r\n", ex);
//                return new RecipeActionResult(true, ex.Message);
//            }
//        }
//    }

//    public class RecipeActionResult
//    {
//        public bool Success { get; set; }
//        public string Message { get; set; }

//        public RecipeActionResult() { }
//        public RecipeActionResult(bool success, string message)
//        {
//            Success = success;
//            Message = message;
//        }
//    }
//}
