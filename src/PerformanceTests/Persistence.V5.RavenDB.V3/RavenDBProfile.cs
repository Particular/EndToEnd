using NServiceBus;
using NServiceBus.Persistence;

class RavenDBProfile : IProfile
{
    public void Configure(BusConfiguration cfg)
    {
        cfg.UsePersistence<RavenDBPersistence>()
            .DoNotSetupDatabasePermissions()
            .SetConnectionString(this.GetConnectionString("RavenDB"));
    }
}
