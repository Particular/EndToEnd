using System.Collections.Generic;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

/// <summary>
/// Does a continious test where a configured set of messages are 'seeded' on the
/// queue. For each message that is received one message will be published. This
/// means that the sending of the message is part of the receiving context and
/// thus part of the same transaction.
/// 
/// When the test is stopped, the handler stops forwarding the message. The test
/// continues until no new messages are received.
/// </summary>
partial class PublishOneOnOneRunner : PerpetualRunner, IConfigureUnicastBus
{
    readonly ILog Log = LogManager.GetLogger(nameof(PublishOneOnOneRunner));

    protected override async Task Start(ISession session)
    {
        Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
        await Task.Delay(3000).ConfigureAwait(false);
        await base.Start(session).ConfigureAwait(false);
    }

    protected override Task Seed(int i, ISession session)
    {
        return session.Publish(new Event { Data = Data });
    }

    public class Event : IEvent
    {
        public byte[] Data { get; set; }
    }

    public IEnumerable<Mapping> GenerateMappings()
    {
        yield return new Mapping
        {
            MessageType = typeof(Event),
            Endpoint = EndpointName
        };
    }

    partial class Handler
    {
        public static long Count;
    }
}
