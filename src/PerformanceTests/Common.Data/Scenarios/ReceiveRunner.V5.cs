#if Version5
using NServiceBus;
using Tests.Permutations;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner
{
    public partial class Handler : IHandleMessages<Command>
    {
        public IBus Bus { get; set; }

        public void Handle(Command message)
        {
            if (Shutdown) return;
        }
    }

    protected override void CreateMessage(ISendOnlyBus sendOnlyBus, string endpointName)
    {
        var address = new Address(endpointName, "localhost");
        sendOnlyBus.Send(address, new Command());
    }
}
#endif