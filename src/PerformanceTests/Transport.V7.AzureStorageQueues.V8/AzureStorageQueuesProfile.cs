using System;
using NServiceBus;
using Variables;

class AzureStorageQueuesProfile : IProfile, INeedContext
{
    public IContext Context { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        if ((int)MessageSize.Medium < (int)Context.Permutation.MessageSize) throw new NotSupportedException($"Message size {Context.Permutation.MessageSize} not supported by ASQ.");

        var transport = endpointConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .SanitizeQueueNamesWith(queueName => queueName.Replace('.', '-'));

        transport
            .ConnectionString(ConfigurationHelper.GetConnectionString("AzureStorageQueue"));

        var Permutation = Context.Permutation;

        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default && !Context.IsSendOnly) transport.Transactions(Permutation.GetTransactionMode());

    }

}
