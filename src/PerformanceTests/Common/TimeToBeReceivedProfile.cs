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
    readonly TimeSpan TTBR = Settings.RunDuration + TimeSpan.FromMinutes(5); // Max clock offset allowed is 5 minutes
    public Permutation Permutation { private get; set; }

    public void Configure(Cfg cfg)
    {
        if (Permutation.Transport == Transport.MSMQ && Permutation.TransactionMode != TransactionMode.None)
        {
            Log.WarnFormat("TimeToBeReceived NOT set to '{0}' for transactional MSMQ as TransactionMode is not None!", TTBR);
            return;
        }

        Log.InfoFormat("TimeToBeReceived set to '{0}'.", TTBR);
        cfg.Conventions().DefiningTimeToBeReceivedAs(type => TTBR);
    }
}
