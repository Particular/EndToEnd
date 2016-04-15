#if Version6
using NServiceBus;
using System.Threading.Tasks;

partial class GatedSendLocalRunner: IProfile
{
    async Task SendLocal(Command msg)
    {
        await this.EndpointInstance.SendLocal(msg);
    }

    public class Handler : IHandleMessages<Command>
    {
        public async Task Handle(Command message, IMessageHandlerContext ctx)
        {
            X.Signal();
        }
    }

    public void Configure(EndpointConfiguration cfg)
    {
        cfg.PurgeOnStartup(true);
    }
}

#endif