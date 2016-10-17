using System;
using System.Diagnostics;
using System.Messaging;
using NServiceBus;
using NServiceBus.Logging;
using Tests.Permutations;
using Variables;

class MsmqProfile : IProfile, INeedPermutation, INeedContext
{
    ILog Log = LogManager.GetLogger(nameof(MsmqProfile));

    public Permutation Permutation { private get; set; }
    public IContext Context { set; private get; }

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var connectionString = ConfigurationHelper.GetConnectionString("MSMQ");

        if (Permutation.Transport == Transport.MSMQ_NoTX)
        {
            connectionString += ";useTransactionalQueues=false";
            if (Permutation.TransactionMode != TransactionMode.None) throw new NotSupportedException("Transaction mode ${Permutation.TransactionMode} not supported for non transactional MSMQ.");
            endpointConfiguration.Conventions().DefiningExpressMessagesAs(_ => true);
        }

        var noTX = Permutation.Transport == Transport.MSMQ_NoTX;
        bool isTransactionalQueue;
        using (var queue = new MessageQueue(@".\Private$\" + Context.EndpointName))
        {
            isTransactionalQueue = queue.Transactional;
        }

        if (noTX && isTransactionalQueue || !noTX && !isTransactionalQueue)
        {
            foreach (var q in MessageQueue.GetPrivateQueuesByMachine("."))
            {
                using (q)
                {
                    Log.InfoFormat("Inspecting queue: {0} / {1}", q.FormatName, q.QueueName);
                    if (q.QueueName.StartsWith("private$\\" + Context.EndpointName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.WarnFormat("Deleting queue: {0}", q.FormatName);
                        MessageQueue.Delete(".\\" + q.QueueName);
                    }
                }
            }
        }

        var transport = endpointConfiguration.UseTransport<MsmqTransport>();
        transport.ConnectionString(connectionString);

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
