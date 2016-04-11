using NServiceBus;

class NHibernateProfile : IProfile
{
    public void Configure(EndpointConfiguration endpointConfiguration)
    {
        endpointConfiguration.UsePersistence<NHibernatePersistence>();
    }
}
