using NServiceBus;
using Common;
using NServiceBus.AzureServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Routing;

class AzureServiceBusProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<AzureServiceBusTransport>()
            .UseTopology<EndpointOrientedTopology>()
                .RegisterPublisherForType(endpointConfiguration.GetSettings().Get<EndpointName>().ToString(), typeof(GatedPublishRunner.Event))
            .ConnectionString(this.GetConnectionString("AzureServiceBus"));

        endpointConfiguration.PurgeOnStartup(false);
    }
}
