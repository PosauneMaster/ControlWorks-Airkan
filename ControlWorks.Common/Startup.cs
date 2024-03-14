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

            if (String.IsNullOrEmpty(ConfigurationProvider.LogFilePath)) 
            {
                ConfigurationProvider.LogFilePath = Path.Combine(ConfigurationProvider.BaseDirectory, "Logs");
            }

            if (!Directory.Exists(ConfigurationProvider.LogFilePath))
            {
                Directory.CreateDirectory(ConfigurationProvider.LogFilePath);
            }

            Trace.Listeners.Add(new ControlWorksListener());
            Trace.Listeners.Add(new ConsoleTraceListener());

            try
            {
                int x = 0; int y = 0;
                int z = x / y;
            }
            catch (Exception ex)
            {
                Trace.TraceError("An Error", ex, new ArgumentException("There is an Argument Exception", new Exception("Inner Exception", new Exception("Inner Exception 2"))));
            }

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
            Trace.TraceInformation($"LogFilename={ConfigurationProvider.LogFilename}");
            Trace.TraceInformation($"ServiceDescription={ConfigurationProvider.ServiceDescription}");
            Trace.TraceInformation($"ServiceDisplayName={ConfigurationProvider.ServiceDisplayName}");
            Trace.TraceInformation($"ServiceName={ConfigurationProvider.ServiceName}");
            Trace.TraceInformation($"SmartConveyorDB={ConfigurationProvider.SmartConveyorDbConnectionString}");
            Trace.TraceInformation($"ControlworksSettingsDB={ConfigurationProvider.ControlworksSettingsDbConnectionString}");
            Trace.TraceInformation($"ControlworksLogsDB={ConfigurationProvider.ControlworksLogsDbConnectionString}");
        }
    }
}
