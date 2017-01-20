using System.Threading.Tasks;
using CommonMessages;
using NServiceBus;

public static class SagaInitiator
{
    public static async Task InitiateSaga(this IEndpointInstance bus)
    {
        foreach (var endpoint in EndpointNames.All)
        {
            var message = new SagaInitiateRequestingMessage
            {
                TargetEndpoint = endpoint
            };
            await bus.SendLocal(message)
                .ConfigureAwait(false);
        }
    }
}