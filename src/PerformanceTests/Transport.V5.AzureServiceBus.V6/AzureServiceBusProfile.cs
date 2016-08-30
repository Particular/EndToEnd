﻿using System;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Settings;
using Tests.Permutations;
using Variables;

class AzureServiceBusProfile : IProfile, INeedPermutation
{
    readonly ILog Log = LogManager.GetLogger<AzureServiceBusProfile>();
    readonly string connectionstring = ConfigurationHelper.GetConnectionString("AzureServiceBus");

    public Permutation Permutation { private get; set; }

    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.ScaleOut().UseSingleBrokerQueue();

        busConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .ConnectionString(connectionstring);

        InitTransactionMode(busConfiguration.Transactions());
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
            case TransactionMode.Atomic: // Can mimic batched sends behavior using transaction scope enlistment. Results in transmitting message after all processing completes.
                transactionSettings.EnableDistributedTransactions();
                Log.WarnFormat("Using TransactionMode.Atomic results in batched sends but not actual atomic receive with sends.");
                return;
            case TransactionMode.Transactional:
            default:
                throw new NotSupportedException("TransactionMode: " + mode);
        }
    }
}
