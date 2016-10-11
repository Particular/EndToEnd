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
    long first;
    long last;
    readonly long applicationStart = GetTimestamp();
    long warmup;
    long numberOfMessages;
    long numberOfRetries;
    TimeSpan sendTimeNoTx = TimeSpan.Zero;
    TimeSpan sendTimeWithTx = TimeSpan.Zero;

    Process process;
    PerformanceCounter privateBytesCounter;
    Timer perfCountersTimer;
    Permutation permutation;

    static ConcurrentBag<double> perfCounterValues = new ConcurrentBag<double>();

    static Logger logger = LogManager.GetLogger("Statistics");

    public long NumberOfMessages => numberOfMessages;
    public long NumberOfRetries => numberOfRetries;
    public double Throughput { get; private set; }

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
        if (instance != null) throw new InvalidOperationException("Instance already active");
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

        ConfigureSplunk();
    }

    static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    double ToSeconds(long start, long end)
    {
        return (end - start) / (double)Stopwatch.Frequency;
    }

    public void Reset(string testName)
    {
        warmup = GetTimestamp();
        Interlocked.Exchange(ref numberOfMessages, 0);
        Interlocked.Exchange(ref numberOfRetries, 0);
        sendTimeNoTx = TimeSpan.Zero;
        sendTimeWithTx = TimeSpan.Zero;
        perfCountersTimer.Dispose();

        GlobalDiagnosticsContext.Set("testname", testName);
    }

    public void Dump()
    {
        var durationSeconds = Math.Max(-1, ToSeconds(warmup, last));

        LogStats("ReceiveFirstLastDuration", durationSeconds, "s");
        LogStats("NumberOfMessages", NumberOfMessages, "#");

        Throughput = NumberOfMessages / durationSeconds;

        LogStats("Throughput", Throughput, "msg/s");

        LogStats("NumberOfRetries", NumberOfRetries, "#");

        var ttfm = Math.Max(-1, ToSeconds(applicationStart, first));

        LogStats("TimeToFirstMessage", ttfm, "s");

        if (sendTimeNoTx != TimeSpan.Zero)
            LogStats("Sending", Convert.ToDouble(NumberOfMessages / 2) / sendTimeNoTx.TotalSeconds, "msg/s");

        if (sendTimeWithTx != TimeSpan.Zero)
            LogStats("SendingInsideTX", Convert.ToDouble(NumberOfMessages / 2) / sendTimeWithTx.TotalSeconds, "msg/s");

        var counterValues = perfCounterValues.ToList();
        LogStats("PrivateBytes-Min", counterValues.Min() / 1024, "kb");
        LogStats("PrivateBytes-Max", counterValues.Max() / 1024, "kb");
        LogStats("PrivateBytes-Avg", counterValues.Average() / 1024, "kb");
    }

    static void LogStats(string key, double value, string unit)
    {
        logger.Info("{0}: {1:0.0} ({2})", key, value, unit);
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

    public void UpdateFirst()
    {
        if (first == 0)
        {
            first = GetTimestamp();
        }
    }

    public void UpdateLast()
    {
        last = GetTimestamp();
    }


    public void IncMessages()
    {
        Interlocked.Increment(ref numberOfMessages);
    }

    public void IncRetries()
    {
        Interlocked.Increment(ref numberOfRetries);
    }
}
