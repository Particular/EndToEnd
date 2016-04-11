#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using NServiceBus;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner : BaseRunner, IStartAndStop
{
    const int seedSize = 30000;

    public ReceiveRunner() : base(seedSize)
    {
    }

    public void Start()
    {
    }

    public void Stop()
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

