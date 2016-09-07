#if Version5
using NServiceBus;

partial class PublishOneOnOneRunner
{
    public partial class Handler : IHandleMessages<Event>
    {
        public IBus Bus { get; set; }

        public void Handle(Event message)
        {
            if (!Shutdown) Bus.Publish(message);
            System.Threading.Interlocked.Increment(ref Count);
        }
    }
}

#endif