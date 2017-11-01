using System;
using System.Diagnostics;
using NServiceBus;
using NServiceBus.Logging;
using Tests.Permutations;
using Variables;

class MsmqProfile : IProfile, INeedPermutation
{
    ILog Log = LogManager.GetLogger(nameof(MsmqProfile));

    public Permutation Permutation { private get; set; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var transport = endpointConfiguration.UseTransport<MsmqTransport>();
        transport.DisableDeadLetterQueueing();

        if (Permutation.TransactionMode != TransactionMode.Default
            && Permutation.TransactionMode != TransactionMode.None
            && Permutation.TransactionMode != TransactionMode.Receive
            && Permutation.TransactionMode != TransactionMode.Atomic
            && Permutation.TransactionMode != TransactionMode.Transactional
            ) throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);

        if (Permutation.TransactionMode != TransactionMode.Default) transport.Transactions(Permutation.GetTransactionMode());

        RunInspections();
    }

    long SizeThreshold = 1024 * 1024 * 1024; // 1GB
    long CountThreshold = 100000;

    void RunInspections()
    {
        try
        {
            long size, count;

            using (var bytes = new PerformanceCounter("MSMQ Service", "Total bytes in all queues"))
            {
                size = bytes.RawValue;
            }

            using (var messages = new PerformanceCounter("MSMQ Service", "Total messages in all queues"))
            {
                count = messages.RawValue;
            }

            Log.InfoFormat("MSMQ Currently contains {0:N0} messages, occupying {1:N0} bytes", count, size);

            if (count > CountThreshold || size > SizeThreshold)
            {
                Log.WarnFormat("MSMQ message count ({0:N0}) or size ({1:N0}) exceeded. Please verify if MSMQ has a lot of (journaled) messages or message in the system (transactional) dead letter queue.", CountThreshold, SizeThreshold);
            }
        }
        catch (Exception ex)
        {
            Log.Debug("Optional MSMQ inspections failed to run.", ex);
        }
    }
}
