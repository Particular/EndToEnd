using NServiceBus;

class InMemoryProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UsePersistence<InMemoryPersistence>();
    }
}
