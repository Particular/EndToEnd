using System.Threading.Tasks;
using NServiceBus;

class Session : ISession
{
    readonly IBus instance;
    readonly Address localAddress;

    public Session(IBus instance)
    {
        this.instance = instance;
    }

    public Session(ISendOnlyBus instance)
    {
        this.instance = (IBus)instance;
        var unicastBus = (NServiceBus.Unicast.UnicastBus)instance;
        var machine = unicastBus.Configure.LocalAddress.Machine;
        var queue = unicastBus.Configure.LocalAddress.Queue;

        localAddress = new Address(queue, machine);
    }

    public Task Send(object message)
    {
        instance.Send(message);
        return Task.FromResult(0);
    }

    public Task Publish(object message)
    {
        instance.Publish(message);
        return Task.FromResult(0);
    }

    public Task SendLocal(object message)
    {
        if (localAddress == null)
            instance.SendLocal(message);
        else
            instance.Send(localAddress, message);

        return Task.FromResult(0);
    }

    public Task Close()
    {
        instance.Dispose();
        return Task.FromResult(0);
    }
}
