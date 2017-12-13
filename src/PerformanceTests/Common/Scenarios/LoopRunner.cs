using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

abstract class LoopRunner : BaseRunner
{
    protected static ILog Log = LogManager.GetLogger(nameof(LoopRunner));

    Task loopTask;

    static readonly AsyncCountdownEvent countdownEvent = new AsyncCountdownEvent(0);
    static long count;
    static long latencySum;

    CancellationToken stopLoopCancellationToken;
    CancellationTokenSource stopLoopCancellationTokenSource;
    int batchSize = 16;

    protected abstract Task SendMessage(ISession session);

    protected override Task Start(ISession session)
    {
        stopLoopCancellationTokenSource = new CancellationTokenSource();
        stopLoopCancellationToken = stopLoopCancellationTokenSource.Token;
        loopTask = Task.Run(() => Loop(session), CancellationToken.None);
        return Task.FromResult(0);
    }

    protected override Task Stop()
    {
        using (stopLoopCancellationTokenSource)
        {
            stopLoopCancellationTokenSource.Cancel();
            return loopTask;
        }
    }

    async Task Loop(ISession session)
    {
        try
        {
            Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
            await Task.Delay(3000, CancellationToken.None).ConfigureAwait(false);

            Log.Info("Starting");
            var start = Stopwatch.StartNew();
            countdownEvent.Reset(batchSize);

            const int MinimumBatchSeedDuration = 2500;

            Log.InfoFormat("BatchHelper type: {0}", BatchHelper.Instance.GetType());

            while (!stopLoopCancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write("1");
                    countdownEvent.Reset(batchSize);
                    var batchDuration = Stopwatch.StartNew();

                    await BatchHelper.Instance.Batch(batchSize, i => SendMessage(session)).ConfigureAwait(false);

                    count += batchSize;

                    var duration = batchDuration.ElapsedMilliseconds;
                    Log.InfoFormat("Batch completed with size {0,7:N0} duration {1,7:N0}ms", batchSize, duration);
                    if (duration < MinimumBatchSeedDuration)
                    {
                        batchSize *= 2;
                        Log.InfoFormat("Increasing batch size to {0,7:N0} as sending took {1,7:N0}ms which is less then {2:N0}ms", batchSize, duration, MinimumBatchSeedDuration);
                    }
                    Console.Write("2");
                    await countdownEvent.WaitAsync(stopLoopCancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            Log.Info("Stopped");

            var elapsed = start.Elapsed.TotalSeconds;
            var avg = count / elapsed;
            var statsLog = LogManager.GetLogger("Statistics");
            var avgLatency = latencySum / TimeSpan.TicksPerMillisecond / count;
            statsLog.InfoFormat(Statistics.StatsFormatInt, "LoopLastBatchSize", batchSize, "#");
            statsLog.InfoFormat(Statistics.StatsFormatInt, "LoopCount", count, "#");
            statsLog.InfoFormat(Statistics.StatsFormatDouble, "LoopDuration", elapsed, "s");
            statsLog.InfoFormat(Statistics.StatsFormatDouble, "LoopThroughputAvg", avg, "msg/s");
            statsLog.InfoFormat(Statistics.StatsFormatDouble, "LoopLatency", avgLatency, "ms");
        }
        catch (Exception ex)
        {
            Log.Error("Loop", ex);
            throw;
        }
    }

    static void Signal()
    {
        if (countdownEvent == null)
        {
            Log.Warn("Count down event not initialized yet, probably receiving message from previous session.");
            return;
        }

        try
        {
            countdownEvent.Signal();
        }
        catch (InvalidOperationException)
        {
            Log.Warn("Receiving more messages than originally send, probably receiving message from previous session.");
        }
    }

    static void AddLatency(TimeSpan latency)
    {
        Interlocked.Add(ref latencySum, latency.Ticks);
    }


    internal class Handler<K> : IHandleMessages<K>
    {
#if Version5
        public IBus Bus { get; set; }
        public void Handle(K message)
        {
            var now = DateTime.UtcNow;
            var at = DateTimeExtensions.ToUtcDateTime(Bus.GetMessageHeader(message, Headers.TimeSent));
            AddLatency(now - at);
            Signal();
        }
#else
        public Task Handle(K message, IMessageHandlerContext context)
        {
            var now = DateTime.UtcNow;
            var at = DateTimeExtensions.ToUtcDateTime(context.MessageHeaders[Headers.TimeSent]);
            AddLatency(now - at);
            Signal();
            return Task.FromResult(0);
        }
#endif
    }

}
