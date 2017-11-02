using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;

/// <summary>
/// Seeds a large set of messages to the transport and after seeding completes,
/// processes all the message with a NOOP handler.
/// </summary>    
partial class ReceiveRunner : BaseRunner, ICreateSeedData
{
    static readonly ILog Log = LogManager.GetLogger(nameof(ReceiveRunner));
    static readonly CountdownEvent countdownEvent = new CountdownEvent(0);
    int seedCount;

    public class Command : ICommand
    {
        public byte[] Data { get; set; }
    }

    partial class Handler
    {
    }

    public Task SendMessage(ISession session)
    {
        Interlocked.Increment(ref seedCount);
        return session.Send(EndpointName, new Command { Data = Data });
    }

    static void Signal()
    {
        try
        {
            countdownEvent.Signal();
        }
        catch (Exception ex)
        {
            Log.Debug("Ignoring", ex);
        }
    }

    protected override Task Wait(Task baseTask)
    {
        return Task.WhenAny(baseTask, WaitForCountDown());
    }

    async Task WaitForCountDown()
    {
        var start = DateTime.UtcNow;
        await Task.Yield();
        countdownEvent.Wait();
        Log.InfoFormat("All messages received in {0:N}s!", (DateTime.UtcNow - start).TotalSeconds);
    }

    protected override Task Setup()
    {
        countdownEvent.Reset(seedCount);
        return Task.FromResult(0);
    }
}

