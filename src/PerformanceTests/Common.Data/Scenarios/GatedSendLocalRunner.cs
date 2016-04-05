﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

partial class GatedSendLocalRunner : LoopRunner
{
    const int batchSize = 4096;
    ILog Log = LogManager.GetLogger(typeof(GatedSendLocalRunner));
    static CountdownEvent X;

    protected override void Loop(object o)
    {
        Log.Warn("Sleeping for the bus to purge the queue. Loop requires the queue to be empty.");
        Thread.Sleep(5000);

        X = new CountdownEvent(batchSize);

        var po = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount - 1 // Leave one core for transport and persistence
        };

        while (!Shutdown)
        {
            X.Reset();

            Parallel.For(0, X.InitialCount, po, i =>
            {
                SendLocal(CommandGenerator.Create());
            });

            try
            {
                X.Wait(stopLoop.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
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