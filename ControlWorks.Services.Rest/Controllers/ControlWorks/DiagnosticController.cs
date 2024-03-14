using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ControlWorks.Services.Rest.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class DiagnosticController : ApiController
    {
        [HttpGet]
        [Route("api/Diagnostic/GetHeartbeat")]
        public IHttpActionResult GetHeartbeat()
        {
            try
            {
                return Ok(DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                ex.Data.Add("DiagnosticController.Operation", "GetHeartbeat");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Diagnostic/GetLog")]
        public IHttpActionResult GetLog()
        {
            try
            {
                return Ok();
                //var response = new LogFileResponse(_log);
                //response.Build();
                //return Ok(response);
            }
            catch (Exception ex)
            {
                ex.Data.Add("DiagnosticController.Operation", "GetLog");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Diagnostic/GetServiceInfo")]
        public IHttpActionResult GetServiceInfo()
        {
            try
            {
                //var logResponse = new LogFileResponse(_log);
                //logResponse.Build();

                var response = new ServiceInfoResponse(String.Empty);
                response.Build();
                return Ok(response);
            }
            catch (Exception ex)
            {
                ex.Data.Add("DiagnosticController.Operation", "GetLog");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

    }


    public class HeartBeatInfo
    {
        public DateTime RequestTime { get; set; }
        public string RequestName { get; set; }
    }

}
