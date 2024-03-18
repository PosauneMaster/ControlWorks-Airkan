using ControlWorks.Common.Logging;

using NLog;

using System;
using System.Diagnostics;
using System.IO;

namespace ControlWorks.Common
{
    public static class Startup
    {
        public static void Initialize()
        {
            if (!Directory.Exists(ConfigurationProvider.BaseDirectory))
            {
                Directory.CreateDirectory(ConfigurationProvider.BaseDirectory);
            }

            ConfigurationProvider.SettingsDirectory = Path.Combine(ConfigurationProvider.BaseDirectory, "Settings");

            if (!Directory.Exists(ConfigurationProvider.SettingsDirectory))
            {
                Directory.CreateDirectory(ConfigurationProvider.SettingsDirectory);
            }

            Trace.Listeners.Add(new ControlWorksListener());

            Trace.TraceInformation(new string('*', 30));
            Trace.TraceInformation("Starting application...");
            Trace.TraceInformation(new string('*', 30));

            Trace.TraceInformation("Starting Initialization...");


            WriteStartupLog();
            Trace.TraceInformation("Initialization Complete.");
        }


        public static void WriteStartupLog()
        {
            Trace.TraceInformation($"BaseDirectory={ConfigurationProvider.BaseDirectory}");
            Trace.TraceInformation($"SettingsDirectory={ConfigurationProvider.SettingsDirectory}");
            Trace.TraceInformation($"Port={ConfigurationProvider.Port}");
            Trace.TraceInformation($"ShutdownTriggerVariable={ConfigurationProvider.ShutdownTriggerVariable}");
            Trace.TraceInformation($"SourceStationId={ConfigurationProvider.SourceStationId}");
            Trace.TraceInformation($"MessageTimeout={ConfigurationProvider.MessageTimeout}");
            Trace.TraceInformation($"LogFilePath={ConfigurationProvider.LogFilePath}");
            Trace.TraceInformation($"ServiceDescription={ConfigurationProvider.ServiceDescription}");
            Trace.TraceInformation($"ServiceDisplayName={ConfigurationProvider.ServiceDisplayName}");
            Trace.TraceInformation($"ServiceName={ConfigurationProvider.ServiceName}");
        }
    }
}
