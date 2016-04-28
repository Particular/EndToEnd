﻿using NServiceBus;

class RabbitMQProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<RabbitMQTransport>()
            .ConnectionString(ConfigurationHelper.GetConnectionString("RabbitMQ"));
    }
}
