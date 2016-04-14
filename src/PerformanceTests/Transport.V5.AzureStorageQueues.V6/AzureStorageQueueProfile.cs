using NServiceBus;
using Common;

class AzureStorageQueueProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .ConnectionString(this.GetConnectionString("AzureStorageQueue"));
    }
}
