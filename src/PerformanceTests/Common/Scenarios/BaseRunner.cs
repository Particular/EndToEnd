#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
#if Version7
using NServiceBus.Configuration.AdvancedExtensibility;
#endif
#if Version6
using NServiceBus.Configuration.AdvanceExtensibility;
#endif
using System;
using System.Linq;
using System.Reflection;
using NServiceBus;
using NServiceBus.Logging;
using Tests.Permutations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Scenarios;
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
    protected readonly BatchHelper.IBatchHelper BatchHelper = global::BatchHelper.Instance;
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
        Log.Info("Creating send only endpoint...");
        await CreateSendOnlyEndpoint().ConfigureAwait(false);

        try
        {
            Log.InfoFormat("Start seeding messages for {0:N} seconds...", Settings.SeedDuration.TotalSeconds);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Settings.SeedDuration);

            var count = 0L;
            var start = Stopwatch.StartNew();

            const int MinimumBatchSeedDuration = 2500;
            var batchSize = 512;

            Parallel.ForEach(IterateUntilFalse(() => !cts.Token.IsCancellationRequested),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                b =>
              {
                  var currentBatchSize = batchSize;
                  var sw = Stopwatch.StartNew();
                  BatchHelper.Batch(currentBatchSize, i => instance.SendMessage(Session)).ConfigureAwait(false).GetAwaiter().GetResult();
                  Interlocked.Add(ref count, currentBatchSize);
                  var duration = sw.ElapsedMilliseconds;
                  if (duration < MinimumBatchSeedDuration)
                  {
                      batchSize = currentBatchSize * 2; // Last writer wins
                      Log.InfoFormat("Increasing seed batch size to {0,7:N0} as sending took {1,7:N0}ms which is less then {2:N0}ms", batchSize, duration, MinimumBatchSeedDuration);
                  }
              }
            );

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

#if Version5
    async Task CreateOrPurgeAndDrainQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        ShortcutBehavior.Shortcut = true; // Required, as instance already receives messages before DrainMessages() is called!
        var instance = Bus.Create(configuration).Start();
        await DrainMessages().ConfigureAwait(false);
        await new Session(instance).CloseWithSuppress().ConfigureAwait(false);
    }

    Task CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        var instance = Bus.CreateSendOnly(configuration);
        Session = new Session(instance);
        return Task.FromResult(0);
    }

    Task CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.CustomConfigurationSource(this);
        configuration.DefineCriticalErrorAction(OnCriticalError);

        if (IsSendOnly)
        {
            Session = new Session(Bus.CreateSendOnly(configuration));
            return Task.FromResult(0);
        }

        configuration.PurgeOnStartup(!IsSeedingData && IsPurgingSupported);
        Session = new Session(Bus.Create(configuration).Start());
        return Task.FromResult(0);
    }

    BusConfiguration CreateConfiguration()
    {
        var configuration = new Configuration();
        configuration.EndpointName(EndpointName);
        configuration.EnableInstallers();

        var scanableTypes = GetTypesToInclude();
        configuration.TypesToScan(scanableTypes);

        configuration.ApplyProfiles(this);

        return configuration;
    }

    List<Type> GetTypesToInclude()
    {
        var location = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var asm = new NServiceBus.Hosting.Helpers.AssemblyScanner(location).GetScannableAssemblies();

        var allTypes = (from a in asm.Assemblies
                        from b in a.GetLoadableTypes()
                        select b).ToList();

        var allTypesToExclude = GetTypesToExclude(allTypes);

        var finalInternalListToScan = allTypes.Except(allTypesToExclude);

        return finalInternalListToScan.ToList();
    }

    void OnCriticalError(string errorMessage, Exception exception)
    {
        try
        {
            try
            {
                Log.Fatal("OnCriticalError", exception);
                Session.CloseWithSuppress().ConfigureAwait(false).GetAwaiter().GetResult();
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
        finally
        {
            Environment.FailFast("NServiceBus critical error", exception);
        }
    }

#else
    async Task CreateOrPurgeAndDrainQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        ShortcutBehavior.Shortcut = true; // Required, as instance already receives messages before DrainMessages() is called!
        var instance = await Endpoint.Start(configuration).ConfigureAwait(false);
        await DrainMessages().ConfigureAwait(false);
        await new Session(instance).CloseWithSuppress().ConfigureAwait(false);
    }

    async Task CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.SendOnly();
        var instance = await Endpoint.Start(configuration).ConfigureAwait(false);
        Session = new Session(instance);
    }

    async Task CreateEndpoint()
    {
        var configuration = CreateConfiguration();

        var x = this as IConfigureUnicastBus;

        if (x != null)
        {
            var routing = new RoutingSettings<FakeAbstractTransport>(configuration.GetSettings());

            foreach (var m in x.GenerateMappings())
            {
                var isEvent = typeof(IEvent).IsAssignableFrom(m.MessageType);
                if (isEvent) routing.RegisterPublisher(m.MessageType, m.Endpoint);
                else routing.RouteToEndpoint(m.MessageType, m.Endpoint);

            }
        }

        if (IsSendOnly)
        {
            configuration.SendOnly();
            Session = new Session(await Endpoint.Start(configuration).ConfigureAwait(false));
            return;
        }

        //configuration.PurgeOnStartup(!IsSeedingData && IsPurgingSupported);

        var instance = Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult();
        Session = new Session(instance);
    }

    Configuration CreateConfiguration()
    {
        var configuration = new Configuration(EndpointName);
        configuration.EnableInstallers();
        configuration.AssemblyScanner().ExcludeTypes(GetTypesToExclude().ToArray());
        configuration.ApplyProfiles(this);
        configuration.DefineCriticalErrorAction(OnCriticalError);
        return configuration;
    }

    IEnumerable<Type> GetTypesToExclude()
    {
        return GetTypesToExclude(Assembly.GetAssembly(this.GetType()).GetTypes());
    }

    async Task OnCriticalError(ICriticalErrorContext context)
    {
        try
        {
            try
            {
                Log.Fatal("OnCriticalError", context.Exception);
                await context.Stop();
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }
        finally
        {
            Environment.FailFast("NServiceBus critical error", context.Exception);
        }
    }

#endif

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
    bool IsPurgingSupported => Permutation.Transport != Transport.AzureServiceBus;

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
}