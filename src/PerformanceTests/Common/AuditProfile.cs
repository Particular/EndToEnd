#if Version6 || Version7
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using NServiceBus;
using NServiceBus.Features;
using Tests.Permutations;

class AuditProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(Configuration cfg)
    {
        if (Permutation.AuditMode == Variables.Audit.Off) cfg.DisableFeature<Audit>();

#if Version6 || Version7
            cfg.AuditProcessedMessagesTo(System.Configuration.ConfigurationManager.AppSettings["NServiceBus/AuditQueue"]);
#endif
    }
}
