using NServiceBus;

class RabbitMQProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration
            .UseTransport<RabbitMQTransport>()
            .ConnectionStringName("RabbitMQ");
    }
}
