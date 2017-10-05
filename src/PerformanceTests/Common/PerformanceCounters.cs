#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

using NServiceBus;

class PerformanceCounters : IProfile
{
    public void Configure(Configuration cfg)
    {
#if Version7
        cfg.EnableWindowsPerformanceCounters();
#else
#pragma warning disable 618
        cfg.EnableCriticalTimePerformanceCounter();
#pragma warning restore 618
#endif
    }
}