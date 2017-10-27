#if Version6 || Version7
using System;
using NServiceBus;
using Configuration = NServiceBus.EndpointConfiguration;
#else
using Configuration = NServiceBus.BusConfiguration;
#endif
using Tests.Permutations;

class ErrorProfile : IProfile, INeedPermutation
{
    public Permutation Permutation { private get; set; }

    public void Configure(Configuration cfg)
    {
#if Version6 || Version7
        cfg.SendFailedMessagesTo(System.Configuration.ConfigurationManager.AppSettings["NServiceBus/ErrorQueue"]);
        cfg.Recoverability()
            .CustomPolicy(ExponentialBackoffPolicy.Process)
            .Immediate(c => c.NumberOfRetries(1))
            .Delayed(c => c.TimeIncrease(TimeSpan.FromMilliseconds(3000)).NumberOfRetries(100));
#endif
    }
}

#if Version6 || Version7
class ExponentialBackoffPolicy
{
    static Random random = new Random();

    public static RecoverabilityAction Process(RecoverabilityConfig config, NServiceBus.Transport.ErrorContext context)
    {
        var action = DefaultRecoverabilityPolicy.Invoke(config, context);
        if (!(action is DelayedRetry)) return action;
        if (context.DelayedDeliveriesPerformed > config.Delayed.MaxNumberOfRetries) return action;
        double jitter;
        lock (random) jitter = random.NextDouble();
        jitter = 1 + jitter * 0.2;// max 20%
        var delayInSeconds = config.Delayed.TimeIncrease.TotalSeconds;
        var retries = context.DelayedDeliveriesPerformed;
        if (retries > 0) delayInSeconds *= Math.Pow(2, retries);
        delayInSeconds *= jitter;
        return RecoverabilityAction.DelayedRetry(TimeSpan.FromSeconds(delayInSeconds));
    }
}
#endif