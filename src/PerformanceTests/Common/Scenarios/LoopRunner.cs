using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

abstract class LoopRunner : BaseRunner
{
    protected ILog Log = LogManager.GetLogger(nameof(LoopRunner));

    Task loopTask;

    static AsyncCountdownEvent countdownEvent;
    static long count;
    static long latencySum;

    CancellationToken stopLoopCancellationToken;
    CancellationTokenSource stopLoopCancellationTokenSource;
    int BatchSize = 16;

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
            countdownEvent = new AsyncCountdownEvent(BatchSize);

            while (!stopLoopCancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write("1");
                    countdownEvent.Reset(BatchSize);
                    var batchDuration = Stopwatch.StartNew();

                    await BatchHelper.Batch(BatchSize, i => SendMessage(session)).ConfigureAwait(false);

                    count += BatchSize;

                    if (batchDuration.Elapsed < TimeSpan.FromSeconds(2.5))
                    {
                        BatchSize *= 2;
                        Log.InfoFormat("Batch size increased to {0}", BatchSize);
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

            var duration = start.Elapsed.TotalSeconds;
            var avg = count / duration;
            var statsLog = LogManager.GetLogger("Statistics");
            var avgLatency = latencySum / TimeSpan.TicksPerMillisecond / count;
            statsLog.InfoFormat("{0}: {1:0.0} ({2})", "LoopLastBatchSize", BatchSize, "#");
            statsLog.InfoFormat("{0}: {1:0.0} ({2})", "LoopCount", count, "#");
            statsLog.InfoFormat("{0}: {1:0.0} ({2})", "LoopDuration", duration, "s");
            statsLog.InfoFormat("{0}: {1:0.0} ({2})", "LoopThroughputAvg", avg, "msg/s");
            statsLog.InfoFormat("{0}: {1:0.0} ({2})", "LoopLatency", avgLatency, "ms");
        }
        catch (Exception ex)
        {
            Log.Error("Loop", ex);
            throw;
        }
    }

    static void Signal()
    {
        Interlocked.Increment(ref count);
        countdownEvent.Signal();
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
