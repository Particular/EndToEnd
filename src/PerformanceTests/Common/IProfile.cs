#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

public interface IProfile
{
    void Configure(Configuration busConfiguration);
}