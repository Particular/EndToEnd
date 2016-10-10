#if Version6
using Cfg = NServiceBus.EndpointConfiguration;
using NServiceBus;
#else
using Cfg = NServiceBus.BusConfiguration;
#endif

using System;
using NServiceBus.Logging;
using Tests.Permutations;
using Variables;

class TimeToBeReceivedProfile : IProfile, INeedPermutation
{
    readonly ILog Log = LogManager.GetLogger(nameof(TimeToBeReceivedProfile));
    readonly TimeSpan TTBR = Settings.RunDuration;
    public Permutation Permutation { private get; set; }

    public void Configure(Cfg cfg)
    {
        if (Permutation.Transport == Transport.MSMQ)
        {
            Log.WarnFormat("TimeToBeReceived NOT set to '{0}' for transactional MSMQ!", TTBR);
            return;
        }

        Log.InfoFormat("TimeToBeReceived set to '{0}'.", TTBR);
        cfg.Conventions().DefiningTimeToBeReceivedAs(type => TTBR);
    }
}
