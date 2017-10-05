#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif

using NServiceBus;
using Tests.Permutations;
using Variables;

class OutboxProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { get; set; }

    public void Configure(Configuration cfg)
    {
        if (Permutation.OutboxMode == Outbox.On) cfg.EnableOutbox();
    }

}