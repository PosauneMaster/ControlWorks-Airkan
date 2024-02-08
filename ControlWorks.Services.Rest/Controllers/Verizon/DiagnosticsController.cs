using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using ControlWorks.Services.Rest.Processors;

namespace ControlWorks.Services.Rest.Controllers.Verizon
{
    public class DiagnosticsController : ApiController
    {
        private IDiagnosticsProcessor _diagnosticsProcessor;

        public DiagnosticsController()
        {
        }

        public DiagnosticsController(IDiagnosticsProcessor diagnosticsProcessor)
        {
            _diagnosticsProcessor = diagnosticsProcessor;
        }

        [HttpGet]
        [Route("api/sorting/v1/diagnostics/healthcheck")]
        public async Task<IHttpActionResult> HealthCheck()
        {
            try
            {
                var result = await _diagnosticsProcessor.GetHealthCheck();

                return Ok(result);

            }
            catch (Exception ex)
            {
                ex.Data.Add("TestingController.Operation", "HealthCheck");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

    }
}
