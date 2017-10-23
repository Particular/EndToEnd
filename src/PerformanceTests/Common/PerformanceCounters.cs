#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

using NServiceBus;

class PerformanceCounters : IProfile
{
    public void Configure(Configuration cfg)
    {
#pragma warning disable 618
        cfg.EnableCriticalTimePerformanceCounter();
#pragma warning restore 618
    }
}