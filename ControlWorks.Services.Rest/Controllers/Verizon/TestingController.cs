using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Controllers.Verizon
{
    public class TestingController : ApiController
    {
        private readonly IRecipeProcessor _recipeProcessor;
        private readonly IItemProcessor _itemProcessor;
        private readonly IAlarmProcessor _alarmProcessor;
        private readonly IDiagnosticsProcessor _diagnosticsProcessor;


        public TestingController() { }
        public TestingController(IRecipeProcessor recipeProcessor, IItemProcessor itemProcessor, IAlarmProcessor alarmProcessor, IDiagnosticsProcessor diagnosticsProcessor)
        {
            _recipeProcessor = recipeProcessor;
            _itemProcessor = itemProcessor;
            _alarmProcessor = alarmProcessor;
            _diagnosticsProcessor = diagnosticsProcessor;
        }

        [HttpGet]
        [Route("api/sorting/v1/testing/incrementitem")]
        public async Task<IHttpActionResult> IncrementItem(string recipeReference, string itemId)
        {
            try
            {
                _recipeProcessor.IncrementItem(recipeReference, itemId);

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("TestingController.Operation", "Add");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/sorting/v1/testing/addunmatcheditem")]
        public async Task<IHttpActionResult> AddUnmatchedItem(string itemId, string reference, string specification)
        {
            try
            {
                _itemProcessor.AddUnmatchedItems(itemId, reference, specification);

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("TestingController.Operation", "AddUnmatchedItem");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/sorting/v1/testing/addalarm")]
        public async Task<IHttpActionResult> AddAlarm(string description)
        {
            try
            {
                _alarmProcessor.AddAlarm(description);

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("TestingController.Operation", "AddAlarm");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/sorting/v1/testing/updatestatus")]
        public async Task<IHttpActionResult> UpdateStatus(string status)
        {
            try
            {
                _diagnosticsProcessor.UpdateStatus(status);

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("TestingController.Operation", "UpdateStatus");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

    }
}
