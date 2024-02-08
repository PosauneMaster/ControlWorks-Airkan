using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ControlWorks.Common;
using ControlWorks.Services.Rest;
using log4net;


namespace ControlWorks.Services
{
    public interface IHost
    {
        void Start();
        void Stop();
    }

    public class Host : IHost
    {
        private readonly ILog Log = ConfigurationProvider.Logger;

        public void Start()
        {
            try
            {

                WebApiApplication.Start();

                if (!ConfigurationProvider.RestApiTestMode)
                {
                    var factory = new TaskFactory();
                }
            }
            catch(Exception ex)
            {
                Log.Error($"Host:Start {ex.Message}", ex);
            }

        }

        public void Stop()
        {
            try
            {
                WebApiApplication.PviApp.Disconnect();
            }
            catch(Exception ex)
            {
                Log.Error($"Host:Start {ex.Message}", ex);
            }
        }

    }
}
