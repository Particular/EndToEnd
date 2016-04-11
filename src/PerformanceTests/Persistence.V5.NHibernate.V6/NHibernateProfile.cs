using System;
using System.Configuration;
using NServiceBus;
using NServiceBus.Persistence;

class NHibernateProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        var value = ConfigurationManager.ConnectionStrings["NHibernate"];
        if (ConfigurationManager.ConnectionStrings == null) throw new InvalidOperationException("Connection string 'NHibernate' not configured.");

        busConfiguration
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(value.ConnectionString);
    }
}
