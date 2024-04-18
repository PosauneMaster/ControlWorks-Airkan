using ControlWorks.Services.PVI.Variables;
using ControlWorks.Services.Rest.Processors;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace ControlWorks.Services.Rest.Controllers.ControlWorks
{
    public class AirkanController : ApiController
    {
        [HttpPost]
        [Route("api/Airkan/SetVariable")]
        public async Task<IHttpActionResult> SetVariable(AirkanVariable airkanVariable)
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.SendVariableAsync<AirkanVariable>("SetVariable", airkanVariable);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "SetVariable");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Airkan/SetVariableList")]
        public async Task<IHttpActionResult> SetVariableList(IEnumerable<AirkanVariable> airkanVariable)
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.SendVariableListAsync<AirkanVariable>("SetVariable", airkanVariable);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "SetVariable");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Airkan/GetVariableList")]
        public async Task<IHttpActionResult> GetVariableList()
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                var variables = await airkanProcessor.GetAirkanVariablesAsync();

                return Ok(variables);
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "GetVariableList");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/Airkan/GetAirkanInputFiles")]
        public async Task<IHttpActionResult> GetAirkanInputFiles()
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                var files = await airkanProcessor.GetAirkanInputFilesAsync();

                return Ok(files);
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "GetAirkanInputFiles");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpPost]
        [Route("api/Airkan/ProcessFile")]
        public async Task<IHttpActionResult> ProcessFile(string filename)
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.ProcessFileAsync(filename);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "ProcessFile");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Airkan/ProcessFileByIndex")]
        public async Task<IHttpActionResult> ProcessFileByIndex(int index)
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.ProcessFileByIndexAsync(index.ToString());

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "ProcessFileByIndex");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Airkan/SetFileTransferLocation")]
        public async Task<IHttpActionResult> SetFileTransferLocation(string location)
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.ProcessSetFileTransferLocation(location);

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "SetFileTransferLocation");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpPost]
        [Route("api/Airkan/ProcessBarCode")]
        public async Task<IHttpActionResult> ProcessBarCode()
        {
            try
            {
                var airkanProcessor = new AirkanProcessor(WebApiApplication.PviApp);

                await airkanProcessor.ProcessBarCode();

                return Ok();
            }
            catch (Exception ex)
            {
                ex.Data.Add("AirkanController.Operation", "ProcessBarCode");
                Trace.TraceError(ex.Message, ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


    }
}
