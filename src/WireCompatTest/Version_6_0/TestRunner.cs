using System;
using System.Threading.Tasks;
using NServiceBus;

public static class TestRunner
{
    public static string EndpointName { get; set; }

    public static async Task RunTests(IEndpointInstance bus)
    {
        await Task.Delay(TimeSpan.FromSeconds(25));
        await bus.InitiateDataBus();
        await bus.InitiatePubSub();
        await bus.InitiateSaga();
        await bus.InitiateSendReply();
        bus.InitiateSendReturn();

        await Task.Delay(TimeSpan.FromMinutes(1));
        await bus.Stop();
        DataBusVerifier.AssertExpectations();
        PubSubVerifier.AssertExpectations();
        SagaVerifier.AssertExpectations();
        SendReplyVerifier.AssertExpectations();
        SendReturnVerifier.AssertExpectations();
    }
}