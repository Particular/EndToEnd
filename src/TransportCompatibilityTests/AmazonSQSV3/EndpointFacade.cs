namespace AmazonSQSV3
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using TransportCompatibilityTests.Common;
    using TransportCompatibilityTests.Common.AmazonSQS;
    using TransportCompatibilityTests.Common.Messages;

    public class EndpointFacade : IEndpointFacade
    {
        MessageStore messageStore;
        CallbackResultStore callbackResultStore;
        IEndpointInstance endpointInstance;
        SubscriptionStore subscriptionStore;

        public void Dispose()
        {
            endpointInstance.Stop().GetAwaiter().GetResult();
        }

        public void Bootstrap(EndpointDefinition endpointDefinition)
        {
            InitializeEndpoint(endpointDefinition.As<AmazonSQSEndpointDefinition>())
                .GetAwaiter()
                .GetResult();
        }

        async Task InitializeEndpoint(AmazonSQSEndpointDefinition endpointDefinition)
        {
            var endpointConfiguration = new EndpointConfiguration(endpointDefinition.Name);

            endpointConfiguration.Conventions()
                .DefiningMessagesAs(
                    t => t.Namespace != null && t.Namespace.EndsWith(".Messages") && t != typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningEventsAs(t => t == typeof(TestEvent));
            endpointConfiguration.Conventions().DefiningCommandsAs(t => t.FullName.EndsWith("Command"));

            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.EnableInstallers();

            var transportExtensions = endpointConfiguration.UseTransport<SqsTransport>();

            endpointConfiguration.SendFailedMessagesTo("error");
            endpointConfiguration.AuditProcessedMessagesTo("audit");

            endpointConfiguration.EnableCallbacks();

            var routing = transportExtensions.Routing();
            foreach (var mapping in endpointDefinition.Mappings)
            {
                routing.RouteToEndpoint(mapping.MessageType, mapping.TransportAddress);
            }

            foreach (var publisher in endpointDefinition.Publishers)
            {
                routing.RegisterPublisher(publisher.MessageType, publisher.TransportAddress);
            }

            endpointConfiguration.MakeInstanceUniquelyAddressable("A");

            messageStore = new MessageStore();
            callbackResultStore = new CallbackResultStore();
            subscriptionStore = new SubscriptionStore();

            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(messageStore));
            endpointConfiguration.RegisterComponents(c => c.RegisterSingleton(subscriptionStore));

            endpointInstance = await Endpoint.Start(endpointConfiguration);
        }

        public void SendCommand(Guid messageId)
        {
            endpointInstance.Send(new TestCommand { Id = messageId }).GetAwaiter().GetResult();
        }

        public void SendRequest(Guid requestId)
        {
            endpointInstance.Send(new TestRequest { RequestId = requestId }).GetAwaiter().GetResult();
        }

        public void PublishEvent(Guid eventId)
        {
            endpointInstance.Publish(new TestEvent { EventId = eventId }).GetAwaiter().GetResult();
        }

        public void SendAndCallbackForInt(int value)
        {
            Task.Run(async () =>
            {
                var result = await endpointInstance.Request<int>(new TestIntCallback { Response = value }, new SendOptions());

                callbackResultStore.Add(result);
            });
        }

        public void SendAndCallbackForEnum(CallbackEnum value)
        {
            Task.Run(async () =>
            {
                var result = await endpointInstance.Request<CallbackEnum>(new TestEnumCallback { CallbackEnum = value }, new SendOptions());

                callbackResultStore.Add(result);
            });
        }

        public Guid[] ReceivedMessageIds => messageStore.GetAll();
        public Guid[] ReceivedResponseIds => messageStore.Get<TestResponse>();
        public Guid[] ReceivedEventIds => messageStore.Get<TestEvent>();
        public int[] ReceivedIntCallbacks => callbackResultStore.Get<int>();
        public CallbackEnum[] ReceivedEnumCallbacks => callbackResultStore.Get<CallbackEnum>();
        public int NumberOfSubscriptions => subscriptionStore.NumberOfSubscriptions;
    }
}