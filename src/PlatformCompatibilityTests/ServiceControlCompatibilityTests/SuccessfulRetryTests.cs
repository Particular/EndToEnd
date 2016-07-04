namespace ServiceControlCompatibilityTests
{
    using NServiceBus;
    using NUnit.Framework;
    using System;
    using System.Threading.Tasks;

    [TestFixture]
    class SuccessfulRetryTests : SqlScTest
    {
        [TestCaseSource(nameof(AllTransports))]
        [Timeout(300000)]
        public async Task Can_successfully_retry_a_failed_message(Type transportDetailType)
        {
            var endpointFactory = await StartUp("Retry", transportDetailType, map =>
            {
                map[SenderEndpointName] = ServerA;
                map[ProcessorEndpointName] = ServerB;
            });
            await RunTest(endpointFactory);
        }

        async Task RunTest(IEndpointFactory endpointFactory)
        {
            var testContext = new TestContext();

            var sender = await endpointFactory.CreateEndpoint(SenderEndpointName);
            var processor = await endpointFactory.CreateEndpoint(new EndpointDetails(ProcessorEndpointName).With<TestMessageHandler>().With(testContext));

            testContext.ShouldFail = true;
            var failingMessageId = await sender.Send(processor, new TestMessage());
            var failedMessage = await ServiceControl.WaitForFailedMessage(failingMessageId);

            testContext.ShouldFail = false;
            await ServiceControl.RetryMessageId(failedMessage.Id);
            var handled = await testContext.WaitForDone();

            Assert.IsTrue(handled, "Did not process the retry successfully within the time limit");
        }

        const string SenderEndpointName = "Sender";
        const string ProcessorEndpointName = "Processor";
    }

    class TestMessage : ICommand
    {
    }

    class TestMessageHandler : IHandleMessages<TestMessage>
    {
        readonly TestContext testContext;

        public TestMessageHandler(TestContext testContext)
        {
            this.testContext = testContext;
        }

        public Task Handle(TestMessage message, IMessageHandlerContext context)
        {
            if (testContext.ShouldFail)
            {
                throw new ApplicationException("Failing...");
            }

            testContext.Done(true);
            return Task.FromResult(0);
        }
    }

    class TestContext : TestContextBase
    {
        public bool ShouldFail { get; set; }
    }
}