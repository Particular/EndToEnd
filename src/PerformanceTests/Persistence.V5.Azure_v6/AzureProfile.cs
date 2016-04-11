using NServiceBus;

public class AzureProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UsePersistence<AzureStoragePersistence>();
    }
}
