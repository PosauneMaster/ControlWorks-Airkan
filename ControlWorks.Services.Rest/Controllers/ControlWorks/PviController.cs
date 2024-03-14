using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlWorks.Services.Rest.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class PviController : ApiController
    {
        public async Task<IHttpActionResult> GetDetails()
        {
            try
            {
                
                var processor = new ServiceProcessor(WebApiApplication.PviApp);

                var details = await processor.GetServiceDetails();

                if (details == null)
                {
                    var message = "Pvi Service not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("PviController.Operation", "GetDetails");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }



    }
}
