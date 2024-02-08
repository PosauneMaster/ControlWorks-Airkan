using System.Web.Http;
using ControlWorks.Common;
using ControlWorks.Services.PVI.Pvi;
using ControlWorks.Services.Rest.Models;
using ControlWorks.Services.Rest.Processors;

using log4net;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Owin;

using Swagger.Net.Application;

using Unity;
using Unity.WebApi;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace ControlWorks.Services.Rest
{
    public class WebApiApplication
    {
        private static readonly ILog Log = LogManager.GetLogger("ControlWorksLogger");
        public static IPviApplication PviApp { get; private set; }

        static WebApiApplication()
        {
            PviApp = new ControlWorks.Services.PVI.Pvi.PviAplication();
        }

        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();

            config.DependencyResolver = new UnityDependencyResolver(UnityService.Create());

            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

            // Configure Web API for self-host. 
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "ActionRoute",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.EnableSwagger(c => c.SingleApiVersion("v1", "Control Works")).EnableSwaggerUi();

            app.UseWebApi(config);
        }

        public static void Start()
        {
            try
            {

                var hostUrl = $"http://*:{ConfigurationProvider.Port}";

                Log.Info($"Starting WebApi at host {hostUrl}");

                WebApp.Start<WebApiApplication>(hostUrl);
            }
            catch(System.Exception ex)
            {

            }
        }
    }
}
