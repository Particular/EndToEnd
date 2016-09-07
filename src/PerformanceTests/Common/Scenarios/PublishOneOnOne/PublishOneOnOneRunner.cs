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

    protected override async Task Start(ISession session)
    {
        Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
        await Task.Delay(3000).ConfigureAwait(false);
        var maxConcurrencyLevel = ConcurrencyLevelConverter.Convert(Permutation.ConcurrencyLevel);
        var seedSize = maxConcurrencyLevel * 2;
        Log.InfoFormat("Seeding {0} messages based on concurrency level of {1}.", seedSize, maxConcurrencyLevel);
        await TaskHelper.ParallelFor(seedSize, () => session.Publish(new Event
        {
            Data = Data
        })).ConfigureAwait(false);
    }

    protected override async Task Stop()
    {
        Handler.Shutdown = true;

        long current;

        Log.Info("Draining queue...");
        do
        {
            current = Handler.Count;
            Log.Debug("Delaying to detect receive activity...");
            await Task.Delay(100).ConfigureAwait(false);
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
