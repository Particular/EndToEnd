using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
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
    static Timer perfCountersTimer;

    static ConcurrentBag<double> perfCounterValues = new ConcurrentBag<double>();

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
        perfCountersTimer = new Timer(state =>
            perfCounterValues.Add(privateBytesCounter.NextValue()),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));
    }

    Statistics(string permutationId)
    {
        ConfigureSplunk(permutationId);
    }

    public void Reset()
    {
        Warmup = DateTime.UtcNow;
        Interlocked.Exchange(ref NumberOfMessages, 0);
        Interlocked.Exchange(ref NumberOfRetries, 0);
        SendTimeNoTx = TimeSpan.Zero;
        SendTimeWithTx = TimeSpan.Zero;
        perfCountersTimer.Dispose();
    }

    public void Dump()
    {
        var durationSeconds = (Last - Warmup).TotalSeconds;

        LogStats("NumberOfMessages", NumberOfMessages, "#");

        var throughput = NumberOfMessages / durationSeconds;

        LogStats("Throughput", throughput, "msg/s");

        LogStats("NumberOfRetries", NumberOfRetries, "#");
        LogStats("TimeToFirstMessage", (First - AplicationStart).Value.TotalSeconds, "s");

        if (SendTimeNoTx != TimeSpan.Zero)
            LogStats("Sending", Convert.ToDouble(NumberOfMessages / 2) / SendTimeNoTx.TotalSeconds, "msg/s");

        if (SendTimeWithTx != TimeSpan.Zero)
            LogStats("SendingInsideTX", Convert.ToDouble(NumberOfMessages / 2) / SendTimeWithTx.TotalSeconds, "msg/s");

        var counterValues = perfCounterValues.ToList();
        LogStats("PrivateBytes-Min", counterValues.Min() / 1024, "kb");
        LogStats("PrivateBytes-Max", counterValues.Max() / 1024, "kb");
        LogStats("PrivateBytes-Avg", counterValues.Average() / 1024, "kb");
    }

    static void LogStats(string key, double value, string unit)
    {
        logger.Debug($"{key}: {value:0.0} ({unit})");
    }

    void ConfigureSplunk(string permutationId)
    {
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