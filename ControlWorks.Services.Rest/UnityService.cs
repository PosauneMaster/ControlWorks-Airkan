using ControlWorks.Services.Rest.Processors;

using Unity;

namespace ControlWorks.Services.Rest
{
    public static class UnityService
    {
        public static UnityContainer Create()
        {
            
            var container = new UnityContainer();
            var mapper = AutoMapConfiguration.CreateMapper();
            container.RegisterInstance(mapper);
            //container.RegisterType<IRecipeProcessor, RecipeProcessor>();
            container.RegisterType<IRecipeService, RecipeService>();
            //container.RegisterType<IItemProcessor, ItemProcessor>();
            //container.RegisterType<IDiagnosticsProcessor, DiagnosticsProcessor>();
            container.RegisterInstance(WebApiApplication.PviApp);
            

            return container;

        }
    }
}
