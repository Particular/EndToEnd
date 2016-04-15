using NServiceBus;

class RabbitMQProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<RabbitMQTransport>()
            //.DisableCallbackReceiver()
            .ConnectionString(this.GetConnectionString("RabbitMQ"));
    }
}
