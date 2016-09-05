using System;
using System.Threading;
using System.Threading.Tasks;

/// <remarks>
/// Based on https://github.com/StephenCleary/AsyncEx
/// </remarks>
public sealed class AsyncCountdownEvent
{
    public AsyncCountdownEvent(int count)
    {
        taskCompletionSource = new TaskCompletionSource<object>();
        this.count = count;
        InitialCount = count;
    }

    public int CurrentCount => Interlocked.CompareExchange(ref count, 0, 0);
    public int InitialCount { get; private set; }

    public Task WaitAsync()
    {
        return WaitAsync(CancellationToken.None);
    }

    public async Task WaitAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        using (cancellationToken.Register(() => taskCompletionSource.TrySetCanceled()))
        {
            await taskCompletionSource.Task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Attempts to subtract the specified value from the current count. This method throws
    /// <see cref="InvalidOperationException" /> if the count is already at zero or if the new count would be less than zero.
    /// </summary>
    /// <param name="signalCount">The amount to change the current count. This must be greater than zero.</param>
    public void Signal(int signalCount)
    {
        if (!ModifyCount(-signalCount))
        {
            throw new InvalidOperationException("Cannot decrement count.");
        }
    }

    /// <summary>
    /// Attempts to subtract one from the current count. This method throws <see cref="InvalidOperationException" /> if the
    /// count is already at zero or if the new count would be less than zero.
    /// </summary>
    public void Signal()
    {
        Signal(1);
    }

    bool ModifyCount(int signalCount)
    {
        var sw = new SpinWait();

        do
        {
            var oldCount = CurrentCount;
            if (oldCount == 0)
                return false;
            var newCount = oldCount + signalCount;
            if (newCount < 0)
                return false;
            if (Interlocked.CompareExchange(ref count, newCount, oldCount) == oldCount)
            {
                if (newCount == 0)
                    taskCompletionSource.SetResult(null);
                return true;
            }

            sw.SpinOnce();
        } while (true);
    }

    public void Reset(int initialCount)
    {
        var sw = new SpinWait();

        do
        {
            var tcs = taskCompletionSource;
            if ((!tcs.Task.IsCompleted || Interlocked.CompareExchange(ref taskCompletionSource, new TaskCompletionSource<object>(), tcs) == tcs) && Interlocked.Exchange(ref count, initialCount) == initialCount)
            {
                InitialCount = initialCount;
                return;
            }

            sw.SpinOnce();
        } while (true);
    }

    public void Reset()
    {
        Reset(InitialCount);
    }

    TaskCompletionSource<object> taskCompletionSource;
    int count;
}
