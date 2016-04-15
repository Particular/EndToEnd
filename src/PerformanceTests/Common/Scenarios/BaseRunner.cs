#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using System;
using System.Configuration;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Config.ConfigurationSource;
using NServiceBus.Logging;
using Tests.Permutations;

public abstract class BaseRunner : IConfigurationSource, IContext
{
    readonly ILog Log = LogManager.GetLogger("BaseRunner");

#if Version5
    protected IBus EndpointInstance { get; private set; }
#else
    protected IEndpointInstance EndpointInstance { get; private set; }
#endif

    public Permutation Permutation { get; private set; }
    public string EndpointName { get; private set; }

    public virtual void Execute(Permutation permutation, string endpointName)
    {
        Permutation = permutation;
        EndpointName = endpointName;

        CreateSeedData();

        EndpointInstance = CreateEndpoint();

        try
        {
            Start();
            Log.InfoFormat("Warmup: {0}", Settings.WarmupDuration);
            System.Threading.Thread.Sleep(Settings.WarmupDuration);
            Statistics.Instance.Reset();
            Log.InfoFormat("Run: {0}", Settings.RunDuration);
            System.Threading.Thread.Sleep(Settings.RunDuration);
            Statistics.Instance.Dump();
            Stop();
        }
        finally
        {
#if Version5
            using(EndpointInstance){}
#else
            EndpointInstance.Stop().GetAwaiter().GetResult();
#endif
        }
    }

    protected abstract void Start();
    protected abstract void Stop();

    private void CreateSeedData()
    {
        var seedCreator = this as ICreateSeedData;

        if (seedCreator == null) return;

        if (seedCreator.SeedSize == 0) throw new InvalidOperationException("SeedSize was not set.");

        CreateQueues();

        var sendonlyInstance = CreateSendOnlyEndpoint();

        try
        {
            Parallel.For(0, seedCreator.SeedSize, (i, state) =>
            {
                if (i % 5000 == 0)
                    Log.Info($"Seeded {i} messages.");

                ((ICreateSeedData)this).SendMessage(sendonlyInstance, EndpointName);
            });
            Log.Info($"Seeded total of {seedCreator.SeedSize} messages.");
        }
        finally
        {
#if Version5
            using(sendonlyInstance){}
#else
            sendonlyInstance.Stop().GetAwaiter().GetResult();
#endif
        }
    }

#if Version5
    void CreateQueues()
    {
        var configuration = CreateConfiguration();
        configuration.PurgeOnStartup(false);
        var createQueuesBus = Bus.Create(configuration).Start();
        createQueuesBus.Dispose();
    }

    ISendOnlyBus CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        return Bus.CreateSendOnly(configuration);
    }

    IBus CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.PurgeOnStartup(false);
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.CustomConfigurationSource(this);
        return Bus.Create(configuration).Start();
    }

    Configuration CreateConfiguration()
    {
        var configuration = new Configuration();
        configuration.EndpointName(EndpointName);
        configuration.EnableInstallers();
        configuration.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
        configuration.ApplyProfiles(this);

        return configuration;
    }
#else
    void CreateQueues()
    {
        var configuration = CreateConfiguration();
        configuration.PurgeOnStartup(true);
        Endpoint.Create(configuration).GetAwaiter().GetResult();
    }

    IEndpointInstance CreateSendOnlyEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.SendOnly();
        return Endpoint.Start(configuration).GetAwaiter().GetResult();
    }

    IEndpointInstance CreateEndpoint()
    {
        var configuration = CreateConfiguration();
        configuration.EnableFeature<NServiceBus.Performance.SimpleStatisticsFeature>();
        configuration.PurgeOnStartup(false);
        configuration.CustomConfigurationSource(this);

        return Endpoint.Start(configuration).GetAwaiter().GetResult();
    }

    Configuration CreateConfiguration()
    {
        var configuration = new Configuration(EndpointName);
        configuration.EnableInstallers();
        configuration.ApplyProfiles(this);

        return configuration;
    }
#endif

    public T GetConfiguration<T>() where T : class, new()
    {
        IConfigureUnicastBus configureUnicastBus;

        if (typeof(T) == typeof(UnicastBusConfig) && null != (configureUnicastBus = this as IConfigureUnicastBus))
        {
            //read from existing config 
            var config = (UnicastBusConfig)ConfigurationManager.GetSection(typeof(UnicastBusConfig).Name);
            if (config != null) throw new InvalidOperationException("UnicastBUs Configuration should be in code.");

            return new UnicastBusConfig
            {
                MessageEndpointMappings = configureUnicastBus.GenerateMappings()
            } as T;
        }

        return ConfigurationManager.GetSection(typeof(T).Name) as T;
    }

}