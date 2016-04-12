using Metrics;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NLog;
using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using LogLevel = NLog.LogLevel;

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

    static Process process = Process.GetCurrentProcess();
    static PerformanceCounter privateBytesCounter = new PerformanceCounter("Process", "Private Bytes", process.ProcessName);

    [NonSerialized]
    public Meter Meter;

    static Logger logger = LogManager.GetLogger("Statistics");

    static Statistics instance;
    public static Statistics Instance
    {
        get
        {
            if (instance == null) throw new InvalidOperationException("Statistics wasn't initialized. Call 'Initialize' first.");

            return instance;
        }
    }

    public static void Initialize(string permutationId)
    {
        instance = new Statistics(permutationId);
        instance.StartTime = DateTime.UtcNow;
    }

    Statistics(string permutationId)
    {
        ConfigureMetrics(permutationId);
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
        var durationSeconds = (Last - Warmup).TotalSeconds;

        LogStats("NumberOfMessages", NumberOfMessages, "#");

        var throughput = NumberOfMessages / durationSeconds;

        LogStats("Throughput", throughput, "msg/s");

        Trace.WriteLine(string.Format("##teamcity[buildStatisticValue key='ReceiveThroughput' value='{0}']", Math.Round(throughput))); // [Hadi] Do we need this?

        LogStats("NumberOfRetries", NumberOfRetries, "#");
        LogStats("TimeToFirstMessage", (First - AplicationStart).Value.TotalSeconds, "s");

        if (SendTimeNoTx != TimeSpan.Zero)
            LogStats("Sending", Convert.ToDouble(NumberOfMessages / 2) / SendTimeNoTx.TotalSeconds, "msg/s");

        if (SendTimeWithTx != TimeSpan.Zero)
            LogStats("SendingInsideTX", Convert.ToDouble(NumberOfMessages / 2) / SendTimeWithTx.TotalSeconds, "msg/s");

        LogStats("PrivateBytes", privateBytesCounter.NextValue() / 1024, "kb");
    }

    static void LogStats(string key, double value, string unit)
    {
        logger.Debug($"{key}: {value:0.0} ({unit})");
    }

    public void Signal()
    {
        Meter.Mark(); //[Hadi] Don't think we need this anymore.
    }

    void ConfigureMetrics(string permutationId)
    {
        Meter = Metric.Meter("", Unit.Commands);
        Trace.Listeners.Add(new NLogTraceListener());

        var assemblyLocation = Assembly.GetEntryAssembly().Location;
        var assemblyFolder = Path.GetDirectoryName(assemblyLocation);
        var reportFolder = Path.Combine(assemblyFolder, "reports", DateTime.UtcNow.ToString("yyyy-MM-dd--HH-mm-ss"));

        Metric
            .Config.WithAllCounters()
            .WithReporting(report => report.WithCSVReports(reportFolder, TimeSpan.FromSeconds(1)));

        var url = ConfigurationManager.AppSettings["SplunkURL"];
        var port = int.Parse(ConfigurationManager.AppSettings["SplunkPort"]);
        var sessionId = GetSessionId();

        var config = new LoggingConfiguration();
        var target = new NetworkTarget
        {
            Address = $"tcp://{url}:{port}",
            Layout = Layout.FromString("${level}~${gdc:item=sessionid}~${gdc:item=permutationId}~${message}~${newline}"),
        };

        config.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, target));
        LogManager.Configuration = config;

        GlobalDiagnosticsContext.Set("sessionid", sessionId);
        GlobalDiagnosticsContext.Set("permutationId", permutationId);

        logger.Debug($"Splunk Tracelogger configured at {url}:{port}");
    }

    static string GetSessionId()
    {
        return Environment.GetCommandLineArgs().Where(arg => arg.StartsWith("--sessionId")).Select(arg => arg.Substring("--sessionId".Length + 1)).First();
    }
}