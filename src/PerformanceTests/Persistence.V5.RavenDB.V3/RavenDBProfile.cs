using Common;
using NServiceBus;
using NServiceBus.Logging;
using NServiceBus.Persistence;

class RavenDBProfile : IProfile
{
    readonly ILog Log = LogManager.GetLogger(typeof(RavenDBProfile));
    public void Configure(BusConfiguration cfg)
    {
        cfg.UsePersistence<RavenDBPersistence>()
            .DoNotSetupDatabasePermissions()
            .SetConnectionString(this.GetConnectionString("RavenDB"));
    }
}
