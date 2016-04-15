#if Version6
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using System;
using System.Runtime;
using Tests.Permutations;
using Variables;

class CheckGCServer : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(Configuration cfg)
    {
        if (Permutation.GarbageCollector == GarbageCollector.Client && GCSettings.IsServerGC) throw new InvalidOperationException("GarbageCollector is set to Server but must be Client.");
        if (Permutation.GarbageCollector == GarbageCollector.Server && !GCSettings.IsServerGC) throw new InvalidOperationException("GarbageCollector is set to Client but must be Server.");
    }
}