using System;
using System.Data.Common;
using NServiceBus;
using Tests.Permutations;
using Variables;

class AmazonSQSProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var cs = new DbConnectionStringBuilder { ConnectionString = ConfigurationHelper.GetConnectionString("AmazonSQS") };

        // https://docs.particular.net/transports/sqs/configuration-options
        var transport = endpointConfiguration
            .UseTransport<SqsTransport>()
            .QueueNamePrefix(cs["QueueNamePrefix"].ToString())
            .MaxTTLDays(Convert.ToInt32(cs["MaxTTLDays"]))
            .NativeDeferral(Convert.ToBoolean(cs["NativeDeferral"]))
            .Region(cs["Region"].ToString())
            .S3BucketForLargeMessages(cs["S3BucketForLargeMessages"].ToString(), cs["S3KeyPrefix"].ToString())
            ;

        // https://docs.particular.net/transports/sqs/transaction-support
        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());
    }
}
