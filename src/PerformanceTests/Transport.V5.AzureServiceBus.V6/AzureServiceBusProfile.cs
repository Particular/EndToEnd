using NServiceBus;

class AzureServiceBusProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .ConnectionStringName("AzureServiceBus");
    }
}
