using System;
using System.Configuration;

namespace ControlWorks.Common
{
    public static class ConfigurationProvider
    {
        public static string Port => ConfigurationManager.AppSettings["Port"];
        public static string ShutdownTriggerVariable => ConfigurationManager.AppSettings["ShutdownTriggerVariable"];
        public static byte SourceStationId => (byte)(Convert.ToByte(ConfigurationManager.AppSettings["SourceStationId"]).Equals(0) ? 0x64 : Convert.ToByte(ConfigurationManager.AppSettings["SourceStationId"]));
        public static int MessageTimeout
        {
            get
            {
                if (Int32.TryParse(ConfigurationManager.AppSettings["MessageTimeout"], out var timeout))
                {
                    return timeout;
                }

                return 1000;
            }
        }
        public static string BaseDirectory => ConfigurationManager.AppSettings["BaseDirectory"];
        public static string SettingsDirectory { get; internal set; }
        public static string ServiceDescription => "ControlWorks wrapper service for REST API";
        public static string ServiceDisplayName => "ControlWorksRESTApi";
        public static string ServiceName => "ControlWorks.Services.Rest";
        public static string ConnectionMode => ConfigurationManager.AppSettings["ConnectionMode"];
        public static int AddOrderWaitTime
        {
            get
            {
                if (Int32.TryParse(ConfigurationManager.AppSettings["AddOrderWaitTime"], out var waitTime))
                {
                    return waitTime;
                }

                return 50;
            }
        }


        private static bool? _verboseVariableLogging = null;
        public static bool VerboseVariableLogging
        {
            get
            {
                if (_verboseVariableLogging.HasValue)
                {
                    return _verboseVariableLogging.Value;
                }

                if (Boolean.TryParse(ConfigurationManager.AppSettings["VerboseVariableLogging"], out var verboseVariables))
                {
                    return verboseVariables;
                }
                else
                {
                    return false;
                }
            }
        }

        public static int PollingMilliseconds
        {
            get 
            {
                const int defaultPollingTime = 60000;
                if (Int32.TryParse(ConfigurationManager.AppSettings["PollingMilliseconds"], out var pollingTime))
                {
                    return pollingTime;
                }

                return defaultPollingTime;
            }
        }

        public static bool RestApiTestMode
        {
            get
            {
                if (Boolean.TryParse(ConfigurationManager.AppSettings["RestApiTestMode"], out var result))
                {
                    return result;
                }

                return false;
            }
        }
        public static string LogFilePath => ConfigurationManager.AppSettings["LogFilePath"];
        public static string CpuSettings => ConfigurationManager.AppSettings["CpuSettings"];
        public static string VariableSettings => ConfigurationManager.AppSettings["VariableSettings"];
        public static string VariableTasks => ConfigurationManager.AppSettings["VariableTasks"];
        public static string AirkanBartenderFolder => ConfigurationManager.AppSettings["AirkanBartenderFolder"];
        public static string AirkanNetworkFolder => ConfigurationManager.AppSettings["AirkanNetworkFolder"];
    }
}
