namespace Utils
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using NLog;

    public class TraceLogger
    {
        public static void Initialize()
        {
            var url = ConfigurationManager.AppSettings["SplunkURL"];
            var port = int.Parse(ConfigurationManager.AppSettings["SplunkPort"]);

            Trace.Listeners.Add(new NLogTraceListener());

            Trace.WriteLine($"Splunk Tracelogger configured at {url}:{port}");
        }
    }
}