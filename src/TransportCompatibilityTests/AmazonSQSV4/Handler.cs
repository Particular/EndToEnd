namespace AmazonSQSV4
{
    using System.Threading.Tasks;
    using NServiceBus;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.Messages;

    public class Handler : IHandleMessages<TestCommand>, IHandleMessages<TestEvent>, IHandleMessages<TestRequest>, IHandleMessages<TestResponse>, IHandleMessages<TestIntCallback>, IHandleMessages<TestEnumCallback>
    {
        public MessageStore Store { get; set; }

        public Task Handle(TestCommand message, IMessageHandlerContext context)
        {
            Store.Add<TestCommand>(message.Id);
            return Task.FromResult(0);
        }

        public Task Handle(TestEvent message, IMessageHandlerContext context)
        {
            Store.Add<TestEvent>(message.EventId);
            return Task.FromResult(0);
        }

        public Task Handle(TestRequest message, IMessageHandlerContext context)
        {
            return context.Reply(new TestResponse { ResponseId = message.RequestId });
        }

        public Task Handle(TestResponse message, IMessageHandlerContext context)
        {
            Store.Add<TestResponse>(message.ResponseId);
            return Task.FromResult(0);
        }

        public Task Handle(TestIntCallback message, IMessageHandlerContext context)
        {
            return context.Reply(message.Response);
        }

        public Task Handle(TestEnumCallback message, IMessageHandlerContext context)
        {
            return context.Reply(message.CallbackEnum);
        }
    }
}
