using NServiceBus;

class AzureStorageQueueProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration
            .UseTransport<AzureStorageQueueTransport>()
            .ConnectionStringName("AzureStorageQueue");
    }
}
