using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Controllers.ControlWorks
{

    public class LogController : ApiController
    {
        public async Task<IHttpActionResult> GetLogDetails()
        {
            try
            {
                var processor = new LogProcessor();
                var logItem = await processor.GetLogFileNamesAsync();

                return Ok(logItem);
            }
            catch (Exception ex)
            {
                ex.Data.Add("PviController.Operation", "GetLogDetails");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}
