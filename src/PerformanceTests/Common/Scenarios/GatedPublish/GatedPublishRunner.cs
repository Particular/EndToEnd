using System.Threading.Tasks;
using NServiceBus;
using System.Collections.Generic;

/// <summary>
/// Performs a continious test where a batch of messages is send via the bus without
/// a transaction and a handler processes these in parallel. Once all messages are
/// received it repeats this. Due to the fact that the sending is not transactional
/// the handler will already process messages while the batch is still being send.
/// </summary>
class GatedPublishRunner : LoopRunner, IConfigureUnicastBus
{
    protected override Task SendMessage(ISession session)
    {
        return session.Publish(new Event
        {
            Data = Data
        });
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
            Endpoint = EndpointName,
        };
    }

    class Handler : Handler<Event>
    {
    }
}

