#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using System;
using System.Linq;
using NServiceBus.Logging;
using Tests.Permutations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Variables;

public abstract partial class BaseRunner : IContext
{
    readonly ILog Log = LogManager.GetLogger("BaseRunner");

    public Permutation Permutation { get; private set; }
    public string EndpointName { get; private set; }
    ISession Session { get; set; }

    protected byte[] Data { private set; get; }
    public bool IsSendOnly { get; set; }
    protected int MaxConcurrencyLevel { private set; get; }

    protected static bool Shutdown { private set; get; }
    protected readonly Statistics Statistics = Statistics.Instance;

    public async Task Execute(Permutation permutation, string endpointName)
    {
        Permutation = permutation;
        EndpointName = endpointName;

        InitData();
        MaxConcurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);

        Log.Info("Creating or purging/draining queues...");
        await CreateOrPurgeAndDrainQueues().ConfigureAwait(false);

        var seedCreator = this as ICreateSeedData;
        if (seedCreator != null)
        {
            Log.InfoFormat("Create seed data...");
            await CreateSeedData(seedCreator).ConfigureAwait(false);
        }


        await Setup().ConfigureAwait(false);
        Statistics.Reset(GetType().Name);

        Log.InfoFormat("Create receiving endpoint...");
        await CreateEndpoint().ConfigureAwait(false);

