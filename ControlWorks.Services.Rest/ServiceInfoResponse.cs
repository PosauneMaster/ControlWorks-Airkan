using ControlWorks.Common;

using System.Reflection;

namespace ControlWorks.Services.Rest
{
    public class ServiceInfoResponse
    {
        public string Version { get; set; }
        public string LogFile { get; set; }
        public string Port => ConfigurationProvider.Port;
        public byte SourceStationId => ConfigurationProvider.SourceStationId;
        public string BaseDirectory => ConfigurationProvider.BaseDirectory;
        public string SettingsDirectory => ConfigurationProvider.SettingsDirectory;

        public ServiceInfoResponse(string logFile)
        {
            LogFile = logFile;
        }

        public void Build()
        {
            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
