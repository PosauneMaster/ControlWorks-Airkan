using ControlWorks.Common;
using ControlWorks.Services.Rest;

using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace ControlWorks.Services
{
    public interface IHost
    {
        void Start();
        void Stop();
    }

    public class Host : IHost
    {
        public void Start()
        {
            try
            {

                WebApiApplication.Start();

                if (!ConfigurationProvider.RestApiTestMode)
                {
                    var pviApp = WebApiApplication.PviApp;
                    var factory = new TaskFactory();
                    factory.StartNew(() => pviApp.Connect(), TaskCreationOptions.LongRunning);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Host:Start {ex.Message}", ex);
            }

        }

        public void Stop()
        {
            try
            {
                WebApiApplication.PviApp.Disconnect();
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Host:Start {ex.Message}", ex);
            }
        }

    }
}
