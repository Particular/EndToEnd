﻿using System.Net;
using NServiceBus;

class AzureStorageQueueProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .ConnectionString(this.GetConnectionString("AzureStorageQueue"));
    }
}
