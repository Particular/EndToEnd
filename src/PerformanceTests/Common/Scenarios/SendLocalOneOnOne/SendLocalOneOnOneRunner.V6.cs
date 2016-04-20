﻿#if Version6
using NServiceBus;
using System.Threading.Tasks;

partial class SendLocalOneOnOneRunner
{
    public partial class Handler : IHandleMessages<Command>
    {
        public async Task Handle(Command message, IMessageHandlerContext ctx)
        {
            if (Shutdown) return;
            await ctx.SendLocal(message);
            System.Threading.Interlocked.Increment(ref Count);
        }
    }
}

#endif
