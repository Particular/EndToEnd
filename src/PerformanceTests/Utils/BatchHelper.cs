using System;
using System.Threading;
using System.Threading.Tasks;

public class BatchHelper
{
    public static IBatchHelper Instance;

    public interface IBatchHelper
    {
        Task Batch(int count, Func<int, Task> action);
    }

    public class TaskWhenAll : IBatchHelper
    {
        public Task Batch(int count, Func<int, Task> action)
        {
            var sends = new Task[count];
            for (var i = 0; i < count; i++) sends[i] = action(i);
            return Task.WhenAll(sends);
        }
    }

    public class ParallelFor : IBatchHelper
    {
        static ParallelOptions po = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        public Task Batch(int count, Func<int, Task> action)
        {
            return Task.Run(() => Parallel.For(0, count, po, i => action(i).GetAwaiter().GetResult()));
        }
    }

    public class TaskWhenAllThrottled : IBatchHelper
    {
        public async Task Batch(int count, Func<int, Task> action)
        {
            var throttler = new SemaphoreSlim(Environment.ProcessorCount);

            var sends = new Task[count];
            for (var i = 0; i < count; i++)
            {
                await throttler.WaitAsync();
                sends[i] = Invoke(i, throttler, action);
            }

            await Task.WhenAll(sends);
        }

        static async Task Invoke(int i, SemaphoreSlim throttler, Func<int, Task> action)
        {
            try
            {
                await action(i);
            }
            finally
            {
                throttler.Release();
            }
        }
    }
}
