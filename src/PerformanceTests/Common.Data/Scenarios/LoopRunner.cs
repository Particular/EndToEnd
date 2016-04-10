using System.Threading;
using System.Threading.Tasks;

abstract class LoopRunner : IStartAndStop
{
    Task loopTask;
    protected CancellationTokenSource stopLoop { get; private set; }
    protected bool Shutdown { get; private set; }

    public void Start()
    {
        stopLoop = new CancellationTokenSource();
        loopTask = Task.Factory.StartNew(Loop, TaskCreationOptions.LongRunning);
    }

    public void Stop()
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

    protected abstract void Loop(object o);
}
