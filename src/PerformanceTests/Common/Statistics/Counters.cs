using System;
using System.Diagnostics;
using System.Linq;

class Counters
{
    const string category = "PerfTest";
    public PerformanceCounter receives, sends, concurrency;

    static Counters()
    {
        if (Environment.GetCommandLineArgs().Contains("--remove"))
        {
            Uninstall();
            Environment.Exit(0);
        }

        if (Environment.GetCommandLineArgs().Contains("--install"))
        {
            Install();
            Environment.Exit(0);
        }
    }

    static void Install()
    {
        if (PerformanceCounterCategory.Exists(category))
        {
            Console.WriteLine("Already registered!");
            Environment.Exit(0);
        }

        Console.WriteLine("Installing...");
        var counters = new CounterCreationDataCollection();
        //counters.Add(new CounterCreationData("Receive concurrency", "Current receive concurrency", PerformanceCounterType.));
        //counters.Add(new CounterCreationData("Send concurrency", "Current send concurrency", PerformanceCounterType.ElapsedTime));
        counters.Add(new CounterCreationData("Receives", "Number of receives", PerformanceCounterType.RateOfCountsPerSecond64));
        counters.Add(new CounterCreationData("Sends", "Number of sends", PerformanceCounterType.RateOfCountsPerSecond64));
        counters.Add(new CounterCreationData("Concurrency", "How many messages are being processed concurrently", PerformanceCounterType.NumberOfItems64));
        PerformanceCounterCategory.Create(category, "Performance tests", PerformanceCounterCategoryType.SingleInstance, counters);
        Console.WriteLine("Installed!");
    }

    static void Uninstall()
    {
        if (PerformanceCounterCategory.Exists(category))
        {
            Console.WriteLine("Removing...");
            PerformanceCounterCategory.Delete(category);
            Console.WriteLine("Removed!");
        }
        else
        {
            Console.WriteLine("Not registered!");
        }
    }

    public Counters()
    {
        if (PerformanceCounterCategory.CounterExists("Concurrency", category))
        {
            Console.WriteLine("Setup");
            receives = new PerformanceCounter(category, "Receives", false);
            sends = new PerformanceCounter(category, "Sends", false);
            concurrency = new PerformanceCounter(category, "Concurrency", false);
            concurrency.RawValue = 0;
        }
    }

    long GetTimestamp()
    {
        return Stopwatch.GetTimestamp();
    }

    public void Start()
    {
        receives.Increment();
        concurrency.Increment();
    }

    public void End()
    {
        concurrency.Decrement();
    }
}