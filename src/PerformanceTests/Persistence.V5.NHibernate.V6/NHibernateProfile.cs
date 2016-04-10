using NServiceBus;

class NHibernateProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UsePersistence<NHibernatePersistence>();
    }
}
