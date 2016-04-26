using System;
using System.Threading.Tasks;

class ParallelForRunner : SendLoop
{
    ParallelOptions options = new ParallelOptions();

    protected override Task Batch(int count, Func<Task> action)
    {
        Parallel.For(0, count, options, i =>
        {
            action().GetAwaiter().GetResult();
        });
        return Task.CompletedTask;
    }
}