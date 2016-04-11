﻿
using System.Threading.Tasks;
using Common.Scenarios;
#if Version6
    using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

using NServiceBus;
using NServiceBus.Logging;
using Tests.Permutations;

public abstract class BaseRunner
{
    readonly ILog log = LogManager.GetLogger("BaseRunner");

    readonly int seedSize;

#if Version5
    ISendOnlyBus endpoint = null;
#else
    IEndpointInstance endpoint = null;
#endif

    public BaseRunner(int seedSize)
    {
        this.seedSize = seedSize;
    }

    public virtual void Execute(Permutation permutation, string endpointName)
    {
        if (this is ICreateSeedData) CreateSeedData(permutation, endpointName);
    }

    private void CreateSeedData(Permutation permutation, string endpointName)
    {
        var configuration = CreateConfiguration(permutation, endpointName);
        CreateQueues(configuration);

        configuration = CreateConfiguration(permutation, endpointName);
        CreateSendOnlyEndpoint(configuration);

        Parallel.For(0, seedSize, ((i, state) =>
        {
            if (i % 5000 == 0)
                log.Info($"Seeded {i} messages.");

            ((ICreateSeedData) this).SendMessage(endpoint, endpointName);
        }));
        log.Info($"Seeded total of {seedSize} messages.");
    }

#if Version5
    void CreateQueues(Configuration configuration)
    {
        var createQueuesBus = Bus.Create(configuration).Start();
        createQueuesBus.Dispose();
    }

    void CreateSendOnlyEndpoint(Configuration configuration)
    {
        endpoint = Bus.CreateSendOnly(configuration);
    }

    BusConfiguration CreateConfiguration(Permutation permutation, string endpointName)
    {
        var configuration = new BusConfiguration();
        configuration.EndpointName(endpointName);
        configuration.EnableInstallers();
        configuration.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
        configuration.PurgeOnStartup(true); // Should this be on so we don't polute with weird messages we can't handle?
        configuration.ApplyProfiles(permutation);

        return configuration;
    }
#else
    void CreateQueues(Configuration configuration)
    {
        Endpoint.Create(configuration).GetAwaiter().GetResult();
    }

    void CreateSendOnlyEndpoint(Configuration configuration)
    {
        configuration.SendOnly();
        endpoint = Endpoint.Start(configuration).GetAwaiter().GetResult();
    }

    EndpointConfiguration CreateConfiguration(Permutation permutation, string endpointName)
    {
        var configuration = new EndpointConfiguration(endpointName);
        configuration.EnableInstallers();
        configuration.PurgeOnStartup(true);
        configuration.ApplyProfiles(permutation);

        return configuration;
    }
#endif
}
