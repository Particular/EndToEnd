using NServiceBus;
using NServiceBus.Persistence;

class NHibernateProfile : IProfile
{
    public void Configure(EndpointConfiguration cfg)
    {
        cfg
            .UsePersistence<NHibernatePersistence>()
            .ConnectionString(this.GetConnectionString("NHibernate"))
            //.EnableCachingForSubscriptionStorage(TimeSpan.FromSeconds(5))
            ;
    }
}
