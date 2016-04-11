namespace Utils
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using Splunk.Logging;

    public class TraceLogger
    {
        public static void Initialize()
        {
            var url = ConfigurationManager.AppSettings["SplunkURL"];
            var port = int.Parse(ConfigurationManager.AppSettings["SplunkPort"]);

            var listener = new TcpTraceListener(url, port, new ExponentialBackoffTcpReconnectionPolicy());
            Trace.Listeners.Add(listener);

            listener.AddLoggingFailureHandler(ex => 
            {
                Console.Error.WriteLine("Splunk logger failed: {0}", ex);
            });

            Trace.WriteLine($"Splunk Tracelogger configured at {url}:{port}");
        }
    }
}