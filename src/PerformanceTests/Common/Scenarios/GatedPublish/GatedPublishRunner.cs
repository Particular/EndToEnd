using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Config;

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

    public MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        var messageType = typeof(Event);

        Log.InfoFormat("Mapping {0} to {1}", messageType, EndpointName);

        mappings.Add(new MessageEndpointMapping
        {
            AssemblyName = messageType.Assembly.FullName,
            TypeFullName = messageType.FullName,
            Endpoint = EndpointName
        });

        return mappings;
    }

    class Handler : Handler<Event>
    {
    }
}


