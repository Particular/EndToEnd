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
partial class SendLocalOneOnOneRunner : PerpetualRunner
{
    protected override Task Seed(int i, ISession session)
    {
        return session.Send(EndpointName, new Command { Data = Data });
    }

    public class Command : ICommand
    {
        public byte[] Data { get; set; }
    }

    partial class Handler
    {
        public static long Count;
    }
}
