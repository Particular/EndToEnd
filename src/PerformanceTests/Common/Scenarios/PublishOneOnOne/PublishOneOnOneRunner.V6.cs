#if Version6 || Version7
using NServiceBus;
using System.Threading.Tasks;

partial class PublishOneOnOneRunner
{
    public partial class Handler : IHandleMessages<Event>
    {
        public async Task Handle(Event message, IMessageHandlerContext ctx)
        {
            if (Shutdown) return;
            await ctx.Publish(message).ConfigureAwait(false);
            System.Threading.Interlocked.Increment(ref Count);
        }
    }
}

#endif
