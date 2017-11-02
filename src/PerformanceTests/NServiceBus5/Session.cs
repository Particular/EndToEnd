using System.Threading.Tasks;
using NServiceBus;

class Session : ISession
{
    readonly IBus instance;

    public Session(IBus instance)
    {
        this.instance = instance;
    }

    public Session(ISendOnlyBus instance)
    {
        this.instance = (IBus)instance;
    }

   public Task Send(object message)
    {
        instance.Send(message);
        return Task.FromResult(0);
    }

    public Task Send(string destination, object message)
    {
        instance.Send(destination, message);
        return Task.FromResult(0);
    }

    public Task Publish(object message)
    {
        instance.Publish(message);
        return Task.FromResult(0);
    }

    public Task Close()
    {
        instance.Dispose();
        return Task.FromResult(0);
    }
}
