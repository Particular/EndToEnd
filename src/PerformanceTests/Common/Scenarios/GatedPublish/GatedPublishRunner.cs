﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Logging;

/// <summary>
/// Performs a continious test where a batch of messages is send via the bus without
/// a transaction and a handler processes these in parallel. Once all messages are
/// received it repeats this. Due to the fact that the sending is not transactional
/// the handler will already process messages while the batch is still being send.
/// </summary>
partial class GatedPublishRunner : LoopRunner, IConfigureUnicastBus
{
    int batchSize = 16;
    ILog Log = LogManager.GetLogger(typeof(GatedPublishRunner));
    static CountdownEvent X;

    protected override async Task Loop(object o)
    {
        try
        {
            Log.Info("Starting");

            X = new CountdownEvent(batchSize);

            while (!Shutdown)
            {
                try
                {
                    Console.Write("1");
                    X.Reset(batchSize);

                    var d = Stopwatch.StartNew();

                    var sends = new Task[X.InitialCount];
                    for (var i = 0; i < X.InitialCount; i++) sends[i] = Publish(new Event());
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

    public MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        var messageType = typeof(Event);

        Log.InfoFormat("Mapping {0} to {1}", messageType, endpointName);

        mappings.Add(new MessageEndpointMapping
        {
            AssemblyName = messageType.Assembly.FullName,
            TypeFullName = messageType.FullName,
            Endpoint = endpointName
        });

        return mappings;
    }
}


