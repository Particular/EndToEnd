using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

abstract class BaseLoop : BaseRunner
{
    static ILog Log = LogManager.GetLogger(nameof(BaseLoop));

    Task loopTask;
    int count;
    Stopwatch start;
    double duration;
    double avg;

    CancellationTokenSource stopLoop { get; set; }
    int BatchSize { get; set; } = 16;
    protected abstract Task SendMessage(ISession session);

    protected override async Task Start(ISession session)
    {
        stopLoop = new CancellationTokenSource();
        loopTask = await Task.Factory.StartNew(() => Loop(session), TaskCreationOptions.LongRunning);
    }

    protected override Task Stop()
    {
        using (stopLoop)
        {
            stopLoop.Cancel();
            using (loopTask)
            {
                loopTask.Wait();
            }
        }
        return Task.FromResult(0);
    }

    async Task Loop(ISession session)
    {
        try
        {
            Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
            await Task.Delay(3000).ConfigureAwait(false);
            Log.Info("Starting");
            start = Stopwatch.StartNew();




            while (!Shutdown)
            {
                try
                {
                    Console.Write("1");

                    var batchDuration = Stopwatch.StartNew();

                    await Batch(BatchSize, () => SendMessage(session)).ConfigureAwait(false);

                    count += BatchSize;

                    if (batchDuration.Elapsed < TimeSpan.FromSeconds(2.5))
                    {
                        BatchSize *= 2;
                        Log.InfoFormat("Batch size increased to {0,7:N0}", BatchSize);
                    }
                    Console.Write("2");

                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            Log.Info("Stopped");

            duration = start.Elapsed.TotalSeconds;
            avg = count / duration;
            var statsLog = LogManager.GetLogger("Statistics");

            statsLog.InfoFormat(Statistics.StatsFormatInt, "LoopLastBatchSize", BatchSize, "#");
            statsLog.InfoFormat(Statistics.StatsFormatInt, "LoopCount", count, "#");
            statsLog.InfoFormat(Statistics.StatsFormatDouble, "LoopDuration", duration, "s");
            statsLog.InfoFormat(Statistics.StatsFormatDouble, "LoopThroughputAvg", avg, "msg/s");

        }
        catch (Exception ex)
        {
            Log.Error("Loop", ex);
        }
    }

    protected abstract Task Batch(int count, Func<Task> action);

    internal class Handler<K> : IHandleMessages<K>
    {
        static readonly TimeSpan WarningInterval = TimeSpan.FromSeconds(10);
        static object l = new object();
        DateTime last;
#if Version5
        public void Handle(K message)
        {
            WarnAtReceive();
        }
#else
        public Task Handle(K message, IMessageHandlerContext context)
        {
            WarnAtReceive();
            return Task.FromResult(0);
        }
#endif

        void WarnAtReceive()
        {
            lock (l)
            {
                var now = DateTime.UtcNow;
                if (last >= now) return;
                last += WarningInterval;
                Log.Warn("Messages received, this should NOT happen during");
            }
        }
    }
}