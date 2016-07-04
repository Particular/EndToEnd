namespace ServiceControlCompatibilityTests
{
    using System.Threading.Tasks;
    using Infrastructure;
    using NServiceBus;
    using NServiceBus.Features;

    interface IEndpointFactory
    {
        Task<EndpointProxy> CreateEndpoint(IEndpointDetails endpointDetails);
    }

    class EndpointFactory : IEndpointFactory
    {
        ITransportDetails transportDetails;

        public EndpointFactory(ITransportDetails transportDetails)
        {
            this.transportDetails = transportDetails;
        }

        public async Task<EndpointProxy> CreateEndpoint(IEndpointDetails endpointDetails)
        {
            var config = new EndpointConfiguration(endpointDetails.Name);
            config.UsePersistence<InMemoryPersistence>();
            config.EnableInstallers();
            config.DisableFeature<SecondLevelRetries>();
            config.DisableFeature<FirstLevelRetries>();

            var container = endpointDetails.CreateContainer();
            config.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));

            config.SendFailedMessagesTo("error");
            config.AuditProcessedMessagesTo("audit");

            transportDetails.ConfigureEndpoint(endpointDetails.Name, config);

            return new EndpointProxy(endpointDetails.Name, await Endpoint.Create(config));
        }
    }

    static class EndpointFactoryExtensions
    {
        public static Task<EndpointProxy> CreateEndpoint(this IEndpointFactory factory, string endpointName)
        {
            return factory.CreateEndpoint(new EndpointDetails(endpointName));
        }
    }
}