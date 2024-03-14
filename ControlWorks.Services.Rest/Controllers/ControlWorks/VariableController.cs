using ControlWorks.Services.PVI.Variables;
using ControlWorks.Services.Rest.Models;
using ControlWorks.Services.Rest.Processors;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlWorks.Services.Rest.Controllers
{
    //[ApiExplorerSettings(IgnoreApi = true)]
    public class VariablesController : ApiController
    {
        [HttpGet]
        [Route("api/Variables/GetByCpu")]
        public async Task<IHttpActionResult> GetByCpu(string name)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var settings = await variableProcessor.FindByCpu(name);

                if (settings == null)
                {
                    var message = $"Variables for Cpu {name} not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetByCpu");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Variables/GetConfigurationByCpu")]
        public async Task<IHttpActionResult> GetConfigurationByCpu(string name)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var settings = await variableProcessor.GetVariableConfiguration(name);

                if (settings == null)
                {
                    var message = $"Variables for Cpu {name} not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetByCpu");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpGet]
        [Route("api/Variables/GetVariableInfo")]
        public async Task<IHttpActionResult> GetVariableInfo(string name)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var mapping = await variableProcessor.FindVariable(name);

                if (mapping == null)
                {
                    var message = $"Variables for Cpu {name} not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(mapping);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetByCpu");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }




        [HttpGet]
        [Route("api/Variables/GetByIp")]
        public async Task<IHttpActionResult> GetByIp(string ip)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var settings = await variableProcessor.FindByIp(ip);

                if (settings == null)
                {
                    var message = $"Variables for Ip {ip} not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetByIp");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Variables/GetActiveByCpu")]
        public async Task<IHttpActionResult> GetActiveByCpu(string name)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var settings = await variableProcessor.FindActiveByCpuName(name);

                if (settings == null)
                {
                    var message = "Variables not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetAciveByCpu");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpGet]
        [Route("api/Variables/GetDetailsByCpu")]
        public async Task<IHttpActionResult> GetDetailsByCpu(string name)
        {
            try
            {
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);

                var settings = await variableProcessor.GetVariableDetails(name);

                if (settings == null)
                {
                    var message = "Variables not found";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                return Ok(settings);
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "GetVariables");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpPost]
        [Route("api/Variables/Update")]
        public async Task<IHttpActionResult> Update([FromBody]VariableInfo variableInfo)
        {
            try
            {
                if (variableInfo == null)
                {
                    var message = "Variable Info is null";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);
                //await variableProcessor.UpdateVariables(variableInfo.CpuName, variableInfo.Variables);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "Update");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpPost]
        [Route("api/Variables/AddByCpuName")]
        public async Task<IHttpActionResult> AddByCpuName([FromBody]VariableDetailName variableDetail)
        {
            try
            {
                if (variableDetail == null)
                {
                    var message = "Variable Info is null";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);
                await variableProcessor.AddVariableByCpuName(variableDetail.CpuName, variableDetail.TaskName, variableDetail.VariableName);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "AddByCpuName");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Variables/AddByIpAddress")]
        public async Task<IHttpActionResult> AddByIpAddress([FromBody] VariableDetailIp variableDetail)
        {
            try
            {
                if (variableDetail == null)
                {
                    var message = "Variable Info is null";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);
                await variableProcessor.AddVariableByIpAddress(variableDetail.IpAddress, variableDetail.TaskName, variableDetail.VariableName);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "AddByIpAddress");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpDelete]
        [Route("api/Variables/DeleteByIpAddress")]
        public async Task<IHttpActionResult> DeleteByIpAddress([FromBody] VariableDetailIp variableDetail)
        {
            try
            {
                if (variableDetail == null)
                {
                    var message = "Variable Name is null";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }
                var variableProcessor = new VariableProcessor(WebApiApplication.PviApp);
                await variableProcessor.DeleteVariable(variableDetail.IpAddress, variableDetail.TaskName, variableDetail.VariableName );

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("VariableController.Operation", "Delete");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


    }
}
