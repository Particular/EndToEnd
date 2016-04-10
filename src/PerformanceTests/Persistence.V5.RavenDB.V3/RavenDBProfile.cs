using NServiceBus;
using NServiceBus.Persistence;

class RavenDBProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UsePersistence<RavenDBPersistence>();
    }
}
