using NServiceBus;
using Common;

class AzureServiceBusProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .ConnectionString(this.GetConnectionString("AzureServiceBus"));
    }
}
