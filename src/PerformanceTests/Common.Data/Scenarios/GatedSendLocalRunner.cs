﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

/// <summary>
/// Performs a continious test where a batch of messages is send via the bus without
/// a transaction and a handler processes these in parallel. Once all messages are
/// received it repeats this. Due to the fact that the sending is not transactional
/// the handler will already process messages while the batch is still being send.
/// </summary>
partial class GatedSendLocalRunner : LoopRunner
{
    const int batchSize = 4096;
    ILog Log = LogManager.GetLogger(typeof(GatedSendLocalRunner));
    static CountdownEvent X;

    protected override void Loop(object o)
    {
        Log.Warn("Sleeping for the bus to purge the queue. Loop requires the queue to be empty.");
        Thread.Sleep(5000);
        Log.Info("Starting");

        X = new CountdownEvent(batchSize);

        var po = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount - 1, // Leave one core for transport and persistence,
            CancellationToken = stopLoop.Token
        };

        while (!Shutdown)
        {
            try
            {
                Console.Write("1");
                X.Reset();

                Parallel.For(0, X.InitialCount, po, i =>
                {
                    SendLocal(CommandGenerator.Create());
                });
                Console.Write("2");

                X.Wait(stopLoop.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
        Log.Info("Stopped");
    }

    static class CommandGenerator
    {
        static long orderId;

        public static Command Create()
        {
            var id = Interlocked.Increment(ref orderId);
            return new Command
            {
                OrderId = id.ToString(CultureInfo.InvariantCulture)
            };
        }
    }

    public class Command : ICommand
    {
        public string OrderId { get; set; }
        public decimal Value { get; set; }
    }
}
