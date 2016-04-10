using System;
using System.Threading.Tasks;
using NServiceBus;

/// <summary>
/// Does a continious test where a configured set of messages are 'seeded' on the
/// queue. For each message that is received one message will be send. This means
/// that the sending of the message is part of the receiving context and thus
/// part of the same transaction.
/// 
/// Then the test is stopped the handler stops forwarding the message. The test
/// waits until no new messages are received.
/// </summary>
partial class SendLocalOneOnOneRunner : IStartAndStop
{
    const int seedSize = 1000;

    public void Start()
    {
        var po = new ParallelOptions
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount - 1 // Leave one core for transport and persistence
        };

        Parallel.For(0, seedSize, po, async i =>
        {
            await SendLocal(new Command());
        });
    }

    public void Stop()
    {
        Handler.Shutdown = true;

        // Wait until no new messages are processed so queues will be empty.
        long current = 0;
        while (current < (current = System.Threading.Interlocked.Read(ref Handler.Count)))
        {
            System.Threading.Thread.Sleep(250);
        }
    }

    public class Command : ICommand
    {
    }

    partial class Handler
    {
        public static bool Shutdown;
        public static long Count;
    }
}
