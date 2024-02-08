using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Controllers.Verizon
{
    public class ItemsController : ApiController
    {

        private readonly IItemProcessor _itemProcessor;

        public ItemsController() { }
        public ItemsController(IItemProcessor itemProcessor)
        {
            _itemProcessor = itemProcessor;
        }

        [HttpGet]
        [Route("api/sorting/v1/items/unmatched")]
        public async Task<IHttpActionResult> Unmatched()
        {
            try
            {
                var result = _itemProcessor.GetUnmatchedItems();

                return Ok(result);

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Unmatched");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpDelete]
        [Route("api/sorting/v1/items/deleteallunmatcheditems")]
        public async Task<IHttpActionResult> DeleteAllUnmatchedItems()
        {
            try
            {
                var result = _itemProcessor.DeleteAllUnmatchedItems();

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "DeleteAllUnmatchedItems");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpDelete]
        [Route("api/sorting/v1/items/deleteunmatcheditem")]
        public async Task<IHttpActionResult> DeleteUnmatchedItem(string itemId, string reference)
        {
            try
            {
                var result = _itemProcessor.DeleteUnmatchedItem(itemId, reference);

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "DeleteUnmatchedItem");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}
