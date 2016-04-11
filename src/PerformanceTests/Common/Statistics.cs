using Metrics;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

[Serializable]
public class Statistics
{
    public DateTime? First;
    public DateTime Last;
    public readonly DateTime AplicationStart = DateTime.UtcNow;
    public DateTime StartTime;
    public DateTime Warmup;
    public long NumberOfMessages;
    public long NumberOfRetries;
    public TimeSpan SendTimeNoTx = TimeSpan.Zero;
    public TimeSpan SendTimeWithTx = TimeSpan.Zero;

    [NonSerialized]
    public Meter Meter;

    static Statistics instance;
    public static Statistics Instance
    {
        get
        {
            if (instance == null) throw new InvalidOperationException("Statistics wasn't initialized. Call 'Initialize' first.");

            return instance;
        }
    }

    public static void Initialize()
    {
        instance = new Statistics();
        instance.StartTime = DateTime.UtcNow;
    }

    Statistics()
    {
        ConfigureMetrics();
    }

    public void Reset()
    {
        Warmup = DateTime.UtcNow;
        Interlocked.Exchange(ref NumberOfMessages, 0);
        Interlocked.Exchange(ref NumberOfRetries, 0);
        SendTimeNoTx = TimeSpan.Zero;
        SendTimeWithTx = TimeSpan.Zero;
    }

    public void Dump()
    {
        Trace.WriteLine("");
        Trace.WriteLine("---------------- Statistics ----------------");

        var durationSeconds = (Last - Warmup).TotalSeconds;

        PrintStats("NumberOfMessages", NumberOfMessages, "#");

        var throughput = NumberOfMessages / durationSeconds;

        PrintStats("Throughput", throughput, "msg/s");

        Trace.WriteLine(string.Format("##teamcity[buildStatisticValue key='ReceiveThroughput' value='{0}']", Math.Round(throughput)));

        PrintStats("NumberOfRetries", NumberOfRetries, "#");
        PrintStats("TimeToFirstMessage", (First - AplicationStart).Value.TotalSeconds, "s");

        if (SendTimeNoTx != TimeSpan.Zero)
            PrintStats("Sending", Convert.ToDouble(NumberOfMessages / 2) / SendTimeNoTx.TotalSeconds, "msg/s");

        if (SendTimeWithTx != TimeSpan.Zero)
            PrintStats("SendingInsideTX", Convert.ToDouble(NumberOfMessages / 2) / SendTimeWithTx.TotalSeconds, "msg/s");
    }

    static void PrintStats(string key, double value, string unit)
    {
        Trace.WriteLine($"{key}: {value:0.0} ({unit})");
    }

    public void Signal()
    {
        Meter.Mark();
    }

    void ConfigureMetrics()
    {
        Meter = Metric.Meter("", Unit.Commands);

        Trace.Listeners.Add(new NLogTraceListener());

        Metric
            .Config.WithAllCounters()
            .WithReporting(report => report.WithNLogCSVReports(TimeSpan.FromSeconds(1)));

        var url = ConfigurationManager.AppSettings["SplunkURL"];
        var port = int.Parse(ConfigurationManager.AppSettings["SplunkPort"]);

        var config = new LoggingConfiguration();
        config.LoggingRules.Add(
            new LoggingRule("*", LogLevel.Debug, new NetworkTarget
            {
                Address = $"tcp://{url}:{port}",
                Layout = Layout.FromString("${message}")
            }));
        LogManager.Configuration = config;

        Trace.WriteLine($"Splunk Tracelogger configured at {url}:{port}");
    }
}