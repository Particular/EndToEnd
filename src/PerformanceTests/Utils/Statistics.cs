using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;
using Tests.Permutations;

[Serializable]
public class Statistics : IDisposable
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

    Process process;
    PerformanceCounter privateBytesCounter;
    Timer perfCountersTimer;
    Permutation permutation;

    static ConcurrentBag<double> perfCounterValues = new ConcurrentBag<double>();

    static Logger logger = LogManager.GetLogger("Statistics");

    ~Statistics()
    {
        Dispose(true);
    }

    public void Dispose()
    {
        Dispose(false);
    }

    void Dispose(bool disposing)
    {
        if (disposing)
        {
            process?.Dispose();
            privateBytesCounter?.Dispose();
            perfCountersTimer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    static Statistics instance;
    public static Statistics Instance
    {
        get
        {
            if (instance == null) throw new InvalidOperationException("Statistics wasn't initialized. Call 'Initialize' first.");

            return instance;
        }
    }

    public static IDisposable Initialize(Permutation permutation)
    {
        if(instance!=null) throw new InvalidOperationException("Instance already active");
        return instance = new Statistics(permutation);
    }

    Statistics(Permutation permutation)
    {
        this.permutation = permutation;
        process = Process.GetCurrentProcess();
        privateBytesCounter = new PerformanceCounter("Process", "Private Bytes", process.ProcessName);
        perfCountersTimer = new Timer(state =>
            perfCounterValues.Add(privateBytesCounter.NextValue()),
            null,
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(1));

        StartTime = DateTime.UtcNow;
        ConfigureSplunk();
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

    void ConfigureSplunk()
    {
        var sessionId = GetSessionId();
        GlobalDiagnosticsContext.Set("sessionid", sessionId);
        GlobalDiagnosticsContext.Set("permutationId", permutation.Id);
        GlobalDiagnosticsContext.Set("testcategory", permutation.Category);
        GlobalDiagnosticsContext.Set("testdescription", permutation.Description);
    }

    static string GetSessionId()
    {
        return Environment.GetCommandLineArgs().Where(arg => arg.StartsWith("--sessionId")).Select(arg => arg.Substring("--sessionId".Length + 1)).First();
    }
}
