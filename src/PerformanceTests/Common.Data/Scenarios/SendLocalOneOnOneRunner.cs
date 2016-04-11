using System.Collections.Generic;
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
partial class SendLocalOneOnOneRunner : BaseRunner
{
    const int seedSize = 1000;

    protected override void Start()
    {
        var sends = new List<Task>(seedSize);
        for (var i = 0; i < seedSize; i++) sends.Add(SendLocal(new Command()));
        Task.WaitAll(sends.ToArray());
    }

    protected override void Stop()
    {
        Handler.Shutdown = true;
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
