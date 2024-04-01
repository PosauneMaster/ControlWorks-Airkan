using ControlWorks.Common;

using System;
using System.Diagnostics;
using System.Web.Mvc;

using Topshelf;

namespace ControlWorks.Services
{

    class Program
    {
        public static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Startup.Initialize();

            Trace.TraceInformation("Starting Service...");

            var rc = HostFactory.Run(x =>
            {
                x.Service<Host>(s =>
                {
                    s.ConstructUsing(n => new Host());
                    s.WhenStarted(tc => tc.Start());
                    s.WhenStopped(tc => tc.Stop());
                });
                x.RunAsLocalSystem();
                x.SetDescription("ControlWorks wrapper service for REST API");
                x.SetDisplayName("ControlWorksRESTApi");
                x.SetServiceName("ControlWorks.Services.Rest");

                x.EnableServiceRecovery(r =>
                {
                    r.RestartService(1);
                    r.SetResetPeriod(1);
                });

                x.OnException((exception) =>
                {
                    Trace.TraceError("Topshelf service error");
                    Trace.TraceError(exception.Message, exception);
                });
            });
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Trace.TraceError("Unhandled Application Domain Error");
            if (e.ExceptionObject is Exception ex)
            {
                Trace.TraceError(ex.Message, ex);
            }
        }
    }
}
