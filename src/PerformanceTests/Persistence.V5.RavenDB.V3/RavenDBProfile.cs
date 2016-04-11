using System;
using System.Configuration;
using NServiceBus;
using NServiceBus.Persistence;
using NServiceBus.RavenDB;

class RavenDBProfile : IProfile
{
    public void Configure(BusConfiguration cfg)
    {
        var value = ConfigurationManager.ConnectionStrings["RavenDB"];

        if (value == null) throw new InvalidOperationException("Connection string 'RavenDB' not configured.");

        var connectionParameters = new ConnectionParameters
        {
            Url = value.ConnectionString
        };

        cfg.UsePersistence<RavenDBPersistence>()
           .SetDefaultDocumentStore(connectionParameters);
    }
}
