using Common;
using NServiceBus;

class RavenDBProfile : IProfile
{
    public void Configure(EndpointConfiguration cfg)
    {
        cfg.UsePersistence<RavenDBPersistence>()
           .DoNotSetupDatabasePermissions()
           .SetConnectionString(this.GetConnectionString("RavenDB"));
    }
}