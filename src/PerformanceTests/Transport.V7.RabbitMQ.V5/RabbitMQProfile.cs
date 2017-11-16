using System;
using System.Data.Common;
using NServiceBus;
using Tests.Permutations;
using Variables;

class RabbitMQProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public RabbitMQProfile()
    {
#if NETCOREAPP2_0
        System.Console.WriteLine("this is the .net core version of the rabbitmq profile! :tada:");
#endif
#if NET452
        System.Console.WriteLine("this is the full framework version of the rabbitmq profile! :<");
#endif
    }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {


        var cs = ConfigurationHelper.GetConnectionString("RabbitMQ");
        var builder = new DbConnectionStringBuilder { ConnectionString = cs };

        var transport = endpointConfiguration
            .UseTransport<RabbitMQTransport>()
            .UseConventionalRoutingTopology()
            .PrefetchMultiplier(Permutation.PrefetchMultiplier);

        transport
            .ConnectionString(builder.ToString());

        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default)
            transport.Transactions(Permutation.GetTransactionMode());
    }
}
