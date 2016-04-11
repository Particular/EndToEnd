using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Transactions;
using Common.Encryption;
using Common.Messages;
using NServiceBus;
using Utils;

public static class DataBusInitiator
{
#if Version6
    public static void InitiateDataBus(this IEndpointInstance bus, BusCreationOptions options)
#else
    public static void InitiateDataBus(this IBus bus, BusCreationOptions options)
#endif
    {
        var endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');

        // sends half the messages without transactions
        Statistics.Instance.SendTimeNoTx = SeedInputQueue(
            bus,
            options.NumberOfMessages / 2,
            endpointName,
            options.NumberOfThreads,
            options.TwoPhaseCommit,
            options.UseEncryption,
            createTransaction: false);

        // sends the other half with transactions
        Statistics.Instance.SendTimeWithTx = SeedInputQueue(
            bus,
            options.NumberOfMessages / 2,
            endpointName,
            options.NumberOfThreads,
            options.TwoPhaseCommit,
            options.UseEncryption,
            createTransaction: true);
    }

#if Version6
    static TimeSpan SeedInputQueue(IEndpointInstance bus, int numberOfMessages, string inputQueue, int numberOfThreads, bool twoPhaseCommit, bool encryption, bool createTransaction)
#else
    static TimeSpan SeedInputQueue(IBus bus, int numberOfMessages, string inputQueue, int numberOfThreads, bool twoPhaseCommit, bool encryption, bool createTransaction)
#endif
    {
        var sw = new Stopwatch();

        sw.Start();
        Parallel.For(
            0,
            numberOfMessages,
            new ParallelOptions { MaxDegreeOfParallelism = numberOfThreads },
            x =>
            {
                var message = CreateMessage(encryption);
                message.TwoPhaseCommit = twoPhaseCommit;
                message.Id = x;

                if (createTransaction)
                {
                    using (var tx = new TransactionScope())
                    {
#if Version6
                        bus.Send(inputQueue, message).GetAwaiter().GetResult();
#else
                        bus.Send(inputQueue, message);
#endif
                        tx.Complete();
                    }
                }
                else
                {
#if Version6
                    bus.Send(inputQueue, message).GetAwaiter().GetResult();
#else
                    bus.Send(inputQueue, message);
#endif
                }
            });
        sw.Stop();

        return sw.Elapsed;
    }

    public const string EncryptedBase64Value = "encrypted value";
    const string MySecretMessage = "A secret";

    static MessageBase CreateMessage(bool encryption)
    {
        if (encryption)
        {
            // need a new instance of a message each time
            var message = new EncryptionTestMessage
            {
                Secret = MySecretMessage,
                CreditCard = new ClassForNesting { EncryptedProperty = MySecretMessage },
                LargeByteArray = new byte[1], // the length of the array is not the issue now
                ListOfCreditCards =
                    new List<ClassForNesting>
                    {
                            new ClassForNesting {EncryptedProperty = MySecretMessage},
                            new ClassForNesting {EncryptedProperty = MySecretMessage}
                    }
            };
            message.ListOfSecrets = new ArrayList(message.ListOfCreditCards);

            return message;
        }

        return new TestMessage();
    }
}