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
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Scenarios;
using Variables;

public abstract class BaseRunner : IConfigurationSource, IContext
{
    const double SeedDurationMax = 0.80;
    const double SeedDurationMin = 0.05;

    readonly ILog Log = LogManager.GetLogger("BaseRunner");

    string SeedReceiveRatioPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "Perftests",
        Permutation.Category,
        Permutation.Description,
        Permutation.Tests[0],
        Permutation.Id,
        "SeedDurationFactor.txt");
    double seedAvg;
    long seedCount;

    public Permutation Permutation { get; private set; }
    public string EndpointName { get; private set; }
    ISession Session { get; set; }

    protected byte[] Data { private set; get; }
    protected bool SendOnly { get; set; }
    protected int MaxConcurrencyLevel { private set; get; }

    protected static bool Shutdown { private set; get; }
    protected readonly BatchHelper.IBatchHelper BatchHelper = global::BatchHelper.Instance;

    /// <summary>
    /// Suppressing ObjectDisposedException failures to workaround https://github.com/Particular/NServiceBus.AzureServiceBus/issues/337
    /// </summary>
    bool SuppressException(Exception ex)
    {
        return ex is ObjectDisposedException;
    }

    public async Task Execute(Permutation permutation, string endpointName)
    {
        Permutation = permutation;
        EndpointName = endpointName;

        InitData();
        MaxConcurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);

        try
        {
            await CreateOrPurgeQueues().ConfigureAwait(false); // Workaround for pubsub to self with purge on startup
        }
        catch (Exception ex) when (SuppressException(ex))
        {
            Log.Warn("Suppressing", ex);
        }

        var seedCreator = this as ICreateSeedData;
        if (seedCreator != null)
        {
            Log.InfoFormat("Create seed data...");
            try
            {
                ReadPermutationSeedDurationFactor();
                await CreateSeedData(seedCreator).ConfigureAwait(false);
            }
            catch (Exception ex) when (SuppressException(ex))
            {
                Log.Warn("Suppressing", ex);
            }
        }
        else
        {
            seedCount = Int64.MaxValue;
            Interlocked.Exchange(ref NServiceBus.Performance.StatisticsBehavior.ShortcutCount, 0);
        }

        Log.InfoFormat("Create receiving endpoint...");
        await CreateEndpoint().ConfigureAwait(false);

        try
        {
            Log.Info("Starting...");
            await Start(Session).ConfigureAwait(false);
            Log.Info("Started");

            if (!IsSeedingData)
            {
                Log.InfoFormat("Warmup: {0}", Settings.WarmupDuration);
                await Task.Delay(Settings.WarmupDuration).ConfigureAwait(false);
            }

            var runDuration = IsSeedingData
                ? Settings.RunDuration - Settings.SeedDuration
                : Settings.RunDuration;

            Log.InfoFormat("Run: {0}", runDuration);

            Statistics.Instance.Reset(GetType().Name);

            var interval = TimeSpan.FromSeconds(1.5);
            var current = NServiceBus.Performance.StatisticsBehavior.ShortcutCount;
            var stopTimestamp = DateTime.UtcNow.Add(runDuration);
            bool isExpired, hasIncomingMessages;
            var remainingTime = stopTimestamp - DateTime.UtcNow;
            do
            {
                Log.InfoFormat("Waiting until run duration expires in {0,5:N1}s or all messages received ({1,7:N0} of {2,7:N0} / {3,4:N1}%)", remainingTime.TotalSeconds, current, seedCount, current * 100.0 / seedCount);
                await Task.Delay(remainingTime > interval ? interval : remainingTime).ConfigureAwait(false);
                remainingTime = stopTimestamp - DateTime.UtcNow;
                current = NServiceBus.Performance.StatisticsBehavior.ShortcutCount;
                hasIncomingMessages = seedCount > current;
                isExpired = remainingTime.Ticks < 0;
            } while (hasIncomingMessages && !isExpired);
            if (!hasIncomingMessages) Log.Info("No more incoming messages.");
            if (isExpired) Log.Info("Maximum run duration expired.");

            Statistics.Instance.Dump();
            WritePermutationSeedDurationFactor();

            Shutdown = true;
            Log.Info("Stopping...");
            await Stop().ConfigureAwait(false);
            Log.Info("Stopped");
        }
        finally
        {
            try
            {
                await Session.Close().ConfigureAwait(false);
            }
            catch (Exception ex) when (SuppressException(ex))
            {
                Log.Warn("Suppressing", ex);
            }
        }
    }

    protected virtual Task Start(ISession session)
    {
        return Task.FromResult(0);
    }

    protected virtual Task Stop()
    {
        return DrainMessages();
    }

    async Task CreateSeedData(ICreateSeedData instance)
    {
        Log.Info("Creating or purging queues...");
        try
        {
            await CreateOrPurgeQueues().ConfigureAwait(false);
        }
        catch (Exception ex) when (SuppressException(ex))
        {
            Log.WarnFormat("Suppress", ex);
        }
        Log.Info("Creating send only endpoint...");
        try
        {
            await CreateSendOnlyEndpoint().ConfigureAwait(false);
        }
        catch (Exception ex) when (SuppressException(ex))
        {
            Log.WarnFormat("Suppress", ex);
        }

        try
        {
            Log.InfoFormat("Start seeding messages for {0} seconds...", Settings.SeedDuration.TotalSeconds);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(Settings.SeedDuration);

            var count = 0L;
            var start = Stopwatch.StartNew();
            var expires = DateTime.UtcNow.Add(Settings.SeedDuration);

            const int minimumBatchSeedDuration = 2500;
            var batchSize = 512;

            Parallel.ForEach(IterateUntilFalse(() => !cts.Token.IsCancellationRequested),
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                b =>
              {
                  var currentBatchSize = batchSize;

                  if (expires < DateTime.UtcNow || currentBatchSize <= 0)
                  {
                      Log.DebugFormat("Expired ({0}) or batch size ({1:N0})", expires, currentBatchSize);
                      return;
                  }

                  var sw = Stopwatch.StartNew();
                  BatchHelper.Batch(currentBatchSize, i => instance.SendMessage(Session)).ConfigureAwait(false).GetAwaiter().GetResult();
                  Interlocked.Add(ref count, currentBatchSize);
                  var duration = sw.ElapsedMilliseconds;

                  var now = DateTime.UtcNow;
                  var remaining = expires - now;

                  if (remaining.TotalMilliseconds < duration)
                  {
                      batchSize = (int)(currentBatchSize * (remaining.TotalMilliseconds / duration));
                      Log.InfoFormat("Batchsize set to {0:N0} because ending", batchSize);
                  }
                  else if (duration < minimumBatchSeedDuration && (2 * duration) < remaining.TotalMilliseconds)
                  {
                      batchSize = currentBatchSize * 2; // Last writer wins
                      Log.InfoFormat("Increasing seed batch size to {0,4:N0} as sending took {1,5:N0}ms which is less then {2:N0}ms", batchSize, duration, minimumBatchSeedDuration);
                  }
              }
            );

            seedCount = count;
            var elapsed = start.Elapsed;
            seedAvg = count / elapsed.TotalSeconds;
            Log.InfoFormat("Done seeding, seeded {0:N0} messages, {1:N1} msg/s", count, seedAvg);
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedThroughputAvg", seedAvg, "msg/s");
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedCount", count, "#");
            LogManager.GetLogger("Statistics").InfoFormat("{0}: {1:0.0} ({2})", "SeedDuration", elapsed.TotalMilliseconds, "ms");
        }
        finally
        {
            await Session.Close().ConfigureAwait(false);
        }
    }

