using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;

namespace ControlWorks.Services.Rest
{
    public class LogFileResponse
    {

        private readonly ILog _log;


        public string FileName { get; set; }
        public string Contents { get; set; }

        public LogFileResponse()
        {
        }

        public LogFileResponse(ILog log)
        {
            _log = log;
        }

        public void Build()
        {
            try
            {
                var name = "LogFileAppender";

                var rootAppender = _log.Logger.Repository
                    .GetAppenders()
                    .OfType<RollingFileAppender>()
                    .FirstOrDefault(fa => fa.Name == name);

                var sb = new StringBuilder();

                string filename = rootAppender != null ? rootAppender.File : string.Empty;
                FileName = filename;

                if (File.Exists(filename))
                {

                    using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        string line;
                        while ((line = sr.ReadLine()) != null)
                        {
                            sb.AppendLine(line);
                        }
                    }
                }

                Contents = sb.ToString();

            }
            catch (Exception e)
            {
                _log.Error($"LogFileResponse.Build {e.Message}", e);
            }
        }



    }
}
