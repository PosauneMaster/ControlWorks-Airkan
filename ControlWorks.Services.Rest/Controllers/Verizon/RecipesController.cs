using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI;
using ControlWorks.Services.Rest.Models;
using ControlWorks.Services.Rest.Processors;

using Newtonsoft.Json;

namespace ControlWorks.Services.Rest.Controllers.Verizon
{
    public class RecipesController : ApiController
    {
        private readonly IRecipeProcessor _recipeProcessor;

        public RecipesController() { }
        public RecipesController(IRecipeProcessor recipeProcessor)
        {
            _recipeProcessor = recipeProcessor;
        }

        [HttpPost]
        [Route("api/sorting/v1/recipes/add")]
        public async Task<IHttpActionResult> Add([FromBody] Recipe recipe)
        {
            try
            {
                var result = await _recipeProcessor.AddRecipe(recipe);

                if (!result.Success)
                {
                    return Content(HttpStatusCode.BadRequest, result.Message);
                }

                return Content(HttpStatusCode.Created, result.Message);
            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Add");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/sorting/v1/recipes/active")]
        public async Task<IHttpActionResult> Active()
        {
            try
            {
                var result = await _recipeProcessor.GetActiveRecipes();

                return Ok(result);

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Active");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpGet]
        [Route("api/sorting/v1/recipes/complete")]
        public async Task<IHttpActionResult> Complete()
        {
            try
            {
                var result = await _recipeProcessor.GetCompleteRecipes();

                return Ok(result);

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Complete");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }


        [HttpDelete]
        [Route("api/sorting/v1/recipes/delete")]
        public async Task<IHttpActionResult> Delete(string reference)
        {
            try
            {
                var result = _recipeProcessor.Delete(reference);
                return Ok();

            }
            catch (Exception ex)
            {
                ex.Data.Add("RecipesController.Operation", "Delete");
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
            }
        }

        [HttpDelete]
        [Route("api/sorting/v1/recipes/deleteall")]
        public async Task<IHttpActionResult> DeleteAll()
        {
            try
            {
                var result = _recipeProcessor.DeleteAll();
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
