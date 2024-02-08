using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ControlWorks.Services.Rest.Mocks;
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
            container.RegisterType<IRecipeProcessor, RecipeProcessor>();
            //container.RegisterType<IRecipeProcessor, RecipeProcessorMock>();
            container.RegisterType<IRecipeService, RecipeService>();
            container.RegisterType<IBinService, BinService>();
            container.RegisterType<IItemProcessor, ItemProcessor>();
            container.RegisterType<IAlarmProcessor, AlarmProcessor>();
            //container.RegisterType<IAlarmProcessor, AlarmProcessorMock>();
            container.RegisterType<IDiagnosticsProcessor, DiagnosticsProcessor>();
            container.RegisterInstance(WebApiApplication.PviApp);
            

            return container;

        }
    }
}
