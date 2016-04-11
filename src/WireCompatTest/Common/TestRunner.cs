using System;
using System.Threading;
using NServiceBus;

public static class TestRunner
{
    public static string EndpointName { get; set; }

    public static void RunTests(IBus bus)
    {
        Thread.Sleep(TimeSpan.FromSeconds(25));
        bus.InitiateDataBus();
        bus.InitiatePubSub();
        bus.InitiateSaga();
        bus.InitiateSendReply();
        bus.InitiateSendReturn();

        Thread.Sleep(TimeSpan.FromMinutes(1));
        var disposable = bus as IDisposable;
        disposable?.Dispose();
        DataBusVerifier.AssertExpectations();
        PubSubVerifier.AssertExpectations();
        SagaVerifier.AssertExpectations();
        SendReplyVerifier.AssertExpectations();
        SendReturnVerifier.AssertExpectations();
    }
}