#if Version5
    async Task CreateOrPurgeQueues()
    {
        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        using (Bus.Create(configuration).Start())
        {
            await DrainMessages().ConfigureAwait(false);
        }
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
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.CustomConfigurationSource(this);
        configuration.DefineCriticalErrorAction(OnCriticalError);

        if (SendOnly)
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
        var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
                Session.Close();
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
    async Task CreateOrPurgeQueues()
    {
        IEndpointInstance instance = null;

        var configuration = CreateConfiguration();
        if (IsPurgingSupported) configuration.PurgeOnStartup(true);
        try
        {
            instance = await Endpoint.Start(configuration).ConfigureAwait(false);
            await DrainMessages().ConfigureAwait(false);
        }
        finally
        {
            if (instance != null)
            {
                await instance.Stop().ConfigureAwait(false);
            }
        }
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
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.CustomConfigurationSource(this);

        if (SendOnly)
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
        configuration.ExcludeTypes(GetTypesToExclude().ToArray());
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

    protected async Task DrainMessages()
    {
        const int DrainPollInterval = 1500;
        NServiceBus.Performance.StatisticsBehavior.Shortcut = true;
        var startCount = NServiceBus.Performance.StatisticsBehavior.ShortcutCount;
        long current;
        var start = Stopwatch.StartNew();

        Log.Info("Draining queue...");
        do
        {
            current = NServiceBus.Performance.StatisticsBehavior.ShortcutCount;
            Log.DebugFormat("Delaying to detect receive activity, last count is {0}...", current);
            await Task.Delay(DrainPollInterval).ConfigureAwait(false);
        } while (NServiceBus.Performance.StatisticsBehavior.ShortcutCount > current);

        var diff = current - startCount;
        Log.InfoFormat("Drained {0:N0} message(s) in {0:N0}ms", diff, start.ElapsedMilliseconds);
        NServiceBus.Performance.StatisticsBehavior.Shortcut = false;
    }

    void ReadPermutationSeedDurationFactor()
    {
        var fi = new FileInfo(SeedReceiveRatioPath);
        if (!fi.Exists) return;
        Log.InfoFormat("Reading seed/receive ration from '{0}'.", fi);
        //Convert.ToDouble(ConfigurationManager.AppSettings["SeedDurationFactor"], CultureInfo.InvariantCulture)
        var lines = File.ReadAllLines(fi.FullName);

        var ratio = lines.Select(l => double.Parse(l, CultureInfo.InvariantCulture)).Average();
        ratio = Math.Min(SeedDurationMax, Math.Max(SeedDurationMin, ratio));
        Log.InfoFormat("Set SeedDurationFactor to {0:N}.", ratio);
        Settings.SeedDuration = TimeSpan.FromSeconds((Settings.RunDuration + Settings.WarmupDuration).TotalSeconds * ratio);
    }

    void WritePermutationSeedDurationFactor()
    {
        var i = Statistics.Instance;
        var receivedAvg = i.Throughput;
        var seedReceiveRatio = receivedAvg / (seedAvg + receivedAvg);

        if (double.IsNaN(seedReceiveRatio)) return;

        Log.InfoFormat("SeedDurationFactor ratio: {0:N} ({1:N}/{2:N})", seedReceiveRatio, seedAvg, receivedAvg);
        var fi = new FileInfo(SeedReceiveRatioPath);
        Log.InfoFormat("Writing SeedDurationFactor ratio to '{0}'.", fi);
        fi.Directory.Create();
        File.AppendAllText(fi.FullName, seedReceiveRatio.ToString(CultureInfo.InvariantCulture) + Environment.NewLine);
    }
}
