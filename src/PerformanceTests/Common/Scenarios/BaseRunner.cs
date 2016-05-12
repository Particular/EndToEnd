#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;
using Tests.Permutations;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Common.Scenarios;
using Variables;

public abstract class BaseRunner : IConfigurationSource, IContext
{
    readonly ILog Log = LogManager.GetLogger("BaseRunner");

    public Permutation Permutation { get; private set; }
    public string EndpointName { get; private set; }
    ISession Session { get; set; }

    protected byte[] Data { private set; get; }
    protected bool SendOnly { get; set; }

    public virtual void Execute(Permutation permutation, string endpointName)
    {
        Permutation = permutation;
        EndpointName = endpointName;

        InitData();

        CreateSeedData();
        CreateEndpoint();

        try
        {
            Start(Session);

            if (!IsSeedingData)
            {
                Log.InfoFormat("Warmup: {0}", Settings.WarmupDuration);
                Task.Delay(Settings.WarmupDuration).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            var runDuration = IsSeedingData
                ? Settings.RunDuration - Settings.SeedDuration
                : Settings.RunDuration;

            Log.InfoFormat("Run: {0}", runDuration);

            Statistics.Instance.Reset(GetType().Name);
            Task.Delay(runDuration).ConfigureAwait(false).GetAwaiter().GetResult();
            Statistics.Instance.Dump();

            Stop();
        }
        finally
        {
            Session.Close();
        }
    }

    protected virtual void Start(ISession session)
    {
    }

    protected virtual void Stop()
    {
    }

    void CreateSeedData()
    {
        var seedCreator = this as ICreateSeedData;
        if (seedCreator == null) return;

        CreateOrPurgeQueues();
        CreateSendOnlyEndpoint();

        try
        {
            Log.InfoFormat("Start seeding messages for {0} seconds...", Settings.SeedDuration.TotalSeconds);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Settings.SeedDuration);

            var maxConcurrency = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);

            var instance = (ICreateSeedData)this;
            var count = 0L;
            var start = Stopwatch.StartNew();

            var po = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency
            };
            Parallel.ForEach(IterateUntilFalse(() => !cts.Token.IsCancellationRequested), po, b =>
            {
                instance.SendMessage(Session).ConfigureAwait(false).GetAwaiter().GetResult();
                Interlocked.Increment(ref count);
            });

            var elapsed = start.Elapsed;
            var avg = count / elapsed.TotalSeconds;
            Log.InfoFormat("Done seeding, seeded {0:N0} messages, {1:N1} msg/s", count, avg);
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedThroughputAvg", avg, "msg/s");
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedCount", count, "#");
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedDuration", elapsed.TotalMilliseconds, "ms");
        }
        finally
        {
            Session.Close();
        }
    }

#if Version5
    void CreateOrPurgeQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        using (Bus.Create(configuration).Start()) { }
    }

    void CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        var instance = Bus.CreateSendOnly(configuration);
        Session = new Session(instance);
    }

    void CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.CustomConfigurationSource(this);

        if (SendOnly)
        {
            Session = new Session(Bus.CreateSendOnly(configuration));
            return;
        }

        configuration.PurgeOnStartup(!IsSeedingData && IsPurgingSupported);
        Session = new Session(Bus.Create(configuration).Start());
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
#else
    void CreateOrPurgeQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        var instance = Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult();
        instance.Stop().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    void CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.SendOnly();
        var instance = Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult();
        Session = new Session(instance);
    }

    void CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.CustomConfigurationSource(this);

        if (SendOnly)
        {
            configuration.SendOnly();
            Session = new Session(Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult());
            return;
        }

        configuration.PurgeOnStartup(!IsSeedingData && IsPurgingSupported);

        var instance = Endpoint.Start(configuration).ConfigureAwait(false).GetAwaiter().GetResult();
        Session = new Session(instance);
    }

    Configuration CreateConfiguration()
    {
        var configuration = new Configuration(EndpointName);
        configuration.EnableInstallers();

        configuration.ExcludeTypes(GetTypesToExclude().ToArray());

        configuration.ApplyProfiles(this);

        return configuration;
    }

    IEnumerable<Type> GetTypesToExclude()
    {
        return GetTypesToExclude(Assembly.GetAssembly(this.GetType()).GetTypes());
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

    public T GetConfiguration<T>() where T : class, new()
    {
        IConfigureUnicastBus configureUnicastBus;

        //read from existing config 
        var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);
        if (config != null) throw new InvalidOperationException("UnicastBUs Configuration should be in code using IConfigureUnicastBus interface.");

        if (typeof(T) == typeof(UnicastBusConfig) && null != (configureUnicastBus = this as IConfigureUnicastBus))
        {
            return new UnicastBusConfig
            {
                MessageEndpointMappings = configureUnicastBus.GenerateMappings()
            } as T;
        }

        return ConfigurationManager.GetSection(typeof(T).Name) as T;
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
}
