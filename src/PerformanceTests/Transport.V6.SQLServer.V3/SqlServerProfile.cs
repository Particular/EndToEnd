﻿using NServiceBus;

class SqlServerProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UseTransport<SqlServerTransport>();
    }
}
