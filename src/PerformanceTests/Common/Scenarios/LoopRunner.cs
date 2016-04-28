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

    static CountdownEvent countdownEvent { get; set; }
    static long count;
    static long latencySum;

    CancellationTokenSource stopLoop { get; set; }
    bool Shutdown { get; set; }
    int BatchSize { get; set; } = 16;
    protected abstract Task SendMessage();

    protected override void Start()
    {
        stopLoop = new CancellationTokenSource();
        loopTask = Task.Factory.StartNew(Loop, TaskCreationOptions.LongRunning);
    }

    protected override void Stop()
    {
        Shutdown = true;
        using (stopLoop)
        {
            stopLoop.Cancel();
            using (loopTask)
            {
                loopTask.Wait();
            }
        }
    }

    async Task Loop(object o)
    {
        try
        {
            Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
            Thread.Sleep(3000);
            Log.Info("Starting");
            var start = Stopwatch.StartNew();
            countdownEvent = new CountdownEvent(BatchSize);

            while (!Shutdown)
            {
                try
                {
                    Console.Write("1");
                    countdownEvent.Reset(BatchSize);
                    var batchDuration = Stopwatch.StartNew();

                    await TaskHelper.ParallelFor(BatchSize, SendMessage);

                    count += BatchSize;

                    if (batchDuration.Elapsed < TimeSpan.FromSeconds(2.5))
                    {
                        BatchSize *= 2;
                        Log.InfoFormat("Batch size increased to {0}", BatchSize);
                    }
                    Console.Write("2");
                    countdownEvent.Wait(stopLoop.Token);
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
