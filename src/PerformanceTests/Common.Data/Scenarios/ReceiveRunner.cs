#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using NServiceBus;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner : BaseRunner
{
    protected override void Start()
    {
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
    }
}

