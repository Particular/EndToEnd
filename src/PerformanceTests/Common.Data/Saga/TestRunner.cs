using System;
using Common.Saga;
using NServiceBus;
using Utils;

public static class TestRunner
{
    public static string EndpointName { get; set; }

#if Version6
    public static void RunTests(IEndpointInstance bus, BusCreationOptions options)
#else
    public static void RunTests(IBus bus, BusCreationOptions options)
#endif
    {
        Statistics.Initialize(options.NumberOfMessages);
        AppDomain.CurrentDomain.SetData("Statistics", Statistics.Instance);

        if (options.Saga)
        {
            bus.SeedSagaMessages(options);
        }
        else
        {
            bus.InitiateDataBus(options);
        }

        Statistics.WaitUntilCompleted();
        Statistics.Instance.Dump();
        var disposable = bus as IDisposable;
        disposable?.Dispose();
    }
}