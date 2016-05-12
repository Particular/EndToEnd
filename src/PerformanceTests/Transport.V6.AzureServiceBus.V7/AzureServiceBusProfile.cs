using NServiceBus;
using NServiceBus.AzureServiceBus;
using NServiceBus.AzureServiceBus.Addressing;

class AzureServiceBusProfile : IProfile
{
    readonly string connectionstring = ConfigurationHelper.GetConnectionString("AzureServiceBus");

    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .UseTopology<ForwardingTopology>()
            .ConnectionString(connectionstring)
            .Sanitization().UseStrategy<EndpointOrientedTopologySanitization>();
    }
}
