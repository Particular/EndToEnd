#if Version5
using System.Threading.Tasks;
using NServiceBus;

partial class GatedPublishRunner : IProfile
{
    async Task SendLocal(object msg)
    {
        EndpointInstance.SendLocal(msg);
    }

    async Task Publish(object msg)
    {
        EndpointInstance.Publish(msg);
    }

    public class Handler : IHandleMessages<Event>
    {
        public void Handle(Event message)
        {
            X.Signal();
        }
    }

    public void Configure(BusConfiguration cfg)
    {
        cfg.PurgeOnStartup(true);
    }
}
#endif
