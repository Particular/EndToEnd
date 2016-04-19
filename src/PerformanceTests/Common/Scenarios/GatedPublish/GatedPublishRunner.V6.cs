﻿#if Version6
using NServiceBus;
using System.Threading.Tasks;

partial class GatedPublishRunner
{
    async Task SendLocal(object msg)
    {
        await EndpointInstance.SendLocal(msg);
    }

    async Task Publish(object msg)
    {
        await EndpointInstance.Publish(msg);
    }

    public class Handler : IHandleMessages<Event>
    {
        public async Task Handle(Event message, IMessageHandlerContext ctx)
        {
            X.Signal();
        }
    }
}

#endif