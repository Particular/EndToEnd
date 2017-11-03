using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using log4net;
using Tests.Permutations;

[Serializable]
public class Statistics : IDisposable
{
    public const string StatsFormatInt = "{0}: {1:N0} ({2})";
    public const string StatsFormatDouble = "{0}: {1:N} ({2})";

    long first;
    long last;
    readonly long applicationStart = GetTimestamp();
    long warmup;
    long numberOfMessages;
    long numberOfRetries;
    TimeSpan sendTimeNoTx = TimeSpan.Zero;
    TimeSpan sendTimeWithTx = TimeSpan.Zero;

    Process process;

    static ILog logger = LogManager.GetLogger("Statistics");

    public long NumberOfMessages => numberOfMessages;
    public long NumberOfRetries => numberOfRetries;


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
        return instance = new Statistics();
    }

    Statistics()
    {
        process = Process.GetCurrentProcess();
    }

    static long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    double ToSeconds(long start, long end)
    {
        return (end - start) / (double)Stopwatch.Frequency;
    }

    public void Reset()
    {
        warmup = GetTimestamp();
        Interlocked.Exchange(ref numberOfMessages, 0);
        Interlocked.Exchange(ref numberOfRetries, 0);
        sendTimeNoTx = TimeSpan.Zero;
        sendTimeWithTx = TimeSpan.Zero;
    }

    public void Dump()
    {
        var durationSeconds = Math.Max(-1, ToSeconds(warmup, last));

        LogStats("ReceiveFirstLastDuration", durationSeconds, "s", StatsFormatDouble);
        LogStats("NumberOfMessages", NumberOfMessages, "#", StatsFormatInt);

        var throughput = NumberOfMessages / durationSeconds;

        LogStats("Throughput", throughput, "msg/s", StatsFormatDouble);

        LogStats("NumberOfRetries", NumberOfRetries, "#", StatsFormatInt);

        var ttfm = Math.Max(-1, ToSeconds(applicationStart, first));

        LogStats("TimeToFirstMessage", ttfm, "s", StatsFormatDouble);

        if (sendTimeNoTx != TimeSpan.Zero)
            LogStats("Sending", Convert.ToDouble(NumberOfMessages / 2) / sendTimeNoTx.TotalSeconds, "msg/s", StatsFormatDouble);

        if (sendTimeWithTx != TimeSpan.Zero)
            LogStats("SendingInsideTX", Convert.ToDouble(NumberOfMessages / 2) / sendTimeWithTx.TotalSeconds, "msg/s", StatsFormatDouble);

        LogStats("PrivateMemorySize", process.PrivateMemorySize64 / 1024, "kB", StatsFormatInt);
        LogStats("PeakWorkingSet", process.PeakWorkingSet64 / 1024, "kB", StatsFormatInt);
        LogStats("PeakPagedMemorySize", process.PeakPagedMemorySize64 / 1024, "kB", StatsFormatInt);
        LogStats("PeakVirtualMemorySize", process.PeakVirtualMemorySize64 / 1024, "kB", StatsFormatInt);
    }

    static void LogStats(string key, double value, string unit, string format)
    {
        logger.InfoFormat(format, key, value, unit);
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