        try
        {
            Log.Info("Starting...");
            await Start(Session).ConfigureAwait(false);
            Log.Info("Started");

            await Wait(WaitUntilRunDurationExpires()).ConfigureAwait(false);

            Statistics.Dump();
            Shutdown = true;
            Log.Info("Stopping...");
            await Stop().ConfigureAwait(false);
            Log.Info("Stopped");
        }
        finally
        {
            Log.Info("Closing...");
            await Session.CloseWithSuppress().ConfigureAwait(false);
            Log.Info("Closed");
        }
    }

    protected virtual Task Start(ISession session)
    {
        return Task.FromResult(0);
    }

    protected virtual Task Stop()
    {
        return Task.FromResult(0);
    }

    async Task CreateSeedData(ICreateSeedData instance)
    {
        var sendOnly = IsSendOnly;
        IsSendOnly = true; // Needed as performance counter profiles apply configuration that is not allowed on send-only endpoints.
        try
        {
            Log.Info("Creating send only endpoint...");
            await CreateSendOnlyEndpoint().ConfigureAwait(false);
        }
        finally
        {
            IsSendOnly = sendOnly;
        }

        try
        {
            Log.InfoFormat("Start seeding messages for {0:N} seconds until {1}...", Settings.SeedDuration.TotalSeconds, DateTime.Now + TimeSpan.FromSeconds(Settings.SeedDuration.TotalSeconds));
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Settings.SeedDuration);

            var count = 0L;
            var start = Stopwatch.StartNew();

            const int MinimumBatchSeedDuration = 2500;
            var batchSize = 512;

            Log.InfoFormat("BatchHelper type: {0}", BatchHelper.Instance.GetType());

            while (!cts.Token.IsCancellationRequested)
            {
                var currentBatchSize = batchSize;
                var sw = Stopwatch.StartNew();
                await BatchHelper.Instance.Batch(currentBatchSize,
                    async i =>
                    {
                        try
                        {
                            await RetryWithBackoff(() => instance.SendMessage(Session), cts.Token, i.ToString(), 5)
                                .ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            Interlocked.Decrement(ref count);
                        }
                    }).ConfigureAwait(false);
                Interlocked.Add(ref count, currentBatchSize);
                var duration = sw.ElapsedMilliseconds;
                if (duration < MinimumBatchSeedDuration)
                {
                    batchSize = currentBatchSize * 2; // Last writer wins
                    Log.InfoFormat("Increasing seed batch size to {0,7:N0} as sending took {1,7:N0}ms which is less then {2:N0}ms", batchSize, duration, MinimumBatchSeedDuration);
                }
            }

            var elapsed = start.Elapsed;
            var avg = count / elapsed.TotalSeconds;
            Log.InfoFormat("Done seeding, seeded {0:N0} messages, {1:N1} msg/s", count, avg);
            LogManager.GetLogger("Statistics").InfoFormat(Statistics.StatsFormatDouble, "SeedThroughputAvg", avg, "msg/s");
            LogManager.GetLogger("Statistics").InfoFormat(Statistics.StatsFormatInt, "SeedCount", count, "#");
            LogManager.GetLogger("Statistics").InfoFormat(Statistics.StatsFormatDouble, "SeedDuration", elapsed.TotalMilliseconds, "ms");
        }
        finally
        {
            await Session.CloseWithSuppress().ConfigureAwait(false);
        }
    }

    IEnumerable<Type> GetTypesToExclude(IEnumerable<Type> allTypes)
    {
        var allTypesToExclude = (from t in allTypes
                                 where (t.IsSubclassOf(typeof(BaseRunner)) || t.IsSubclassOf(typeof(LoopRunner))) && t != GetType()
                                 select t).ToList();

        Log.InfoFormat("This is test {0}, excluding :", GetType().Name);
        foreach (var theType in allTypesToExclude)
        {
            Log.InfoFormat("- {0}", theType.Name);
        }
        return allTypesToExclude;
    }

    void InitData()
    {
        Data = new byte[(int)Permutation.MessageSize];
        new Random(0).NextBytes(Data);
    }

    bool IsSeedingData => this is ICreateSeedData;
    bool IsPurgingSupported => (Permutation.Transport != Transport.AzureServiceBus);

    static IEnumerable<int> IterateUntilFalse(Func<bool> condition)
    {
        var i = 0;
        while (condition()) yield return i++;
    }

    async Task DrainMessages()
    {
        try
        {
            const int DrainPollInterval = 1500;
            var startCount = ShortcutBehavior.Count;
            long current;
            var start = Stopwatch.StartNew();
            ShortcutBehavior.Shortcut = true;

            Log.Info("Draining queue...");
            do
            {
                current = ShortcutBehavior.Count;
                Log.DebugFormat("Delaying to detect receive activity, last count is {0:N0}...", current);
                await Task.Delay(DrainPollInterval).ConfigureAwait(false);
            } while (ShortcutBehavior.Count > current);

            var diff = current - startCount;
            Log.InfoFormat("Drained {0:N0} message(s) in {1:N0}ms", diff, start.ElapsedMilliseconds);
        }
        finally
        {
            ShortcutBehavior.Shortcut = false;
        }
    }

    protected virtual Task Wait(Task baseTask)
    {
        return baseTask;
    }

    protected virtual Task Setup()
    {
        return Task.FromResult(0);
    }

    async Task WaitUntilRunDurationExpires()
    {
        Log.InfoFormat("Run: Duration {0}, until {1}", Settings.RunDuration, DateTime.Now + Settings.RunDuration);
        await Task.Delay(Settings.RunDuration).ConfigureAwait(false);
        Log.Info("Run duration expired.");
    }

    static Random random = new Random();
    async Task RetryWithBackoff(Func<Task> action, CancellationToken token, string id, int maxAttempts)
    {
        while (true)
        {
            var attempts = 0;
            try
            {
                await action()
                    .ConfigureAwait(false);
                return;
            }
            catch (Exception ex)
            {
                if (attempts > maxAttempts) throw new InvalidOperationException("Exhausted send retries.", ex);
                double next;
                lock (random) next = random.NextDouble();
                next *= 0.2; // max 20% jitter
                next += 1D;
                next *= 100 * Math.Pow(2, attempts++);
                var delay = TimeSpan.FromMilliseconds(next); // Results in 100ms, 200ms, 400ms, 800ms, etc. including max 20% random jitter.
                Log.WarnFormat("{0} attempt {1} / {2} : {3} ({4})", id, attempts, delay, ex.Message, ex.GetType());
                    await Task.Delay(delay, token)
                        .ConfigureAwait(false);
            }
        }
    }
}
