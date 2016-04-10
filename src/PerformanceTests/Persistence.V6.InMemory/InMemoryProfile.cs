using NServiceBus;

class InMemoryProfile : IProfile
{
    public void Configure(EndpointConfiguration busConfiguration)
    {
        busConfiguration.UsePersistence<InMemoryPersistence>();
    }
}
