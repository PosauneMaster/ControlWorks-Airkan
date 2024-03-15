using ControlWorks.Common;
using ControlWorks.Services.Rest.Models;

using System.IO;
using System.Threading.Tasks;

namespace ControlWorks.Services.Rest.Processors
{
    public class LogProcessor
    {
        public async Task<LogItem> GetLogFileNamesAsync()
        {
            return await Task.Run(() =>
            {
                var logItem = new LogItem();
                logItem.LogDirectory = ConfigurationProvider.LogFilePath;
                foreach (var file in Directory.GetFiles(ConfigurationProvider.LogFilePath, "*.log", SearchOption.AllDirectories))
                {
                   logItem.AddLogFile(file);

                }

                return logItem;
            });
        }
    }
}
