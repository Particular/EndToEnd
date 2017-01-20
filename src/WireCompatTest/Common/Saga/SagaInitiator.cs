using System.Threading.Tasks;
using CommonMessages;
using NServiceBus;

public static class SagaInitiator
{
    public static void InitiateSaga(this IBus bus)
    {
        Parallel.ForEach(EndpointNames.All, endpoint =>
        {
            var message = new SagaInitiateRequestingMessage
            {
                TargetEndpoint = endpoint
            };
            bus.SendLocal(message);
        });
    }
}