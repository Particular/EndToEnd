using System;
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
    public class TaskWhenAllTaskRun : IBatchHelper
    {
        public Task Batch(int count, Func<int, Task> action)
        {
            var sends = new Task[count];
            for (var i = 0; i < count; i++)
            {
                var i2 = i;
                sends[i] = Task.Run(() => action(i2));
            }
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
}
