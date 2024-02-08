using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Controllers.Verizon
{
    public class AlarmsController : ApiController
    {
        private readonly IAlarmProcessor _alarmProcessor;

        public AlarmsController() { }
        public AlarmsController(IAlarmProcessor alarmProcessor)
        {
            _alarmProcessor = alarmProcessor;
        }

        [HttpGet]
        [Route("api/sorting/v1/alarms/active")]
        public async Task<IHttpActionResult> Active()
        {
            try
            {
                var result = await _alarmProcessor.GetActiveAlarms();

                return Ok(result);
            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Unmatched");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpDelete]
        [Route("api/sorting/v1/alarms/delete")]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
                await _alarmProcessor.Delete(id);
                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Delete");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpDelete]
        [Route("api/sorting/v1/alarms/deleteall")]
        public async Task<IHttpActionResult> DeleteAll()
        {
            try
            {
                await _alarmProcessor.DeleteAll();

                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "DeleteAll");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }




    }
}
