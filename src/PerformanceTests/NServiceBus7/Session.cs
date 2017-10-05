using System.Threading.Tasks;
using NServiceBus;

class Session : ISession
{
    readonly IEndpointInstance instance;

    public Session(IEndpointInstance instance)
    {
        this.instance = instance;
    }

    public Task Send(object message)
    {
        return instance.Send(message);
    }

    public Task Publish(object message)
    {
        return instance.Publish(message);
    }

    public Task SendLocal(object message)
    {
        return instance.SendLocal(message);
    }

    public Task Close()
    {
        return instance.Stop();
    }
}
