using NServiceBus;
using NServiceBus.Persistence;

class RavenDBProfile : IProfile
{
    public void Configure(EndpointConfiguration cfg)
    {
        cfg.UsePersistence<RavenDBPersistence>()
           .SetConnectionStringName("RavenDB");
    }
}