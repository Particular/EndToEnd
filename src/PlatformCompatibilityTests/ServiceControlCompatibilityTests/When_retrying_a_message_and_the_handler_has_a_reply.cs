namespace ServiceControlCompatibilityTests
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NUnit.Framework;

    [TestFixture]
    class When_retrying_a_message_and_the_handler_has_a_reply : SqlScTest
    {
        [TestCaseSource(nameof(AllTransports))]
        [Timeout(300000)]
        public Task When_successfully_retry_a_failed_message_with_reply_it_gets_routed_back_to_sender(Type transportDetailsDType)
        {
            var endpointFactory = StartUp("Retry-with-Reply", transportDetailsDType, mapping =>
            {
                mapping[SenderEndpointName] = ServerA;
                mapping[ResponderEndpointName] = ServerB;
            });
            return RunReplyTest(endpointFactory);
        }

        async Task RunReplyTest(IEndpointFactory endpointFactory)
        {
            var testContext = new TestContext();

            var sender = await endpointFactory.CreateEndpoint(new EndpointDetails(SenderEndpointName).With<TestResponseHandler>().With(testContext));
            var responder = await endpointFactory.CreateEndpoint(new EndpointDetails(ResponderEndpointName).With<TestRequestHandler>().With(testContext));

            testContext.ShouldFail = true;
            var failingMessageId = await sender.Send(responder, new TestRequest());
            var failedMessage = await ServiceControl.WaitForFailedMessage(failingMessageId);

            testContext.ShouldFail = false;
            await ServiceControl.RetryMessageId(failedMessage.Id);
            var handled = testContext.WaitForDone().Wait(60000);

            Assert.IsTrue(handled, "Did not process the retry successfully within the time limit");
        }

        const string SenderEndpointName = "Sender";
        const string ResponderEndpointName = "Responder";

        class TestRequest : IMessage
        {
        }

        class TestResponse : IMessage
        {
        }

        class TestRequestHandler : IHandleMessages<TestRequest>
        {
            TestContext testContext;

            public TestRequestHandler(TestContext testContext)
            {
                this.testContext = testContext;
            }

            public Task Handle(TestRequest message, IMessageHandlerContext context)
            {
                if (testContext.ShouldFail)
                {
                    throw new Exception("Failed to process request");
                }

                return context.Reply(new TestResponse());
            }
        }

        class TestResponseHandler : IHandleMessages<TestResponse>
        {
            TestContext testContext;

            public TestResponseHandler(TestContext testContext)
            {
                this.testContext = testContext;
            }

            public Task Handle(TestResponse message, IMessageHandlerContext context)
            {
                testContext.Done(true);
                return Task.FromResult(0);
            }
        }

        class TestContext : TestContextBase
        {
            public bool ShouldFail { get; set; }
        }
    }
}