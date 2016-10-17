using System;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;
using NServiceBus.Logging;
using Tests.Permutations;
using Variables;

class AzureServiceBusProfile : IProfile, INeedPermutation
{
    readonly ILog Log = LogManager.GetLogger(nameof(AzureServiceBusProfile));
    readonly string connectionstring = ConfigurationHelper.GetConnectionString("AzureServiceBus");

    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration
            .UseTransport<AzureServiceBusTransport>();

        var concurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);

        transport.MessageReceivers().PrefetchCount(Permutation.PrefetchMultiplier);

        transport
            .UseTopology<ForwardingTopology>()
            .ConnectionString(connectionstring)
            .Sanitization().UseStrategy<ValidateAndHashIfNeeded>()
            ;

        transport.MessagingFactories().BatchFlushInterval(TimeSpan.FromMilliseconds(100)); // Improves batched sends 

        var numberOfFactoriesAndClients = Math.Min(64, concurrencyLevel); // Making sure that number of (receive) clients is equal to the number of factories, improves receive performance

        Log.InfoFormat("Concurrency level: {0}", concurrencyLevel);
        Log.InfoFormat("Messaging factories per namespace: {0}", numberOfFactoriesAndClients);
        Log.InfoFormat("Clients per entity: {0}", numberOfFactoriesAndClients);
        Log.InfoFormat("Prefetch count: {0}", Permutation.PrefetchMultiplier);

        transport.MessagingFactories().NumberOfMessagingFactoriesPerNamespace(numberOfFactoriesAndClients);
        transport.NumberOfClientsPerEntity(numberOfFactoriesAndClients);
        transport.Queues().EnablePartitioning(true);
        transport.Topics().EnablePartitioning(true);

        if (Permutation.Transport == Transport.AzureServiceBus_AMQP) transport.TransportType(TransportType.Amqp);


        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            && Permutation.TransactionMode != TransactionMode.Atomic
            || Permutation.TransactionMode == TransactionMode.Atomic && Permutation.Transport == Transport.AzureServiceBus_AMQP
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());
    }
}
