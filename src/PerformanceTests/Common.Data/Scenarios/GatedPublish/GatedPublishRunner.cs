using System;
using System.Collections.Generic;
using System.Diagnostics;
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
partial class GatedPublishRunner : LoopRunner
{
    int batchSize = 16;
    ILog Log = LogManager.GetLogger(typeof(GatedPublishRunner));
    static CountdownEvent X;

    protected override async Task Loop(object o)
    {
        try
        {
            Log.Warn("Sleeping for the bus to purge the queue. Loop requires the queue to be empty.");
            Thread.Sleep(5000);
            Log.Info("Starting");

            X = new CountdownEvent(batchSize);

            while (!Shutdown)
            {
                try
                {
                    Console.Write("1");
                    X.Reset(batchSize);

                    var d = Stopwatch.StartNew();

                    var sends = new List<Task>(X.InitialCount);
                    for (var i = 0; i < X.InitialCount; i++) sends.Add(Publish(new Event()));
                    await Task.WhenAll(sends);

                    if (d.Elapsed < TimeSpan.FromSeconds(2.5))
                    {
                        batchSize *= 2;
                        Log.InfoFormat("Batch size increased to {0}", batchSize);
                    }


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
        catch (Exception ex)
        {
            Log.Error("Loop", ex);
        }
    }

    public class Event : IEvent
    {
    }
}
