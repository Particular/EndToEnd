using NServiceBus;

class MsmqProfile : IProfile
{
    public void Configure(BusConfiguration busConfiguration)
    {
        busConfiguration.UseTransport<MsmqTransport>()
            .ConnectionString("deadLetter=false;journal=false");
    }
}
