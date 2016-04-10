using NServiceBus;

class RavenDBProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UsePersistence<RavenDBPersistence>();
    }
}
