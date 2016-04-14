#if Version5
using Common.Scenarios;
using NServiceBus;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner : ICreateSeedData
{
    public int SeedSize { get; set; } = 50000;
    
    public partial class Handler : IHandleMessages<Command>
    {
        public IBus Bus { get; set; }

        public void Handle(Command message)
        {
        }
    }

    public void SendMessage(ISendOnlyBus sendOnlyBus, string endpointName)
    {
        var address = new Address(endpointName, "localhost");
        sendOnlyBus.Send(address, new Command());
    }
}
#endif