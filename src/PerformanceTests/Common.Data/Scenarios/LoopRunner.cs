using System.Threading;
using System.Threading.Tasks;

public abstract class LoopRunner : BaseRunner
{
    Task loopTask;
    protected CancellationTokenSource stopLoop { get; private set; }
    protected bool Shutdown { get; private set; }

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

    protected abstract Task Loop(object o);
}
