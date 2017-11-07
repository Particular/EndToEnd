using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NServiceBus.Logging;

public abstract class PerpetualRunner : BaseRunner
{
    readonly ILog Log = LogManager.GetLogger(nameof(PerpetualRunner));

    protected override async Task Start(ISession session)
    {
        const int duration = 5000;
        await base.Start(session);
        var seedSize = MaxConcurrencyLevel * Permutation.PrefetchMultiplier * 2;

        Log.InfoFormat("Trying to seed {0:N0} items within {1:N0}ms...", seedSize, duration);

        Log.InfoFormat("BatchHelper type: {0}", BatchHelper.Instance.GetType());

        var start = Stopwatch.StartNew();
        var chunkSize = 16;
        var count = 0;
        do
        {
            if (start.ElapsedMilliseconds > duration)
            {
                Log.WarnFormat("Seed window ({0:N0}ms) expired!", duration);
                break;
            }
            var remaining = seedSize - count;
            var j = chunkSize > remaining ? remaining : chunkSize;

            Log.InfoFormat("\tSeeding {0:N0} items...", j);

            await BatchHelper.Instance.Batch(j, i => Seed(i + count /*no issue, as we await*/, session)).ConfigureAwait(false);
            chunkSize *= 2;
            count += j;
        } while (count < seedSize);

        Log.InfoFormat("Seeded {0:N0} of {1:N0} ({2:N1}%)", count, seedSize, count * 100.0 / seedSize);
    }

    protected abstract Task Seed(int i, ISession session);

    protected override async Task Wait(Task baseTask)
    {
        Log.InfoFormat("Warmup: {0}, until {1}", Settings.WarmupDuration, DateTime.Now + Settings.WarmupDuration);
        await Task.Delay(Settings.WarmupDuration).ConfigureAwait(false);
        await baseTask.ConfigureAwait(false);
    }
}
