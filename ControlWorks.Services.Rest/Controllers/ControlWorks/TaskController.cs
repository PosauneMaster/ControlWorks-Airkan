using ControlWorks.Services.Rest.Processors;

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlWorks.Services.Rest.Controllers
{
    public class TaskController : ApiController
    {
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetTaskNames")]
        public async Task<IHttpActionResult> GetTaskNames(string cpuName)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetTaskNames(cpuName);

                if (details == null)
                {
                    var message = $"Tasks not found for cpu {cpuName}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetTaskNames");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetTaskNamesByIp")]
        public async Task<IHttpActionResult> GetTaskNamesByIp(string ipAddress)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetTaskNamesByIp(ipAddress);

                if (details == null)
                {
                    var message = $"Tasks not found for ipAddress {ipAddress}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetTaskNamesByIp");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetCpuDetailsByName")]
        public async Task<IHttpActionResult> GetCpuDetailsByName(string cpuName)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetCpuDetailsByName(cpuName);

                if (details == null)
                {
                    var message = $"Task details not found for cpu {cpuName}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetCpuDetailsByName");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetCpuDetailsByIp")]
        public async Task<IHttpActionResult> GetCpuDetailsByIp(string ip)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetCpuDetailsByIp(ip);

                if (details == null)
                {
                    var message = $"Task details not found for ip {ip}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetCpuDetailsByIp");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetTaskDetailsByName")]
        public async Task<IHttpActionResult> GetTaskDetailsByName(string cpuName, string taskName)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetTaskDetailsByName(cpuName, taskName);

                if (details == null)
                {
                    var message = $"Task details not found for cpu={cpuName}, taskName={taskName}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetTaskDetailsByName");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [System.Web.Http.HttpGet]
        [System.Web.Http.Route("api/Cpu/GetTaskDetailsByIp")]
        public async Task<IHttpActionResult> GetTaskDetailsByIp(string ip, string taskName)
        {
            try
            {
                var requestProcessor = new RequestProcessor(WebApiApplication.PviApp);

                var details = await requestProcessor.GetTaskDetailsByIp(ip, taskName);
                if (details == null)
                {
                    var message = $"Task details not found for ip={ip}, taskName={taskName}";
                    return ResponseMessage(Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
                }

                return Ok(details);
            }
            catch (Exception ex)
            {
                ex.Data.Add("TaskController.Operation", "GetTaskDetailsByIp");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }
    }
}