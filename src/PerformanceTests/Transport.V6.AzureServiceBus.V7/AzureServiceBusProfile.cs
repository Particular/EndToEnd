using System;
using NServiceBus;
using Tests.Permutations;
using Variables;

class AzureServiceBusProfile : IProfile, INeedPermutation
{
    readonly string connectionstring = ConfigurationHelper.GetConnectionString("AzureServiceBus");

    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {

        var transport = endpointConfiguration
            .UseTransport<AzureServiceBusTransport>();

        var concurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);
        transport.MessageReceivers().PrefetchCount(concurrencyLevel * Permutation.PrefetchMultiplier);

        transport
            .UseTopology<ForwardingTopology>()
            .ConnectionString(connectionstring)
            .Sanitization().UseStrategy<ValidateAndHashIfNeeded>()
            ;

        transport.MessagingFactories().BatchFlushInterval(TimeSpan.FromMilliseconds(50));
        transport.MessagingFactories().NumberOfMessagingFactoriesPerNamespace(Math.Max(5, concurrencyLevel / 32));
        transport.Queues().EnablePartitioning(true);

        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            && Permutation.TransactionMode != TransactionMode.Atomic
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());


    }
}