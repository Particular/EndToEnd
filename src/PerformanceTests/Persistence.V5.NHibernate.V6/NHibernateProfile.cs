using System;
using System.Configuration;
using NServiceBus;
using NServiceBus.Persistence;

class NHibernateProfile : IProfile
{
    public void Configure(BusConfiguration cfg)
    {
        var value = ConfigurationManager.ConnectionStrings["NHibernate"];
        if (value == null) throw new InvalidOperationException("Connection string 'NHibernate' not configured.");

        cfg
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(value.ConnectionString)
            //.EnableCachingForSubscriptionStorage(TimeSpan.FromSeconds(5))
            ;
    }
}
