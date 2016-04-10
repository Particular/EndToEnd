namespace Common.Saga
{
    using System;
    using NServiceBus;
    using Utils;

    public static class SagaInitiator
    {
#if Version6
        public static void SeedSagaMessages(this IEndpointInstance bus, BusCreationOptions options)
#else
        public static void SeedSagaMessages(this IBus bus, BusCreationOptions options)
#endif
        {
            var endpointName = "PerformanceTests_" + AppDomain.CurrentDomain.FriendlyName.Replace(' ', '_');

            for (var i = 1; i <= options.NumberOfMessages / options.Concurrency; i++)
            {
                for (var j = 0; j < options.Concurrency; j++)
                {
#if Version6
                    bus.Send(endpointName, new StartSagaMessage { Id = i }).GetAwaiter().GetResult();
#else
                    bus.Send(endpointName, new StartSagaMessage { Id = i });
#endif
                }
            }
        }
    }
}
