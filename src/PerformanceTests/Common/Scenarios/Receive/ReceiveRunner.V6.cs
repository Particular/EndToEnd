#if Version6 || Version7
using NServiceBus;
using System.Threading.Tasks;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner
{
    public partial class Handler : IHandleMessages<Command>
    {
        public Task Handle(Command message, IMessageHandlerContext ctx)
        {
            Signal();
            return Task.FromResult(0);
        }
    }
}
#endif