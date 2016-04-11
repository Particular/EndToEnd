
using System.Messaging;
using Common;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Unicast.Queuing;

class MsmqProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UseTransport<MsmqTransport>()
            .ConnectionString("deadLetter=false;journal=false");
    }
}
