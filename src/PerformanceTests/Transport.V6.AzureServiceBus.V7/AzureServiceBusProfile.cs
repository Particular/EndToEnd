using NServiceBus;

class AzureServiceBusProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .ConnectionStringName("AzureServiceBus");

        endpointConfiguration.PurgeOnStartup(false);
    }
}
