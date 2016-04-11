using System;
using System.Configuration;
using NServiceBus;
using NServiceBus.Persistence;

class NHibernateProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        var value = ConfigurationManager.ConnectionStrings["NHibernate"];
        if (ConfigurationManager.ConnectionStrings==null) throw new InvalidOperationException("Connection string 'NHibernate' not configured.");

        endpointConfiguration
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(value.ConnectionString);
    }
}
