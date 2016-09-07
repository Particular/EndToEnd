using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Config;
using NServiceBus.Logging;

/// <summary>
/// Does a continious test where a configured set of messages are 'seeded' on the
/// queue. For each message that is received one message will be published. This
/// means that the sending of the message is part of the receiving context and
/// thus part of the same transaction.
/// 
/// Then the test is stopped the handler stops forwarding the message. The test
/// waits until no new messages are received.
/// </summary>
partial class PublishOneOnOneRunner : BaseRunner, IConfigureUnicastBus
{
    readonly ILog Log = LogManager.GetLogger(nameof(PublishOneOnOneRunner));
    const int seedSize = 100;

    protected override async Task Start(ISession session)
    {
        var start = Stopwatch.StartNew();

        do
        {
            await TaskHelper.ParallelFor(seedSize, () => session.Publish(new Event
            {
                Data = Data
            }));
        } while (start.ElapsedMilliseconds < 2500);
    }

    protected override async Task Stop()
    {
        Handler.Shutdown = true;

        long current;

        Log.Info("Draining queue...");
        do
        {
            current = Handler.Count;
            await Task.Delay(100);
        } while (Handler.Count > current);
    }

    public class Event : IEvent
    {
        public byte[] Data { get; set; }
    }

    public MessageEndpointMappingCollection GenerateMappings()
    {
        var mappings = new MessageEndpointMappingCollection();

        var messageType = typeof(Event);

        mappings.Add(new MessageEndpointMapping
        {
            AssemblyName = messageType.Assembly.FullName,
            TypeFullName = messageType.FullName,
            Endpoint = EndpointName
        });

        return mappings;
    }

    partial class Handler
    {
        public static bool Shutdown;
        public static long Count;
    }
}
