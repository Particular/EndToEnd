using System;
using System.Data.Common;
using NServiceBus;
using NServiceBus.Settings;
using Tests.Permutations;
using Variables;

class AmazonSQSProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(BusConfiguration cfg)
    {
        var cs = ConfigurationHelper.GetConnectionString("AmazonSQS");
        var builder = new DbConnectionStringBuilder { ConnectionString = cs };
        builder.Remove("NativeDeferral");

        // https://docs.particular.net/transports/sqs/configuration-options?version=sqs_1
        cfg.UseTransport<SqsTransport>()
            .ConnectionString(builder.ToString());

        // https://docs.particular.net/transports/sqs/transaction-support
        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        InitTransactionMode(cfg.Transactions());
    }

    void InitTransactionMode(TransactionSettings transactionSettings)
    {
        var mode = Permutation.TransactionMode;
        switch (mode)
        {
            case TransactionMode.Default:
                return;
            case TransactionMode.None:
                transactionSettings.Disable();
                return;
            case TransactionMode.Receive:
                transactionSettings.DisableDistributedTransactions();
                return;
            case TransactionMode.Atomic:
            case TransactionMode.Transactional:
            default:
                throw new NotSupportedException("TransactionMode: " + mode);
        }
    }
}
