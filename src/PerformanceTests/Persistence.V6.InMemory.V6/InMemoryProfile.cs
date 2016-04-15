using NServiceBus;

class InMemoryProfile : IProfile
{
    public void Configure(EndpointConfiguration cfg)
    {
        cfg.UsePersistence<InMemoryPersistence>();
    }
}
