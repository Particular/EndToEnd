using System;
#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using Tests.Permutations;
using Variables;

class CheckPlatform : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(Configuration cfg)
    {
        if (Permutation.Platform == Platform.x64 && !Environment.Is64BitProcess) throw new InvalidOperationException("Environment is x86 but must be x64.");
        if (Permutation.Platform == Platform.x86 && Environment.Is64BitProcess) throw new InvalidOperationException("Environment is x64 but must be x86.");
    }
}