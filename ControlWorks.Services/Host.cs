using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ControlWorks.Common;
using ControlWorks.Services.Rest;



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
                    var factory = new TaskFactory();
                }
            }
            catch(Exception ex)
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
            catch(Exception ex)
            {
                Trace.TraceError($"Host:Start {ex.Message}", ex);
            }
        }

    }
}
