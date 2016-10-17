using System;
using System.Messaging;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Settings;
using Tests.Permutations;
using Variables;

class MsmqProfile : IProfile, INeedPermutation, INeedContext
{
    ILog Log = LogManager.GetLogger(nameof(MsmqProfile));

    public Permutation Permutation { private get; set; }
    public IContext Context { set; private get; }

    public void Configure(BusConfiguration busConfiguration)
    {
        var connectionString = ConfigurationHelper.GetConnectionString("MSMQ");

        if (Permutation.Transport == Transport.MSMQ_NoTX)
        {
            connectionString += ";useTransactionalQueues=false";
            if (Permutation.TransactionMode != TransactionMode.None) throw new NotSupportedException("Transaction mode ${Permutation.TransactionMode} not supported for non transactional MSMQ.");
            busConfiguration.Conventions().DefiningExpressMessagesAs(_ => true);
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

        busConfiguration.UseTransport<MsmqTransport>()
            .ConnectionString(connectionString);

        InitTransactionMode(busConfiguration.Transactions());
    }

    void InitTransactionMode(TransactionSettings transactionSettings)
    {
        switch (Permutation.TransactionMode)
        {
            case TransactionMode.Default:
                return;
            case TransactionMode.None:
                transactionSettings.Disable();
                return;
            case TransactionMode.Transactional:
                transactionSettings.EnableDistributedTransactions();
                return;
            case TransactionMode.Atomic:
                transactionSettings.DisableDistributedTransactions();
                return;
            case TransactionMode.Receive:
            default:
                throw new NotSupportedException("TransactionMode: " + Permutation.TransactionMode);
        }
    }
}
