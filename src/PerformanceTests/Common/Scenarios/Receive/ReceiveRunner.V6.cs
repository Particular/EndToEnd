﻿#if Version6
using NServiceBus;
using System.Threading.Tasks;
using Common.Scenarios;

/// <summary>
/// Does a continuous test where a pre-seeded amount of messages will be handled
/// </summary>    
partial class ReceiveRunner : ICreateSeedData
{
    public int SeedSize { get; set; } = 50000;

    public partial class Handler : IHandleMessages<Command>
    {
        public async Task Handle(Command message, IMessageHandlerContext ctx)
        {
        }
    }

    public async Task SendMessage(IEndpointInstance endpointInstance)
    {
        await endpointInstance.SendLocal(new Command { Data = Data });
    }
}
#endif