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
            countdownEvent = new CountdownEvent(BatchSize);

            Log.Warn("Sleeping 3,000ms for the instance to purge the queue and process subscriptions. Loop requires the queue to be empty.");
            Thread.Sleep(3000);
            Log.Info("Starting");

            while (!Shutdown)
            {
                try
                {
                    Console.Write("1");
                    countdownEvent.Reset(BatchSize);

                    var d = Stopwatch.StartNew();

                    await TaskHelper.ParallelFor(BatchSize, SendMessage);

                    if (d.Elapsed < TimeSpan.FromSeconds(2.5))
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
        }
        catch (Exception ex)
        {
            Log.Error("Loop", ex);
        }
    }

    internal static void Signal()
    {
        countdownEvent.Signal();
    }

    internal class Handler<K> : IHandleMessages<K>
    {
#if Version5
        public void Handle(K message)
        {
            Signal();
        }
#else
        public Task Handle(K message, IMessageHandlerContext context)
        {
            Signal();
            return Task.FromResult(0);
        }
#endif
    }
}

