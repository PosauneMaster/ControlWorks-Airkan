using Microsoft.SqlServer.Server;

using System;
using System.Diagnostics;

namespace ControlWorks.Common.Logging
{
    public class ControlWorksListener : TraceListener
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            base.TraceData(eventCache, source, eventType, id, data);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            base.TraceEvent(eventCache, source, eventType, id);
        }
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        { 
            if (eventType == TraceEventType.Error)
            {
                if (args != null && args.Length > 0)
                {
                    var ex = args[0] as Exception;
                    if (ex != null)
                    {
                        Logger.Error(ex, $"{source}|{format}");
                        base.TraceEvent(eventCache, source, eventType, id, format, args);
                        return;
                    }
                }
            }
            Logger.Info($"{source}|{format}");

            base.TraceEvent(eventCache, source, eventType, id, format, args);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType == TraceEventType.Error)
            {
                Logger.Error($"{source}|{message}");
            }
            else
            {
                Logger.Info($"{source}|{message}");
            }

            base.TraceEvent(eventCache, source, eventType, id, message);
        }

        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            base.TraceTransfer(eventCache, source, id, message, relatedActivityId);
        }

        public override void Write(string message)
        {
        }

        public override void WriteLine(string message)
        {
        }
    }
}